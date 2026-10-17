namespace Application.Services.Logging;

public static class AuditEventCategories
{
    public const string Auth = "Auth";
    public const string Audit = "Audit";
    public const string Credit = "Credit";
    public const string Customer = "Customer";
    public const string Staff = "Staff";
    public const string Profile = "Profile";
}

public static class AuditEventActions
{
    public const string Login = "Login";
    public const string LoginFailed = "LoginFailed";
    public const string Register = "Register";
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Approve = "Approve";
    public const string Reject = "Reject";
    public const string Cancel = "Cancel";
    public const string Upload = "Upload";
}
