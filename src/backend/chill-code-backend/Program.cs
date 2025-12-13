using ChillCodeBackend;
using ChillCodeBackend.Data.Db;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

// Development CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Dev",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173", "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

var app = builder.Build();

// Ensure database is created and migrations are applied at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChillCodeDbContext>();
    dbContext.Database.Migrate();
}

startup.Configure(app);

app.Run();
