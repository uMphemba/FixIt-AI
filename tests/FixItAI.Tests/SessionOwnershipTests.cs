using System.Security.Claims;
using FixItAI.Api.Controllers;
using FixItAI.Api.Data;
using FixItAI.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace FixItAI.Tests;
public class SessionOwnershipTests
{
    [Fact]
    public async Task OtherUsersCannotReadOrChangeSessions()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AppDbContext(options);
        var id = Guid.NewGuid();
        db.Sessions.Add(new TroubleshootingSession { Id = id, OwnerId = "owner", DeviceCategory = "Other equipment", DeviceName = "Device", ProblemDescription = "A device has stopped working", CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var controller = new TroubleshootingController(db, NullLogger<TroubleshootingController>.Instance) {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "other")], "test")) } }
        };
        Assert.IsType<NotFoundResult>(await controller.Get(id, CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.SaveState(id, new SessionStateRequest { ProgressJson = "{}" }, CancellationToken.None));
        Assert.Null((await db.Sessions.FindAsync(id))!.ProgressJson);
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "owner")], "test"));
        Assert.IsType<OkObjectResult>(await controller.Get(id, CancellationToken.None));
        Assert.IsType<NoContentResult>(await controller.SaveState(id, new SessionStateRequest { ProgressJson = "{\"index\":1}" }, CancellationToken.None));
        Assert.Equal("{\"index\":1}", (await db.Sessions.FindAsync(id))!.ProgressJson);
    }
}
