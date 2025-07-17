using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Web.Infrastructure.Hangfire
{
    internal class RecurringJobsService(
            [NotNull] IBackgroundJobClient backgroundJobs,
            [NotNull] IRecurringJobManager recurringJobs,
            [NotNull] ILogger<RecurringJobScheduler> logger
            ) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
               // recurringJobs.AddOrUpdate("NBP-A-hourly", () => importNBPExchangeCurrency.RunAsync("A"), "0 * * * *");
               // recurringJobs.AddOrUpdate("Vies-check", () => checkViesJob.RunAsync(), "*/1 * * * *");
            }
            catch (Exception e)
            {
                logger.LogError(e, "An exception occurred while creating recurring jobs.");
            }

            return Task.CompletedTask;
        }
    }
}
