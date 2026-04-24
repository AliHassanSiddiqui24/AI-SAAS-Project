using Microsoft.AspNetCore.Mvc;
using AICRMPro.Application.DTOs;
using AICRMPro.Application.Services;

namespace AICRMPro.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            
            // Set refresh token as httpOnly cookie
            Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to true in production with HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
                // Remove explicit domain to allow localhost cookies to work across ports
            });
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            
            // Set refresh token as httpOnly cookie
            Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to true in production with HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
                // Remove explicit domain to allow localhost cookies to work across ports
            });
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest? request = null)
    {
        try
        {
            // Try to get refresh token from request body first (localStorage fallback), then from cookie
            string? refreshToken = null;
            
            if (request?.RefreshToken != null)
            {
                refreshToken = request.RefreshToken;
                Console.WriteLine("[Auth] Refresh token from request body");
            }
            else
            {
                refreshToken = Request.Cookies["refreshToken"];
                Console.WriteLine("[Auth] Refresh token from cookie");
            }
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { error = "Refresh token not found" });
            }

            var tokenRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
            var response = await _authService.RefreshTokenAsync(tokenRequest);
            
            // Set new refresh token as httpOnly cookie
            Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to true in production with HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
                // Remove explicit domain to allow localhost cookies to work across ports
            });
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest? request = null)
    {
        try
        {
            // Try to get refresh token from request body first (localStorage fallback), then from cookie
            string? refreshToken = null;
            
            if (request?.RefreshToken != null)
            {
                refreshToken = request.RefreshToken;
                Console.WriteLine("[Auth] Logout - Refresh token from request body");
            }
            else
            {
                refreshToken = Request.Cookies["refreshToken"];
                Console.WriteLine("[Auth] Logout - Refresh token from cookie");
            }
            
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var tokenRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
                await _authService.LogoutAsync(tokenRequest);
            }

            // Clear the refresh token cookie
            Response.Cookies.Delete("refreshToken");
            
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
