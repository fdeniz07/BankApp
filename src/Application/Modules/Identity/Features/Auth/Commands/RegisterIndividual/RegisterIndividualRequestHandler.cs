using Application.Common.CustomerNumbers;
using Application.Modules.Identity.Features.Auth.Rules;
using Application.Modules.Customers.Features.Customers.Rules;
using Application.Modules.Customers.Features.IndividualCustomers.Rules;
using Application.Services.Logging;
using Application.Services.Messaging;
using Application.Services.Repositories;
using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Identity.Features.Auth.Commands.RegisterIndividual;

public class RegisterIndividualRequestHandler
    : IRequestHandler<RegisterIndividualRequest, RegisteredIndividualResponse>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IOperationClaimRepository _operationClaimRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly IndividualCustomerBusinessRules _individualCustomerBusinessRules;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly CustomerLifecycleBusinessRules _lifecycleRules;
    private readonly IAuditLogger _auditLogger;
    private readonly IEmailConfirmationPublisher _emailConfirmationPublisher;

    public RegisterIndividualRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IApplicationUserRepository applicationUserRepository,
        IOperationClaimRepository operationClaimRepository,
        IUserOperationClaimRepository userOperationClaimRepository,
        IndividualCustomerBusinessRules individualCustomerBusinessRules,
        AuthBusinessRules authBusinessRules,
        CustomerLifecycleBusinessRules lifecycleRules,
        IAuditLogger auditLogger,
        IEmailConfirmationPublisher emailConfirmationPublisher)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _applicationUserRepository = applicationUserRepository;
        _operationClaimRepository = operationClaimRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _individualCustomerBusinessRules = individualCustomerBusinessRules;
        _authBusinessRules = authBusinessRules;
        _lifecycleRules = lifecycleRules;
        _auditLogger = auditLogger;
        _emailConfirmationPublisher = emailConfirmationPublisher;
    }

    public async Task<RegisteredIndividualResponse> Handle(
        RegisterIndividualRequest request,
        CancellationToken cancellationToken)
    {
        await _lifecycleRules.GenderShouldBeValid(request.Gender);
        string gender = Gender.Normalize(request.Gender);

        string customerNumber = CustomerNumberGenerator.NextIndividual(request.Locale);
        await _individualCustomerBusinessRules.CustomerNumberShouldBeUnique(customerNumber);
        await _individualCustomerBusinessRules.EmailShouldBeUnique(request.Email);
        await _individualCustomerBusinessRules.NationalIdShouldBeUnique(request.NationalId);
        await _authBusinessRules.EmailShouldBeUnique(request.Email);

        IndividualCustomer individualCustomer = new()
        {
            CustomerNumber = customerNumber,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            FirstName = request.FirstName,
            LastName = request.LastName,
            NationalId = request.NationalId,
            DateOfBirth = request.DateOfBirth,
            Gender = gender,
            MotherName = request.MotherName,
            FatherName = request.FatherName,
            IsActive = true
        };

        await _individualCustomerRepository.AddAsync(individualCustomer, cancellationToken);

        HashingHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
        string confirmationToken = Convert.ToHexString(Guid.NewGuid().ToByteArray())
            + Convert.ToHexString(Guid.NewGuid().ToByteArray());

        ApplicationUser applicationUser = new()
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CustomerId = individualCustomer.Id,
            IsActive = true,
            EmailConfirmed = false,
            EmailConfirmationToken = confirmationToken,
            EmailConfirmationTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth
        };
        await _applicationUserRepository.AddAsync(applicationUser, cancellationToken);

        OperationClaim? customerClaim = await _operationClaimRepository.GetAsync(
            c => c.Name == GeneralOperationClaims.Customer,
            cancellationToken: cancellationToken);

        if (customerClaim is null)
            throw new InvalidOperationException($"Operation claim '{GeneralOperationClaims.Customer}' is not seeded.");

        await _userOperationClaimRepository.AddAsync(
            new UserOperationClaim
            {
                UserId = applicationUser.Id,
                OperationClaimId = customerClaim.Id
            },
            cancellationToken);

        string displayName = $"{individualCustomer.FirstName} {individualCustomer.LastName}".Trim();
        const string customerCategory = "Individual";

        await _emailConfirmationPublisher.PublishAsync(
            new EmailConfirmationRequested(
                applicationUser.Id,
                applicationUser.Email,
                confirmationToken),
            cancellationToken);

        _auditLogger.Log(
            AuditEventCategories.Auth,
            AuditEventActions.Register,
            "Individual customer registered; email confirmation pending.",
            actorUserId: applicationUser.Id,
            actorEmail: applicationUser.Email,
            targetType: nameof(IndividualCustomer),
            targetId: individualCustomer.Id);

        return new RegisteredIndividualResponse
        {
            RequiresEmailConfirmation = true,
            Email = applicationUser.Email,
            DisplayName = displayName,
            CustomerCategory = customerCategory,
            Message = "Registration successful. Please confirm your email before signing in."
        };
    }
}
