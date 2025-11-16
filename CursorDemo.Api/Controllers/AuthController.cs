using CursorDemo.Application.DTOs;
using CursorDemo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CursorDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// </remarks>
    [HttpPost("login")]
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

