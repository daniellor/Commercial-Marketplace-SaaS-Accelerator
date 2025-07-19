using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.SaaS.Accelerator.DataAccess.DataModel;
public class EmailTemplateModel
{
    public Guid SubscriptionId { get; internal set; }
    public object PlanName { get; internal set; }
    public string OfferName { get; internal set; }
    public string SubscriptionStatus { get; internal set; }
    public string PurchaserEmail { get; internal set; }
    public Guid? PurchaserTenantId { get; internal set; }
    public DateTimeOffset? StartDate { get; internal set; }
    public DateTimeOffset? EndDate { get; internal set; }
    public string CustomerName { get; internal set; }
    public string SubscriptionName { get; internal set; }
}
