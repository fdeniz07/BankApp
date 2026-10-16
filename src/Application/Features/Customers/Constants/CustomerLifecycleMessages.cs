namespace Application.Features.Customers.Constants;

public static class CustomerLifecycleMessages
{
    public const string MustBeInactiveBeforeDelete =
        "Customer must be inactive before delete.";

    public const string HasOpenCreditApplications =
        "Customer has open credit applications (Pending or Approved) and cannot be deleted.";

    public const string InvalidGender =
        "Gender must be one of: Female, Male, PreferNotToSay.";
}
