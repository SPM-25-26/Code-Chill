namespace ChillCodeBackend.Services
{
    public record RefreshTokenResult(string Token, DateTimeOffset ExpiresAt);
}
