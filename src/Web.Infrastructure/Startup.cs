using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Web.Infrastructure.Hangfire;
using Web.Infrastructure.Persistence;

namespace Web.Infrastructure;

public static class Startup
{
    public static void AddWebServices(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddPersistence()
            .AddHangfireService(config);
    }

    public static void AddWebApplications(this WebApplication app, IConfiguration config)
    {

        app.UseHangfireDashboard(config);
    }
}