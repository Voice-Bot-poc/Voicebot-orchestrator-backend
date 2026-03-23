namespace VoiceBot.Domain.Exceptions;

/// <summary>
/// Thrown when the STT microservice is unreachable or returns an error response.
/// Lives in the Domain layer so any layer above can catch it without depending on infrastructure.
/// </summary>
public class SttException : Exception
{
    public int? StatusCode { get; }

    public SttException(string message) : base(message) { }

    public SttException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public SttException(string message, Exception innerException) : base(message, innerException) { }
}
