using Azure.Identity;
using Marketplace.SaaS.Accelerator.CustomerSite.Controllers;
using Marketplace.SaaS.Accelerator.CustomerSite.WebHook;
using Marketplace.SaaS.Accelerator.DataAccess.Contracts;
using Marketplace.SaaS.Accelerator.DataAccess.Services;
using Marketplace.SaaS.Accelerator.Services.Configurations;
using Marketplace.SaaS.Accelerator.Services.Contracts;
using Marketplace.SaaS.Accelerator.Services.Services;
using Marketplace.SaaS.Accelerator.Services.Utilities;
using Marketplace.SaaS.Accelerator.Services.WebHook;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Marketplace.SaaS;
using Serilog;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Web.Infrastructure;
using Web.Infrastructure.Util;

namespace Marketplace.SaaS.Accelerator.CustomerSite;

/// <summary>
/// Program.
/// </summary>
public class Program
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddDebug()
                .AddConsole();
        });
        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
                           loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
        builder.Services.AddHttpLogging(_ => { });
        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        var config = new SaaSApiClientConfiguration()
        {
            AdAuthenticationEndPoint = builder.Configuration["SaaSApiConfiguration:AdAuthenticationEndPoint"],
            ClientId = builder.Configuration["SaaSApiConfiguration:ClientId"],
            ClientSecret = builder.Configuration["SaaSApiConfiguration:ClientSecret"],
            MTClientId = builder.Configuration["SaaSApiConfiguration:MTClientId"],
            FulFillmentAPIBaseURL = builder.Configuration["SaaSApiConfiguration:FulFillmentAPIBaseURL"],
            FulFillmentAPIVersion = builder.Configuration["SaaSApiConfiguration:FulFillmentAPIVersion"],
            GrantType = builder.Configuration["SaaSApiConfiguration:GrantType"],
            Resource = builder.Configuration["SaaSApiConfiguration:Resource"],
            SaaSAppUrl = builder.Configuration["SaaSApiConfiguration:SaaSAppUrl"],
            SignedOutRedirectUri = builder.Configuration["SaaSApiConfiguration:SignedOutRedirectUri"],
            TenantId = builder.Configuration["SaaSApiConfiguration:TenantId"],
            Environment = builder.Configuration["SaaSApiConfiguration:Environment"]
        };
        var creds = new ClientSecretCredential(config.TenantId.ToString(), config.ClientId.ToString(), config.ClientSecret);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.Cookie.MaxAge = options.ExpireTimeSpan;
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = $"{config.AdAuthenticationEndPoint}/common/v2.0";
                options.ClientId = config.MTClientId;
                options.ResponseType = OpenIdConnectResponseType.IdToken;
                options.CallbackPath = "/Home/Index";
                options.SignedOutRedirectUri = config.SignedOutRedirectUri;
                options.TokenValidationParameters.NameClaimType = ClaimConstants.CLAIM_SHORT_NAME;
                options.TokenValidationParameters.ValidateIssuer = false;
                options.SaveTokens = true;
                options.RequireHttpsMetadata = true; // If your proxy terminates SSL
                //options.Events.OnRedirectToIdentityProvider = async n =>
                //{
                //    n.ProtocolMessage.RedirectUri = "https://order-testing.hyperionsystem.app/Home/Index";
                //    await Task.FromResult(0);
                //};
            });
        builder.Services
            .AddTransient<IClaimsTransformation, CustomClaimsTransformation>()
            .AddScoped<ExceptionHandlerAttribute>()
            .AddScoped<RequestLoggerActionFilter>();

        if (!Uri.TryCreate(config.FulFillmentAPIBaseURL, UriKind.Absolute, out var fulfillmentBaseApi))
        {
            fulfillmentBaseApi = new Uri("https://marketplaceapi.microsoft.com/api");
        }

        builder.Services
            .AddSingleton<IFulfillmentApiService>(new FulfillmentApiService(new MarketplaceSaaSClient(fulfillmentBaseApi, creds), config, new FulfillmentApiClientLogger()))
            .AddSingleton<SaaSApiClientConfiguration>(config)
            .AddSingleton<ValidateJwtToken>();

        // Add the assembly version
        builder.Services.AddSingleton<IAppVersionService>(new AppVersionService(Assembly.GetExecutingAssembly()?.GetName()?.Version, Assembly.GetEntryAssembly().GetAssemblyLinkTime()));

        builder.Services.AddWebServices(builder.Configuration);

        InitializeRepositoryServices(builder.Services);

        builder.Services.AddMvc(option => {
            option.EnableEndpointRouting = false;
            option.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        });
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedPrefix;
            options.KnownProxies.Add(IPAddress.Parse("172.21.0.1"));
            options.KnownProxies.Add(IPAddress.Parse("10.0.0.4"));

        });
        var app = builder.Build();
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.None,
            Secure = CookieSecurePolicy.Always,
        });
        app.UseForwardedHeaders();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        
        app.UseAuthentication();
        app.UseHttpLogging();
        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        });
        app.Run();
    }

    private static void InitializeRepositoryServices(IServiceCollection services)
    {
        services.AddScoped<ISubscriptionsRepository, SubscriptionsRepository>();
        services.AddScoped<IPlansRepository, PlansRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<ISubscriptionLogRepository, SubscriptionLogRepository>();
        services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();
        services.AddScoped<IWebhookProcessor, WebhookProcessor>();
        services.AddScoped<IWebhookHandler, WebHookHandler>();
        services.AddScoped<IApplicationConfigRepository, ApplicationConfigRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<IOffersRepository, OffersRepository>();
        services.AddScoped<IOfferAttributesRepository, OfferAttributesRepository>();
        services.AddScoped<IPlanEventsMappingRepository, PlanEventsMappingRepository>();
        services.AddScoped<IEventsRepository, EventsRepository>();
        services.AddScoped<IEmailService, SMTPEmailService>();
        services.AddScoped<SaaSClientLogger<HomeController>>();
        services.AddScoped<IWebNotificationService, WebNotificationService>();
    }
}