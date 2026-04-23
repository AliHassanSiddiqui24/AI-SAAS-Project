using Microsoft.EntityFrameworkCore;
using AICRMPro.Infrastructure.Data;

namespace AICRMPro.API;

public class TestDbConnection
{
    public static void TestConnection()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
// Is line ko change karein
optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=aicrmpro;Username=postgres;Password=localdev123");        
        using var context = new AppDbContext(optionsBuilder.Options, new Infrastructure.Services.CurrentTenant());
        
        try
        {
            var canConnect = context.Database.CanConnect();
            Console.WriteLine($"Can connect to database: {canConnect}");
            
            if (canConnect)
            {
                var migrations = context.Database.GetAppliedMigrations();
                Console.WriteLine($"Applied migrations: {string.Join(", ", migrations)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to database: {ex.Message}");
        }
    }
}
