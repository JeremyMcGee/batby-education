using BatbyEducation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class TutorAvailabilityConfiguration : IEntityTypeConfiguration<TutorAvailability>
{
    public void Configure(EntityTypeBuilder<TutorAvailability> builder)
    {
        builder.HasKey(ta => ta.Id);

        builder.HasOne<Tutor>()
            .WithMany()
            .HasForeignKey(ta => ta.TutorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ta => new { ta.TutorId, ta.DayOfWeek });
    }
}
