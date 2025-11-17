using Asp.Versioning;
using CursorDemo.Application.DTOs;
using CursorDemo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CursorDemo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Login and get JWT token
    /// </summary>
    /// <remarks>
    /// Demo credentials:
    /// - Username: elif
    /// - Password: 1234
    /// 
    /// This endpoint is available at:
    /// - /api/v1/auth/login
    /// - /api/v2/auth/login
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginDto loginDto)
    {
        // Hardcoded demo user
        if (loginDto.Username == "elif" && loginDto.Password == "1234")
        {
            var token = _tokenService.GenerateToken(loginDto.Username);
            var response = new TokenResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            return Ok(response);
        }

        return Unauthorized(new { message = "Invalid username or password" });
    }
}

