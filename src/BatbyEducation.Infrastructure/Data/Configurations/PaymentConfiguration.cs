using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.InvoiceId);

        builder.Property(p => p.Amount)
            .HasPrecision(10, 2);

        builder.Property(p => p.Method)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<PaymentMethod>(v));
    }
}
