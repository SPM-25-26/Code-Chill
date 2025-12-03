namespace ChillCodeBackend.Services
{
    public interface IEmailTemplateService
    {
        (string subject, string html, string text) BuildVerificationEmail(
            string recipientEmail,
            string verificationLink
        );
        (string subject, string html, string text) BuildRegistrationSuccessEmail(
            string recipientEmail
        );
    }
}
