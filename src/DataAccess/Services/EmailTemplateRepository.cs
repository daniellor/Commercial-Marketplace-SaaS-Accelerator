using Marketplace.SaaS.Accelerator.DataAccess.Context;
using Marketplace.SaaS.Accelerator.DataAccess.Contracts;
using Marketplace.SaaS.Accelerator.DataAccess.DataModel;
using Marketplace.SaaS.Accelerator.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.SaaS.Accelerator.DataAccess.Services;

/// <summary>
/// Repository to access Email Templates.
/// </summary>
/// <seealso cref="IEmailTemplateRepository" />
public class EmailTemplateRepository : IEmailTemplateRepository
{
    /// <summary>
    /// The context.
    /// </summary>
    private readonly SaasKitContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailTemplateRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public EmailTemplateRepository(SaasKitContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Gets the email template for subscription status.
    /// </summary>
    /// <param name="status">The subscription status.</param>
    /// <returns>
    /// Email template relevant to the status of the subscription.
    /// </returns>
    public EmailTemplate GetTemplateForStatus(string status)
    {
        var template = this.context.EmailTemplate.Where(s => s.Status == status).FirstOrDefault();
        if (template != null)
        {
            return template;
        }

        return null;
    }

    /// <summary>
    /// Generates the HTML body for a subscription email using the provided email template model.
    /// </summary>
    /// <param name="emailTemplateModel">The model containing subscription and customer details for the email template.</param>
    /// <returns>
    /// A string containing the HTML-formatted body for the subscription email.
    /// </returns>
    public string GetEmailBodyForSubscription(EmailTemplateModel emailTemplateModel)
    {
        StringBuilder htmlBody = new StringBuilder();
        htmlBody.AppendLine($"<tr><td><b>Customer Email Address</b></td> <td>{emailTemplateModel.PurchaserEmail}</td> </tr>");
        htmlBody.AppendLine($"<tr><td><b>Customer Name</b></td> <td>{emailTemplateModel.CustomerName}</td> </tr>");
        htmlBody.AppendLine($"<tr><td><b>Customer Tenant</b></td> <td>{emailTemplateModel.PurchaserTenantId}</td> </tr>");
        htmlBody.AppendLine($"<tr><td><b>SaaS Subscription Id</b></td> <td>{emailTemplateModel.SubscriptionId}</td> </tr>");
        htmlBody.AppendLine($"<tr><td><b>SaaS Subscription Name</b></td> <td>{emailTemplateModel.SubscriptionName}</td> </tr>");
        htmlBody.AppendLine($"<tr><td><b>SaaS Subscription Status</b></td> <td>{emailTemplateModel.SubscriptionStatus}</td> </tr>");
        htmlBody.AppendLine($"<tr><td><b>Plan</b></td> <td>{emailTemplateModel.PlanName}</td> </tr>");
        return htmlBody.ToString();
    }

    /// <summary>
    /// Retrieves subscription information for the specified subscription ID.
    /// </summary>
    /// <param name="subscriptionID">The unique identifier of the subscription.</param>
    /// <returns>
    /// An <see cref="EmailTemplateModel"/> containing details about the subscription.
    /// </returns>
    public EmailTemplateModel GetSubscriptionInfo(Guid subscriptionID)
    {
        return EmailTemplateDataQuery(subscriptionID).Single();
    }
    private IQueryable<EmailTemplateModel> EmailTemplateDataQuery(Guid subscriptionID)
    {
        return from s in this.context.Subscriptions
               join p in this.context.Plans on s.AmpplanId equals p.PlanId
               join o in this.context.Offers on s.AmpOfferId equals o.OfferId
               where s.AmpsubscriptionId == subscriptionID
               select new EmailTemplateModel()
               {
                   SubscriptionId = s.AmpsubscriptionId,
                   SubscriptionName = s.Name,
                   PlanName = p.DisplayName,
                   OfferName = o.OfferName,
                   SubscriptionStatus = s.SubscriptionStatus,
                   PurchaserEmail = s.PurchaserEmail,
                   PurchaserTenantId = s.PurchaserTenantId,
                   StartDate = s.StartDate,
                   EndDate = s.EndDate,
                   CustomerName=s.User.FullName,
               };
    }

    /// <summary>
    /// Returns a welcome text message based on the process status and the subscription status in the provided email template model.
    /// </summary>
    /// <param name="emailTemplateModel">The model containing subscription and customer details for the email template.</param>
    /// <param name="processStatus">The status of the process (e.g., "success", "failure").</param>
    /// <returns>
    /// A string containing the appropriate welcome message for the given process and subscription status.
    /// </returns>
    public string WelcomeText(EmailTemplateModel emailTemplateModel, string processStatus)
    {
        switch (processStatus)
        {
            case "failure":
                return "Your request for the subscription has been failed.";
            case "success":
                if (emailTemplateModel.SubscriptionStatus == "PendingActivation")
                {
                    return "A request for purchase with the following details is awaiting your action for activation.";
                }
                else if (emailTemplateModel.SubscriptionStatus == "Subscribed")
                {
                    return "Your request for the purchase has been approved.";
                }
                else if (emailTemplateModel.SubscriptionStatus == "Unsubscribed")
                {
                    return "A subscription with the following details was deleted from Azure.";
                }
                return string.Empty;
            default:
                return string.Empty;
        }
    }
    /// <summary>
    /// Gets all email templates
    /// </summary>
    /// <returns>
    /// List of email templates
    /// </returns>
    public IEnumerable<EmailTemplate> GetAll()
    {
        var templates = this.context.EmailTemplate;
        return templates;
    }

    /// <summary>
    /// Saves email configuration field
    /// </summary>
    /// <returns>
    /// True or False
    /// </returns>
    public string SaveEmailTemplateByStatus(EmailTemplate template)
    {
        var emailTemplate = this.context.EmailTemplate.Where(a => a.Status == template.Status).SingleOrDefault();
        if (emailTemplate != null)
        {
            emailTemplate.IsActive = template.IsActive;
            emailTemplate.Subject = template.Subject;
            emailTemplate.Description = template.Description;
            emailTemplate.TemplateBody = template.TemplateBody;
            emailTemplate.ToRecipients = template.ToRecipients;
            emailTemplate.Bcc = template.Bcc;
            emailTemplate.Cc = template.Cc;
            this.context.SaveChanges();
        }
        return template.Status;
    }
}