using ChillCodeBackend;

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
            policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
        }
    );
});

var app = builder.Build();
startup.Configure(app);

app.Run();
