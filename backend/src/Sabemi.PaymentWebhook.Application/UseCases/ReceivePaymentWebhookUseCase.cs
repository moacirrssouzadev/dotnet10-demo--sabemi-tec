using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Sabemi.PaymentWebhook.Application.Abstractions;
using Sabemi.PaymentWebhook.Application.DTOs;
using Sabemi.PaymentWebhook.Domain.Entities;
using Sabemi.PaymentWebhook.Domain.Enums;

namespace Sabemi.PaymentWebhook.Application.UseCases;

public sealed class ReceivePaymentWebhookUseCase
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IWebhookEventRepository _webhookEvents;
    private readonly IPaymentProcessingQueue _queue;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<PaymentWebhookRequest> _validator;
    private readonly IClock _clock;
    private readonly ILogger<ReceivePaymentWebhookUseCase> _logger;

    public ReceivePaymentWebhookUseCase(
        IWebhookEventRepository webhookEvents,
        IPaymentProcessingQueue queue,
        IUnitOfWork unitOfWork,
        IValidator<PaymentWebhookRequest> validator,
        IClock clock,
        ILogger<ReceivePaymentWebhookUseCase> logger)
    {
        _webhookEvents = webhookEvents;
        _queue = queue;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _clock = clock;
        _logger = logger;
    }

    public async Task<ReceiveWebhookResult> ExecuteAsync(PaymentWebhookRequest request, string rawPayload, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var errorMessage = string.Join("; ", validation.Errors.Select(error => error.ErrorMessage));
            return await PersistInvalidAsync(rawPayload, errorMessage, request.IdTransacao, cancellationToken);
        }

        var existingEvent = await _webhookEvents.GetByTransactionIdAsync(request.IdTransacao!, cancellationToken);
        if (existingEvent is not null)
        {
            _logger.LogWarning(
                "Duplicate webhook ignored for transaction {TransactionId} and event {WebhookEventId}",
                request.IdTransacao,
                existingEvent.Id);

            return new ReceiveWebhookResult(
                ReceiveWebhookOutcome.Duplicate,
                existingEvent.Id,
                "Webhook already received and will not be reprocessed.");
        }

        var status = Enum.Parse<PaymentStatus>(request.Status!, ignoreCase: false);
        var webhookEvent = WebhookEvent.Accepted(
            request.IdTransacao!,
            request.IdContrato!,
            request.Valor,
            request.DataPagamento,
            status,
            NormalizeJson(rawPayload),
            _clock.UtcNow);

        await _webhookEvents.AddAsync(webhookEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _queue.EnqueueAsync(webhookEvent.Id, cancellationToken);

        _logger.LogInformation(
            "Webhook received for transaction {TransactionId}, contract {ContractId}, event {WebhookEventId}",
            webhookEvent.TransactionId,
            webhookEvent.ContractId,
            webhookEvent.Id);

        return new ReceiveWebhookResult(
            ReceiveWebhookOutcome.Accepted,
            webhookEvent.Id,
            "Webhook accepted for background processing.");
    }

    private async Task<ReceiveWebhookResult> PersistInvalidAsync(
        string rawPayload,
        string errorMessage,
        string? transactionId,
        CancellationToken cancellationToken)
    {
        var webhookEvent = WebhookEvent.Invalid(ToJsonPayload(rawPayload), errorMessage, _clock.UtcNow, transactionId);

        await _webhookEvents.AddAsync(webhookEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "Invalid webhook persisted as event {WebhookEventId}. Error: {ErrorMessage}",
            webhookEvent.Id,
            errorMessage);

        return new ReceiveWebhookResult(ReceiveWebhookOutcome.Invalid, webhookEvent.Id, errorMessage);
    }

    private static string NormalizeJson(string rawPayload)
    {
        using var document = JsonDocument.Parse(rawPayload);
        return document.RootElement.GetRawText();
    }

    private static string ToJsonPayload(string rawPayload)
    {
        try
        {
            return NormalizeJson(rawPayload);
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(new { raw = rawPayload });
        }
    }
}
