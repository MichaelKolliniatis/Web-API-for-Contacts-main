using AutoMapper;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Data.Repositories
{
    public interface IPersonRepository : IGenericRepository<Person>
    {
        Task<List<PersonDto>> GetFilteredProjectedAsync(int? countryId, int? professionId, int? hobbyId, AutoMapper.IConfigurationProvider mapperConfig, CancellationToken ct = default);

        Task<PersonDto?> GetProjectedByIdAsync(int id, AutoMapper.IConfigurationProvider mapperConfig, CancellationToken ct = default);

    }
}