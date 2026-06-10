namespace Sabemi.PaymentWebhook.Application.DTOs;

public sealed record WebhookEventQuery(string? Status, string? ContractId);
