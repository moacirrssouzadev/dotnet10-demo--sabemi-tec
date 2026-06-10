using Microsoft.Extensions.Logging;
using Sabemi.PaymentWebhook.Application.Abstractions;
using Sabemi.PaymentWebhook.Domain.Entities;
using Sabemi.PaymentWebhook.Domain.Enums;

namespace Sabemi.PaymentWebhook.Application.UseCases;

public sealed class ProcessPaymentWebhookUseCase
{
    private readonly IWebhookEventRepository _webhookEvents;
    private readonly IContractStatusRepository _contractStatuses;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<ProcessPaymentWebhookUseCase> _logger;

    public ProcessPaymentWebhookUseCase(
        IWebhookEventRepository webhookEvents,
        IContractStatusRepository contractStatuses,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<ProcessPaymentWebhookUseCase> logger)
    {
        _webhookEvents = webhookEvents;
        _contractStatuses = contractStatuses;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid webhookEventId, CancellationToken cancellationToken)
    {
        var webhookEvent = await _webhookEvents.GetByIdAsync(webhookEventId, cancellationToken);
        if (webhookEvent is null)
        {
            _logger.LogWarning("Webhook event {WebhookEventId} was not found for processing", webhookEventId);
            return;
        }

        try
        {
            _logger.LogInformation(
                "Processing webhook event {WebhookEventId} for transaction {TransactionId}",
                webhookEvent.Id,
                webhookEvent.TransactionId);

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            if (webhookEvent.Status == PaymentStatus.ERRO)
            {
                webhookEvent.MarkFailed("Partner bank returned ERRO status.", _clock.UtcNow);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            if (webhookEvent.ContractId is null)
            {
                webhookEvent.MarkFailed("Contract id is required for processing.", _clock.UtcNow);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            var contractStatus = await _contractStatuses.GetByContractIdAsync(webhookEvent.ContractId, cancellationToken);
            if (contractStatus is null)
            {
                contractStatus = ContractStatus.CreateFrom(webhookEvent, _clock.UtcNow);
                await _contractStatuses.AddAsync(contractStatus, cancellationToken);
            }
            else
            {
                contractStatus.UpdateFrom(webhookEvent, _clock.UtcNow);
            }

            webhookEvent.MarkProcessed(_clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Webhook event {WebhookEventId} processed for contract {ContractId}",
                webhookEvent.Id,
                webhookEvent.ContractId);
        }
        catch (Exception exception)
        {
            webhookEvent.MarkFailed(exception.Message, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            _logger.LogError(
                exception,
                "Error processing webhook event {WebhookEventId}",
                webhookEvent.Id);
        }
    }
}
