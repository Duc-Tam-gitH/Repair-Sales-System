using Microsoft.Extensions.Configuration;

namespace R_SS.BLL.Helpers;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;

    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    public int ExpiresInMinutes { get; set; } = 60;

    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var expiresInMinutes = int.TryParse(section["ExpiresInMinutes"], out var minutes) && minutes > 0
            ? minutes
            : 60;

        return new JwtSettings
        {
            Key = section["Key"] ?? string.Empty,
            Issuer = section["Issuer"],
            Audience = section["Audience"],
            ExpiresInMinutes = expiresInMinutes
        };
    }
}
