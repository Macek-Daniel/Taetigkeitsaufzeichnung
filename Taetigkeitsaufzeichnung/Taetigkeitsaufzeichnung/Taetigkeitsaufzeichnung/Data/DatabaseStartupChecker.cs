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
            catch (SqlException ex)
            {
                // HIER: Wir geben den echten Fehler aus!
                var msg = $"ECHTER FEHLER: {ex.Message}\n\n" +
                          "Prüfe ConnectionString in appsettings.json!";

                Console.Error.WriteLine(msg);
                throw new InvalidOperationException(msg);
            }
        }
        
    }
}
