using System.ComponentModel.DataAnnotations;

public class CreateUpdateDeleteHobbyDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name of the hobby is missing.")]
    [MaxLength(100, ErrorMessage = "Name cannot have more than 100 characters.")]
    public string Name { get; set; } = string.Empty;
}