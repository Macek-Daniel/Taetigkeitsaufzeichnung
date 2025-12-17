using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Taetigkeitsaufzeichnung.Data
{
    public static class DatabaseStartupChecker
    {
        public static void EnsureSchemaOrThrow(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TaetigkeitsaufzeichnungContext>();

            try
            {
                // Probe a couple of mapped tables. If they don't exist, EF will throw a SqlException.
                _ = context.Schuljahre.AsNoTracking().Select(s => s.SchuljahrID).FirstOrDefault();
                _ = context.Taetigkeiten.AsNoTracking().Select(t => t.TaetigkeitID).FirstOrDefault();
            }
            catch (SqlException)
            {
                var msg =
                    "Database schema is missing. Execute the SQL script to create all tables:" +
                    "\n  sqlcmd -S localhost,1433 -U sa -P \"Password123!\" -Q \"IF DB_ID('Taetigkeitsaufzeichnung') IS NULL CREATE DATABASE [Taetigkeitsaufzeichnung];\"" +
                    "\n  sqlcmd -S localhost,1433 -U sa -P \"Password123!\" -d Taetigkeitsaufzeichnung -b -i Taetigkeitsaufzeichnung/database/script.sql" +
                    "\nThen restart the application.";

                Console.Error.WriteLine(msg);
                throw new InvalidOperationException(msg);
            }
        }
    }
}
