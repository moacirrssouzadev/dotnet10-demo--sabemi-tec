using System.Threading.Channels;
using Sabemi.PaymentWebhook.Application.Abstractions;

namespace Sabemi.PaymentWebhook.Infrastructure.BackgroundProcessing;

public sealed class PaymentProcessingQueue : IPaymentProcessingQueue
{
    private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public async ValueTask EnqueueAsync(Guid webhookEventId, CancellationToken cancellationToken)
    {
        await _queue.Writer.WriteAsync(webhookEventId, cancellationToken);
    }

    public async ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
