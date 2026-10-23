namespace WebApi.Realtime;

public static class CreditApplicationRealtimeGroups
{
    public const string Staff = "staff";

    public static string Customer(Guid customerId) => $"customer:{customerId:D}";
}
