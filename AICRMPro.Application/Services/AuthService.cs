using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BCrypt.Net;
using AICRMPro.Application.DTOs;
using AICRMPro.Application.Settings;
using AICRMPro.Domain.Entities;
using AICRMPro.Domain.Enums;
using AICRMPro.Domain.Interfaces;
using AICRMPro.Infrastructure.Data;

namespace AICRMPro.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(RefreshTokenRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly ICurrentTenant _currentTenant;

    public AuthService(
        AppDbContext context,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings,
        ICurrentTenant currentTenant)
    {
        _context = context;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
        _currentTenant = currentTenant;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Clear tenant context for global operations
        _currentTenant.SetTenant(null);

        // Check if user already exists (ignore tenant filters)
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            throw new Exception("User with this email already exists");
        }

        // Check if tenant with this name already exists
        var existingTenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Name == request.CompanyName);

        if (existingTenant != null)
        {
            throw new Exception("Company with this name already exists");
        }

        // Create tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.CompanyName,
            Slug = request.CompanyName.ToLower().Replace(" ", "-") + "-" + Guid.NewGuid().ToString("N")[..8],
            Plan = Plan.Free,
            MaxAICallsPerDay = 100,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Role.TenantAdmin, // First user in tenant is admin
            IsActive = true
        };

        // Set tenant context for saving
        _currentTenant.SetTenant(null); // Clear tenant context for tenant creation

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        _currentTenant.SetTenant(tenant.Id); // Set tenant context for user creation

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.TenantId, user.Role.ToString());
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        // Clear tenant context
        _currentTenant.SetTenant(null);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                TenantId = user.TenantId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                TenantName = tenant.Name
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email (ignore tenant filter)
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            throw new Exception("Invalid email or password");
        }

        if (!user.IsActive || !user.Tenant.IsActive)
        {
            throw new Exception("Account is inactive");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid email or password");
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.TenantId, user.Role.ToString());
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                TenantId = user.TenantId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                TenantName = user.Tenant.Name
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Find refresh token
        var refreshTokenEntity = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .Include(rt => rt.User)
            .ThenInclude(u => u.Tenant)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshTokenEntity == null)
        {
            throw new Exception("Invalid refresh token");
        }

        if (refreshTokenEntity.IsRevoked)
        {
            throw new Exception("Refresh token has been revoked");
        }

        if (refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            throw new Exception("Refresh token has expired");
        }

        if (!refreshTokenEntity.User.IsActive || !refreshTokenEntity.User.Tenant.IsActive)
        {
            throw new Exception("Account is inactive");
        }

        // Revoke old refresh token
        refreshTokenEntity.IsRevoked = true;
        await _context.SaveChangesAsync();

        // Generate new tokens
        var accessToken = _tokenService.GenerateAccessToken(
            refreshTokenEntity.User.Id,
            refreshTokenEntity.User.TenantId,
            refreshTokenEntity.User.Role.ToString());

        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Save new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = refreshTokenEntity.User.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            User = new UserDto
            {
                Id = refreshTokenEntity.User.Id,
                TenantId = refreshTokenEntity.User.TenantId,
                Name = refreshTokenEntity.User.Name,
                Email = refreshTokenEntity.User.Email,
                Role = refreshTokenEntity.User.Role,
                TenantName = refreshTokenEntity.User.Tenant.Name
            }
        };
    }

    public async Task LogoutAsync(RefreshTokenRequest request)
    {
        var refreshTokenEntity = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshTokenEntity != null)
        {
            refreshTokenEntity.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }
}
