using Sabemi.PaymentWebhook.Application.Abstractions;

namespace Sabemi.PaymentWebhook.Infrastructure.Clock;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
}
