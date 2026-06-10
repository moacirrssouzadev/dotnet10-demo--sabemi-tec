using Microsoft.EntityFrameworkCore;
using Sabemi.PaymentWebhook.Application.Abstractions;
using Sabemi.PaymentWebhook.Domain.Entities;
using Sabemi.PaymentWebhook.Infrastructure.Persistence;

namespace Sabemi.PaymentWebhook.Infrastructure.Repositories;

public sealed class ContractStatusRepository : IContractStatusRepository
{
    private readonly PaymentWebhookDbContext _dbContext;

    public ContractStatusRepository(PaymentWebhookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ContractStatus?> GetByContractIdAsync(string contractId, CancellationToken cancellationToken)
    {
        return _dbContext.ContractStatuses.FirstOrDefaultAsync(contractStatus => contractStatus.ContractId == contractId, cancellationToken);
    }

    public async Task AddAsync(ContractStatus contractStatus, CancellationToken cancellationToken)
    {
        await _dbContext.ContractStatuses.AddAsync(contractStatus, cancellationToken);
    }
}
