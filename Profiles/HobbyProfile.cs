using AutoMapper;
using Web_API_for_Contacts_2._0.Models;
using Web_API_for_Contacts_2._0.Dtos;

namespace Web_API_for_Contacts_2._0.Profiles
{
    public class HobbyProfile : Profile
    {
        public HobbyProfile()
        {

            CreateMap<CreateUpdateHobbyDto, Hobby>();
            CreateMap<Hobby, IdNameDto>();
            CreateMap<PersonHobby, IdNameDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.HobbyId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Hobby!.Name));

        }
    }
}