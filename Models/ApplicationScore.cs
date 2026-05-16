using Ats.Api.Enums;

namespace Ats.Api.Models;

public class ApplicationScore
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public ScoreDimension Dimension { get; set; }
    public int Score { get; set; }
    public string Comment { get; set; } = string.Empty;
    public Guid UpdatedByTeamMemberId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;
    public TeamMember UpdatedBy { get; set; } = null!;
}
