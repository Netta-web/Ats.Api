using Ats.Api.Dtos;
using Ats.Api.Enums;

namespace Ats.Api.Services;

public interface IJobsService
{
    Task<JobResponseDto> CreateJobAsync(CreateJobRequestDto dto);
    Task<PagedResult<JobResponseDto>> GetJobsAsync(int page, int pageSize, JobStatus? status);
    Task<JobResponseDto?> GetJobByIdAsync(Guid id);
}
