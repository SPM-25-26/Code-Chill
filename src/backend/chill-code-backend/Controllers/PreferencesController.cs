using System.Security.Claims;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Services.Preferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChillCodeBackend.Controllers
{
    [ApiController]
    [Route("api/preferences")]
    public class PreferencesController : ControllerBase
    {
        private readonly IPreferenceRepository _preferenceRepository;
        private readonly IPreferenceScoringService _scoringService;
        private readonly IPreferenceResponseRepository _responseRepository;
        private readonly IQuestionnaireSchemaProvider _schemaProvider;
        private readonly ILogger<PreferencesController> _logger;

        public PreferencesController(
            IPreferenceRepository preferenceRepository,
            IPreferenceScoringService scoringService,
            IPreferenceResponseRepository responseRepository,
            IQuestionnaireSchemaProvider schemaProvider,
            ILogger<PreferencesController> logger
        )
        {
            _preferenceRepository = preferenceRepository;
            _scoringService = scoringService;
            _responseRepository = responseRepository;
            _schemaProvider = schemaProvider;
            _logger = logger;
        }

        [HttpGet("schema")]
        public ActionResult<QuestionnaireSchemaDto> GetSchema()
        {
            return Ok(_schemaProvider.GetSchema());
        }

        [Authorize]
        [HttpGet("response")]
        public async Task<ActionResult<PreferenceResponseDto>> GetLatestResponse(
            CancellationToken cancellationToken
        )
        {
            var userId = ResolveUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var response = await _responseRepository.GetLatestAsync(userId, cancellationToken);
            if (response is null)
            {
                return NotFound();
            }

            return Ok(
                new PreferenceResponseDto
                {
                    QuestionnaireVersion = response.QuestionnaireVersion,
                    Answers = response.Answers,
                    DerivedPreferences = response.DerivedPreferences,
                }
            );
        }

        [Authorize]
        [HttpPut("response")]
        public async Task<ActionResult<PreferenceResponseDto>> SaveResponse(
            PreferenceResponseDto request,
            [FromQuery] bool includePreview = false,
            CancellationToken cancellationToken = default
        )
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
                return Unauthorized();

            var schema = _schemaProvider.GetSchema();
            var answers = request.Answers ?? new Dictionary<string, object>();

            TravelPreferenceDto derived;
            try
            {
                derived = _schemaProvider.MapAnswersToPreferences(answers);
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(ex.Message);
            }

            var preferenceEntity = MapToEntity(derived);
            var savedPreference = await _preferenceRepository.UpsertAsync(
                userId,
                preferenceEntity,
                cancellationToken
            );

            var responseEntity = new PreferenceResponse
            {
                QuestionnaireVersion = string.IsNullOrWhiteSpace(request.QuestionnaireVersion)
                    ? schema.Version
                    : request.QuestionnaireVersion,
                Answers = answers,
                DerivedPreferences = MapToDto(savedPreference),
            };

            await _responseRepository.UpsertAsync(userId, responseEntity, cancellationToken);

            PreferencePreviewDto? preview = null;
            if (includePreview)
            {
                preview = await _scoringService.BuildPreviewAsync(derived, cancellationToken);
            }

            return Ok(
                new PreferenceResponseDto
                {
                    QuestionnaireVersion = responseEntity.QuestionnaireVersion,
                    Answers = responseEntity.Answers,
                    DerivedPreferences = responseEntity.DerivedPreferences,
                    Preview = preview,
                }
            );
        }

        private static TravelPreferenceDto MapToDto(UserPreference preference)
        {
            return new TravelPreferenceDto
            {
                TravelVibe = preference.TravelVibe,
                ActivityLevel = preference.ActivityLevel,
                CitySize = preference.CitySize,
                Interests = preference.Interests,
            };
        }

        private static UserPreference MapToEntity(TravelPreferenceDto dto)
        {
            return new UserPreference
            {
                TravelVibe = dto.TravelVibe.Trim(),
                ActivityLevel = dto.ActivityLevel.Trim(),
                CitySize = dto.CitySize.Trim(),
                Interests = dto.Interests.Select(i => i.Trim()).ToList(),
            };
        }

        private Guid ResolveUserId()
        {
            var claim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }
}
