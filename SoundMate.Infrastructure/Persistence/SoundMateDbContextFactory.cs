using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SoundMate.Infrastructure.Persistence;

/// <summary>
/// Used ONLY at design time by the EF Core tools (<c>dotnet ef migrations add</c> /
/// <c>database update</c>). It lets us generate and apply migrations without starting the
/// API. At runtime the DbContext is registered from the API with its configured
/// connection string.
/// </summary>
public sealed class SoundMateDbContextFactory : IDesignTimeDbContextFactory<SoundMateDbContext>
{
    public SoundMateDbContext CreateDbContext(string[] args)
    {
        // Development connection string (LocalDB); replaced by the real one via configuration
        // when deploying. "migrations add" does not even open a connection.
        const string connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=SoundMate;Trusted_Connection=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<SoundMateDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new SoundMateDbContext(options);
    }
}
