using System.Security.Claims;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Services.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChillCodeBackend.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IPreferenceRepository _preferenceRepository;
        private readonly ChatContextService _contextService;
        private readonly ChatPromptBuilder _promptBuilder;
        private readonly IChatCompletionClient _chatClient;
        private readonly ChatResponseNormalizer _responseNormalizer;

        public ChatController(
            IPreferenceRepository preferenceRepository,
            ChatContextService contextService,
            ChatPromptBuilder promptBuilder,
            IChatCompletionClient chatClient,
            ChatResponseNormalizer responseNormalizer
        )
        {
            _preferenceRepository = preferenceRepository;
            _contextService = contextService;
            _promptBuilder = promptBuilder;
            _chatClient = chatClient;
            _responseNormalizer = responseNormalizer;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ChatResponseDto>> Chat(
            [FromBody] ChatRequestDto request,
            CancellationToken cancellationToken
        )
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message is required.");
            }

            var userId = ResolveUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var preference = await _preferenceRepository.GetByUserIdAsync(userId, cancellationToken);
            var preferenceSummary = BuildPreferenceSummary(preference);

            var locationSummary = BuildLocationSummary(
                request.Municipality,
                request.Location?.Latitude,
                request.Location?.Longitude
            );

            var contextItems = await _contextService.BuildContextAsync(
                new ChatContextRequest
                {
                    Language = request.Language,
                    Municipality = request.Municipality,
                    Latitude = request.Location?.Latitude,
                    Longitude = request.Location?.Longitude,
                    MaxItems = request.MaxContextItems ?? 40
                },
                cancellationToken
            );

            var prompt = _promptBuilder.BuildPrompt(
                new ChatPromptInput
                {
                    UserMessage = request.Message,
                    Conversation = request.Conversation ?? new List<ChatMessageDto>(),
                    PreferenceSummary = preferenceSummary,
                    LocationSummary = locationSummary,
                    Items = contextItems
                }
            );

            string rawResponse;
            try
            {
                rawResponse = await _chatClient.GetCompletionAsync(prompt, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }

            var reply = _responseNormalizer.Normalize(rawResponse);

            return Ok(
                new ChatResponseDto
                {
                    Reply = reply,
                    ContextItemsUsed = contextItems.Count
                }
            );
        }

        [Authorize]
        [HttpGet("health")]
        public async Task<ActionResult<ChatResponseDto>> Health(CancellationToken cancellationToken)
        {
            var prompt = _promptBuilder.BuildPrompt(
                new ChatPromptInput
                {
                    UserMessage = "ping",
                    Conversation = new List<ChatMessageDto>(),
                    PreferenceSummary = "-",
                    LocationSummary = "-",
                    Items = new List<TourismContextItem>()
                }
            );

            string rawResponse;
            try
            {
                rawResponse = await _chatClient.GetCompletionAsync(prompt, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }

            var reply = _responseNormalizer.Normalize(rawResponse);

            return Ok(
                new ChatResponseDto
                {
                    Reply = reply,
                    ContextItemsUsed = 0
                }
            );
        }

        private Guid ResolveUserId()
        {
            var claim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        private static string BuildPreferenceSummary(UserPreference? preference)
        {
            if (preference == null)
            {
                return "-";
            }

            var interests = preference.Interests.Count == 0
                ? "-"
                : string.Join(", ", preference.Interests);

            return string.Join(
                " | ",
                $"travelVibe={preference.TravelVibe}",
                $"activityLevel={preference.ActivityLevel}",
                $"citySize={preference.CitySize}",
                $"interests={interests}",
                $"familyFriendly={(preference.FamilyFriendly.HasValue ? preference.FamilyFriendly.Value.ToString() : "-")}"
            );
        }

        private static string BuildLocationSummary(
            string? municipality,
            double? latitude,
            double? longitude
        )
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(municipality))
            {
                parts.Add($"municipality={municipality.Trim()}");
            }

            if (latitude.HasValue && longitude.HasValue)
            {
                parts.Add($"coords={latitude:0.0000},{longitude:0.0000}");
            }

            return parts.Count == 0 ? "-" : string.Join(" | ", parts);
        }
    }
}
