namespace ChillCodeBackend.Models.Options
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        public string FromName { get; set; } = "Chill & Code";
        public string FromEmail { get; set; } = "no-reply@example.com";
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string BrandName { get; set; } = "Chill & Code";
        public string SupportEmail { get; set; } = "support@example.com";
        public string UnsubscribeUrl { get; set; } = "https://example.com/unsubscribe";
        public string VerificationBaseUrl { get; set; } = "https://example.com/verify";
    }
}
