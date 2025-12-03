namespace ChillCodeBackend.Models.Options
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        public string FromName { get; set; } = "No-Reply Chill & Code";
        public string FromEmail { get; set; } = "chilAndCode@gmx.ch";
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool UseEncryption { get; set; } = true;
        public bool DisableCertificateRevocationCheck { get; set; } = false;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string BrandName { get; set; } = "Chill & Code";
        public string SupportEmail { get; set; } = "chilAndCode@gmx.ch";
        public string UnsubscribeUrl { get; set; } = "https://example.com/unsubscribe";
        public string VerificationBaseUrl { get; set; } = "https://example.com/verify";
    }
}
