using AutoMapper;
using Web_API_for_Contacts_2._0.Models;
using Web_API_for_Contacts_2._0.Dtos;

namespace Web_API_for_Contacts_2._0.Profiles
{
    public class CountryProfile : Profile
    {
        public CountryProfile()
        {

        CreateMap<CreateUpdateDeleteCountryDto, Country>();
        
        }
    }
}
