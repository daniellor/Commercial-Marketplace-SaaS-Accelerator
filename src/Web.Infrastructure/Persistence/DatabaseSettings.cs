namespace Web.Infrastructure.Persistence
{
    public class DatabaseSettings
    {
        public string DBProvider { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;

        public bool EnableSensitiveDataLogging { get; set; }

    }
}
