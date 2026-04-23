using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using AICRMPro.Infrastructure.Data;
using AICRMPro.Infrastructure.Services;

namespace AICRMPro.API;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Yahan hum connection string ko bilkul simple aur direct rakh rahe hain
        optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5433;Database=aicrmpro;Username=postgres;Password=localdev123;Pooling=false;Include Error Detail=true;");

        return new AppDbContext(optionsBuilder.Options, new CurrentTenant());
    }
}