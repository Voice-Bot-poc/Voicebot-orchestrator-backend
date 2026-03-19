using VoiceBot.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configure logging (built-in way)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register orchestrator
builder.Services.AddScoped<IVoiceOrchestrator, VoiceOrchestrator>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// ✅ Use built-in logger
app.Logger.LogInformation("Application starting...");
app.Logger.LogInformation("Services configured successfully.");

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Application running.");

app.Run();