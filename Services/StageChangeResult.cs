using Ats.Api.Dtos.Applications;

namespace Ats.Api.Services;

public record StageChangeResult
{
    public ApplicationSummaryDto? Application { get; init; }
    public bool ApplicationNotFound { get; init; }
    public bool TeamMemberNotFound { get; init; }
    public string? TransitionError { get; init; }

    public static StageChangeResult Success(ApplicationSummaryDto app) =>
        new() { Application = app };

    public static StageChangeResult AppNotFound() =>
        new() { ApplicationNotFound = true };

    public static StageChangeResult MemberNotFound() =>
        new() { TeamMemberNotFound = true };

    public static StageChangeResult InvalidTransition(string message) =>
        new() { TransitionError = message };
}
