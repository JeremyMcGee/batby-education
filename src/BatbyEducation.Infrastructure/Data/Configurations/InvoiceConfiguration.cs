using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber)
            .IsRequired();

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(i => i.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.TotalAmount)
            .HasPrecision(10, 2);

        builder.Property(i => i.TotalPaid)
            .HasPrecision(10, 2);

        builder.Property(i => i.Status)
            .HasConversion(
                v => v.ToString(),
                v => v == "Issued" ? InvoiceStatus.Created : Enum.Parse<InvoiceStatus>(v));

        builder.HasMany(i => i.LineItems)
            .WithOne()
            .HasForeignKey(li => li.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Invoice.LineItems))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
