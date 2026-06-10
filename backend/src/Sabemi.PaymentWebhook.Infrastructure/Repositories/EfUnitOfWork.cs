using Sabemi.PaymentWebhook.Application.Abstractions;
using Sabemi.PaymentWebhook.Infrastructure.Persistence;

namespace Sabemi.PaymentWebhook.Infrastructure.Repositories;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly PaymentWebhookDbContext _dbContext;

    public EfUnitOfWork(PaymentWebhookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
