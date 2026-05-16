using Ats.Api.Data;
using Ats.Api.Dtos;
using Ats.Api.Enums;
using Ats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ats.Api.Services;

public class JobsService(AppDbContext db) : IJobsService
{
    public async Task<JobResponseDto> CreateJobAsync(CreateJobRequestDto dto)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            Status = JobStatus.Open
        };

        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        return ToDto(job);
    }

    public async Task<PagedResult<JobResponseDto>> GetJobsAsync(int page, int pageSize, JobStatus? status)
    {
        var query = db.Jobs.AsNoTracking();

        if (status.HasValue)
            query = query.Where(j => j.Status == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(j => j.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobResponseDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Location = j.Location,
                Status = j.Status.ToString()
            })
            .ToListAsync();

        return new PagedResult<JobResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<JobResponseDto?> GetJobByIdAsync(Guid id)
    {
        return await db.Jobs
            .AsNoTracking()
            .Where(j => j.Id == id)
            .Select(j => new JobResponseDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Location = j.Location,
                Status = j.Status.ToString()
            })
            .FirstOrDefaultAsync();
    }

    private static JobResponseDto ToDto(Job job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Description = job.Description,
        Location = job.Location,
        Status = job.Status.ToString()
    };
}
