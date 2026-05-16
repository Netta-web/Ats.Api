using Ats.Api.Enums;

namespace Ats.Api.Models;

public class StageHistory
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public ApplicationStage FromStage { get; set; }
    public ApplicationStage ToStage { get; set; }
    public Guid ChangedByTeamMemberId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;

    public Application Application { get; set; } = null!;
    public TeamMember ChangedBy { get; set; } = null!;
}
