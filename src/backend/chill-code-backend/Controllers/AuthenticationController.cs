using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Services;
using Microsoft.AspNetCore.Http;
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
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IUserStore userStore,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService,
        ILogger<AuthenticationController> logger
    )
    {
        _userStore = userStore;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
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
