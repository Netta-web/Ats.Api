namespace Ats.Api.Dtos.Applications;

public class ApplicationProfileDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CoverLetter { get; set; } = string.Empty;
    public string CurrentStage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<NoteDto> Notes { get; set; } = [];
    public List<ScoreDto> Scores { get; set; } = [];
    public List<StageHistoryDto> StageHistory { get; set; } = [];
}

public class NoteDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ScoreDto
{
    public Guid Id { get; set; }
    public string Dimension { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class StageHistoryDto
{
    public Guid Id { get; set; }
    public string FromStage { get; set; } = string.Empty;
    public string ToStage { get; set; } = string.Empty;
    public string ChangedByName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
