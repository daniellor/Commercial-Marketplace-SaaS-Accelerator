using Azure.Identity;
using Marketplace.SaaS.Accelerator.AdminSite.Controllers;
using Marketplace.SaaS.Accelerator.DataAccess.Contracts;
using Marketplace.SaaS.Accelerator.DataAccess.Services;
using Marketplace.SaaS.Accelerator.Services.Configurations;
using Marketplace.SaaS.Accelerator.Services.Contracts;
using Marketplace.SaaS.Accelerator.Services.Models;
using Marketplace.SaaS.Accelerator.Services.Services;
using Marketplace.SaaS.Accelerator.Services.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Marketplace.Metering;
using Microsoft.Marketplace.SaaS;
using System;
using System.Reflection;
using Web.Infrastructure;
using Web.Infrastructure.AspNet;
using Web.Infrastructure.Util;

namespace AdminSite
{
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
            var kestrelSettings = builder.Configuration.GetSection(nameof(KestrelSettings)).Get<KestrelSettings>();

            if (kestrelSettings!.Enabled)
            {
                builder.WebHost.UseKestrel(option =>
                {
                    option.ListenAnyIP(kestrelSettings.DefaultEndpointPort, configure => configure.UseHttps());
                    if (kestrelSettings!.HttpEndpointPort != null)
                        option.ListenAnyIP(kestrelSettings.HttpEndpointPort.Value);
                });
            }
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddDebug()
                    .AddConsole();
            });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var config = new SaaSApiClientConfiguration()
            {
                AdAuthenticationEndPoint = builder.Configuration["SaaSApiConfiguration:AdAuthenticationEndPoint"],
                ClientId = builder.Configuration["SaaSApiConfiguration:ClientId"] ?? Guid.Empty.ToString(),
                ClientSecret = builder.Configuration["SaaSApiConfiguration:ClientSecret"] ?? string.Empty,
                FulFillmentAPIBaseURL = builder.Configuration["SaaSApiConfiguration:FulFillmentAPIBaseURL"],
                MTClientId = builder.Configuration["SaaSApiConfiguration:MTClientId"] ?? Guid.Empty.ToString(),
                FulFillmentAPIVersion = builder.Configuration["SaaSApiConfiguration:FulFillmentAPIVersion"],
                GrantType = builder.Configuration["SaaSApiConfiguration:GrantType"],
                Resource = builder.Configuration["SaaSApiConfiguration:Resource"],
                SaaSAppUrl = builder.Configuration["SaaSApiConfiguration:SaaSAppUrl"],
                SignedOutRedirectUri = builder.Configuration["SaaSApiConfiguration:SignedOutRedirectUri"],
                TenantId = builder.Configuration["SaaSApiConfiguration:TenantId"] ?? Guid.Empty.ToString(),
                IsAdminPortalMultiTenant = builder.Configuration["SaaSApiConfiguration:IsAdminPortalMultiTenant"]
            };
            var knownUsers = new KnownUsersModel()
            {
                KnownUsers = builder.Configuration["KnownUsers"],
            };
            var creds = new ClientSecretCredential(config.TenantId.ToString(), config.ClientId.ToString(), config.ClientSecret);
            var boolMultiTenant = config.IsAdminPortalMultiTenant?.ToLower().Trim() ?? "false";



            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddOpenIdConnect(options =>
                {

                    if (boolMultiTenant == "false")
                    {
                        options.Authority = $"{config.AdAuthenticationEndPoint}/{config.TenantId}/v2.0";
                    }
                    else
                    {
                        options.Authority = $"{config.AdAuthenticationEndPoint}/common/v2.0";
                    }
                    options.ClientId = config.MTClientId;
                    options.ResponseType = OpenIdConnectResponseType.IdToken;
                    options.CallbackPath = "/Home/Index";
                    options.SignedOutRedirectUri = config.SignedOutRedirectUri;
                    options.TokenValidationParameters.NameClaimType = ClaimConstants.CLAIM_SHORT_NAME;
                    options.TokenValidationParameters.ValidateIssuer = false;
                })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.Cookie.MaxAge = options.ExpireTimeSpan;
                    options.SlidingExpiration = true;
                });

            builder.Services
                .AddTransient<IClaimsTransformation, CustomClaimsTransformation>()
                .AddScoped<ExceptionHandlerAttribute>()
                .AddScoped<RequestLoggerActionFilter>()
            ;

            if (!Uri.TryCreate(config.FulFillmentAPIBaseURL, UriKind.Absolute, out var fulfillmentBaseApi))
            {
                fulfillmentBaseApi = new Uri("https://marketplaceapi.microsoft.com/api");
            }

            builder.Services
                .AddSingleton<IFulfillmentApiService>(new FulfillmentApiService(new MarketplaceSaaSClient(fulfillmentBaseApi, creds), config, new FulfillmentApiClientLogger()))
                .AddSingleton<IMeteredBillingApiService>(new MeteredBillingApiService(new MarketplaceMeteringClient(creds), config, new SaaSClientLogger<MeteredBillingApiService>()))
                .AddSingleton(config)
                .AddSingleton(knownUsers);

            // Add the assembly version
            builder.Services.AddSingleton<IAppVersionService>(new AppVersionService(Assembly.GetExecutingAssembly()?.GetName()?.Version, Assembly.GetEntryAssembly().GetAssemblyLinkTime()));

            builder.Services
                .AddScoped<ApplicationConfigService>();

            builder.Services.AddWebServices(builder.Configuration);


            InitializeRepositoryServices(builder.Services);

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(5);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddMvc(option =>
            {
                option.EnableEndpointRouting = false;
                option.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
            builder.Services.AddControllersWithViews();

            builder.Services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddScoped<OffersService>();

            var app = builder.Build();

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
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Run();

        }


        /// <summary>
        /// Initializes the repository services.
        /// </summary>
        /// <param name="services">The services.</param>
        private static void InitializeRepositoryServices(IServiceCollection services)
        {
            services.AddScoped<ISubscriptionsRepository, SubscriptionsRepository>();
            services.AddScoped<IPlansRepository, PlansRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ISubscriptionLogRepository, SubscriptionLogRepository>();
            services.AddScoped<IApplicationConfigRepository, ApplicationConfigRepository>();
            services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();
            services.AddScoped<ISubscriptionUsageLogsRepository, SubscriptionUsageLogsRepository>();
            services.AddScoped<IMeteredDimensionsRepository, MeteredDimensionsRepository>();
            services.AddScoped<IKnownUsersRepository, KnownUsersRepository>();
            services.AddScoped<IOffersRepository, OffersRepository>();
            services.AddScoped<IValueTypesRepository, ValueTypesRepository>();
            services.AddScoped<IOfferAttributesRepository, OfferAttributesRepository>();
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
            services.AddScoped<IPlanEventsMappingRepository, PlanEventsMappingRepository>();
            services.AddScoped<IEventsRepository, EventsRepository>();
            services.AddScoped<KnownUserAttribute>();
            services.AddScoped<IEmailService, SMTPEmailService>();
            services.AddScoped<ISAGitReleasesService, SAGitReleasesService>();
            services.AddScoped<ISchedulerFrequencyRepository, SchedulerFrequencyRepository>();
            services.AddScoped<IMeteredPlanSchedulerManagementRepository, MeteredPlanSchedulerManagementRepository>();
            services.AddScoped<SaaSClientLogger<HomeController>>();
            services.AddScoped<SaaSClientLogger<PlansController>>();
            services.AddScoped<SaaSClientLogger<OffersController>>();
            services.AddScoped<SaaSClientLogger<KnownUsersController>>();
            services.AddScoped<SaaSClientLogger<ApplicationLogController>>();
            services.AddScoped<SaaSClientLogger<ApplicationConfigController>>();
            services.AddScoped<SaaSClientLogger<SchedulerController>>();
        }
    }
}


