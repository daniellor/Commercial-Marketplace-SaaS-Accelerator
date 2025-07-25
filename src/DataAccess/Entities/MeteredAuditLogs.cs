using System;

namespace Marketplace.SaaS.Accelerator.DataAccess.Entities;

public partial class MeteredAuditLogs
{
    public int Id { get; set; }
    public int? SubscriptionId { get; set; }
    public string RequestJson { get; set; }
    public string ResponseJson { get; set; }
    public string StatusCode { get; set; }
    public string RunBy { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public int CreatedBy { get; set; }
    public DateTimeOffset? SubscriptionUsageDate { get; set; }

    public virtual Subscriptions Subscription { get; set; }
}