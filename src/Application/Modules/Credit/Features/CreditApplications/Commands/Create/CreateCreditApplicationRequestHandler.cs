using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Create;

public class CreateCreditApplicationRequestHandler
    : IRequestHandler<CreateCreditApplicationRequest, CreatedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public CreateCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        ICustomerRepository customerRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<CreatedCreditApplicationResponse> Handle(
        CreateCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.IsCustomer && !_currentUserService.IsBankStaff)
            request.CustomerId = _currentUserService.GetRequiredCustomerId();
        else
            await _branchAuthorization.EnsureCanModifyCustomerAsync(request.CustomerId, cancellationToken);

        await _businessRules.CustomerShouldExistAndBeActive(request.CustomerId);
        await _businessRules.CreditSubTypeShouldExistAndBeActive(request.CreditSubTypeId);
        await _businessRules.CustomerCategoryShouldMatchCreditSubType(request.CustomerId, request.CreditSubTypeId);
        await _businessRules.RequestShouldBeWithinCreditSubTypeLimits(
            request.CreditSubTypeId,
            request.RequestedAmount,
            request.RequestedTermMonths);

        CreditApplication creditApplication = _mapper.Map<CreditApplication>(request);
        string applicationNumber = $"APP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        await _businessRules.ApplicationNumberShouldBeUnique(applicationNumber);
        creditApplication.InitializeAsNewApplication(applicationNumber);

        Customer? customer = await _customerRepository.GetAsync(
            c => c.Id == request.CustomerId,
            cancellationToken: cancellationToken);
        creditApplication.BranchId = customer?.BranchId;

        await _creditApplicationRepository.AddAsync(creditApplication, cancellationToken);

        return _mapper.Map<CreatedCreditApplicationResponse>(creditApplication);
    }
}
