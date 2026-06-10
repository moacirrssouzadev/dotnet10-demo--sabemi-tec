using Microsoft.EntityFrameworkCore;
using Sabemi.PaymentWebhook.Application.Abstractions;
using Sabemi.PaymentWebhook.Application.DTOs;
using Sabemi.PaymentWebhook.Domain.Entities;
using Sabemi.PaymentWebhook.Domain.Enums;
using Sabemi.PaymentWebhook.Infrastructure.Persistence;

namespace Sabemi.PaymentWebhook.Infrastructure.Repositories;

public sealed class WebhookEventRepository : IWebhookEventRepository
{
    private readonly PaymentWebhookDbContext _dbContext;

    public WebhookEventRepository(PaymentWebhookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        await _dbContext.WebhookEvents.AddAsync(webhookEvent, cancellationToken);
    }

    public Task<WebhookEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.WebhookEvents.FirstOrDefaultAsync(webhookEvent => webhookEvent.Id == id, cancellationToken);
    }

    public Task<WebhookEvent?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken)
    {
        return _dbContext.WebhookEvents.FirstOrDefaultAsync(webhookEvent => webhookEvent.TransactionId == transactionId, cancellationToken);
    }

    public async Task<IReadOnlyList<WebhookEventDto>> ListAsync(WebhookEventQuery query, CancellationToken cancellationToken)
    {
        var events = _dbContext.WebhookEvents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<PaymentStatus>(query.Status, out var status))
        {
            events = events.Where(webhookEvent => webhookEvent.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.ContractId))
        {
            events = events.Where(webhookEvent => webhookEvent.ContractId == query.ContractId);
        }

        return await events
            .OrderByDescending(webhookEvent => webhookEvent.CreatedAt)
            .Select(webhookEvent => new WebhookEventDto(
                webhookEvent.Id,
                webhookEvent.TransactionId,
                webhookEvent.ContractId,
                webhookEvent.Amount,
                webhookEvent.PaymentDate,
                webhookEvent.Status.ToString(),
                webhookEvent.Processed,
                webhookEvent.ErrorMessage,
                webhookEvent.CreatedAt,
                webhookEvent.ProcessedAt))
            .ToListAsync(cancellationToken);
    }
}
