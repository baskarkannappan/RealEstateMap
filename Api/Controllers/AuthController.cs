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
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest? request)
    {
        // Optional public key check. This is not user auth; it is app-to-api hygiene.
        var configuredPublicKey = _configuration["ApiClient:PublicApiKey"];
        if (!string.IsNullOrWhiteSpace(configuredPublicKey) &&
            !string.Equals(request?.PublicApiKey, configuredPublicKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Token request rejected due to invalid public API key.");
            return Unauthorized();
        }

        var signingKey = _configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Missing Jwt:SigningKey.");
        var issuer = _configuration["Jwt:Issuer"] ?? "RealEstateMap.Api";
        var audience = _configuration["Jwt:Audience"] ?? "RealEstateMap.Client";
        var expiresUtc = DateTime.UtcNow.AddMinutes(20);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "public-map-client"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim("scope", "houses.read")
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
