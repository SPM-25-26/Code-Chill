using System;
using ChillCodeBackend.Data;
using ChillCodeBackend.Data.Api;
using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Options;
using ChillCodeBackend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer(
                    (document, context, cancellationToken) =>
                    {
                        document.Info.Title = "MyWeb API Test";
                        document.Info.Version = "v1";
                        document.Info.Description = "This API powers my awesome WebApi app.";
                        return Task.CompletedTask;
                    }
                );
            });

            services.AddControllers();
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
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddHttpClient<IUnicamApiProxy, UnicamApiProxy>(client =>
            {
                var baseUrl = _configuration["ExternalApi:BaseUrl"];
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
        }

        public void Configure(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseCors("Dev");
            app.UseAuthorization();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
                options.RoutePrefix = "swagger";
            });

            app.MapControllers();
        }
    }
}
