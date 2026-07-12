using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne<Tutor>()
            .WithMany()
            .HasForeignKey(s => s.TutorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.TutorId);
        builder.HasIndex(s => s.StudentId);

        builder.Property(s => s.Status)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<SessionStatus>(v));

        builder.Property(s => s.Subject)
            .IsRequired();

        builder.Property(s => s.RateOverride)
            .HasPrecision(8, 2);
    }
}
