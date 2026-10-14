namespace Core.Caching;

public class CacheSettings
{
    public const string SectionName = "CacheSettings";

    /// <summary>Default sliding expiration in minutes.</summary>
    public int SlidingExpirationMinutes { get; set; } = 30;
}
