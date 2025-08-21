using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController(ContactsDbContext context, IMapper mapper) : ControllerBase
    {

        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<Country>>> GetCountries()
        {
            return Ok(await _context.Countries.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountryById(int id)
        {
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
                return NotFound(new { message = $"There is no country with id {id}" });

            return Ok(country);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCountry([FromBody] CreateUpdateDeleteCountryDto input)
        {
            var existingCountry = await _context.Countries
                .AnyAsync(c => c.Name.ToLower() == input.Name.ToLower());

            if (existingCountry)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            var newCountry = _mapper.Map<Country>(input);

            _context.Countries.Add(newCountry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCountryById), new { id = newCountry.Id }, newCountry);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCountry(int id, [FromBody] CreateUpdateDeleteCountryDto input)
        {
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
                return NotFound(new { message = $"There is no country with id {id}" });

            var existingCountry = await _context.Countries
                .AnyAsync(c => c.Id != id && c.Name.ToLower() == input.Name.ToLower());

            if (existingCountry)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            country.Name = input.Name;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Country updated successfully with name '{input.Name}'" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country is null)
            {
                return NotFound(new { message = "" });
            }

            var personWithCountry = await _context.Persons
                .AnyAsync(p => p.CountryId == id);

            if (personWithCountry)
                return Conflict(new { message = $"Cannot delete the country '{country.Name}' because there is at least one person associated with it." });

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"'{country.Name}' deleted successfully." });
        }


    }
}