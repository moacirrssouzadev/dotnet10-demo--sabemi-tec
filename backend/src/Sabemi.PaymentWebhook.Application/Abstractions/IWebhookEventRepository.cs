using Sabemi.PaymentWebhook.Application.DTOs;
using Sabemi.PaymentWebhook.Domain.Entities;

namespace Sabemi.PaymentWebhook.Application.Abstractions;

public interface IWebhookEventRepository
{
    Task AddAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken);

    Task<WebhookEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<WebhookEvent?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<WebhookEventDto>> ListAsync(WebhookEventQuery query, CancellationToken cancellationToken);
}
