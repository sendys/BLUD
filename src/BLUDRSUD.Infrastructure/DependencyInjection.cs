using BLUDRSUD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLUDRSUD.Infrastructure;

/// <summary>
/// DI registration for Infrastructure (spec: Dependency Injection, Options Pattern).
/// Identity registration is performed in the Web host where ASP.NET Core shared framework is available.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddBludInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Connection string 'DefaultConnection' missing.");

        services.AddDbContext<BludRsudDbContext>(options =>
        {
            // Pomelo MySQL provider — utf8mb4 charset, MariaDB/MySQL 8 compatible (spec section 1 & 24).
            // Pinned to MariaDB 10.4 to allow offline migration scaffolding (no DB connection needed for `dotnet ef migrations add`).
            var serverVersion = ServerVersion.Parse("10.4", Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MariaDb);
            options.UseMySql(cs, serverVersion, mySql =>
            {
                mySql.MigrationsAssembly(typeof(BludRsudDbContext).Assembly.FullName);
            });
            options.EnableSensitiveDataLogging(config.GetValue<bool>("Logging:EnableSensitiveDataLogging"));
        });

        return services;
    }
}
