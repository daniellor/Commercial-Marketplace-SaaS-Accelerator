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
            }
            catch (Exception e)
            {
                logger.LogError(e, "An exception occurred while creating recurring jobs.");
            }

            return Task.CompletedTask;
        }
    }
}
