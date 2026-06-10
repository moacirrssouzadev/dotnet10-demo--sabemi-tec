using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabemi.PaymentWebhook.Domain.Entities;

namespace Sabemi.PaymentWebhook.Infrastructure.Persistence.Configurations;

public sealed class ContractStatusConfiguration : IEntityTypeConfiguration<ContractStatus>
{
    public void Configure(EntityTypeBuilder<ContractStatus> builder)
    {
        builder.ToTable("contract_status");

        builder.HasKey(contractStatus => contractStatus.Id);

        builder.Property(contractStatus => contractStatus.Id).HasColumnName("id");
        builder.Property(contractStatus => contractStatus.ContractId).HasColumnName("contract_id").HasMaxLength(100).IsRequired();
        builder.Property(contractStatus => contractStatus.LastTransactionId).HasColumnName("last_transaction_id").HasMaxLength(100).IsRequired();
        builder.Property(contractStatus => contractStatus.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
        builder.Property(contractStatus => contractStatus.Amount).HasColumnName("amount").HasColumnType("numeric(18,2)");
        builder.Property(contractStatus => contractStatus.LastPaymentDate).HasColumnName("last_payment_date").HasColumnType("timestamp without time zone");
        builder.Property(contractStatus => contractStatus.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp without time zone");

        builder.HasIndex(contractStatus => contractStatus.ContractId)
            .IsUnique()
            .HasDatabaseName("ux_contract_status_contract_id");
    }
}
