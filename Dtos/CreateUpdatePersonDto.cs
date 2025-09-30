using System.ComponentModel.DataAnnotations;

namespace Web_API_for_Contacts_2._0.Dtos
{
    public class CreateUpdatePersonDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required.")]
        [MaxLength(100, ErrorMessage = "First name can't be longer than 100 characters.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Name cannot contain numbers.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is required.")]
        [MaxLength(100, ErrorMessage = "Last name can't be longer than 100 characters.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Name cannot contain numbers.")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? Phone { get; set; }

        public int? CountryId { get; set; }

        public int? ProfessionId { get; set; }

        public List<int> HobbyIds { get; set; } = new();
    }
}