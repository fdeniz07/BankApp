namespace WebApi.Constants;

public static class RateLimitPolicies
{
    /// <summary>Default API traffic (lists, reads, most controllers).</summary>
    public const string Fixed = "fixed";

    /// <summary>Stricter limit for login / register / confirm-email.</summary>
    public const string Auth = "auth";

    /// <summary>Credit application mutating endpoints (create / approve / reject / cancel / update / delete).</summary>
    public const string Write = "write";
}
