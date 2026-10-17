using Application.Features.Auth.Rules;
using Application.Services.Logging;
using Application.Services.Repositories;
using Core.Security.Entities;
using Core.Security.JWT;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginRequestHandler : IRequestHandler<LoginRequest, LoggedResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly ITokenHelper _tokenHelper;
    private readonly AuthBusinessRules _businessRules;
    private readonly IAuditLogger _auditLogger;

    public LoginRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        IIndividualCustomerRepository individualCustomerRepository,
        ICorporateCustomerRepository corporateCustomerRepository,
        ITokenHelper tokenHelper,
        AuthBusinessRules businessRules,
        IAuditLogger auditLogger)
    {
        _applicationUserRepository = applicationUserRepository;
        _individualCustomerRepository = individualCustomerRepository;
        _corporateCustomerRepository = corporateCustomerRepository;
        _tokenHelper = tokenHelper;
        _businessRules = businessRules;
        _auditLogger = auditLogger;
    }

    public async Task<LoggedResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _applicationUserRepository.GetAsync(
            u => u.Email == request.Email,
            cancellationToken: cancellationToken);

        try
        {
            await _businessRules.UserShouldExist(user);
            await _businessRules.UserShouldBeActive(user!);
            await _businessRules.PasswordShouldBeCorrect(request.Password, user!.PasswordHash, user.PasswordSalt);
        }
        catch
        {
            _auditLogger.Log(
                AuditEventCategories.Auth,
                AuditEventActions.LoginFailed,
                "Login failed.",
                actorEmail: request.Email);
            throw;
        }

        (string? displayName, string? customerCategory) = await ResolveProfileAsync(user!, cancellationToken);
        IList<OperationClaim> claims = await _applicationUserRepository.GetClaimsAsync(user!, cancellationToken);
        AccessToken accessToken = _tokenHelper.CreateToken(
            user!, claims, user!.CustomerId, displayName, customerCategory);

        _auditLogger.Log(
            AuditEventCategories.Auth,
            AuditEventActions.Login,
            "Login succeeded.",
            actorUserId: user.Id,
            actorEmail: user.Email);

        return new LoggedResponse
        {
            AccessToken = accessToken,
            DisplayName = displayName,
            CustomerCategory = customerCategory
        };
    }

    private async Task<(string? DisplayName, string? CustomerCategory)> ResolveProfileAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        if (!user.CustomerId.HasValue)
        {
            string? composed = $"{user.FirstName} {user.LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(composed))
                return (composed, null);
            return (ResolveStaffDisplayName(user.Email), null);
        }

        IndividualCustomer? individual = await _individualCustomerRepository.GetAsync(
            c => c.Id == user.CustomerId.Value,
            cancellationToken: cancellationToken);
        if (individual is not null)
            return ($"{individual.FirstName} {individual.LastName}".Trim(), "Individual");

        CorporateCustomer? corporate = await _corporateCustomerRepository.GetAsync(
            c => c.Id == user.CustomerId.Value,
            cancellationToken: cancellationToken);
        if (corporate is not null)
            return (corporate.CompanyName, "Corporate");

        return (user.Email.Split('@')[0], null);
    }

    private static string ResolveStaffDisplayName(string email) =>
        email.Trim().ToLowerInvariant() switch
        {
            "staff.tr@bank.local" => "Ayşe Yılmaz",
            "staff.en@bank.local" => "Jane Smith",
            "staff.de@bank.local" => "Anna Müller",
            "staff@bank.local" => "Bank Staff",
            "manager@bank.local" => "Bank Manager",
            _ => email.Contains('@') ? email.Split('@')[0] : email
        };
}
