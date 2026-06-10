using Sabemi.PaymentWebhook.Domain.Entities;

namespace Sabemi.PaymentWebhook.Application.Abstractions;

public interface IContractStatusRepository
{
    Task<ContractStatus?> GetByContractIdAsync(string contractId, CancellationToken cancellationToken);

    Task AddAsync(ContractStatus contractStatus, CancellationToken cancellationToken);
}
