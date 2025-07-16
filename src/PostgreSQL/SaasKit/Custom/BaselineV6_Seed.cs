using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.SaaS.Accelerator.DataAccess.Migrations.PostgreSQL.SaasKit.Custom
{
    internal static class BaselineV6_Seed
    {
        public static void BaselineV6_SeedData(this MigrationBuilder migrationBuilder)
        {
            var seedDate = DateTime.Now;
            migrationBuilder.Sql(@$"
do $$
begin
IF NOT EXISTS (SELECT * FROM public.""SchedulerFrequency"" WHERE ""Frequency"" = 'OneTime')
then
    INSERT INTO public.""SchedulerFrequency"" (""Frequency"") VALUES ('OneTime');
end if;
End $$;
");
            migrationBuilder.Sql(@$"
INSERT INTO public.""ApplicationConfiguration"" ( ""Name"", ""Value"", ""Description"") VALUES ( 'EnableHourlyMeterSchedules', 'False', 'This will enable to run Hourly meter scheduled items');
INSERT INTO public.""ApplicationConfiguration"" ( ""Name"", ""Value"", ""Description"") VALUES ( 'EnableDailyMeterSchedules', 'False', 'This will enable to run Daily meter scheduled items');
INSERT INTO public.""ApplicationConfiguration"" ( ""Name"", ""Value"", ""Description"") VALUES ( 'EnableWeeklyMeterSchedules', 'False', 'This will enable to run Weekly meter scheduled items');
INSERT INTO public.""ApplicationConfiguration"" ( ""Name"", ""Value"", ""Description"") VALUES ( 'EnableMonthlyMeterSchedules', 'False', 'This will enable to run Monthly meter scheduled items');
INSERT INTO public.""ApplicationConfiguration"" ( ""Name"", ""Value"", ""Description"") VALUES ( 'EnableYearlyMeterSchedules', 'False', 'This will enable to run Yearly meter scheduled items');
INSERT INTO public.""ApplicationConfiguration"" ( ""Name"", ""Value"", ""Description"") VALUES ( 'EnableOneTimeMeterSchedules', 'False', 'This will enable to run OneTime meter scheduled items');
");
        }
    }
}