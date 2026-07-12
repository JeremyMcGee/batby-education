using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.HasKey(le => le.Id);

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(le => le.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(le => new { le.LedgerType, le.EntryDate });

        builder.Property(le => le.Amount)
            .HasPrecision(10, 2);

        builder.Property(le => le.LedgerType)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<PaymentMethod>(v));

        builder.Property(le => le.InvoiceReference)
            .IsRequired();

        builder.Property(le => le.StudentName)
            .IsRequired();
    }
}
