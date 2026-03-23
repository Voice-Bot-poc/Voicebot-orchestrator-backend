namespace VoiceBot.Application.Interfaces;

/// <summary>
/// Contract for Speech-to-Text transcription.
/// Defined in the Application layer so the orchestrator has no knowledge of HTTP or infrastructure.
/// The concrete implementation lives in VoiceBot.Infrastructure.
/// </summary>
public interface ISttService
{
    /// <summary>
    /// Sends raw audio bytes to the STT backend and returns the transcribed text.
    /// </summary>
    /// <param name="audioBytes">The raw audio file bytes to transcribe.</param>
    /// <param name="fileName">
    /// The original file name (including extension), used to set the correct
    /// Content-Disposition filename in the multipart upload so the STT service
    /// can detect the audio format.
    /// </param>
    /// <param name="cancellationToken">Propagates cancellation from the HTTP request.</param>
    /// <returns>The transcript produced by the STT service.</returns>
    Task<string> TranscribeAsync(byte[] audioBytes, string fileName, CancellationToken cancellationToken = default);
}
