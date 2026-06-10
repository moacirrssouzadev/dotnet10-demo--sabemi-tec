using Sabemi.PaymentWebhook.Domain.Enums;

namespace Sabemi.PaymentWebhook.Domain.Entities;

public sealed class WebhookEvent
{
    private WebhookEvent()
    {
        Payload = "{}";
        Status = PaymentStatus.ERRO;
    }

    private WebhookEvent(
        Guid id,
        string? transactionId,
        string? contractId,
        decimal? amount,
        DateTime? paymentDate,
        PaymentStatus status,
        string payload,
        string? errorMessage,
        DateTime createdAt)
    {
        Id = id;
        TransactionId = transactionId;
        ContractId = contractId;
        Amount = amount;
        PaymentDate = paymentDate;
        Status = status;
        Payload = payload;
        ErrorMessage = errorMessage;
        CreatedAt = createdAt;
        Processed = errorMessage is not null;
        ProcessedAt = errorMessage is not null ? createdAt : null;
    }

    public Guid Id { get; private set; }
    public string? TransactionId { get; private set; }
    public string? ContractId { get; private set; }
    public decimal? Amount { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string Payload { get; private set; }
    public bool Processed { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public static WebhookEvent Accepted(
        string transactionId,
        string contractId,
        decimal amount,
        DateTime paymentDate,
        PaymentStatus status,
        string payload,
        DateTime createdAt)
    {
        return new WebhookEvent(
            Guid.NewGuid(),
            transactionId,
            contractId,
            amount,
            paymentDate,
            status,
            payload,
            null,
            createdAt);
    }

    public static WebhookEvent Invalid(string payload, string errorMessage, DateTime createdAt, string? transactionId = null)
    {
        return new WebhookEvent(
            Guid.NewGuid(),
            transactionId,
            null,
            null,
            null,
            PaymentStatus.ERRO,
            payload,
            errorMessage,
            createdAt);
    }

    public void MarkProcessed(DateTime processedAt)
    {
        Processed = true;
        ErrorMessage = null;
        ProcessedAt = processedAt;
    }

    public void MarkFailed(string errorMessage, DateTime processedAt)
    {
        Processed = true;
        ErrorMessage = errorMessage;
        ProcessedAt = processedAt;
    }
}
