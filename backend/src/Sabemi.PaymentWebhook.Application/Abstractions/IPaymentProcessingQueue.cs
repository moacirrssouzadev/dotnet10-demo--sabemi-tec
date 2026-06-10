namespace Sabemi.PaymentWebhook.Application.Abstractions;

public interface IPaymentProcessingQueue
{
    ValueTask EnqueueAsync(Guid webhookEventId, CancellationToken cancellationToken);

    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken);
}
