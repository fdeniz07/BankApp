using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace WebApi.IntegrationTests.Infrastructure;

internal static class IdempotencyTestHelper
{
    public static async Task<HttpResponseMessage> PostAsJsonWithIdempotencyAsync<T>(
        this HttpClient client,
        string url,
        T body,
        string? idempotencyKey = null)
    {
        string json = JsonSerializer.Serialize(body);
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation(
            "Idempotency-Key",
            idempotencyKey ?? Guid.NewGuid().ToString("N"));
        return await client.SendAsync(request);
    }
}
