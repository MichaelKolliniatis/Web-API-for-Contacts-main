using AutoMapper;
using Web_API_for_Contacts_2._0.Models;
using Web_API_for_Contacts_2._0.Dtos;

namespace Web_API_for_Contacts_2._0.Profiles
{
    public class ProfessionProfile : Profile
    {
        public ProfessionProfile()
        {

            CreateMap<CreateUpdateProfessionDto, Profession>();
            CreateMap<Profession, IdNameDto>();

        }
    }
}
