using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Services.Preferences;
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

        [HttpGet("response")]
        public async Task<ActionResult<PreferenceResponseDto>> GetLatestResponse(
            CancellationToken cancellationToken
        )
        {
            var userId = ResolveUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User id is required (header X-User-Id).");
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

        [HttpPut("response")]
        public async Task<ActionResult<PreferenceResponseDto>> SaveResponse(
            PreferenceResponseDto request,
            [FromQuery] bool includePreview = false,
            CancellationToken cancellationToken = default
        )
        {
            var userId = ResolveUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User id is required (header X-User-Id).");
            }

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

        [HttpGet("me")]
        public async Task<ActionResult<TravelPreferenceDto>> GetMyPreferences(
            CancellationToken cancellationToken
        )
        {
            var userId = ResolveUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User id is required (header X-User-Id).");
            }

            var preference = await _preferenceRepository.GetByUserIdAsync(
                userId,
                cancellationToken
            );
            if (preference is null)
            {
                return NotFound();
            }

            return Ok(MapToDto(preference));
        }

        [HttpPut]
        public async Task<ActionResult<TravelPreferenceDto>> UpsertPreferences(
            TravelPreferenceDto preferenceDto,
            CancellationToken cancellationToken
        )
        {
            var userId = ResolveUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User id is required (header X-User-Id).");
            }

            var (isValid, error) = PreferenceValidation.Validate(preferenceDto);
            if (!isValid)
            {
                return ValidationProblem(error);
            }

            var preference = MapToEntity(preferenceDto);
            var saved = await _preferenceRepository.UpsertAsync(
                userId,
                preference,
                cancellationToken
            );
            _logger.LogInformation("Preferences saved for user {UserId}", userId);
            return Ok(MapToDto(saved));
        }

        [HttpPost("preview")]
        public async Task<ActionResult<PreferencePreviewDto>> Preview(
            TravelPreferenceDto preferenceDto,
            CancellationToken cancellationToken
        )
        {
            var (isValid, error) = PreferenceValidation.Validate(preferenceDto);
            if (!isValid)
            {
                return ValidationProblem(error);
            }

            var preview = await _scoringService.BuildPreviewAsync(preferenceDto, cancellationToken);
            return Ok(preview);
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
            if (
                Request.Headers.TryGetValue("X-User-Id", out var values)
                && Guid.TryParse(values.FirstOrDefault(), out var id)
            )
            {
                return id;
            }

            return Guid.Empty;
        }
    }
}
