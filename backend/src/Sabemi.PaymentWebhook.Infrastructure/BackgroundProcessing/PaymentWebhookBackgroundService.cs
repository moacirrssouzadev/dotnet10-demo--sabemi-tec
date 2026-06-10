using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sabemi.PaymentWebhook.Application.Abstractions;
using Sabemi.PaymentWebhook.Application.UseCases;

namespace Sabemi.PaymentWebhook.Infrastructure.BackgroundProcessing;

public sealed class PaymentWebhookBackgroundService : BackgroundService
{
    private readonly IPaymentProcessingQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentWebhookBackgroundService> _logger;

    public PaymentWebhookBackgroundService(
        IPaymentProcessingQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentWebhookBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment webhook background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var webhookEventId = await _queue.DequeueAsync(stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var useCase = scope.ServiceProvider.GetRequiredService<ProcessPaymentWebhookUseCase>();
            await useCase.ExecuteAsync(webhookEventId, stoppingToken);
        }
    }
}
