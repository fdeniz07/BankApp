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
    private readonly IBranchAuthorizationService _branchAuthorization;

    public UpdateCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules,
        IBranchAuthorizationService branchAuthorization)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<UpdatedCreditApplicationResponse> Handle(
        UpdateCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? creditApplication = await _creditApplicationRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditApplicationShouldExistWhenSelected(creditApplication);
        CreditApplication entity = creditApplication!;
        await _branchAuthorization.EnsureCanModifyApplicationAsync(entity, cancellationToken);
        await _businessRules.ApplicationShouldBePendingForUpdate(entity);
        await _businessRules.RequestShouldBeWithinCreditSubTypeLimits(
            entity.CreditSubTypeId,
            request.RequestedAmount,
            request.RequestedTermMonths);

        entity = _mapper.Map(request, entity);
        await _creditApplicationRepository.UpdateAsync(entity, cancellationToken);

        return _mapper.Map<UpdatedCreditApplicationResponse>(entity);
    }
}
