using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using R_SS.BLL.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.BLL.Helpers;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtTokenResult GenerateToken(User user, string roleName)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT signing key is missing.");
        }

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiresInMinutes = GetExpirationMinutes();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, roleName)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtTokenResult
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc
        };
    }

    private int GetExpirationMinutes()
    {
        var rawValue = _configuration["Jwt:ExpiresInMinutes"];
        return int.TryParse(rawValue, out var minutes) && minutes > 0 ? minutes : 60;
    }
}
