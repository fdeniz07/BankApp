using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Delete;

public class DeleteCreditApplicationRequestHandler
    : IRequestHandler<DeleteCreditApplicationRequest, DeletedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCreditApplicationRequestHandler(
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

    public async Task<DeletedCreditApplicationResponse> Handle(
        DeleteCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? creditApplication = await _creditApplicationRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditApplicationShouldExistWhenSelected(creditApplication);
        _currentUserService.EnsureCanAccessCustomer(creditApplication!.CustomerId);

        if (creditApplication.Status == CreditApplicationStatus.Pending)
            creditApplication.Status = CreditApplicationStatus.Cancelled;

        await _creditApplicationRepository.DeleteAsync(creditApplication, cancellationToken: cancellationToken);

        return _mapper.Map<DeletedCreditApplicationResponse>(creditApplication);
    }
}
