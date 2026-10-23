using System.Text.Json;
using Application.Services.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Microsoft.Extensions.Logging;

namespace Application.Sagas;

public sealed class CreditApplicationProcessManager : ICreditApplicationProcessManager
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ICreditApplicationSagaRepository _sagas;
    private readonly ILogger<CreditApplicationProcessManager> _logger;

    public CreditApplicationProcessManager(
        ICreditApplicationSagaRepository sagas,
        ILogger<CreditApplicationProcessManager> logger)
    {
        _sagas = sagas;
        _logger = logger;
    }

    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        if (IsEvent(eventType, typeof(CreditApplicationCreated)))
        {
            CreditApplicationCreated? created = JsonSerializer.Deserialize<CreditApplicationCreated>(payload, JsonOptions);
            if (created is null)
                return;
            await HandleCreatedAsync(created, cancellationToken);
            return;
        }

        if (IsEvent(eventType, typeof(CreditApplicationApproved)))
        {
            CreditApplicationApproved? approved = JsonSerializer.Deserialize<CreditApplicationApproved>(payload, JsonOptions);
            if (approved is null)
                return;
            await HandleApprovedAsync(approved, cancellationToken);
            return;
        }

        if (IsEvent(eventType, typeof(CreditApplicationRejected)))
        {
            CreditApplicationRejected? rejected = JsonSerializer.Deserialize<CreditApplicationRejected>(payload, JsonOptions);
            if (rejected is null)
                return;
            await HandleTerminatedAsync(
                rejected.CreditApplicationId,
                rejected.ApplicationNumber,
                $"Rejected: {rejected.RejectionReason}",
                cancellationToken);
            return;
        }

        if (IsEvent(eventType, typeof(CreditApplicationCancelled)))
        {
            CreditApplicationCancelled? cancelled = JsonSerializer.Deserialize<CreditApplicationCancelled>(payload, JsonOptions);
            if (cancelled is null)
                return;
            await HandleTerminatedAsync(
                cancelled.CreditApplicationId,
                cancelled.ApplicationNumber,
                "Cancelled",
                cancellationToken);
            return;
        }

        _logger.LogDebug("Process manager ignored event type {EventType}", eventType);
    }

    private async Task HandleCreatedAsync(CreditApplicationCreated created, CancellationToken cancellationToken)
    {
        CreditApplicationSaga? existing = await _sagas.GetAsync(
            s => s.CreditApplicationId == created.CreditApplicationId,
            cancellationToken: cancellationToken);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Saga already exists for application {ApplicationId}; Created is idempotent.",
                created.CreditApplicationId);
            return;
        }

        CreditApplicationSaga saga = CreditApplicationSaga.StartFromCreated(
            created.CreditApplicationId,
            created.ApplicationNumber);

        await _sagas.AddAsync(saga, cancellationToken);

        _logger.LogInformation(
            "Saga started. ApplicationId={ApplicationId} Number={ApplicationNumber} Status={Status}",
            saga.CreditApplicationId,
            saga.ApplicationNumber,
            saga.Status.Name);
    }

    private async Task HandleApprovedAsync(CreditApplicationApproved approved, CancellationToken cancellationToken)
    {
        CreditApplicationSaga saga = await GetOrCreateAwaitingAsync(
            approved.CreditApplicationId,
            approved.ApplicationNumber,
            cancellationToken);

        if (saga.Status == CreditApplicationSagaStatus.Completed
            || saga.Status == CreditApplicationSagaStatus.Terminated
            || saga.Status == CreditApplicationSagaStatus.Compensated)
        {
            _logger.LogInformation(
                "Saga already terminal ({Status}) for {ApplicationId}; ignoring Approved.",
                saga.Status.Name,
                saga.CreditApplicationId);
            return;
        }

        if (saga.Status == CreditApplicationSagaStatus.Onboarding
            && saga.CurrentStep == CreditApplicationSagaStep.Done)
        {
            return;
        }

        if (saga.Status == CreditApplicationSagaStatus.AwaitingDecision)
            saga.BeginOnboarding();

        try
        {
            await RunOnboardingStepsAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Onboarding failed for {ApplicationId}; compensating.",
                saga.CreditApplicationId);

            if (saga.Status == CreditApplicationSagaStatus.Onboarding)
            {
                saga.Compensate(ex.Message);
                await _sagas.UpdateAsync(saga, cancellationToken);
            }
        }
    }

    private async Task RunOnboardingStepsAsync(CreditApplicationSaga saga, CancellationToken cancellationToken)
    {
        if (saga.CurrentStep.Value < CreditApplicationSagaStep.RiskChecked.Value)
        {
            saga.MarkRiskChecked();
            await _sagas.UpdateAsync(saga, cancellationToken);
            _logger.LogInformation(
                "Saga step RiskChecked. ApplicationId={ApplicationId}",
                saga.CreditApplicationId);
        }

        if (saga.CurrentStep.Value < CreditApplicationSagaStep.ContractPrepared.Value)
        {
            string contractReference = $"CTR-{saga.ApplicationNumber}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
            saga.MarkContractPrepared(contractReference);
            await _sagas.UpdateAsync(saga, cancellationToken);
            _logger.LogInformation(
                "Saga step ContractPrepared. ApplicationId={ApplicationId} ContractReference={ContractReference}",
                saga.CreditApplicationId,
                contractReference);
        }

        if (saga.CurrentStep.Value < CreditApplicationSagaStep.CustomerNotified.Value)
        {
            saga.MarkCustomerNotified();
            await _sagas.UpdateAsync(saga, cancellationToken);
            _logger.LogInformation(
                "Saga step CustomerNotified. ApplicationId={ApplicationId}",
                saga.CreditApplicationId);
        }

        if (saga.Status != CreditApplicationSagaStatus.Completed)
        {
            saga.Complete();
            await _sagas.UpdateAsync(saga, cancellationToken);
            _logger.LogInformation(
                "Saga completed. ApplicationId={ApplicationId} ContractReference={ContractReference}",
                saga.CreditApplicationId,
                saga.ContractReference);
        }
    }

    private async Task HandleTerminatedAsync(
        Guid creditApplicationId,
        string applicationNumber,
        string reason,
        CancellationToken cancellationToken)
    {
        CreditApplicationSaga saga = await GetOrCreateAwaitingAsync(
            creditApplicationId,
            applicationNumber,
            cancellationToken);

        if (saga.Status == CreditApplicationSagaStatus.Completed
            || saga.Status == CreditApplicationSagaStatus.Terminated
            || saga.Status == CreditApplicationSagaStatus.Compensated)
        {
            _logger.LogInformation(
                "Saga already terminal ({Status}) for {ApplicationId}; ignoring terminate ({Reason}).",
                saga.Status.Name,
                creditApplicationId,
                reason);
            return;
        }

        if (saga.Status == CreditApplicationSagaStatus.Onboarding)
        {
            saga.Compensate(reason);
            await _sagas.UpdateAsync(saga, cancellationToken);
            _logger.LogInformation(
                "Saga compensated after decision. ApplicationId={ApplicationId} Reason={Reason}",
                creditApplicationId,
                reason);
            return;
        }

        saga.Terminate(reason);
        await _sagas.UpdateAsync(saga, cancellationToken);
        _logger.LogInformation(
            "Saga terminated. ApplicationId={ApplicationId} Reason={Reason}",
            creditApplicationId,
            reason);
    }

    private async Task<CreditApplicationSaga> GetOrCreateAwaitingAsync(
        Guid creditApplicationId,
        string applicationNumber,
        CancellationToken cancellationToken)
    {
        CreditApplicationSaga? saga = await _sagas.GetAsync(
            s => s.CreditApplicationId == creditApplicationId,
            cancellationToken: cancellationToken);

        if (saga is not null)
            return saga;

        saga = CreditApplicationSaga.StartFromCreated(creditApplicationId, applicationNumber);
        await _sagas.AddAsync(saga, cancellationToken);
        _logger.LogWarning(
            "Saga missing for {ApplicationId}; created AwaitingDecision on late event.",
            creditApplicationId);
        return saga;
    }

    private static bool IsEvent(string eventType, Type clrType) =>
        string.Equals(eventType, clrType.FullName, StringComparison.Ordinal)
        || string.Equals(eventType, clrType.Name, StringComparison.Ordinal)
        || eventType.EndsWith($".{clrType.Name}", StringComparison.Ordinal);
}
