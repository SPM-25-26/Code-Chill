using System.Text;
using ChillCodeBackend.Models.Options;
using Microsoft.Extensions.Options;

namespace ChillCodeBackend.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly EmailOptions _options;

        public EmailTemplateService(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        public (string subject, string html, string text) BuildVerificationEmail(
            string recipientEmail,
            string verificationLink
        )
        {
            var subject = $"{_options.BrandName} - Verify your email";

            var html =
                $@"
<div style=""font-family: Arial, sans-serif; max-width: 640px; margin:0 auto; color:#111;"">
  <h2 style=""color:#111;"">Welcome to {_options.BrandName}!</h2>
  <p>Hi there,</p>
  <p>Thanks for signing up. Please verify your email to activate your account:</p>
  <p style=""margin:24px 0;""><a href=""{verificationLink}"" style=""background:#111;color:#fff;padding:12px 18px;text-decoration:none;border-radius:6px;"">Verify email</a></p>
  <p>If the button doesn't work, copy and paste this link into your browser:<br>{verificationLink}</p>
  <p style=""font-size:12px;color:#555;margin-top:24px;"">
    You receive this email because you created an account at {_options.BrandName}. 
    If this wasn't you, please ignore this email or contact us at {_options.SupportEmail}.
    <br>Unsubscribe or manage preferences: <a href=""{_options.UnsubscribeUrl}"">{_options.UnsubscribeUrl}</a>
  </p>
</div>";

            var text = new StringBuilder()
                .AppendLine($"Welcome to {_options.BrandName}!")
                .AppendLine()
                .AppendLine(
                    "Thanks for signing up. Please verify your email to activate your account:"
                )
                .AppendLine(verificationLink)
                .AppendLine()
                .AppendLine("If this wasn't you, ignore this email or contact support.")
                .AppendLine($"Unsubscribe: {_options.UnsubscribeUrl}")
                .ToString();

            return (subject, html, text);
        }

        public (string subject, string html, string text) BuildRegistrationSuccessEmail(
            string recipientEmail
        )
        {
            var subject = $"{_options.BrandName} - Registration successful";

            var html =
                $@"
<div style=""font-family: Arial, sans-serif; max-width: 640px; margin:0 auto; color:#111;"">
  <h2 style=""color:#111;"">You're in, {recipientEmail}!</h2>
  <p>Welcome to {_options.BrandName}. Your account is ready to use.</p>
  <p>If you didn't create this account, please contact us at {_options.SupportEmail}.</p>
  <p style=""font-size:12px;color:#555;margin-top:24px;"">
    You receive this email because you created an account at {_options.BrandName}.
    <br>Unsubscribe or manage preferences: <a href=""{_options.UnsubscribeUrl}"">{_options.UnsubscribeUrl}</a>
  </p>
</div>";

            var text = new StringBuilder()
                .AppendLine($"Welcome to {_options.BrandName}!")
                .AppendLine("Your account is ready to use.")
                .AppendLine()
                .AppendLine("If you didn't create this account, contact support.")
                .AppendLine($"Unsubscribe: {_options.UnsubscribeUrl}")
                .ToString();

            return (subject, html, text);
        }
    }
}
