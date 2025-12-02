using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Options;
using ChillCodeBackend.Services;
using Microsoft.AspNetCore.Identity;

namespace ChillCodeBackend;

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
        services.AddSingleton<IUserStore, FileUserStore>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.Configure<JwtOptions>(_configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();
    }

    public void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

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
