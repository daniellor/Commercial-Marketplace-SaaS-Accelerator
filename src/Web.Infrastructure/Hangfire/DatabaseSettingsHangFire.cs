using Web.Infrastructure.Persistence;

namespace Web.Infrastructure.Hangfire
{
    public class DatabaseSettingsHangFire : DatabaseSettings
    {
        public string DashboardUserName { get; set; } = default!;
        public string DashboardPassword { get; set; } = default!;

        public string Route { get; set; } = "/jobs";
    }
}
