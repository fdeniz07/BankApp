using Application.Modules.Credit.Features.CreditApplications.Constants;
using Application.Modules.Credit.Features.CreditApplications.Queries.GetPaymentPlan;
using Application.Services.Amortization;
using Application.Services.Amortization.Export;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetPaymentPlanExport;

public class GetPaymentPlanExportRequestHandler
    : IRequestHandler<GetPaymentPlanExportRequest, PaymentPlanExportResult>
{
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IPaymentPlanDocumentExporterResolver _exporterResolver;

    public GetPaymentPlanExportRequestHandler(
        ICreditSubTypeRepository creditSubTypeRepository,
        ICreditTypeRepository creditTypeRepository,
        IPaymentPlanDocumentExporterResolver exporterResolver)
    {
        _creditSubTypeRepository = creditSubTypeRepository;
        _creditTypeRepository = creditTypeRepository;
        _exporterResolver = exporterResolver;
    }

    public async Task<PaymentPlanExportResult> Handle(
        GetPaymentPlanExportRequest request,
        CancellationToken cancellationToken)
    {
        if (!_exporterResolver.TryParseFormat(request.Format, out PaymentPlanExportFormat format))
            throw new BusinessException(
                $"Unsupported payment plan export format '{request.Format}'. Supported: pdf.");

        GetPaymentPlanResponse plan = await BuildPlanAsync(request, cancellationToken);
        if (plan.Installments.Count == 0)
            throw new BusinessException("Payment plan has no installments to export.");

        PaymentPlanExportDocument document = new()
        {
            Locale = request.Locale,
            ApplicationNumber = request.ApplicationNumber,
            CustomerName = request.CustomerName,
            Amount = plan.Amount,
            TermMonths = plan.TermMonths,
            InterestRate = plan.InterestRate,
            CurrencyCode = plan.CurrencyCode,
            MonthlyPayment = plan.MonthlyPayment,
            TotalPayment = plan.TotalPayment,
            TotalInterest = plan.TotalInterest,
            Installments = plan.Installments,
            GeneratedAtUtc = DateTime.UtcNow
        };

        return _exporterResolver.Resolve(format).Export(document);
    }

    private async Task<GetPaymentPlanResponse> BuildPlanAsync(
        GetPaymentPlanExportRequest request,
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
