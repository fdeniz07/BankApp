using Application.Features.CreditApplications.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.CreditApplications.Commands.Delete;

public class DeleteCreditApplicationRequestHandler
    : IRequestHandler<DeleteCreditApplicationRequest, DeletedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;

    public DeleteCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<DeletedCreditApplicationResponse> Handle(
        DeleteCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? creditApplication = await _creditApplicationRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditApplicationShouldExistWhenSelected(creditApplication);

        if (creditApplication!.Status == CreditApplicationStatus.Pending)
            creditApplication.Status = CreditApplicationStatus.Cancelled;

        await _creditApplicationRepository.DeleteAsync(creditApplication, cancellationToken: cancellationToken);

        return _mapper.Map<DeletedCreditApplicationResponse>(creditApplication);
    }
}
