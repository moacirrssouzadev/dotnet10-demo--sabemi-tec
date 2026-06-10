using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Sabemi.PaymentWebhook.Application.Abstractions;
using Sabemi.PaymentWebhook.Application.DTOs;
using Sabemi.PaymentWebhook.Application.UseCases;
using Sabemi.PaymentWebhook.Application.Validators;
using Sabemi.PaymentWebhook.Infrastructure.Persistence;
using Sabemi.PaymentWebhook.Infrastructure.Repositories;

namespace Sabemi.PaymentWebhook.Tests.Application;

public sealed class PaymentWebhookTests
{
    [Fact]
    public async Task ReceivePaymentWebhook_ValidPayload_PersistsAndEnqueuesEvent()
    {
        await using var dbContext = CreateDbContext();
        var queue = new TestQueue();
        var useCase = CreateReceiveUseCase(dbContext, queue);

        var result = await useCase.ExecuteAsync(ValidPayload("TX-001"), CancellationToken.None);

        Assert.Equal(ReceiveWebhookOutcome.Accepted, result.Outcome);
        Assert.Single(dbContext.WebhookEvents);
        Assert.Equal(result.EventId, queue.Enqueued.Single());
    }

    [Fact]
    public async Task ReceivePaymentWebhook_DuplicatePayload_DoesNotEnqueueAgain()
    {
        await using var dbContext = CreateDbContext();
        var queue = new TestQueue();
        var useCase = CreateReceiveUseCase(dbContext, queue);

        await useCase.ExecuteAsync(ValidPayload("TX-002"), CancellationToken.None);
        var duplicate = await useCase.ExecuteAsync(ValidPayload("TX-002"), CancellationToken.None);

        Assert.Equal(ReceiveWebhookOutcome.Duplicate, duplicate.Outcome);
        Assert.Single(dbContext.WebhookEvents);
        Assert.Single(queue.Enqueued);
    }

    [Fact]
    public async Task ReceivePaymentWebhook_InvalidPayload_PersistsErrorMessage()
    {
        await using var dbContext = CreateDbContext();
        var queue = new TestQueue();
        var useCase = CreateReceiveUseCase(dbContext, queue);

        var result = await useCase.ExecuteAsync(
            """{"id_transacao":"","id_contrato":"","valor":0,"data_pagamento":"2026-06-10T10:00:00","status":"INVALIDO"}""",
            CancellationToken.None);

        var webhookEvent = await dbContext.WebhookEvents.SingleAsync();

        Assert.Equal(ReceiveWebhookOutcome.Invalid, result.Outcome);
        Assert.NotNull(webhookEvent.ErrorMessage);
        Assert.Empty(queue.Enqueued);
    }

    [Fact]
    public async Task ProcessPaymentWebhook_SuccessStatus_UpdatesContractStatus()
    {
        await using var dbContext = CreateDbContext();
        var queue = new TestQueue();
        var receiveUseCase = CreateReceiveUseCase(dbContext, queue);
        var receiveResult = await receiveUseCase.ExecuteAsync(ValidPayload("TX-003"), CancellationToken.None);
        var processUseCase = CreateProcessUseCase(dbContext);

        await processUseCase.ExecuteAsync(receiveResult.EventId!.Value, CancellationToken.None);

        var contractStatus = await dbContext.ContractStatuses.SingleAsync();
        var webhookEvent = await dbContext.WebhookEvents.SingleAsync();

        Assert.Equal("CTR001", contractStatus.ContractId);
        Assert.True(webhookEvent.Processed);
        Assert.Null(webhookEvent.ErrorMessage);
    }

    [Fact]
    public async Task PaymentWebhookRequestValidator_RejectsUnknownStatus()
    {
        IValidator<PaymentWebhookRequest> validator = new PaymentWebhookRequestValidator();

        var result = await validator.ValidateAsync(new PaymentWebhookRequest(
            "TX-004",
            "CTR001",
            10,
            new DateTime(2026, 6, 10, 10, 0, 0),
            "PENDENTE"));

        Assert.False(result.IsValid);
    }

    private static ReceivePaymentWebhookUseCase CreateReceiveUseCase(PaymentWebhookDbContext dbContext, TestQueue queue)
    {
        var webhookRepository = new WebhookEventRepository(dbContext);

        return new ReceivePaymentWebhookUseCase(
            webhookRepository,
            queue,
            new EfUnitOfWork(dbContext),
            new PaymentWebhookRequestValidator(),
            new FixedClock(),
            NullLogger<ReceivePaymentWebhookUseCase>.Instance);
    }

    private static ProcessPaymentWebhookUseCase CreateProcessUseCase(PaymentWebhookDbContext dbContext)
    {
        return new ProcessPaymentWebhookUseCase(
            new WebhookEventRepository(dbContext),
            new ContractStatusRepository(dbContext),
            new EfUnitOfWork(dbContext),
            new FixedClock(),
            NullLogger<ProcessPaymentWebhookUseCase>.Instance);
    }

    private static PaymentWebhookDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentWebhookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PaymentWebhookDbContext(options);
    }

    private static string ValidPayload(string transactionId)
    {
        return $$"""
        {
          "id_transacao": "{{transactionId}}",
          "id_contrato": "CTR001",
          "valor": 1500.50,
          "data_pagamento": "2026-06-10T10:00:00",
          "status": "SUCESSO"
        }
        """;
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
    }

    private sealed class TestQueue : IPaymentProcessingQueue
    {
        public List<Guid> Enqueued { get; } = [];

        public ValueTask EnqueueAsync(Guid webhookEventId, CancellationToken cancellationToken)
        {
            Enqueued.Add(webhookEventId);
            return ValueTask.CompletedTask;
        }

        public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(Enqueued[0]);
        }
    }
}
