using Ats.Api.Dtos.Applications;
using Ats.Api.Enums;
using Ats.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ats.Api.Controllers;

[ApiController]
public class ApplicationsController(IApplicationsService applicationsService) : ControllerBase
{
    /// <summary>Submit a job application. Candidates cannot apply twice to the same job.</summary>
    [HttpPost("api/jobs/{jobId:guid}/applications")]
    [ProducesResponseType(typeof(ApplicationSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApplicationSummaryDto>> ApplyToJob(
        Guid jobId,
        [FromBody] ApplyToJobRequestDto dto)
    {
        if (!await applicationsService.JobExistsAsync(jobId))
            return NotFound("Job not found.");

        if (await applicationsService.ApplicationExistsAsync(jobId, dto.CandidateEmail))
            return Conflict("An application from this email already exists for this job.");

        var application = await applicationsService.CreateApplicationAsync(jobId, dto);
        return CreatedAtAction(nameof(GetApplicationProfile), new { id = application.Id }, application);
    }

    /// <summary>List all applications for a job. Filter by stage using ?stage=Screening.</summary>
    [HttpGet("api/jobs/{jobId:guid}/applications")]
    [ProducesResponseType(typeof(List<ApplicationSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ApplicationSummaryDto>>> GetApplicationsForJob(
        Guid jobId,
        [FromQuery] ApplicationStage? stage = null)
    {
        if (!await applicationsService.JobExistsAsync(jobId))
            return NotFound("Job not found.");

        var applications = await applicationsService.GetApplicationsForJobAsync(jobId, stage);
        return Ok(applications);
    }

    /// <summary>Move an application to a new stage. Requires X-Team-Member-Id header.</summary>
    [HttpPatch("api/applications/{id:guid}/stage")]
    [ProducesResponseType(typeof(ApplicationSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationSummaryDto>> MoveStage(
        Guid id,
        [FromBody] MoveStageRequestDto dto,
        [FromHeader(Name = "X-Team-Member-Id")] string? teamMemberIdHeader = null)
    {
        if (string.IsNullOrEmpty(teamMemberIdHeader) || !Guid.TryParse(teamMemberIdHeader, out var teamMemberId))
            return BadRequest("X-Team-Member-Id header is required and must be a valid GUID.");

        var result = await applicationsService.MoveApplicationStageAsync(
            id, dto.ToStage!.Value, teamMemberId, dto.Comment ?? string.Empty);

        if (result.ApplicationNotFound) return NotFound("Application not found.");
        if (result.TeamMemberNotFound) return NotFound("Team member not found.");
        if (result.TransitionError is not null) return BadRequest(result.TransitionError);

        return Ok(result.Application);
    }

    /// <summary>Add a note to an application. Requires X-Team-Member-Id header.</summary>
    [HttpPost("api/applications/{id:guid}/notes")]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NoteDto>> AddNote(
        Guid id,
        [FromBody] CreateNoteRequestDto dto,
        [FromHeader(Name = "X-Team-Member-Id")] string? teamMemberIdHeader = null)
    {
        if (string.IsNullOrEmpty(teamMemberIdHeader) || !Guid.TryParse(teamMemberIdHeader, out var teamMemberId))
            return BadRequest("X-Team-Member-Id header is required and must be a valid GUID.");

        var result = await applicationsService.AddNoteAsync(id, dto, teamMemberId);

        if (result.ApplicationNotFound) return NotFound("Application not found.");
        if (result.TeamMemberNotFound) return NotFound("Team member not found.");

        return CreatedAtAction(nameof(GetNotes), new { id }, result.Note);
    }

    /// <summary>Get all notes for an application, newest first.</summary>
    [HttpGet("api/applications/{id:guid}/notes")]
    [ProducesResponseType(typeof(List<NoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<NoteDto>>> GetNotes(Guid id)
    {
        if (!await applicationsService.ApplicationExistsByIdAsync(id))
            return NotFound("Application not found.");

        var notes = await applicationsService.GetNotesAsync(id);
        return Ok(notes);
    }

    /// <summary>
    /// Set or update a score for an application on a specific dimension.
    /// Valid dimensions: culture-fit, interview, assessment. Requires X-Team-Member-Id header.
    /// </summary>
    [HttpPut("api/applications/{id:guid}/scores/{dimension}")]
    [ProducesResponseType(typeof(ScoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScoreDto>> UpsertScore(
        Guid id,
        string dimension,
        [FromBody] UpsertScoreRequestDto dto,
        [FromHeader(Name = "X-Team-Member-Id")] string? teamMemberIdHeader = null)
    {
        if (string.IsNullOrEmpty(teamMemberIdHeader) || !Guid.TryParse(teamMemberIdHeader, out var teamMemberId))
            return BadRequest("X-Team-Member-Id header is required and must be a valid GUID.");

        var scoreDimension = ParseDimension(dimension);
        if (scoreDimension is null)
            return BadRequest($"Invalid dimension '{dimension}'. Valid values: culture-fit, interview, assessment.");

        var result = await applicationsService.UpsertScoreAsync(
            id, scoreDimension.Value, dto.Score!.Value, dto.Comment ?? string.Empty, teamMemberId);

        if (result.ApplicationNotFound) return NotFound("Application not found.");
        if (result.TeamMemberNotFound) return NotFound("Team member not found.");

        return Ok(result.Score);
    }

    private static ScoreDimension? ParseDimension(string dimension) =>
        dimension.ToLowerInvariant() switch
        {
            "culture-fit" => ScoreDimension.CultureFit,
            "interview"   => ScoreDimension.Interview,
            "assessment"  => ScoreDimension.Assessment,
            _             => null
        };

    /// <summary>Get the full profile of an application including notes, scores, and stage history.</summary>
    [HttpGet("api/applications/{id:guid}")]
    [ProducesResponseType(typeof(ApplicationProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationProfileDto>> GetApplicationProfile(Guid id)
    {
        var profile = await applicationsService.GetApplicationProfileAsync(id);
        if (profile is null) return NotFound();
        return Ok(profile);
    }
}
