using Microsoft.EntityFrameworkCore;
using Sabemi.PaymentWebhook.Domain.Entities;

namespace Sabemi.PaymentWebhook.Infrastructure.Persistence;

public sealed class PaymentWebhookDbContext : DbContext
{
    public PaymentWebhookDbContext(DbContextOptions<PaymentWebhookDbContext> options)
        : base(options)
    {
    }

    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();

    public DbSet<ContractStatus> ContractStatuses => Set<ContractStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentWebhookDbContext).Assembly);
    }
}
