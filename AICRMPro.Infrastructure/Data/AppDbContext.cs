using AICRMPro.Domain.Entities;
using AICRMPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AICRMPro.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenant currentTenant) : base(options)
    {
        _currentTenant = currentTenant;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Tenant entity
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Plan).HasConversion<string>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Role).HasConversion<string>();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Tenant).WithMany(t => t.Users).HasForeignKey(e => e.TenantId);
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserId);
        });

        // Apply global query filter for multi-tenancy
        // This filter will automatically filter all queries by the current tenant
        // Note: We don't apply this to Tenants table as it's used for tenant resolution
        modelBuilder.Entity<User>().HasQueryFilter(e => _currentTenant.TenantId == null || e.TenantId == _currentTenant.TenantId);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(e => _currentTenant.TenantId == null || e.User.TenantId == _currentTenant.TenantId);
    }

    public override int SaveChanges()
    {
        // Set tenant ID for new entities that have TenantId property
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity.GetType().GetProperty("TenantId") != null);

        foreach (var entry in entries)
        {
            if (_currentTenant.TenantId.HasValue)
            {
                entry.Property("TenantId").CurrentValue = _currentTenant.TenantId.Value;
            }
        }

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set tenant ID for new entities that have TenantId property
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity.GetType().GetProperty("TenantId") != null);

        foreach (var entry in entries)
        {
            if (_currentTenant.TenantId.HasValue)
            {
                entry.Property("TenantId").CurrentValue = _currentTenant.TenantId.Value;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
