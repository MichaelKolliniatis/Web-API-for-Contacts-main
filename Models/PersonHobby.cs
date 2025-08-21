namespace Web_API_for_Contacts_2._0.Models
{
    public class PersonHobby
    {
        public int PersonId { get; set; }
        public Person? Person { get; set; }
        public int HobbyId { get; set; }
        public Hobby? Hobby { get; set; }

    }
}
