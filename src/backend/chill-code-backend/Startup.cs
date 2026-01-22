using System;
using System.Text;
using ChillCodeBackend.Data;
using ChillCodeBackend.Data.Api;
using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Data.Repositories.Municipalities;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Options;
using ChillCodeBackend.Services;
using ChillCodeBackend.Services.Chat;
using ChillCodeBackend.Services.Ingestion;
using ChillCodeBackend.Services.Preferences;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

namespace ChillCodeBackend
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chill Code API", Version = "v1" });
                var jwtScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                };

                c.AddSecurityDefinition("Bearer", jwtScheme);
                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } }
                );
            });
            services.AddAuthorization();

            DependencyInjection(services);
        }

        private void DependencyInjection(IServiceCollection services)
        {
            services.AddDbContext<ChillCodeDbContext>(options =>
            {
                var connectionString =
                    _configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=App_Data/chillcode.db";
                options.UseSqlite(connectionString);
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.Configure<JwtOptions>(_configuration.GetSection(JwtOptions.SectionName));
            services.Configure<EmailOptions>(_configuration.GetSection(EmailOptions.SectionName));
            services.Configure<LlmOptions>(_configuration.GetSection(LlmOptions.SectionName));
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddMemoryCache(options => { options.SizeLimit = 2000; });

            // DB
            services.AddScoped<IIngestionPersistence, IngestionPersistence>();
            services.AddScoped<IIngestionOrchestrator, IngestionOrchestrator>();
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IMunicipalityRepository, MunicipalityRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IRouteRepository, RouteRepository>();
            services.AddScoped<IPointOfInterestRepository, PointOfInterestRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IPreferenceRepository, PreferenceRepository>();
            services.AddScoped<IPreferenceResponseRepository, PreferenceResponseRepository>();
            services.AddScoped<IPreferenceScoringService, PreferenceScoringService>();
            services.AddSingleton<IQuestionnaireSchemaProvider, QuestionnaireSchemaProvider>();
            services.AddScoped<ChatContextService>();
            services.AddScoped<ChatContextCache>();
            services.AddScoped<ChatPromptBuilder>();
            services.AddScoped<ChatResponseNormalizer>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtConfig = _configuration
                        .GetSection(JwtOptions.SectionName)
                        .Get<JwtOptions>();
                    if (
                        jwtConfig is null
                        || string.IsNullOrWhiteSpace(jwtConfig.SigningKey)
                        || jwtConfig.SigningKey.Length < 16
                    )
                    {
                        throw new InvalidOperationException(
                            "JWT signing key must be configured and at least 16 characters long."
                        );
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtConfig.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtConfig.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtConfig.SigningKey)
                        ),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1),
                    };
                });

            // Data Api
            services.AddScoped<IUnicamApiService, UnicamApiService>();
            services.AddHttpClient<IUnicamApiProxy, UnicamApiProxy>(client =>
            {
                var baseUrl = _configuration["ExternalApi:BaseUrl"];
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });

            services.AddHttpClient<IChatCompletionClient, OpenAiChatCompletionClient>(
                (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<LlmOptions>>().Value;
                    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                    {
                        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                    }

                    client.Timeout = TimeSpan.FromSeconds(
                        options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 30
                    );
                }
            );
        }

        public void Configure(WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("Dev");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = "swagger";
            });

            app.MapControllers();
        }
    }
}
