using BatbyEducation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatbyEducation.Infrastructure.Data.Configurations;

public class StudentAccountConfiguration : IEntityTypeConfiguration<StudentAccount>
{
    public void Configure(EntityTypeBuilder<StudentAccount> builder)
    {
        builder.HasKey(sa => sa.Id);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(sa => sa.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sa => sa.StudentId).IsUnique();

        builder.Property(sa => sa.CreditBalance)
            .HasPrecision(10, 2);
    }
}
