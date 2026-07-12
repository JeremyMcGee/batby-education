using BatbyEducation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.HasKey(ae => ae.Id);

        builder.HasIndex(ae => ae.EntityId);

        builder.Property(ae => ae.EntityType)
            .IsRequired();

        builder.Property(ae => ae.FieldName)
            .IsRequired();
    }
}
