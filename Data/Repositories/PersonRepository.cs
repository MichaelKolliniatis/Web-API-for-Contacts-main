using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Data.Repositories
{
    public class PersonRepository : GenericRepository<Person>, IPersonRepository
    {
        private readonly ContactsDbContext _ctx;

        public PersonRepository(ContactsDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<List<PersonDto>> GetFilteredProjectedAsync(
            int? countryId,
            int? professionId,
            int? hobbyId,
            AutoMapper.IConfigurationProvider mapperConfig,
            CancellationToken ct = default)
        {
            IQueryable<Person> query = _ctx.Persons.AsNoTracking();

            if (countryId.HasValue)
                query = query.Where(p => p.CountryId == countryId.Value);

            if (professionId.HasValue)
                query = query.Where(p => p.ProfessionId == professionId.Value);

            if (hobbyId.HasValue)
                query = query.Where(p => p.PersonHobbies.Any(ph => ph.HobbyId == hobbyId.Value));

            return await query
                .ProjectTo<PersonDto>(mapperConfig)
                .ToListAsync(ct);
        }

        public async Task<PersonDto?> GetProjectedByIdAsync(
            int id,
            AutoMapper.IConfigurationProvider mapperConfig,
            CancellationToken ct = default)
        {
            return await _ctx.Persons
                .AsNoTracking()
                .Where(p => p.Id == id)
                .ProjectTo<PersonDto>(mapperConfig)
                .FirstOrDefaultAsync(ct);
        }

    }
}