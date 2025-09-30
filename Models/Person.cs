using System.Diagnostics.Metrics;

namespace Web_API_for_Contacts_2._0.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? CountryId { get; set; }
        public int? ProfessionId { get; set; }
        public Country? Country { get; set; }
        public Profession? Profession { get; set; }
        public virtual ICollection<PersonHobby> PersonHobbies { get; set; } = new List<PersonHobby>();

    }
}