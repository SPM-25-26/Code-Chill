using ChillCodeBackend.Models.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ChillCodeBackend.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string? textBody = null,
            CancellationToken cancellationToken = default
        )
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody, TextBody = textBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var secureSocket = _options.UseStartTls
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;
                await client.ConnectAsync(
                    _options.SmtpHost,
                    _options.SmtpPort,
                    secureSocket,
                    cancellationToken
                );

                if (!string.IsNullOrWhiteSpace(_options.SmtpUser))
                {
                    await client.AuthenticateAsync(
                        _options.SmtpUser,
                        _options.SmtpPassword,
                        cancellationToken
                    );
                }

                await client.SendAsync(message, cancellationToken);
            }
            finally
            {
                try
                {
                    await client.DisconnectAsync(true, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to disconnect SMTP client cleanly.");
                }
            }
        }
    }
}
