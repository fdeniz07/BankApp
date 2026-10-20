using Application.Common.CustomerNumbers;
using Application.Features.Auth.Rules;
using Application.Features.Customers.Rules;
using Application.Features.IndividualCustomers.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.IndividualCustomers.Commands.Create;

public class CreateIndividualCustomerRequestHandler
    : IRequestHandler<CreateIndividualCustomerRequest, CreatedIndividualCustomerResponse>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IOperationClaimRepository _operationClaimRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly IMapper _mapper;
    private readonly IndividualCustomerBusinessRules _businessRules;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly CustomerLifecycleBusinessRules _lifecycleRules;

    public CreateIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IApplicationUserRepository applicationUserRepository,
        IOperationClaimRepository operationClaimRepository,
        IUserOperationClaimRepository userOperationClaimRepository,
        IMapper mapper,
        IndividualCustomerBusinessRules businessRules,
        AuthBusinessRules authBusinessRules,
        CustomerLifecycleBusinessRules lifecycleRules)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _applicationUserRepository = applicationUserRepository;
        _operationClaimRepository = operationClaimRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _authBusinessRules = authBusinessRules;
        _lifecycleRules = lifecycleRules;
    }

    public async Task<CreatedIndividualCustomerResponse> Handle(
        CreateIndividualCustomerRequest request,
        CancellationToken cancellationToken)
    {
        await _businessRules.EmailShouldBeUnique(request.Email);
        await _businessRules.NationalIdShouldBeUnique(request.NationalId);
        await _authBusinessRules.EmailShouldBeUnique(request.Email);
        await _lifecycleRules.GenderShouldBeValid(request.Gender);
        request.Gender = Gender.Normalize(request.Gender!);

        IndividualCustomer individualCustomer = _mapper.Map<IndividualCustomer>(request);
        individualCustomer.CustomerNumber = await AllocateIndividualNumberAsync(request.Locale, cancellationToken);
        individualCustomer.IsActive = true;
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

        CreatedIndividualCustomerResponse response = _mapper.Map<CreatedIndividualCustomerResponse>(individualCustomer);
        response.UserId = applicationUser.Id;
        return response;
    }

    private async Task<string> AllocateIndividualNumberAsync(string? locale, CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < 8; attempt++)
        {
            string candidate = CustomerNumberGenerator.NextIndividual(locale);
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

        throw new InvalidOperationException("Could not allocate a unique individual customer number.");
    }
}
