using Marketplace.SaaS.Accelerator.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Web.Infrastructure.Persistence
{
    internal static class Startup
    {
        internal static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            ILogger _logger = Log.ForContext(typeof(Startup));
            services.AddOptions<DatabaseSettingsSaaSAccelerator>()
                .BindConfiguration(nameof(DatabaseSettingsSaaSAccelerator))
                .PostConfigure(databaseSettings =>
                {
                    _logger.Information("Current DB Provider: {dbProvider}", databaseSettings.DBProvider);
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services
                .AddDbContextFactory<SaasKitContext>((p, m) =>
                {
                    var databaseSettings = p.GetRequiredService<IOptions<DatabaseSettingsSaaSAccelerator>>().Value;
                    m.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString, p);
                    m.EnableSensitiveDataLogging(true);
                }, ServiceLifetime.Scoped)
                .AddDbContextFactory<SaasKitContext>((p, m) =>
                {
                    var databaseSettings = p.GetRequiredService<IOptions<DatabaseSettingsSaaSAccelerator>>().Value;
                    m.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString, p);
                    m.EnableSensitiveDataLogging(true);

                }, ServiceLifetime.Scoped);

            return services;

        }
        internal static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider, string connectionString, IServiceProvider p)
        {
            return dbProvider.ToLowerInvariant() switch
            {
                DbProviderKeys.Npgsql => builder.UseNpgsql(connectionString,
                                c => c.MigrationsAssembly("Marketplace.SaaS.Accelerator.DataAccess.Migrations.PostgreSQL")),
                DbProviderKeys.SqlServer => builder.UseSqlServer(connectionString,
                                c => c.MigrationsAssembly("Marketplace.SaaS.Accelerator.DataAccess.Migrations.MSSQL")),
                _ => throw new InvalidOperationException($"DB Provider {dbProvider} is not supported."),
            };
        }

    }

}
