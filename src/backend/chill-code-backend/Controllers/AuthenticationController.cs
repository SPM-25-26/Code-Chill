using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MyWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IUserStore _userStore;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IUserStore userStore,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        ILogger<AuthenticationController> logger
    )
    {
        _userStore = userStore;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
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

            var existingUser = await _userStore.GetByEmailAsync(normalizedEmail, cancellationToken);
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
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, registerRequest.Password);

            var response = await IssueTokensAsync(user, cancellationToken);
            _logger.LogInformation(
                "User registered successfully with email {Email}",
                normalizedEmail
            );
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email {Email}", registerRequest.Email);
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
        var user = await _userStore.GetByEmailAsync(normalizedEmail, cancellationToken);
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
        var user = await _userStore.GetByEmailAsync(normalizedEmail, cancellationToken);
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

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(
    [FromBody] PasswordResetRequestDto request,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");

        var user = await _userStore.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user == null)
            return NotFound("User not found.");

        // Genera token sicuro
        var tokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var resetToken = Convert.ToBase64String(tokenBytes);

        user.PasswordResetToken = resetToken;
        user.PasswordResetExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15); // 15 min

        await _userStore.AddOrUpdateAsync(user, cancellationToken);

        var resetLink = $"https://yourfrontend.com/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email)}";

        await _emailService.SendEmailAsync(
        user.Email,
        "Password Reset Request",
        $"Click the link to reset your password: <a href='{resetLink}'>Reset Password</a>"
    );

        return Ok("Password reset email sent.");
    }


    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(
        [FromBody] PasswordResetDto request,
        [FromQuery] string email,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("Token and new password are required.");

        var user = await _userStore.GetByEmailAsync(email.Trim(), cancellationToken);
        if (user == null || string.IsNullOrWhiteSpace(user.PasswordResetToken) || user.PasswordResetExpiresAt <= DateTimeOffset.UtcNow)
            return BadRequest("This link is invalid or has expired.");

        // Confronta hash token
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
        if (!string.Equals(user.PasswordResetToken, tokenHash, StringComparison.Ordinal))
            return BadRequest("This link is invalid or has expired.");

        // Aggiorna la password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

        user.PasswordResetToken = null;
        user.PasswordResetExpiresAt = null;

        await _userStore.AddOrUpdateAsync(user, cancellationToken);

        return Ok("Password reset successful. You can now log in with your new password.");
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

        if (registerRequest.Password.Length < 8)
        {
            validationProblemResult = new BadRequestObjectResult(
                "Password must be at least 8 characters long."
            );
            return false;
        }

        if (
            !registerRequest.Password.Any(char.IsLetter)
            || !registerRequest.Password.Any(char.IsDigit)
        )
        {
            validationProblemResult = new BadRequestObjectResult(
                "Password must include both letters and numbers."
            );
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
        await _userStore.AddOrUpdateAsync(user, cancellationToken);

        return new AuthResponseDto
        {
            UserName = user.UserName,
            Token = accessToken,
            RefreshToken = refreshTokenResult.Token,
            RefreshTokenExpiresAt = refreshTokenResult.ExpiresAt,
        };
    }

    private string GenerateUserName()
    {
        // Create a friendly default username if none was provided.
        return $"user-{Guid.NewGuid().ToString("N")[..8]}";
    }
}
