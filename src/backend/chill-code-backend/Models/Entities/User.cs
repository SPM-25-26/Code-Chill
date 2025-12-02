namespace ChillCodeBackend.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
        public bool EmailVerified { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

        // Password reset fields
        public string? PasswordResetToken { get; set; }
        public DateTimeOffset? PasswordResetExpiresAt { get; set; }
    }
}
