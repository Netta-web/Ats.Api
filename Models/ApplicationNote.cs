using Ats.Api.Enums;

namespace Ats.Api.Models;

public class ApplicationNote
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public NoteType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid CreatedByTeamMemberId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;
    public TeamMember CreatedBy { get; set; } = null!;
}
