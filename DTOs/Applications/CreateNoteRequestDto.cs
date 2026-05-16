using System.ComponentModel.DataAnnotations;
using Ats.Api.Enums;

namespace Ats.Api.Dtos.Applications;

public class CreateNoteRequestDto
{
    [Required]
    public NoteType? Type { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
}
