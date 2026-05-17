using Ats.Api.Data;
using Ats.Api.Dtos.Applications;
using Ats.Api.Enums;
using Ats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ats.Api.Services;

public class ApplicationsService(AppDbContext db) : IApplicationsService
{
    private static readonly Dictionary<ApplicationStage, ApplicationStage[]> ValidTransitions = new()
    {
        [ApplicationStage.Applied]   = [ApplicationStage.Screening, ApplicationStage.Rejected],
        [ApplicationStage.Screening] = [ApplicationStage.Interview, ApplicationStage.Rejected],
        [ApplicationStage.Interview] = [ApplicationStage.Offer,     ApplicationStage.Rejected],
        [ApplicationStage.Offer]     = [ApplicationStage.Hired,     ApplicationStage.Rejected],
        [ApplicationStage.Hired]     = [],
        [ApplicationStage.Rejected]  = [],
    };

    public Task<bool> JobExistsAsync(Guid jobId) =>
        db.Jobs.AnyAsync(j => j.Id == jobId);

    public Task<bool> ApplicationExistsAsync(Guid jobId, string candidateEmail) =>
        db.Applications.AnyAsync(a => a.JobId == jobId && a.CandidateEmail == candidateEmail);

    public Task<bool> ApplicationExistsByIdAsync(Guid applicationId) =>
        db.Applications.AnyAsync(a => a.Id == applicationId);

    public async Task<NoteResult> AddNoteAsync(Guid applicationId, CreateNoteRequestDto dto, Guid teamMemberId)
    {
        var applicationExists = await db.Applications.AnyAsync(a => a.Id == applicationId);
        if (!applicationExists)
            return NoteResult.AppNotFound();

        var teamMember = await db.TeamMembers
            .AsNoTracking()
            .Where(t => t.Id == teamMemberId)
            .Select(t => new { t.Id, t.Name })
            .FirstOrDefaultAsync();
        if (teamMember is null)
            return NoteResult.MemberNotFound();

        var note = new ApplicationNote
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Type = dto.Type!.Value,
            Description = dto.Description,
            CreatedByTeamMemberId = teamMemberId,
            CreatedAt = DateTime.UtcNow
        };

        db.ApplicationNotes.Add(note);
        await db.SaveChangesAsync();

