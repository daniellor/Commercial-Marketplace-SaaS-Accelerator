using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.SaaS.Accelerator.DataAccess.Migrations.PostgreSQL.SaasKit.Custom
{
    internal static class BaselineV751_Seed
    {
        public static void BaselineV751_SeedData(this MigrationBuilder migrationBuilder)
        {
            var seedDate = DateTime.Now;
            migrationBuilder.Sql(@$"
                    do $$
                    begin
                    IF NOT EXISTS (SELECT * FROM public.""ApplicationConfiguration"" WHERE ""Name"" = 'ValidateWebhookJwtToken')
                     THEN
                         INSERT INTO public.""ApplicationConfiguration"" (""Name"",""Value"",""Description"") VALUES ( 'ValidateWebhookJwtToken', 'true', 'Validates JWT token when webhook event is recieved.');
                     END IF;

                 end $$;
                ");
        }

        public static void BaselineV751_DeSeedData(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"

                IF EXISTS (SELECT * FROM [dbo].[ApplicationConfiguration] WHERE [Name] = 'ValidateWebhookJwtToken')
                BEGIN
                    DELETE FROM [dbo].[ApplicationConfiguration]  WHERE [Name] = 'ValidateWebhookJwtToken'
                END
                GO");
        }
    }
}