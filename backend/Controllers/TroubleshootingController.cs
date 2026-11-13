using FixItAI.Api.Data;
using FixItAI.Api.DTOs;
using FixItAI.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace FixItAI.Api.Controllers;
[ApiController, Route("api/troubleshooting")]
public class TroubleshootingController(AppDbContext db, ILogger<TroubleshootingController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateTroubleshootingRequest request, CancellationToken cancellationToken)
    {
        var session = new TroubleshootingSession { Id = Guid.NewGuid(), DeviceCategory = request.DeviceCategory, DeviceName = request.DeviceName.Trim(), Brand = request.Brand?.Trim(), Model = request.Model?.Trim(), ProblemDescription = request.ProblemDescription.Trim(), CreatedAt = DateTime.UtcNow };
        try { db.Sessions.Add(session); await db.SaveChangesAsync(cancellationToken); }
        catch (Exception ex) when (ex is DbUpdateException or NpgsqlException or InvalidOperationException or TimeoutException)
        {
            logger.LogWarning("Unable to save a troubleshooting session because the database is unavailable or not configured.");
            return StatusCode(503, new { message = "The service cannot save your request right now. Please try again later." });
        }
        return StatusCode(201, new { sessionId = session.Id, status = "submitted", createdAt = session.CreatedAt });
    }
}
