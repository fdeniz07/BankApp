using Application.Common.CustomerNumbers;
using Application.Features.Auth.Rules;
using Application.Features.CorporateCustomers.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Domain.Entities;
using MediatR;

namespace Application.Features.CorporateCustomers.Commands.Create;

public class CreateCorporateCustomerRequestHandler
    : IRequestHandler<CreateCorporateCustomerRequest, CreatedCorporateCustomerResponse>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IOperationClaimRepository _operationClaimRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly IMapper _mapper;
    private readonly CorporateCustomerBusinessRules _businessRules;
    private readonly AuthBusinessRules _authBusinessRules;

    public CreateCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IApplicationUserRepository applicationUserRepository,
        IOperationClaimRepository operationClaimRepository,
        IUserOperationClaimRepository userOperationClaimRepository,
        IMapper mapper,
        CorporateCustomerBusinessRules businessRules,
        AuthBusinessRules authBusinessRules)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _applicationUserRepository = applicationUserRepository;
        _operationClaimRepository = operationClaimRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _authBusinessRules = authBusinessRules;
    }

    public async Task<CreatedCorporateCustomerResponse> Handle(
        CreateCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        await _businessRules.EmailShouldBeUnique(request.Email);
        await _businessRules.TaxNumberShouldBeUnique(request.TaxNumber);
        await _businessRules.CompanyRegistrationNumberShouldBeUnique(request.CompanyRegistrationNumber);
        await _authBusinessRules.EmailShouldBeUnique(request.Email);

        CorporateCustomer corporateCustomer = _mapper.Map<CorporateCustomer>(request);
        corporateCustomer.CustomerNumber = await AllocateCorporateNumberAsync(request.Locale, cancellationToken);
        corporateCustomer.IsActive = true;
        await _corporateCustomerRepository.AddAsync(corporateCustomer, cancellationToken);

        HashingHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        ApplicationUser applicationUser = new()
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CustomerId = corporateCustomer.Id,
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

        CreatedCorporateCustomerResponse response = _mapper.Map<CreatedCorporateCustomerResponse>(corporateCustomer);
        response.UserId = applicationUser.Id;
        return response;
    }

    private async Task<string> AllocateCorporateNumberAsync(string? locale, CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < 8; attempt++)
        {
            string candidate = CustomerNumberGenerator.NextCorporate(locale);
            try
            {
                await _businessRules.CustomerNumberShouldBeUnique(candidate);
                return candidate;
            }
            catch
            {
                // rare collision — retry
            }
        }

        throw new InvalidOperationException("Could not allocate a unique corporate customer number.");
    }
}
