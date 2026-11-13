using System.Security.Cryptography;
using System.Text.Json;
using FixItAI.Api.DTOs;
using FixItAI.Api.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

namespace FixItAI.Api.Controllers;
[ApiController, Authorize, Route("api/diagnostics"), EnableRateLimiting("diagnostics")]
public class DiagnosticsController(DiagnosticService service, IDataProtectionProvider protection) : ControllerBase
{
    private readonly IDataProtector protector = protection.CreateProtector("FixItAI.DiagnosticTicket.v1");
    private const string SafetyMessage = "Stop using the device. If you smell gas or see smoke or fire, move to a safe place and contact the appropriate emergency service or a qualified professional. Do not attempt a repair.";
    [HttpPost("questions")]
    public async Task<IActionResult> Questions(CreateTroubleshootingRequest request, CancellationToken ct)
    {
        if (DiagnosticService.Hazard(JsonSerializer.Serialize(request))) return UnprocessableEntity(new { message = SafetyMessage });
        try {
            var questions = await service.Questions(request, ct);
            var ticket = new QuestionTicket(DateTimeOffset.UtcNow.AddMinutes(30), request, questions.Questions);
            return Ok(new { questions = questions.Questions, ticket = protector.Protect(JsonSerializer.Serialize(ticket)) });
        } catch (DiagnosticException ex) { return StatusCode(ex.StatusCode, new { message = ex.Message }); }
    }
    [HttpPost("result")]
    public async Task<IActionResult> Result(DiagnosticAnswersRequest request, CancellationToken ct)
    {
        QuestionTicket? ticket;
        try { ticket = JsonSerializer.Deserialize<QuestionTicket>(protector.Unprotect(request.Ticket)); }
        catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException) { return BadRequest(new { message = "Your diagnostic session has expired. Please start again." }); }
        if (ticket is null || ticket.ExpiresAt < DateTimeOffset.UtcNow || ticket.Questions is null || ticket.Device is null) return BadRequest(new { message = "Your diagnostic session has expired. Please start again." });
        if (request.Answers.Count != ticket.Questions.Count || request.Answers.Any(a => string.IsNullOrWhiteSpace(a) || a.Length > 1000)) return BadRequest(new { message = "Please answer each question in 1 to 1000 characters." });
        if (request.Answers.Any(DiagnosticService.Hazard)) return UnprocessableEntity(new { message = SafetyMessage });
        try { return Ok(await service.Result(ticket, request.Answers, ct)); }
        catch (DiagnosticException ex) { return StatusCode(ex.StatusCode, new { message = ex.Message }); }
    }
}
