namespace Ats.Api.Dtos.Applications;

public class ApplicationSummaryDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CurrentStage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
