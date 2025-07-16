using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.SaaS.Accelerator.DataAccess.Migrations.PostgreSQL.SaasKit.Custom
{
    internal static class BaselineV5_Seed
    {
        public static void BaselineV5_SeedData(this MigrationBuilder migrationBuilder)
        {
            var seedDate = DateTime.Now;
            migrationBuilder.Sql(@$"
INSERT INTO public.""SchedulerFrequency"" (""Frequency"") VALUES ('Hourly');
INSERT INTO public.""SchedulerFrequency"" (""Frequency"") VALUES ('Daily');
INSERT INTO public.""SchedulerFrequency"" (""Frequency"") VALUES ('Weekly');
INSERT INTO public.""SchedulerFrequency"" (""Frequency"") VALUES ('Monthly');
INSERT INTO public.""SchedulerFrequency"" (""Frequency"") VALUES ('Yearly');");
        }
    }
}