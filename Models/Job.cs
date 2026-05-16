using Ats.Api.Enums;

namespace Ats.Api.Models;

public class Job
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Open;

    public ICollection<Application> Applications { get; set; } = [];
}
