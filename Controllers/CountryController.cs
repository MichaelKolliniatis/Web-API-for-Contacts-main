using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Data.Repositories;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController(IGenericRepository<Country> repo, IUnitOfWork uow, ContactsDbContext context, IMapper mapper) : ControllerBase
    {

        private readonly IGenericRepository<Country> _repo = repo;
        private readonly IUnitOfWork _uow = uow;
        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<IdNameDto>>> GetCountries(CancellationToken ct)
        {
            var dtos = await _repo.GetAllProjectedAsync<IdNameDto>(_mapper.ConfigurationProvider, ct: ct);
            return Ok(dtos);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<IdNameDto>> GetCountryById(int id, CancellationToken ct)
        {
            var dto = await _repo.GetByIdProjectedAsync<IdNameDto>(id, _mapper.ConfigurationProvider, ct);
            if (dto == null)
                return NotFound(new { message = $"There is no country with id {id}" });

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCountry([FromBody] CreateUpdateCountryDto input, CancellationToken ct)
        {
            var exists = await _repo.ExistsAsync(c => c.Name.ToLower() == input.Name.ToLower(), ct);

            if (exists)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            var newCountry = _mapper.Map<Country>(input);

            await _repo.AddAsync(newCountry, ct);
            await _uow.SaveChangesAsync(ct);

            var dto = _mapper.Map<IdNameDto>(newCountry);

            return CreatedAtAction(nameof(GetCountryById), new { id = newCountry.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCountry(int id, [FromBody] CreateUpdateCountryDto input, CancellationToken ct)
        {
            var country = await _repo.GetByIdAsync(id, asNoTracking: false, ct);

            if (country is null)
                return NotFound(new { message = $"There is no country with id {id}" });

            var exists = await _repo.ExistsAsync(c => c.Id != id && c.Name.ToLower() == input.Name.ToLower(), ct);

            if (exists)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            country.Name = input.Name;

            await _repo.UpdateAsync(country, ct);
            await _uow.SaveChangesAsync(ct);

            return Ok(new { message = $"Country updated successfully with name '{input.Name}'" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id, CancellationToken ct)
        {
            var exists = await _repo.ExistsAsync(c => c.Id == id, ct);
            if (!exists)
                return NotFound(new { message = $"No country with Id {id}." });

            var personWithCountry = await _context.Persons
                .AnyAsync(p => p.CountryId == id, ct);
            if (personWithCountry)
                return Conflict(new { message = $"Cannot delete the country with Id {id} because there is at least one person associated with it." });

            await _repo.DeleteByIdAsync(id, ct);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}