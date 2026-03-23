using VoiceBot.Application.Interfaces;

namespace VoiceBot.Infrastructure.Backends;

/// <summary>
/// Stub implementation of ILlmBackend for the WhatsApp-team voice pipeline.
///
/// Wire-in guide (when the WhatsApp team provides their API spec):
///
///   1. Inject HttpClient via IHttpClientFactory with a new named client, e.g. "WhatsAppPipeline".
///      Register in Program.cs:
///        builder.Services.AddHttpClient("WhatsAppPipeline", c =>
///        {
///            c.BaseAddress = new Uri(builder.Configuration["WhatsAppPipeline:BaseUrl"]
///                ?? throw new InvalidOperationException("Missing WhatsAppPipeline:BaseUrl"));
///            c.Timeout = TimeSpan.FromSeconds(30);
///        });
///
///   2. Add to appsettings.json:
///        "WhatsAppPipeline": { "BaseUrl": "https://api.whatsapp-team.example.com" }
///
///   3. Expected request shape (TBD — replace with actual spec):
///        POST /v1/voice/process
///        Content-Type: multipart/form-data
///        Fields: session_id (string), audio (file)
///
///   4. Expected response shape (TBD):
///        {
///          "transcript":    "...",
///          "response_text": "...",
///          "audio_base64":  "...",
///          "audio_format":  "mp3"
///        }
///
///   5. Map to LlmBackendResponse and throw PipelineException on HTTP errors.
///
/// Registration switch (in Program.cs):
///   "PipelineMode": "WhatsAppTeam"  →  registers this class as ILlmBackend.
/// </summary>
public sealed class WhatsAppTeamBackend : ILlmBackend
{
    // TODO: inject HttpClient and ILogger once the API spec is known.
    // public WhatsAppTeamBackend(HttpClient http, ILogger<WhatsAppTeamBackend> logger) { ... }

    /// <inheritdoc />
    public Task<LlmBackendResponse> ProcessAsync(
        string sessionId,
        byte[] audioBytes,
        string audioFileName,
        CancellationToken ct = default)
    {
        // TODO: replace with real HTTP call to WhatsApp team endpoint.
        throw new NotImplementedException(
            "WhatsApp team backend not yet integrated — awaiting their API spec.");
    }
}
