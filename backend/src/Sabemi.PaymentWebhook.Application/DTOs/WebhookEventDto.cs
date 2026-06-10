namespace Sabemi.PaymentWebhook.Application.DTOs;

public sealed record WebhookEventDto(
    Guid Id,
    string? TransactionId,
    string? ContractId,
    decimal? Amount,
    DateTime? PaymentDate,
    string Status,
    bool Processed,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? ProcessedAt);
