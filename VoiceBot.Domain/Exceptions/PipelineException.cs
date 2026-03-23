namespace VoiceBot.Domain.Exceptions;

/// <summary>
/// Thrown when the Python pipeline (STT → LLM → TTS) returns a non-success response
/// or encounters an internal stage failure.
///
/// Domain layer: no HTTP types, no JSON — just the semantic failure with a stage name
/// so callers can route errors intelligently (e.g., the controller maps this to HTTP 400).
/// </summary>
public class PipelineException : Exception
{
    /// <summary>
    /// The pipeline stage that failed, e.g. "stt", "llm", "tts" or "http".
    /// Populated directly from the Python error response where available.
    /// </summary>
    public string Stage { get; }

    /// <summary>
    /// Human-readable detail about the failure, forwarded from the upstream service.
    /// </summary>
    public string Detail { get; }

    public PipelineException(string stage, string detail)
        : base($"Pipeline failed at stage '{stage}': {detail}")
    {
        Stage  = stage;
        Detail = detail;
    }

    public PipelineException(string stage, string detail, Exception innerException)
        : base($"Pipeline failed at stage '{stage}': {detail}", innerException)
    {
        Stage  = stage;
        Detail = detail;
    }
}
