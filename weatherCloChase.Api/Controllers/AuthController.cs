using Microsoft.AspNetCore.Mvc;
using weatherCloChase.Core.Interfaces;

namespace weatherCloChase.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { error = "Email and password are required" });

        var result = await _authService.RegisterAsync(request.Email, request.Password);
        
        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(new
        {
            token = result.Token,
            expiresAt = result.ExpiresAt
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);
        
        if (!result.Success)
            return Unauthorized(new { error = result.Error });

        return Ok(new
        {
            token = result.Token,
            expiresAt = result.ExpiresAt
        });
    }
}

public class AuthRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}