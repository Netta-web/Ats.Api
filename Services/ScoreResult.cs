using Ats.Api.Dtos.Applications;

namespace Ats.Api.Services;

public record ScoreResult
{
    public ScoreDto? Score { get; init; }
    public bool ApplicationNotFound { get; init; }
    public bool TeamMemberNotFound { get; init; }

    public static ScoreResult Success(ScoreDto score) => new() { Score = score };
    public static ScoreResult AppNotFound() => new() { ApplicationNotFound = true };
    public static ScoreResult MemberNotFound() => new() { TeamMemberNotFound = true };
}
