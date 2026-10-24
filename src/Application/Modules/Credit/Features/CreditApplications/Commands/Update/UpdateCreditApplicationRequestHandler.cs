using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Update;

public class UpdateCreditApplicationRequestHandler
    : IRequestHandler<UpdateCreditApplicationRequest, UpdatedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules,
        ICurrentUserService currentUserService)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _currentUserService = currentUserService;
    }

    public async Task<UpdatedCreditApplicationResponse> Handle(
        UpdateCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? creditApplication = await _creditApplicationRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditApplicationShouldExistWhenSelected(creditApplication);
        _currentUserService.EnsureCanAccessCustomer(creditApplication!.CustomerId);
        await _businessRules.ApplicationShouldBePendingForUpdate(creditApplication);
        await _businessRules.RequestShouldBeWithinCreditSubTypeLimits(
            creditApplication.CreditSubTypeId,
            request.RequestedAmount,
            request.RequestedTermMonths);

        creditApplication = _mapper.Map(request, creditApplication);
        await _creditApplicationRepository.UpdateAsync(creditApplication!, cancellationToken);

        return _mapper.Map<UpdatedCreditApplicationResponse>(creditApplication);
    }
}
