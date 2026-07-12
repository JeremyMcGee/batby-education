using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BatbyEducation.Infrastructure.Data;

public static class DatabaseMigrationExtensions
{
    public static async Task MigrateDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BatbyEducationDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
