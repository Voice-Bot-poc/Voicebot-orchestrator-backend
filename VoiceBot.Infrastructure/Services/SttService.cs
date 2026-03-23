using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VoiceBot.Application.Interfaces;
using VoiceBot.Domain.Exceptions;

namespace VoiceBot.Infrastructure.Services;

/// <summary>
/// Concrete implementation of ISttService that calls the external STT microservice
/// via HTTP multipart/form-data.
///
/// Clean Architecture rule: all HTTP plumbing lives here, in Infrastructure.
/// The Application layer only sees the ISttService interface.
/// </summary>
public class SttService : ISttService
{
    // The HttpClient is injected by IHttpClientFactory (named client "SttClient").
    // IHttpClientFactory manages connection pooling and lifetime, avoiding socket exhaustion.
    private readonly HttpClient _httpClient;
    private readonly ILogger<SttService> _logger;

    public SttService(HttpClient httpClient, ILogger<SttService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> TranscribeAsync(byte[] audioBytes, string fileName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending {Bytes} bytes to STT service for file '{File}'.", audioBytes.Length, fileName);

        // Build a multipart form-data body so the STT service receives the
        // file the same way a browser form upload would send it.
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(audioBytes);

        // Set an explicit MIME type so the STT backend can determine the audio format.
        // audio/octet-stream is a safe fallback; adjust if your STT service is stricter.
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/octet-stream");

        // The form field name "file" must match what the STT endpoint expects.
        content.Add(fileContent, "file", fileName);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync("/stt", content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            // Network-level failure (e.g., service is down, DNS failure).
            _logger.LogError(ex, "STT service is unreachable.");
            throw new SttException("The STT service is currently unreachable. Please try again later.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The request timed out (HttpClient timeout, not user cancellation).
            _logger.LogError(ex, "STT service request timed out.");
            throw new SttException("The STT service request timed out.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("STT service returned {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
            throw new SttException(
                $"STT service returned an error ({(int)response.StatusCode}): {errorBody}",
                (int)response.StatusCode
            );
        }

        // Deserialize the JSON response. The STT contract is { "transcript": "..." }.
        SttResponse? sttResult;
        try
        {
            sttResult = await response.Content.ReadFromJsonAsync<SttResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken
            );
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize STT response.");
            throw new SttException("The STT service returned an unexpected response format.", ex);
        }

        if (sttResult is null || string.IsNullOrWhiteSpace(sttResult.Transcript))
        {
            throw new SttException("The STT service returned an empty transcript.");
        }

        _logger.LogInformation("STT transcript received: '{Transcript}'", sttResult.Transcript);
        return sttResult.Transcript;
    }

    // Private DTO — kept internal to Infrastructure so no other layer
    // has to depend on the STT service's wire format.
    private sealed class SttResponse
    {
        public string Transcript { get; set; } = string.Empty;
    }
}
