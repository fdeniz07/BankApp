namespace Core.Security.Constants;

public static class GeneralOperationClaims
{
    public const string Customer = "Customer";
    public const string BankStaff = "BankStaff";
    public const string BankManager = "BankManager";
    /// <summary>Cross-branch admin: full read/write, branch CRUD, staff assignment.</summary>
    public const string SuperAdmin = "SuperAdmin";
}
