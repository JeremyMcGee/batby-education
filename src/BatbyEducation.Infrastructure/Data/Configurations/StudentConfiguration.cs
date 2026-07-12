using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasConversion(
                email => email.Value,
                value => new EmailAddress(value));

        builder.HasIndex(s => s.Email).IsUnique();

        builder.Property(s => s.PhoneNumber)
            .IsRequired();

        builder.Property(s => s.GuardianName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.GuardianEmail)
            .IsRequired()
            .HasConversion(
                email => email.Value,
                value => new EmailAddress(value));

        builder.Property(s => s.HourlyRate)
            .HasPrecision(8, 2);

        builder.HasMany(s => s.AuditHistory)
            .WithOne()
            .HasForeignKey(a => a.EntityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Student.AuditHistory))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
