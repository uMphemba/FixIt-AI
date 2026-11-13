using System.ComponentModel.DataAnnotations;
using FixItAI.Api.Models;

namespace FixItAI.Api.DTOs;
public class CreateTroubleshootingRequest : IValidatableObject
{
    [Required, StringLength(100)] public string DeviceCategory { get; set; } = "";
    [Required, StringLength(120)] public string DeviceName { get; set; } = "";
    [StringLength(100)] public string? Brand { get; set; }
    [StringLength(100)] public string? Model { get; set; }
    [Required, StringLength(2000, MinimumLength = 10)] public string ProblemDescription { get; set; } = "";
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (!DeviceCategories.All.Contains(DeviceCategory))
            yield return new ValidationResult("Select a valid device category.", [nameof(DeviceCategory)]);
        if (ProblemDescription.Trim().Length < 10)
            yield return new ValidationResult("Describe the problem in at least 10 characters.", [nameof(ProblemDescription)]);
    }
}
