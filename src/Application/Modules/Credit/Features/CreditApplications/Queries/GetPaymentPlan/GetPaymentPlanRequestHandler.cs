using Application.Modules.Credit.Features.CreditApplications.Constants;
using Application.Services.Amortization;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetPaymentPlan;

public class GetPaymentPlanRequestHandler : IRequestHandler<GetPaymentPlanRequest, GetPaymentPlanResponse>
{
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly ICreditTypeRepository _creditTypeRepository;

    public GetPaymentPlanRequestHandler(
        ICreditSubTypeRepository creditSubTypeRepository,
        ICreditTypeRepository creditTypeRepository)
    {
        _creditSubTypeRepository = creditSubTypeRepository;
        _creditTypeRepository = creditTypeRepository;
    }

    public async Task<GetPaymentPlanResponse> Handle(
        GetPaymentPlanRequest request,
        CancellationToken cancellationToken)
    {
        decimal interestRate = request.InterestRate ?? 0;
        string currency = "USD";

        if (request.CreditSubTypeId.HasValue)
        {
            CreditSubType? subType = await _creditSubTypeRepository.GetAsync(
                c => c.Id == request.CreditSubTypeId.Value,
                cancellationToken: cancellationToken);
            if (subType is null)
                throw new BusinessException(CreditApplicationMessages.CreditSubTypeNotExists);

            interestRate = request.InterestRate ?? subType.InterestRate;
            CreditType? creditType = await _creditTypeRepository.GetAsync(
                c => c.Id == subType.CreditTypeId,
                cancellationToken: cancellationToken);
            currency = string.IsNullOrWhiteSpace(creditType?.CurrencyCode) ? "USD" : creditType.CurrencyCode;
        }

        IReadOnlyList<PaymentPlanInstallment> installments = PaymentPlanCalculator.Build(
            request.Amount,
            request.TermMonths,
            interestRate);

        return new GetPaymentPlanResponse
        {
            Amount = request.Amount,
            TermMonths = request.TermMonths,
            InterestRate = interestRate,
            CurrencyCode = currency,
            MonthlyPayment = installments.FirstOrDefault()?.Payment ?? 0,
            TotalPayment = installments.Sum(i => i.Payment),
            TotalInterest = installments.Sum(i => i.Interest),
            Installments = installments.ToList()
        };
    }
}
