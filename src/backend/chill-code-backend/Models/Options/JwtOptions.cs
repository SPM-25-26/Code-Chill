namespace ChillCodeBackend.Models.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "MyWebApi";
    public string Audience { get; set; } = "MyWebApiAudience";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 60;
    public int RefreshTokenExpiresInDays { get; set; } = 7;
}
