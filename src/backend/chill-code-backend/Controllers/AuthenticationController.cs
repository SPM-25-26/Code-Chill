using System.ComponentModel.DataAnnotations;
using System.Linq;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Options;
using ChillCodeBackend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MyWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            ITokenService tokenService,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            IOptions<EmailOptions> emailOptions,
            ILogger<AuthenticationController> logger
        )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _emailOptions = emailOptions.Value;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(
            RegisterRequestDto registerRequest,
            CancellationToken cancellationToken
        )
        {
            try
            {
                if (!TryValidateRegistration(registerRequest, out var validationProblemResult))
                {
                    _logger.LogWarning(
                        "Registration validation failed for email {Email}",
                        registerRequest.Email
                    );
                    return validationProblemResult;
                }

                var normalizedEmail = registerRequest.Email.Trim();
                var normalizedUserName = registerRequest.UserName?.Trim() ?? string.Empty;

                var existingUser = await _userRepository.GetByEmailAsync(
                    normalizedEmail,
                    cancellationToken
                );
                if (existingUser != null)
                {
                    _logger.LogInformation(
                        "Registration attempt for existing email {Email}",
                        normalizedEmail
                    );
                    return Conflict("A user with that email already exists.");
                }

                var now = DateTimeOffset.UtcNow;
                var user = new User
                {
                    Email = normalizedEmail,
                    UserName = string.IsNullOrWhiteSpace(normalizedUserName)
                        ? GenerateUserName()
                        : normalizedUserName,
                    EmailVerified = false,
                    CreatedAt = now,
                    ChangedAt = now,
                    EmailVerificationToken = GenerateVerificationToken(),
                    EmailVerificationExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, registerRequest.Password);

                var response = await IssueTokensAsync(user, cancellationToken);
                _logger.LogInformation(
                    "User registered successfully with email {Email}",
                    normalizedEmail
                );

                await SendVerificationEmailAsync(user, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Registration failed for email {Email}",
                    registerRequest.Email
                );
                return StatusCode(StatusCodes.Status500InternalServerError, "Registration failed.");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(
            LoginRequestDto loginRequest,
            CancellationToken cancellationToken
        )
        {
            if (!TryValidateCredentials(loginRequest, out var validationProblemResult))
            {
                return validationProblemResult;
            }

            var normalizedEmail = loginRequest.Email.Trim();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                loginRequest.Password
            );

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid email or password.");
            }

            if (!user.EmailVerified)
            {
                return Unauthorized(
                    "Email not verified. Please verify your email before logging in."
                );
            }

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                user.UserName = GenerateUserName();
            }

            var response = await IssueTokensAsync(user, cancellationToken);

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh(
            RefreshRequestDto request,
            CancellationToken cancellationToken
        )
        {
            if (
                string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.RefreshToken)
            )
            {
                return BadRequest("Email and refresh token are required.");
            }

            var normalizedEmail = request.Email.Trim();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (
                user == null
                || string.IsNullOrWhiteSpace(user.RefreshToken)
                || !string.Equals(user.RefreshToken, request.RefreshToken, StringComparison.Ordinal)
            )
            {
                return Unauthorized("Invalid refresh token.");
            }

            if (
                user.RefreshTokenExpiresAt is null
                || user.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow
            )
            {
                return Unauthorized("Refresh token expired.");
            }

            var response = await IssueTokensAsync(user, cancellationToken);
            return Ok(response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequestDto request,
            CancellationToken cancellationToken
        )
        {
            if (
                string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.NewPassword)
            )
            {
                return BadRequest("Email and new password are required.");
            }

            var user = await _userRepository.GetByEmailAsync(
                request.Email.Trim(),
                cancellationToken
            );

            if (user == null)
            {
                return NotFound("No account found with this email.");
            }

            if (!IsValidPassword(request.NewPassword, out var passwordError))
            {
                return BadRequest(passwordError);
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.ChangedAt = DateTimeOffset.UtcNow;
            await _userRepository.AddOrUpdateAsync(user, cancellationToken);

            return Ok("Password updated successfully.");
        }

        [HttpGet("verify")]
        public async Task<ActionResult> VerifyEmail(
            [FromQuery] string email,
            [FromQuery] string token,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Email and token are required.");
            }

            var normalizedEmail = email.Trim();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.EmailVerified)
            {
                return Ok("Email already verified.");
            }

            if (
                string.IsNullOrWhiteSpace(user.EmailVerificationToken)
                || !string.Equals(user.EmailVerificationToken, token, StringComparison.Ordinal)
            )
            {
                return Unauthorized("Invalid verification token.");
            }

            if (
                user.EmailVerificationExpiresAt is not null
                && user.EmailVerificationExpiresAt <= DateTimeOffset.UtcNow
            )
            {
                return Unauthorized("Verification token expired.");
            }

            user.EmailVerified = true;
            user.EmailVerificationToken = string.Empty;
            user.EmailVerificationExpiresAt = null;
            await _userRepository.AddOrUpdateAsync(user, cancellationToken);

            return Ok("Email verified successfully.");
        }

        private bool TryValidateRegistration(
            RegisterRequestDto registerRequest,
            out ActionResult validationProblemResult
        )
        {
            var emailValidator = new EmailAddressAttribute();
            if (
                string.IsNullOrWhiteSpace(registerRequest.Email)
                || string.IsNullOrWhiteSpace(registerRequest.Password)
            )
            {
                validationProblemResult = new BadRequestObjectResult(
                    "Email and password are required."
                );
                return false;
            }

            var normalizedEmail = registerRequest.Email.Trim();
            if (!emailValidator.IsValid(normalizedEmail))
            {
                validationProblemResult = new BadRequestObjectResult("Email format is invalid.");
                return false;
            }

            if (!IsValidPassword(registerRequest.Password, out var passwordError))
            {
                validationProblemResult = new BadRequestObjectResult(passwordError);
                return false;
            }

            validationProblemResult = null!;
            return true;
        }

        private bool TryValidateCredentials(
            LoginRequestDto loginRequest,
            out ActionResult validationProblemResult
        )
        {
            var emailValidator = new EmailAddressAttribute();
            if (
                string.IsNullOrWhiteSpace(loginRequest.Email)
                || string.IsNullOrWhiteSpace(loginRequest.Password)
            )
            {
                validationProblemResult = new BadRequestObjectResult(
                    "Email and password are required."
                );
                return false;
            }

            var normalizedEmail = loginRequest.Email.Trim();
            if (!emailValidator.IsValid(normalizedEmail))
            {
                validationProblemResult = new BadRequestObjectResult("Email format is invalid.");
                return false;
            }

            validationProblemResult = null!;
            return true;
        }

        private static bool IsValidPassword(string password, out string errorMessage)
        {
            if (password.Length < 8)
            {
                errorMessage = "Password must be at least 8 characters long.";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                errorMessage = "Password must include at least one lowercase letter.";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                errorMessage = "Password must include at least one uppercase letter.";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                errorMessage = "Password must include at least one number.";
                return false;
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                errorMessage = "Password must include at least one special character.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private async Task<AuthResponseDto> IssueTokensAsync(
            User user,
            CancellationToken cancellationToken
        )
        {
            var accessToken = _tokenService.CreateToken(user.Email);
            var refreshTokenResult = _tokenService.CreateRefreshToken();
            user.RefreshToken = refreshTokenResult.Token;
            user.RefreshTokenExpiresAt = refreshTokenResult.ExpiresAt;
            if (user.CreatedAt == default)
            {
                user.CreatedAt = DateTimeOffset.UtcNow;
            }

            user.ChangedAt = DateTimeOffset.UtcNow;
            await _userRepository.AddOrUpdateAsync(user, cancellationToken);

            return new AuthResponseDto
            {
                UserName = user.UserName,
                Token = accessToken,
                RefreshToken = refreshTokenResult.Token,
                RefreshTokenExpiresAt = refreshTokenResult.ExpiresAt,
            };
        }

        private string GenerateVerificationToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string BuildVerificationLink(User user)
        {
            var emailEncoded = Uri.EscapeDataString(user.Email);
            var tokenEncoded = Uri.EscapeDataString(user.EmailVerificationToken);
            var separator = _emailOptions.VerificationBaseUrl.Contains("?") ? "&" : "?";
            return $"{_emailOptions.VerificationBaseUrl}{separator}email={emailEncoded}&token={tokenEncoded}";
        }

        private async Task SendVerificationEmailAsync(
            User user,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(_emailOptions.VerificationBaseUrl))
            {
                _logger.LogWarning(
                    "Verification email not sent: VerificationBaseUrl not configured."
                );
                return;
            }

            if (string.IsNullOrWhiteSpace(user.EmailVerificationToken))
            {
                _logger.LogWarning("Verification email not sent: no verification token present.");
                return;
            }

            var verificationLink = BuildVerificationLink(user);
            var (subject, html, text) = _emailTemplateService.BuildVerificationEmail(
                user.Email,
                verificationLink
            );

            await _emailService.SendAsync(user.Email, subject, html, text, cancellationToken);
        }

        private string GenerateUserName()
        {
            // Create a friendly default username if none was provided.
            return $"user-{Guid.NewGuid().ToString("N")[..8]}";
        }
    }
}
