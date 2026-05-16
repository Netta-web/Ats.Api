using System.ComponentModel.DataAnnotations;

namespace Ats.Api.Dtos;

public class CreateJobRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;
}
