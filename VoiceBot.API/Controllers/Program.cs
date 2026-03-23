using VoiceBot.Application.Interfaces;
using VoiceBot.Application.Services;
using VoiceBot.Infrastructure.Backends;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Controllers & API explorer
// -----------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------------------------------------------------------
// Named HttpClient — "PythonPipeline"
// Base URL comes from "PythonPipeline:BaseUrl" in appsettings.json.
// Using a named client (rather than a typed client) means the client
// can be injected into the backend class that needs it without requiring
// the class itself to be registered as a typed-client host.
// -----------------------------------------------------------------------
builder.Services.AddHttpClient("PythonPipeline", client =>
{
    var baseUrl = builder.Configuration["PythonPipeline:BaseUrl"]
        ?? throw new InvalidOperationException("Missing config key: PythonPipeline:BaseUrl");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout     = TimeSpan.FromSeconds(30);
});

// -----------------------------------------------------------------------
// Pipeline-mode switch
// Reads "PipelineMode" from config; defaults to "Fast" if absent.
// To switch modes: change appsettings.json or set env var PipelineMode=WhatsAppTeam
// No code change required — the right ILlmBackend is resolved at startup.
// -----------------------------------------------------------------------
var pipelineMode = builder.Configuration["PipelineMode"] ?? "Fast";

switch (pipelineMode)
{
    case "Fast":
        // FastPipelineBackend requires a pre-configured HttpClient ("PythonPipeline").
        // We register it as Scoped (not Singleton) so ILogger scope aligns with requests.
        builder.Services.AddScoped<ILlmBackend>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http    = factory.CreateClient("PythonPipeline");
            var logger  = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<FastPipelineBackend>>();

            return new FastPipelineBackend(http, logger);
        });
        break;

    case "WhatsAppTeam":
        // Stub implementation — throws NotImplementedException until their API is integrated.
        builder.Services.AddScoped<ILlmBackend, WhatsAppTeamBackend>();
        break;

    default:
        throw new InvalidOperationException(
            $"Unknown PipelineMode '{pipelineMode}'. Valid values: \"Fast\", \"WhatsAppTeam\".");
}

// IVoiceOrchestrator is in the Application layer — always the same regardless of mode.
builder.Services.AddScoped<IVoiceOrchestrator, VoiceOrchestrator>();

// -----------------------------------------------------------------------
// CORS — allow the Vite frontend dev server
// -----------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -----------------------------------------------------------------------
// Middleware pipeline
// -----------------------------------------------------------------------
var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
