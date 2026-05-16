using Ats.Api.Dtos.Applications;

namespace Ats.Api.Services;

public record NoteResult
{
    public NoteDto? Note { get; init; }
    public bool ApplicationNotFound { get; init; }
    public bool TeamMemberNotFound { get; init; }

    public static NoteResult Success(NoteDto note) => new() { Note = note };
    public static NoteResult AppNotFound() => new() { ApplicationNotFound = true };
    public static NoteResult MemberNotFound() => new() { TeamMemberNotFound = true };
}