        return NoteResult.Success(new NoteDto
        {
            Id = note.Id,
            Type = note.Type.ToString(),
            Description = note.Description,
            CreatedByName = teamMember.Name,
            CreatedAt = note.CreatedAt
        });
    }

    public Task<List<NoteDto>> GetNotesAsync(Guid applicationId) =>
        db.ApplicationNotes
            .AsNoTracking()
            .Where(n => n.ApplicationId == applicationId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NoteDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Description = n.Description,
                CreatedByName = n.CreatedBy.Name,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

    public async Task<ApplicationSummaryDto> CreateApplicationAsync(Guid jobId, ApplyToJobRequestDto dto)
    {
        var application = new Application
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            CandidateName = dto.CandidateName,
            CandidateEmail = dto.CandidateEmail,
            CoverLetter = dto.CoverLetter ?? string.Empty,
            CurrentStage = ApplicationStage.Applied,
            CreatedAt = DateTime.UtcNow
        };

        db.Applications.Add(application);
        await db.SaveChangesAsync();

        return ToSummaryDto(application);
    }

    public async Task<List<ApplicationSummaryDto>> GetApplicationsForJobAsync(Guid jobId, ApplicationStage? stage)
    {
        var query = db.Applications
            .AsNoTracking()
            .Where(a => a.JobId == jobId);

        if (stage.HasValue)
            query = query.Where(a => a.CurrentStage == stage.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ApplicationSummaryDto
            {
                Id = a.Id,
                JobId = a.JobId,
                CandidateName = a.CandidateName,
                CandidateEmail = a.CandidateEmail,
                CurrentStage = a.CurrentStage.ToString(),
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ApplicationProfileDto?> GetApplicationProfileAsync(Guid id)
    {
        return await db.Applications
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new ApplicationProfileDto
            {
                Id = a.Id,
                JobId = a.JobId,
                JobTitle = a.Job.Title,
                CandidateName = a.CandidateName,
                CandidateEmail = a.CandidateEmail,
                CoverLetter = a.CoverLetter,
                CurrentStage = a.CurrentStage.ToString(),
                CreatedAt = a.CreatedAt,
                Notes = a.Notes
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NoteDto
                    {
                        Id = n.Id,
                        Type = n.Type.ToString(),
                        Description = n.Description,
                        CreatedByName = n.CreatedBy.Name,
                        CreatedAt = n.CreatedAt
                    })
                    .ToList(),
                Scores = a.Scores
                    .Select(s => new ScoreDto
                    {
                        Id = s.Id,
                        Dimension = s.Dimension.ToString(),
                        Score = s.Score,
                        Comment = s.Comment,
                        UpdatedByName = s.UpdatedBy.Name,
                        UpdatedAt = s.UpdatedAt
                    })
                    .ToList(),
                StageHistory = a.StageHistories
                    .OrderBy(sh => sh.ChangedAt)
                    .Select(sh => new StageHistoryDto
                    {
                        Id = sh.Id,
                        FromStage = sh.FromStage.ToString(),
                        ToStage = sh.ToStage.ToString(),
                        ChangedByName = sh.ChangedBy.Name,
                        Reason = sh.Reason,
                        ChangedAt = sh.ChangedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<StageChangeResult> MoveApplicationStageAsync(
        Guid applicationId, ApplicationStage toStage, Guid teamMemberId, string comment)
    {
        var application = await db.Applications.FindAsync(applicationId);
        if (application is null)
            return StageChangeResult.AppNotFound();

        var teamMemberExists = await db.TeamMembers.AnyAsync(t => t.Id == teamMemberId);
        if (!teamMemberExists)
            return StageChangeResult.MemberNotFound();

        var allowed = ValidTransitions[application.CurrentStage];
        if (!allowed.Contains(toStage))
        {
            var allowedList = allowed.Length > 0
                ? string.Join(", ", allowed.Select(s => s.ToString()))
                : "none — this is a terminal state";
            return StageChangeResult.InvalidTransition(
                $"Cannot move from '{application.CurrentStage}' to '{toStage}'. Allowed: {allowedList}.");
        }

        var fromStage = application.CurrentStage;
        application.CurrentStage = toStage;

        db.StageHistories.Add(new StageHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            FromStage = fromStage,
            ToStage = toStage,
            ChangedByTeamMemberId = teamMemberId,
            ChangedAt = DateTime.UtcNow,
            Reason = comment
        });

        await db.SaveChangesAsync();
        return StageChangeResult.Success(ToSummaryDto(application));
    }

    public async Task<ScoreResult> UpsertScoreAsync(
        Guid applicationId, ScoreDimension dimension, int score, string comment, Guid teamMemberId)
    {
        var applicationExists = await db.Applications.AnyAsync(a => a.Id == applicationId);
        if (!applicationExists)
            return ScoreResult.AppNotFound();

        var teamMember = await db.TeamMembers
            .AsNoTracking()
            .Where(t => t.Id == teamMemberId)
            .Select(t => new { t.Id, t.Name })
            .FirstOrDefaultAsync();
        if (teamMember is null)
            return ScoreResult.MemberNotFound();

        // Load with tracking — EF generates UPDATE if found, INSERT if new
        var existing = await db.ApplicationScores
            .FirstOrDefaultAsync(s => s.ApplicationId == applicationId && s.Dimension == dimension);

        if (existing is null)
        {
            existing = new ApplicationScore
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                Dimension = dimension
            };
            db.ApplicationScores.Add(existing);
        }

        existing.Score = score;
        existing.Comment = comment;
        existing.UpdatedByTeamMemberId = teamMemberId;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return ScoreResult.Success(new ScoreDto
        {
            Id = existing.Id,
            Dimension = existing.Dimension.ToString(),
            Score = existing.Score,
            Comment = existing.Comment,
            UpdatedByName = teamMember.Name,
            UpdatedAt = existing.UpdatedAt
        });
    }

    private static ApplicationSummaryDto ToSummaryDto(Application a) => new()
    {
        Id = a.Id,
        JobId = a.JobId,
        CandidateName = a.CandidateName,
        CandidateEmail = a.CandidateEmail,
        CurrentStage = a.CurrentStage.ToString(),
        CreatedAt = a.CreatedAt
    };
}
