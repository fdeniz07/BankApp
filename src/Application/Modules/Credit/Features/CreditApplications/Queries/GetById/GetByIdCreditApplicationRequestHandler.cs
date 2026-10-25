using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetById;

public class GetByIdCreditApplicationRequestHandler
    : IRequestHandler<GetByIdCreditApplicationRequest, GetByIdCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;

    public GetByIdCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        ICustomerRepository customerRepository,
        IApplicationUserRepository applicationUserRepository,
        ICreditSubTypeRepository creditSubTypeRepository,
        ICreditTypeRepository creditTypeRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules,
        ICurrentUserService currentUserService)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _customerRepository = customerRepository;
        _applicationUserRepository = applicationUserRepository;
        _creditSubTypeRepository = creditSubTypeRepository;
        _creditTypeRepository = creditTypeRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _currentUserService = currentUserService;
    }

    public async Task<GetByIdCreditApplicationResponse> Handle(
        GetByIdCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? creditApplication = await _creditApplicationRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditApplicationShouldExistWhenSelected(creditApplication);
        _currentUserService.EnsureCanAccessCustomer(creditApplication!.CustomerId);

        GetByIdCreditApplicationResponse response = _mapper.Map<GetByIdCreditApplicationResponse>(creditApplication);

        Customer? customer = await _customerRepository.GetAsync(
            c => c.Id == creditApplication.CustomerId,
            cancellationToken: cancellationToken);

        response.CustomerName = ResolveCustomerDisplayName(customer);
        response.CustomerCategory = ResolveCustomerCategory(customer);
        response.CustomerNumber = customer?.CustomerNumber;
        response.CustomerIdentityNumber = ResolveCustomerIdentityNumber(customer);
        await PopulateDecisionMakerAsync(response, creditApplication.DecisionBy, cancellationToken);

        CreditSubType? subType = await _creditSubTypeRepository.GetAsync(
            c => c.Id == creditApplication.CreditSubTypeId,
            cancellationToken: cancellationToken);
        response.CreditSubTypeName = subType?.Name ?? string.Empty;
        response.CurrencyCode = await ResolveCurrencyCodeAsync(subType, cancellationToken);
        return response;
    }

    private async Task PopulateDecisionMakerAsync(
        GetByIdCreditApplicationResponse response,
        string? decisionBy,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(decisionBy))
            return;

        response.DecisionByEmail = decisionBy;

        string email = decisionBy.Trim();
        ApplicationUser? staff = await _applicationUserRepository.GetAsync(
            u => u.Email == email,
            cancellationToken: cancellationToken);

        if (staff is null)
            return;

        string name = $"{staff.FirstName} {staff.LastName}".Trim();
        response.DecisionByName = string.IsNullOrWhiteSpace(name) ? null : name;
        response.DecisionByPosition = staff.Position?.Name;
        response.DecisionByEmail = staff.Email;
    }

    private async Task<string> ResolveCurrencyCodeAsync(CreditSubType? subType, CancellationToken cancellationToken)
    {
        if (subType is null)
            return "USD";

        CreditType? creditType = await _creditTypeRepository.GetAsync(
            c => c.Id == subType.CreditTypeId,
            cancellationToken: cancellationToken);

        return string.IsNullOrWhiteSpace(creditType?.CurrencyCode) ? "USD" : creditType.CurrencyCode;
    }

    private static string ResolveCustomerDisplayName(Customer? customer) =>
        customer switch
        {
            IndividualCustomer individual => $"{individual.FirstName} {individual.LastName}".Trim(),
            CorporateCustomer corporate => corporate.CompanyName,
            not null => customer.CustomerNumber,
            _ => string.Empty
        };

    private static CreditCategory? ResolveCustomerCategory(Customer? customer) =>
        customer switch
        {
            IndividualCustomer => CreditCategory.Individual,
            CorporateCustomer => CreditCategory.Corporate,
            _ => null
        };

    private static string? ResolveCustomerIdentityNumber(Customer? customer) =>
        customer switch
        {
            IndividualCustomer individual => individual.NationalId,
            CorporateCustomer corporate => corporate.TaxNumber,
            _ => null
        };
}
