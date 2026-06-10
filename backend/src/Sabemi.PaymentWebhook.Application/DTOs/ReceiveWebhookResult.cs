namespace Sabemi.PaymentWebhook.Application.DTOs;

public sealed record ReceiveWebhookResult(ReceiveWebhookOutcome Outcome, Guid? EventId, string Message);

public enum ReceiveWebhookOutcome
{
    Accepted,
    Duplicate,
    Invalid
}
