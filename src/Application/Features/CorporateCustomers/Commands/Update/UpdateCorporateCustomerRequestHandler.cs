using Application.Features.Auth.Rules;
using Application.Features.CorporateCustomers.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.CorporateCustomers.Commands.Update;

public class UpdateCorporateCustomerRequestHandler
    : IRequestHandler<UpdateCorporateCustomerRequest, UpdatedCorporateCustomerResponse>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IMapper _mapper;
    private readonly CorporateCustomerBusinessRules _businessRules;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IApplicationUserRepository applicationUserRepository,
        IMapper mapper,
        CorporateCustomerBusinessRules businessRules,
        AuthBusinessRules authBusinessRules,
        ICurrentUserService currentUserService)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _applicationUserRepository = applicationUserRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _authBusinessRules = authBusinessRules;
        _currentUserService = currentUserService;
    }

    public async Task<UpdatedCorporateCustomerResponse> Handle(
        UpdateCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        _currentUserService.EnsureCanAccessCustomer(request.Id);

        CorporateCustomer? corporateCustomer = await _corporateCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CorporateCustomerShouldExistWhenSelected(corporateCustomer);
        // CustomerNumber is system-assigned and immutable.
        await _businessRules.EmailShouldBeUnique(request.Email, request.Id);
        await _businessRules.TaxNumberShouldBeUnique(request.TaxNumber, request.Id);
        await _businessRules.CompanyRegistrationNumberShouldBeUnique(request.CompanyRegistrationNumber, request.Id);

        ApplicationUser? applicationUser = await _applicationUserRepository.GetAsync(
            u => u.CustomerId == request.Id,
            cancellationToken: cancellationToken);

        if (applicationUser is not null &&
            !string.Equals(applicationUser.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            await _authBusinessRules.EmailShouldBeUnique(request.Email, applicationUser.Id);
            applicationUser.Email = request.Email;
            await _applicationUserRepository.UpdateAsync(applicationUser, cancellationToken);
        }

        corporateCustomer = _mapper.Map(request, corporateCustomer);
        await _corporateCustomerRepository.UpdateAsync(corporateCustomer!, cancellationToken);

        return _mapper.Map<UpdatedCorporateCustomerResponse>(corporateCustomer);
    }
}
