using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Web.Infrastructure.Hangfire;
using ILogger = Serilog.ILogger;

namespace Web.Infrastructure.Hangfire;

internal static class Startup
{
    internal static IServiceCollection AddHangfireService(this IServiceCollection services, IConfiguration config)
    {
        ILogger logger = Log.ForContext(typeof(Startup));
        var connectionString = config.GetSection(nameof(DatabaseSettingsHangFire)).Get<DatabaseSettingsHangFire>()?.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.Warning("No Hangfire settings found in configuration.");
            return services;
        }

        services.AddHangfire(cf =>
                    cf.UsePostgreSqlStorage(c =>
                    c.UseNpgsqlConnection(connectionString)));

        services.AddHostedService<RecurringJobsService>();
        services.AddHangfireServer(options =>
        {
            options.StopTimeout = TimeSpan.FromSeconds(15);
            options.ShutdownTimeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    internal static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app, IConfiguration config)
    {
        var hangFireSettings = config.GetSection(nameof(DatabaseSettingsHangFire)).Get<DatabaseSettingsHangFire>();
        ILogger logger = Log.ForContext(typeof(Startup));

        if (hangFireSettings == null)
        {
            logger.Warning("No Hangfire settings found in configuration.");
            return app;
        }
        var dashboardOptions = new DashboardOptions();
        dashboardOptions.Authorization = new[]
        {
           new HangfireCustomBasicAuthenticationFilter
           {
                User = hangFireSettings!.DashboardUserName!,
                Pass = hangFireSettings.DashboardPassword!
           }
        };

        return app.UseHangfireDashboard(hangFireSettings.Route, dashboardOptions);
    }
}