using BatbyEducation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.HasKey(li => li.Id);

        builder.HasOne<Session>()
            .WithMany()
            .HasForeignKey(li => li.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(li => li.Rate)
            .HasPrecision(8, 2);

        builder.Property(li => li.Amount)
            .HasPrecision(10, 2);

        builder.Property(li => li.TutorName)
            .IsRequired();

        builder.Property(li => li.Subject)
            .IsRequired();
    }
}
