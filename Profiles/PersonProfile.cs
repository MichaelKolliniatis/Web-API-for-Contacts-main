using AutoMapper;
using Web_API_for_Contacts_2._0.Models;
using Web_API_for_Contacts_2._0.Dtos;

namespace Web_API_for_Contacts_2._0.Profiles
{
    public class PersonProfile : Profile
    {
        public PersonProfile()
        {
            CreateMap<Person, PersonDto>()
            .ForMember(dest => dest.HobbyIds,
                opt => opt.MapFrom(src =>
                    src.PersonHobbies != null
                        ? src.PersonHobbies
                            .Where(ph => ph.Hobby != null)
                            .Select(ph => ph.HobbyId)
                            .ToList()
                    : new List<int>()));
        }
    }
}