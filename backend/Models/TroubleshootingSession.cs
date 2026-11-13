namespace FixItAI.Api.Models;
public class TroubleshootingSession
{
    public Guid Id { get; set; }
    public string DeviceCategory { get; set; } = "";
    public string DeviceName { get; set; } = "";
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string ProblemDescription { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
