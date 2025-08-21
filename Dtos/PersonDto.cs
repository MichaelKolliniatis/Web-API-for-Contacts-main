namespace Web_API_for_Contacts_2._0.Dtos
{
    public class PersonDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? CountryId { get; set; }
        public int? ProfessionId { get; set; }
        public List<int> HobbyIds { get; set; } = [];

    }
}
