using Application.Features.CreditApplications.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.CreditApplications.Commands.Create;

public class CreateCreditApplicationRequestHandler
    : IRequestHandler<CreateCreditApplicationRequest, CreatedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;

    public CreateCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<CreatedCreditApplicationResponse> Handle(
        CreateCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        await _businessRules.CustomerShouldExistAndBeActive(request.CustomerId);
        await _businessRules.CreditSubTypeShouldExistAndBeActive(request.CreditSubTypeId);
        await _businessRules.CustomerCategoryShouldMatchCreditSubType(request.CustomerId, request.CreditSubTypeId);
        await _businessRules.RequestShouldBeWithinCreditSubTypeLimits(
            request.CreditSubTypeId,
            request.RequestedAmount,
            request.RequestedTermMonths);

        CreditApplication creditApplication = _mapper.Map<CreditApplication>(request);
        creditApplication.ApplicationNumber = $"APP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        creditApplication.Status = CreditApplicationStatus.Pending;
        creditApplication.ApplicationDate = DateTimeOffset.UtcNow;

        await _businessRules.ApplicationNumberShouldBeUnique(creditApplication.ApplicationNumber);
        await _creditApplicationRepository.AddAsync(creditApplication, cancellationToken);

        return _mapper.Map<CreatedCreditApplicationResponse>(creditApplication);
    }
}
