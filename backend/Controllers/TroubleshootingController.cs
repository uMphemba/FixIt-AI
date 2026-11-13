using FixItAI.Api.Data;
using FixItAI.Api.DTOs;
using FixItAI.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
namespace FixItAI.Api.Controllers;
[ApiController, Authorize, Route("api/troubleshooting")]
public class TroubleshootingController(AppDbContext db, ILogger<TroubleshootingController> logger) : ControllerBase
{
    private string Owner => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    [HttpPost]
    public async Task<IActionResult> Create(CreateTroubleshootingRequest request, CancellationToken cancellationToken)
    {
        var session = new TroubleshootingSession { Id = Guid.NewGuid(), OwnerId = Owner, DeviceCategory = request.DeviceCategory, DeviceName = request.DeviceName.Trim(), Brand = request.Brand?.Trim(), Model = request.Model?.Trim(), ProblemDescription = request.ProblemDescription.Trim(), CreatedAt = DateTime.UtcNow };
        try { db.Sessions.Add(session); await db.SaveChangesAsync(cancellationToken); }
        catch (Exception ex) when (ex is DbUpdateException or NpgsqlException or InvalidOperationException or TimeoutException)
        {
            logger.LogWarning("Unable to save a troubleshooting session because the database is unavailable or not configured.");
            return StatusCode(503, new { message = "The service cannot save your request right now. Please try again later." });
        }
        return StatusCode(201, new { sessionId = session.Id, status = "submitted", createdAt = session.CreatedAt });
    }
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await db.Sessions.AsNoTracking().Where(x => x.OwnerId == Owner).OrderByDescending(x => x.CreatedAt).Take(100).Select(x => new { x.Id, x.DeviceCategory, x.DeviceName, x.ProblemDescription, x.CreatedAt }).ToListAsync(ct));
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var session = await db.Sessions.AsNoTracking().Where(x => x.OwnerId == Owner && x.Id == id).Select(x => new { x.Id, x.DeviceCategory, x.DeviceName, x.Brand, x.Model, x.ProblemDescription, x.CreatedAt, x.DiagnosticResultJson, x.ProgressJson }).SingleOrDefaultAsync(ct);
        return session is null ? NotFound() : Ok(session);
    }
    [HttpPut("{id:guid}/state")]
    public async Task<IActionResult> SaveState(Guid id, SessionStateRequest request, CancellationToken ct)
    {
        var session = await db.Sessions.SingleOrDefaultAsync(x => x.OwnerId == Owner && x.Id == id, ct);
        if (session is null) return NotFound();
        try {
            if (request.DiagnosticResultJson is not null) System.Text.Json.JsonDocument.Parse(request.DiagnosticResultJson).Dispose();
            if (request.ProgressJson is not null) System.Text.Json.JsonDocument.Parse(request.ProgressJson).Dispose();
        } catch (System.Text.Json.JsonException) { return BadRequest(new { message = "Invalid session data." }); }
        session.DiagnosticResultJson = request.DiagnosticResultJson;
        session.ProgressJson = request.ProgressJson;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
public class SessionStateRequest
{
    [StringLength(30000)] public string? DiagnosticResultJson { get; set; }
    [StringLength(5000)] public string? ProgressJson { get; set; }
}
