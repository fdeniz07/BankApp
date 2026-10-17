namespace Application.Features.StaffUsers.Constants;

public static class StaffUserMessages
{
    public const string NotExists = "Staff user does not exist.";
    public const string NotStaffAccount = "User is not a staff account.";
    public const string CannotModifySelf = "You cannot deactivate your own staff account.";
    public const string CannotDeactivateLastManager = "Cannot deactivate the last active bank manager.";
    public const string CannotDemoteLastManager = "Cannot remove manager role from the last active bank manager.";
    public const string BankStaffClaimMissing = "Operation claim 'BankStaff' is not seeded.";
    public const string BankManagerClaimMissing = "Operation claim 'BankManager' is not seeded.";
}
