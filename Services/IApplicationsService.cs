using Ats.Api.Dtos.Applications;
using Ats.Api.Enums;

namespace Ats.Api.Services;

public interface IApplicationsService
{
    Task<bool> JobExistsAsync(Guid jobId);
    Task<bool> ApplicationExistsAsync(Guid jobId, string candidateEmail);
    Task<bool> ApplicationExistsByIdAsync(Guid applicationId);
    Task<NoteResult> AddNoteAsync(Guid applicationId, CreateNoteRequestDto dto, Guid teamMemberId);
    Task<List<NoteDto>> GetNotesAsync(Guid applicationId);
    Task<ApplicationSummaryDto> CreateApplicationAsync(Guid jobId, ApplyToJobRequestDto dto);
    Task<List<ApplicationSummaryDto>> GetApplicationsForJobAsync(Guid jobId, ApplicationStage? stage);
    Task<ApplicationProfileDto?> GetApplicationProfileAsync(Guid id);
    Task<StageChangeResult> MoveApplicationStageAsync(Guid applicationId, ApplicationStage toStage, Guid teamMemberId, string comment);
    Task<ScoreResult> UpsertScoreAsync(Guid applicationId, ScoreDimension dimension, int score, string comment, Guid teamMemberId);
}
