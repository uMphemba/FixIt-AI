using FixItAI.Api.Models;
using Microsoft.AspNetCore.Mvc;
namespace FixItAI.Api.Controllers;
[ApiController, Route("api/device-categories")]
public class DeviceCategoriesController : ControllerBase
{
    [HttpGet] public IActionResult Get() => Ok(new { categories = DeviceCategories.All });
}
