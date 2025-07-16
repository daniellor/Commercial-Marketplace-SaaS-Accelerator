using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.SaaS.Accelerator.DataAccess.Migrations.PostgreSQL.SaasKit.Custom
{
    internal static class BaselineV2_Seed
    {

        public static void BaselineV2_SeedData(this MigrationBuilder migrationBuilder)
        {
            var seedDate = DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture);
            migrationBuilder.Sql(@$"
INSERT INTO public.""ValueTypes""
    (""ValueType"",""CreateDate"",""HTMLType"")
VALUES 
    ('Int','{seedDate}','int'),
    ('String','{seedDate}','string'),
    ('Date','{seedDate}','date');
");

            migrationBuilder.Sql(@$"INSERT INTO public.""Roles"" (""Name"") VALUES ('PublisherAdmin');");

            migrationBuilder.Sql(@$"
INSERT INTO public.""Events""
	(""EventsName"",""IsActive"",""CreateDate"")
VALUES
    ('Activate',true,'{seedDate}'),
	('Unsubscribe',true,'{seedDate}'),
	('Pending Activation',true,'{seedDate}');
");

            migrationBuilder.Sql(@$"
INSERT INTO public.""ApplicationConfiguration""
	(""Name"",""Value"",""Description"")
VALUES
    ('SMTPFromEmail','','SMTP Email'),
	('SMTPPassword','','SMTP Password'),
	('SMTPHost','','SMTP Host'),
	('SMTPPort','','SMTP Port'),
	('SMTPUserName','','SMTP User Name'),
	('SMTPSslEnabled','','SMTP Ssl Enabled'),
	('ApplicationName','Contoso','Application Name'),
	('IsEmailEnabledForSubscriptionActivation','true','Active Email Enabled'),
	('IsEmailEnabledForUnsubscription','true','Unsubscribe Email Enabled'),
	('IsAutomaticProvisioningSupported','false','Skip Activation - Automatic Provisioning Supported'),
	('IsEmailEnabledForPendingActivation','false','Email Enabled For Pending Activation');
");

            migrationBuilder.Sql(@$"
do $$
begin
If not EXISTS (SELECT * FROM public.""ApplicationConfiguration"" WHERE ""Name"" = 'AcceptSubscriptionUpdates') 
then
    INSERT INTO public.""ApplicationConfiguration"" (""Name"",""Value"",""Description"")
    VALUES ('AcceptSubscriptionUpdates','false','Accepts subscriptions plan or quantity updates');
END IF;

IF NOT EXISTS (SELECT * FROM public.""ApplicationConfiguration"" WHERE ""Name"" = 'LogoFile') 
then
    INSERT INTO public.""ApplicationConfiguration"" (""Name"",""Value"",""Description"")
    VALUES ('LogoFile','','Logo File');
END IF;

IF NOT EXISTS (SELECT * FROM public.""ApplicationConfiguration"" WHERE ""Name"" = 'FaviconFile')
then
    INSERT INTO public.""ApplicationConfiguration"" (""Name"",""Value"",""Description"")
    VALUES ('FaviconFile','','Favicon File');
END IF;

end $$;

");
        }
    }
}