using System.ComponentModel.DataAnnotations;
using ChillCodeBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChillCodeBackend.Controllers
{
    [ApiController]
    [Route("api/test-email")]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            ILogger<TestEmailController> logger
        )
        {
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        public class TestEmailRequest
        {
            [Required]
            [EmailAddress]
            public string ToEmail { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail(
            [FromBody] TestEmailRequest request,
            CancellationToken cancellationToken
        )
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var (subject, html, text) = _emailTemplateService.BuildRegistrationSuccessEmail(
                request.ToEmail
            );

            try
            {
                await _emailService.SendAsync(
                    request.ToEmail,
                    subject,
                    html,
                    text,
                    cancellationToken
                );
                return Ok("Test email sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Email}", request.ToEmail);
                return StatusCode(500, "Failed to send test email. Check SMTP configuration.");
            }
        }
    }
}
