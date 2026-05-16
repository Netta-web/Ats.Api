using System.ComponentModel.DataAnnotations;

namespace Ats.Api.Dtos.Applications;

public class ApplyToJobRequestDto
{
    [Required]
    [MaxLength(200)]
    public string CandidateName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string CandidateEmail { get; set; } = string.Empty;

    public string? CoverLetter { get; set; }
}
