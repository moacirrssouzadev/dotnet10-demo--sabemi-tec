using Sabemi.PaymentWebhook.Application.Abstractions;
using Sabemi.PaymentWebhook.Application.DTOs;

namespace Sabemi.PaymentWebhook.Application.UseCases;

public sealed class ListWebhookEventsUseCase
{
    private readonly IWebhookEventRepository _webhookEvents;

    public ListWebhookEventsUseCase(IWebhookEventRepository webhookEvents)
    {
        _webhookEvents = webhookEvents;
    }

    public Task<IReadOnlyList<WebhookEventDto>> ExecuteAsync(WebhookEventQuery query, CancellationToken cancellationToken)
    {
        return _webhookEvents.ListAsync(query, cancellationToken);
    }
}
