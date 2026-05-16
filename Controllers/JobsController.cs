using Ats.Api.Dtos;
using Ats.Api.Enums;
using Ats.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ats.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController(IJobsService jobsService) : ControllerBase
{
    /// <summary>Creates a new job posting.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobResponseDto>> CreateJob([FromBody] CreateJobRequestDto dto)
    {
        var job = await jobsService.CreateJobAsync(dto);
        return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, job);
    }

    /// <summary>Returns a paginated list of jobs. Filter by status using ?status=Open or ?status=Closed.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<JobResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<JobResponseDto>>> GetJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] JobStatus? status = null)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var result = await jobsService.GetJobsAsync(page, pageSize, status);
        return Ok(result);
    }

    /// <summary>Returns a single job by its ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobResponseDto>> GetJobById(Guid id)
    {
        var job = await jobsService.GetJobByIdAsync(id);
        if (job is null) return NotFound();
        return Ok(job);
    }
}
