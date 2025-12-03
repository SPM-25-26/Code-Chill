using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ChillCodeBackend.Models.Options;
using Microsoft.Extensions.Options;

namespace ChillCodeBackend.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly byte[] _signingKeyBytes;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 16)
            {
                throw new InvalidOperationException(
                    "JWT signing key must be configured and at least 16 characters long."
                );
            }

            _signingKeyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        }

        public string CreateToken(string userName)
        {
            var now = DateTimeOffset.UtcNow;
            var header = new Dictionary<string, object> { ["alg"] = "HS256", ["typ"] = "JWT" };

            var payload = new Dictionary<string, object>
            {
                ["sub"] = userName,
                ["unique_name"] = userName,
                ["iss"] = _options.Issuer,
                ["aud"] = _options.Audience,
                ["iat"] = now.ToUnixTimeSeconds(),
                ["exp"] = now.AddMinutes(_options.ExpiresInMinutes).ToUnixTimeSeconds(),
            };

            var headerEncoded = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
            var payloadEncoded = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
            var unsignedToken = $"{headerEncoded}.{payloadEncoded}";
            var signature = ComputeSignature(unsignedToken);

            return $"{unsignedToken}.{signature}";
        }

        public RefreshTokenResult CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Base64UrlEncode(tokenBytes);
            var expiresAt = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenExpiresInDays);
            return new RefreshTokenResult(refreshToken, expiresAt);
        }

        private string ComputeSignature(string unsignedToken)
        {
            using var hmac = new HMACSHA256(_signingKeyBytes);
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken));
            return Base64UrlEncode(signatureBytes);
        }

        private static string Base64UrlEncode(ReadOnlySpan<byte> input)
        {
            return Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
