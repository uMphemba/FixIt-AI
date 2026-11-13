using System.ComponentModel.DataAnnotations;
namespace FixItAI.Api.DTOs;
public class DiagnosticAnswersRequest
{
    [Required, StringLength(16000)] public string Ticket { get; set; } = "";
    [Required, MinLength(1), MaxLength(5)] public List<string> Answers { get; set; } = [];
}
public record QuestionTicket(DateTimeOffset ExpiresAt, CreateTroubleshootingRequest Device, List<string> Questions);
public record DiagnosticQuestionSet(List<string> Questions);
public record PossibleCause(string Cause, string Likelihood, string Explanation);
public record DiagnosticResult(string ProblemSummary, List<PossibleCause> PossibleCauses, List<string> DiagnosticSteps, List<string> ToolsAndMaterials, string Difficulty, List<string> SafetyWarnings, string WhenToContactProfessional);
