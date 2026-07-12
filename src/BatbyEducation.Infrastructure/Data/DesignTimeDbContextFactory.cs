using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BatbyEducation.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BatbyEducationDbContext>
{
    public BatbyEducationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BatbyEducationDbContext>()
            .UseSqlite("Data Source=batbyeducation.db")
            .Options;

        return new BatbyEducationDbContext(options);
    }
}
