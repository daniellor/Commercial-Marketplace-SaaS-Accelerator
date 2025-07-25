using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.SaaS.Accelerator.DataAccess.Migrations.PostgreSQL.SaasKit.Custom
{
    internal static class BaselineV741_Seed
    {
        public static void BaselineV741_SeedData(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"
                    do $$
                    begin
                    IF NOT EXISTS (SELECT * FROM public.""ApplicationConfiguration"" WHERE ""Name"" = 'IsMeteredBillingEnabled')
                    then
                        INSERT INTO public.""ApplicationConfiguration"" ( ""Name"", ""Value"", ""Description"") VALUES ( 'IsMeteredBillingEnabled', 'true', 'Enable Metered Billing Feature');
                    END if;
                    end $$;
                    ");
        }

        public static void BaselineV741_DeSeedData(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"

                IF EXISTS (SELECT * FROM [dbo].[ApplicationConfiguration] WHERE [Name] = 'IsMeteredBillingEnabled')
                BEGIN
                    DELETE FROM [dbo].[ApplicationConfiguration]  WHERE [Name] = 'IsMeteredBillingEnabled'
                END
                GO");
        }
    }
}