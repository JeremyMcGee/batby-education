using System.Text.Json;
using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class TutorConfiguration : IEntityTypeConfiguration<Tutor>
{
    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Email)
            .IsRequired()
            .HasConversion(
                email => email.Value,
                value => new EmailAddress(value));

        builder.HasIndex(t => t.Email).IsUnique();

        builder.Property(t => t.Subjects)
            .IsRequired()
            .HasConversion(
                subjects => JsonSerializer.Serialize(subjects, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null)!)
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(t => t.HourlyRate)
            .HasPrecision(8, 2);
    }
}
