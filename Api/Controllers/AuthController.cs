using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RealEstateMap.Api.Models;

namespace RealEstateMap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var validUsername = _configuration["ApiCredentials:Username"] ?? "map-client";
        var validPassword = _configuration["ApiCredentials:Password"] ?? "map-secret";

        if (!string.Equals(request.Username, validUsername, StringComparison.Ordinal) ||
            !string.Equals(request.Password, validPassword, StringComparison.Ordinal))
        {
            return Unauthorized();
        }

        var signingKey = _configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Missing Jwt:SigningKey.");
        var issuer = _configuration["Jwt:Issuer"] ?? "RealEstateMap.Api";
        var audience = _configuration["Jwt:Audience"] ?? "RealEstateMap.Client";
        var expiresUtc = DateTime.UtcNow.AddMinutes(30);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresUtc,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        return Ok(new LoginResponse { Token = jwt, ExpiresUtc = expiresUtc });
    }
}
