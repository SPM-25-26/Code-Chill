namespace ChillCodeBackend.Services
{
    public interface ITokenService
    {
        string CreateToken(string userName);
        RefreshTokenResult CreateRefreshToken();
    }
}
