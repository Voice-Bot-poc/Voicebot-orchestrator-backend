using VoiceBot.Domain.Models;
using Microsoft.Extensions.Logging;

namespace VoiceBot.Application.Services;

public class VoiceOrchestrator : IVoiceOrchestrator
{
    private readonly ILogger<VoiceOrchestrator> _logger;

    public VoiceOrchestrator(ILogger<VoiceOrchestrator> logger)
    {
        _logger = logger;
    }

    private byte[] CreateWavFile(byte[] pcmData, int sampleRate, short bitsPerSample, short channels)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        int byteRate = sampleRate * channels * bitsPerSample / 8;

        // RIFF header
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + pcmData.Length);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1); // PCM
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)(channels * bitsPerSample / 8));
        writer.Write(bitsPerSample);

        // data chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(pcmData.Length);
        writer.Write(pcmData);

        return memoryStream.ToArray();
    }

    public Task<(string text, byte[] audio)> ProcessAudioAsync(AudioRequest request)
    {
        _logger.LogInformation("Received audio request. Size: {Size} bytes", request.AudioData.Length);

        string dummyText = "Hello! This is a dummy response from backend.";

        // Generate simple WAV tone (sine wave beep)
        int sampleRate = 16000;
        short bitsPerSample = 16;
        short channels = 1;
        int durationSeconds = 2;
        double frequency = 440.0; // A4 tone

        int samples = sampleRate * durationSeconds;
        byte[] audioData = new byte[samples * 2];

        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / sampleRate;
            short value = (short)(Math.Sin(2 * Math.PI * frequency * t) * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(value);
            audioData[i * 2] = bytes[0];
            audioData[i * 2 + 1] = bytes[1];
        }

        byte[] wavBytes = CreateWavFile(audioData, sampleRate, bitsPerSample, channels);

        _logger.LogInformation("Generated dummy WAV audio. Size: {Size} bytes", wavBytes.Length);

        return Task.FromResult((dummyText, wavBytes));
    }
}