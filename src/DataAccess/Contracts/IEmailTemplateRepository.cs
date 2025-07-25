using System;
using System.Collections.Generic;
using Marketplace.SaaS.Accelerator.DataAccess.DataModel;
using Marketplace.SaaS.Accelerator.DataAccess.Entities;

namespace Marketplace.SaaS.Accelerator.DataAccess.Contracts;

/// <summary>
/// Repository to access email templates.
/// </summary>
public interface IEmailTemplateRepository
{
    /// <summary>
    /// Gets the email template for subscription status.
    /// </summary>
    /// <param name="status">The subscription status.</param>
    /// <returns>Email template relevant to the status of the subscription.</returns>
    EmailTemplate GetTemplateForStatus(string status);

    /// <summary>
    /// Generates the HTML body for a subscription email using the provided email template model.
    /// </summary>
    /// <param name="emailTemplateModel">The model containing subscription and customer details for the email template.</param>
    /// <returns>
    /// A string containing the HTML-formatted body for the subscription email.
    /// </returns>
    string GetEmailBodyForSubscription(EmailTemplateModel emailTemplateModel);

    /// <summary>
    /// Retrieves subscription information for the specified subscription ID.
    /// </summary>
    /// <param name="subscriptionID">The unique identifier of the subscription.</param>
    /// <returns>
    /// An <see cref="EmailTemplateModel"/> containing details about the subscription.
    /// </returns>
    EmailTemplateModel GetSubscriptionInfo(Guid subscriptionID);

    /// <summary>
    /// Returns a welcome text message based on the process status and the subscription status in the provided email template model.
    /// </summary>
    /// <param name="emailTemplateModel">The model containing subscription and customer details for the email template.</param>
    /// <param name="processStatus">The status of the process (e.g., "success", "failure").</param>
    /// <returns>
    /// A string containing the appropriate welcome message for the given process and subscription status.
    /// </returns>
    string WelcomeText(EmailTemplateModel emailTemplateModel, string processStatus);

    /// <summary>
    /// Gets all editable email templates
    /// </summary>
    /// <returns> A list of EmailTemplates </returns>
    IEnumerable<EmailTemplate> GetAll();

    /// <summary>
    /// Saves modified EmailTemplate
    /// </summary>
    /// <returns> Returns the status of the modified EmailTemplate </returns>
    string SaveEmailTemplateByStatus(EmailTemplate template);
    
}