using Microsoft.AspNetCore.Mvc;
using VoiceBot.Application.Services;
using VoiceBot.Domain.Exceptions;
using VoiceBot.Domain.Models;

namespace VoiceBot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoiceController : ControllerBase
{
    private readonly IVoiceOrchestrator _orchestrator;

    public VoiceController(IVoiceOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    /// <summary>
    /// Accepts an audio file and returns the LLM text reply + TTS audio (base64).
    /// POST /api/voice/process
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessAudio(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No audio file provided." });

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var request = new AudioRequest
        {
            AudioData = memoryStream.ToArray(),
            FileName  = file.FileName,
        };
f
        try
        {
            var (text, audio) = await _orchestrator.ProcessAudioAsync(request);

            return Ok(new
            {
                text,
                audioBase64 = Convert.ToBase64String(audio),
            });
        }
        catch (PipelineException ex)
        {
            // Map the domain exception to HTTP 400 with a structured JSON body.
            // The controller is the ONLY place where domain exceptions are translated
            // to HTTP status codes — keeping all HTTP concerns out of Application/Domain.
            return BadRequest(new
            {
                stage  = ex.Stage,
                detail = ex.Detail,
            });
        }
        // Note: all other unexpected exceptions propagate to the default ASP.NET
        // exception handler, which logs them and returns a 500.
    }
}
