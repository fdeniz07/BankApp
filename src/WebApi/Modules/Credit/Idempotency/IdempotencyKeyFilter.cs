using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.SmartEnum.SystemTextJson;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace WebApi.Modules.Credit.Idempotency;

/// <summary>
/// Resource filter (runs before model binding) so the request body can be read for fingerprinting.
/// </summary>
public sealed class IdempotencyKeyFilter : IAsyncResourceFilter
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new SmartEnumNameConverter<CreditApplicationStatus, int>());
        return options;
    }

    private readonly IDistributedCache _cache;
    private readonly IdempotencyOptions _options;
    private readonly ILogger<IdempotencyKeyFilter> _logger;

    public IdempotencyKeyFilter(
        IDistributedCache cache,
        IOptions<IdempotencyOptions> options,
        ILogger<IdempotencyKeyFilter> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        HttpRequest request = context.HttpContext.Request;
        request.EnableBuffering();

        if (!request.Headers.TryGetValue(_options.HeaderName, out var headerValues)
            || string.IsNullOrWhiteSpace(headerValues.FirstOrDefault()))
        {
            context.Result = Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Missing Idempotency-Key",
                detail: $"Header '{_options.HeaderName}' is required for this operation.");
            return;
        }

        string idempotencyKey = headerValues.First()!.Trim();
        if (idempotencyKey.Length > 128)
        {
            context.Result = Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Idempotency-Key",
                detail: $"Header '{_options.HeaderName}' must be at most 128 characters.");
            return;
        }

        string fingerprint = await ComputeFingerprintAsync(request);
        string cacheKey = $"idempotency:{idempotencyKey}";
        string lockKey = $"idempotency:lock:{idempotencyKey}";

        byte[]? cachedBytes = await _cache.GetAsync(cacheKey);
        if (cachedBytes is { Length: > 0 })
        {
            IdempotencyCacheEntry? cached = JsonSerializer.Deserialize<IdempotencyCacheEntry>(cachedBytes, JsonOptions);
            if (cached is null)
            {
                await _cache.RemoveAsync(cacheKey);
            }
            else if (!string.Equals(cached.Fingerprint, fingerprint, StringComparison.Ordinal))
            {
                context.Result = Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Idempotency-Key conflict",
                    detail: "The same Idempotency-Key was reused with a different request payload.");
                return;
            }
            else
            {
                _logger.LogInformation(
                    "Idempotency replay. Key={IdempotencyKey} Status={StatusCode}",
                    idempotencyKey,
                    cached.StatusCode);
                context.Result = new ContentResult
                {
                    Content = cached.Body,
                    ContentType = "application/json; charset=utf-8",
                    StatusCode = cached.StatusCode
                };
                return;
            }
        }

        byte[]? existingLock = await _cache.GetAsync(lockKey);
        if (existingLock is { Length: > 0 })
        {
            context.Result = Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Request in progress",
                detail: "A request with this Idempotency-Key is already being processed.");
            return;
        }

        await _cache.SetAsync(
            lockKey,
            "1"u8.ToArray(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.LockSeconds)
            });

        ResourceExecutedContext executed;
        try
        {
            executed = await next();
        }
        finally
        {
            await _cache.RemoveAsync(lockKey);
        }

        if (executed.Canceled || executed.Exception is not null)
            return;

        if (executed.Result is not ObjectResult objectResult)
            return;

        int statusCode = objectResult.StatusCode
            ?? (context.HttpContext.Response.StatusCode is >= 200 and < 600
                ? context.HttpContext.Response.StatusCode
                : StatusCodes.Status200OK);

        string body = objectResult.Value is null
            ? string.Empty
            : JsonSerializer.Serialize(objectResult.Value, objectResult.Value.GetType(), JsonOptions);

        var entry = new IdempotencyCacheEntry
        {
            Fingerprint = fingerprint,
            StatusCode = statusCode,
            Body = body
        };

        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(entry, JsonOptions);
        await _cache.SetAsync(
            cacheKey,
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_options.TtlHours)
            });

        _logger.LogInformation(
            "Idempotency stored. Key={IdempotencyKey} Status={StatusCode}",
            idempotencyKey,
            statusCode);
    }

    private static async Task<string> ComputeFingerprintAsync(HttpRequest request)
    {
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        string body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        string material = $"{request.Method}|{request.Path.Value}|{body}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(hash);
    }

    private static ObjectResult Problem(int statusCode, string title, string detail) =>
        new(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = "https://tools.ietf.org/html/rfc9110#section-15"
        })
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
}
