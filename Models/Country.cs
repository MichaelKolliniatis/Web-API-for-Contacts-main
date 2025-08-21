using System.ComponentModel.DataAnnotations;

namespace Web_API_for_Contacts_2._0.Models
{
    public class Country
    {
        [Required(ErrorMessage = "Person id is required.")]
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name of the country is missing.")]
        [MaxLength(100, ErrorMessage = "Name cannot have more than 100 characters.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Name cannot contain numbers.")]
        public string Name { get; set; } = null!;

    }
}