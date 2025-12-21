namespace ChillCodeBackend.Services
{
    public interface ITokenService
    {
        string CreateToken(Guid userId, string userName);
        RefreshTokenResult CreateRefreshToken();
    }
}
