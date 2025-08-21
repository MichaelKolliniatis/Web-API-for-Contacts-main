using System.ComponentModel.DataAnnotations;

namespace Web_API_for_Contacts_2._0.Dtos
{
    public class DeletePersonDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required.")]
        [MaxLength(100, ErrorMessage = "First name can't be longer than 100 characters.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Name cannot contain numbers.")]
        public string FirstName { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is required.")]
        [MaxLength(100, ErrorMessage = "Last name can't be longer than 100 characters.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Name cannot contain numbers.")]
        public string LastName { get; set; } = null!;
    }
}