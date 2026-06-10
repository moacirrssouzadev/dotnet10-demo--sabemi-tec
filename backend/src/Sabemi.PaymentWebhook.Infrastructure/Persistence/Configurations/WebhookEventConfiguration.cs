using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabemi.PaymentWebhook.Domain.Entities;

namespace Sabemi.PaymentWebhook.Infrastructure.Persistence.Configurations;

public sealed class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("webhook_events");

        builder.HasKey(webhookEvent => webhookEvent.Id);

        builder.Property(webhookEvent => webhookEvent.Id).HasColumnName("id");
        builder.Property(webhookEvent => webhookEvent.TransactionId).HasColumnName("transaction_id").HasMaxLength(100);
        builder.Property(webhookEvent => webhookEvent.ContractId).HasColumnName("contract_id").HasMaxLength(100);
        builder.Property(webhookEvent => webhookEvent.Amount).HasColumnName("amount").HasColumnType("numeric(18,2)");
        builder.Property(webhookEvent => webhookEvent.PaymentDate).HasColumnName("payment_date").HasColumnType("timestamp without time zone");
        builder.Property(webhookEvent => webhookEvent.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
        builder.Property(webhookEvent => webhookEvent.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(webhookEvent => webhookEvent.Processed).HasColumnName("processed").HasDefaultValue(false);
        builder.Property(webhookEvent => webhookEvent.ErrorMessage).HasColumnName("error_message");
        builder.Property(webhookEvent => webhookEvent.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
        builder.Property(webhookEvent => webhookEvent.ProcessedAt).HasColumnName("processed_at").HasColumnType("timestamp without time zone");

        builder.HasIndex(webhookEvent => webhookEvent.TransactionId)
            .IsUnique()
            .HasDatabaseName("ux_webhook_events_transaction_id");
    }
}
