using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sabemi.PaymentWebhook.Application.Abstractions;
using Sabemi.PaymentWebhook.Infrastructure.BackgroundProcessing;
using Sabemi.PaymentWebhook.Infrastructure.Clock;
using Sabemi.PaymentWebhook.Infrastructure.Persistence;
using Sabemi.PaymentWebhook.Infrastructure.Repositories;

namespace Sabemi.PaymentWebhook.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? "Host=localhost;Port=5432;Database=sabemi_payments;Username=sabemi;Password=sabemi";

        services.AddDbContext<PaymentWebhookDbContext>(options => options.UseNpgsql(connectionString));

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPaymentProcessingQueue, PaymentProcessingQueue>();
        // services.AddHostedService<PaymentWebhookBackgroundService>();

        services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
        services.AddScoped<IContractStatusRepository, ContractStatusRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
