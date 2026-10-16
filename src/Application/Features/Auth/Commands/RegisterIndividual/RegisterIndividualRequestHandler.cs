using Application.Features.Auth.Rules;
using Application.Features.Customers.Rules;
using Application.Features.IndividualCustomers.Rules;
using Application.Services.Repositories;
using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Core.Security.JWT;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterIndividual;

public class RegisterIndividualRequestHandler
    : IRequestHandler<RegisterIndividualRequest, RegisteredIndividualResponse>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IOperationClaimRepository _operationClaimRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly ITokenHelper _tokenHelper;
    private readonly IndividualCustomerBusinessRules _individualCustomerBusinessRules;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly CustomerLifecycleBusinessRules _lifecycleRules;

    public RegisterIndividualRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IApplicationUserRepository applicationUserRepository,
        IOperationClaimRepository operationClaimRepository,
        IUserOperationClaimRepository userOperationClaimRepository,
        ITokenHelper tokenHelper,
        IndividualCustomerBusinessRules individualCustomerBusinessRules,
        AuthBusinessRules authBusinessRules,
        CustomerLifecycleBusinessRules lifecycleRules)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _applicationUserRepository = applicationUserRepository;
        _operationClaimRepository = operationClaimRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _tokenHelper = tokenHelper;
        _individualCustomerBusinessRules = individualCustomerBusinessRules;
        _authBusinessRules = authBusinessRules;
        _lifecycleRules = lifecycleRules;
    }

    public async Task<RegisteredIndividualResponse> Handle(
        RegisterIndividualRequest request,
        CancellationToken cancellationToken)
    {
        await _lifecycleRules.GenderShouldBeValid(request.Gender);
        string gender = Gender.Normalize(request.Gender);

        string customerNumber = $"IND-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

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

        ApplicationUser applicationUser = new()
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CustomerId = individualCustomer.Id,
            IsActive = true
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

        IList<OperationClaim> claims = await _applicationUserRepository.GetClaimsAsync(applicationUser, cancellationToken);
        AccessToken accessToken = _tokenHelper.CreateToken(
            applicationUser,
            claims,
            applicationUser.CustomerId,
            displayName,
            customerCategory);

        return new RegisteredIndividualResponse
        {
            AccessToken = accessToken,
            DisplayName = displayName,
            CustomerCategory = customerCategory
        };
    }
}
