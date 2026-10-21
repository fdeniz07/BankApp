namespace WebApi.Idempotency;

public sealed class IdempotencyOptions
{
    public const string SectionName = "Idempotency";

    public string HeaderName { get; set; } = "Idempotency-Key";
    public int TtlHours { get; set; } = 24;
    public int LockSeconds { get; set; } = 30;
}
