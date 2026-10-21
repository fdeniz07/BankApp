namespace WebApi.Idempotency;

public sealed class IdempotencyCacheEntry
{
    public string Fingerprint { get; set; } = default!;
    public int StatusCode { get; set; }
    public string Body { get; set; } = default!;
}
