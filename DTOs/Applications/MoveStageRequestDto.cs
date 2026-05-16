using System.ComponentModel.DataAnnotations;
using Ats.Api.Enums;

namespace Ats.Api.Dtos.Applications;

public class MoveStageRequestDto
{
    [Required]
    public ApplicationStage? ToStage { get; set; }

    public string? Comment { get; set; }
}
