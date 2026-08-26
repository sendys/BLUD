using BLUDRSUD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BLUDRSUD.Infrastructure.Factories;

/// <summary>
/// Design-time DbContext factory (used by `dotnet ef migrations add` etc.).
/// Reads connection string from environment or a local fallback so migrations can be
/// scaffolded without a running web host. The actual runtime configuration comes from
/// appsettings via DependencyInjection.AddBludInfrastructure.
/// </summary>
public class DesignTimeBludDbContextFactory : IDesignTimeDbContextFactory<BludRsudDbContext>
{
    public BludRsudDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("BLUD_CONNECTION_STRING")
                 ?? "Server=localhost;Port=3306;Database=blud_rsud_dev;User=root;Password=;Charset=utf8mb4;";

        var options = new DbContextOptionsBuilder<BludRsudDbContext>();
        var serverVersion = ServerVersion.Parse("10.4", Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MariaDb);
        options.UseMySql(cs, serverVersion,
            mySql => mySql.MigrationsAssembly(typeof(BludRsudDbContext).Assembly.FullName));

        return new BludRsudDbContext(options.Options);
    }
}
