using Sabemi.PaymentWebhook.Domain.Enums;

namespace Sabemi.PaymentWebhook.Domain.Entities;

public sealed class ContractStatus
{
    private ContractStatus()
    {
        ContractId = string.Empty;
        LastTransactionId = string.Empty;
        Status = PaymentStatus.SUCESSO;
    }

    private ContractStatus(
        Guid id,
        string contractId,
        string lastTransactionId,
        PaymentStatus status,
        decimal amount,
        DateTime lastPaymentDate,
        DateTime updatedAt)
    {
        Id = id;
        ContractId = contractId;
        LastTransactionId = lastTransactionId;
        Status = status;
        Amount = amount;
        LastPaymentDate = lastPaymentDate;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; private set; }
    public string ContractId { get; private set; }
    public string LastTransactionId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime LastPaymentDate { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static ContractStatus CreateFrom(WebhookEvent webhookEvent, DateTime updatedAt)
    {
        if (webhookEvent.TransactionId is null || webhookEvent.ContractId is null || webhookEvent.Amount is null || webhookEvent.PaymentDate is null)
        {
            throw new InvalidOperationException("Webhook event does not contain all contract status fields.");
        }

        return new ContractStatus(
            Guid.NewGuid(),
            webhookEvent.ContractId,
            webhookEvent.TransactionId,
            webhookEvent.Status,
            webhookEvent.Amount.Value,
            webhookEvent.PaymentDate.Value,
            updatedAt);
    }

    public void UpdateFrom(WebhookEvent webhookEvent, DateTime updatedAt)
    {
        if (webhookEvent.TransactionId is null || webhookEvent.Amount is null || webhookEvent.PaymentDate is null)
        {
            throw new InvalidOperationException("Webhook event does not contain all contract status fields.");
        }

        LastTransactionId = webhookEvent.TransactionId;
        Status = webhookEvent.Status;
        Amount = webhookEvent.Amount.Value;
        LastPaymentDate = webhookEvent.PaymentDate.Value;
        UpdatedAt = updatedAt;
    }
}
