namespace ChillCodeBackend.Services
{
    public interface IEmailService
    {
        Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string? textBody = null,
            CancellationToken cancellationToken = default
        );
    }
}
