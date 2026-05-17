using System.ComponentModel.DataAnnotations;

namespace Ats.Api.Dtos.Applications;

public class UpsertScoreRequestDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Score must be between 1 and 5.")]
    public int? Score { get; set; }

    public string? Comment { get; set; }
}
