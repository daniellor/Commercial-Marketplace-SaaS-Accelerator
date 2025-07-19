namespace Web.Infrastructure.AspNet;
public class KestrelSettings
{
    public bool Enabled { get; set; }
    public int DefaultEndpointPort { get; set; }

    public int? HttpEndpointPort { get; set; }
}
