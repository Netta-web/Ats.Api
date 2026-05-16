using Ats.Api.Enums;

namespace Ats.Api.Models;

public class Application
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CoverLetter { get; set; } = string.Empty;
    public ApplicationStage CurrentStage { get; set; } = ApplicationStage.Applied;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Job Job { get; set; } = null!;
    public ICollection<ApplicationNote> Notes { get; set; } = [];
    public ICollection<StageHistory> StageHistories { get; set; } = [];
    public ICollection<ApplicationScore> Scores { get; set; } = [];
}
