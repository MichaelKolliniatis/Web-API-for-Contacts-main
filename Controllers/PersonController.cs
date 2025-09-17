using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Models;
using AutoMapper.QueryableExtensions;

namespace Web_API_for_Contacts_2._0.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PersonController(ContactsDbContext context, IMapper mapper) : ControllerBase
{

    private readonly ContactsDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<List<PersonDto>>> GetPersons(
        int? countryId,
        int? professionId,
        int? hobbyId)
    {
        var validationErrors = new List<string>();

        var query = _context.Persons.AsQueryable();

        if (countryId.HasValue)
            query = query.Where(p => p.CountryId == countryId.Value);

        if (professionId.HasValue)
            query = query.Where(p => p.ProfessionId == professionId.Value);

        if (hobbyId.HasValue)
            query = query.Where(p => p.PersonHobbies.Any(ph => ph.HobbyId == hobbyId.Value));

        var persons = await query
            .ProjectTo<PersonDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        if (!persons.Any())
            return NotFound(new { message = "No persons found matching the given criteria." });

        return Ok(persons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PersonDto>> GetPersonById(int id)
    {
        var personDto = await _context.Persons
            .Where(p => p.Id == id)
            .ProjectTo<PersonDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (personDto == null)
            return NotFound($"No person found with ID = {id}");

        return Ok(personDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePerson([FromBody] CreateUpdatePersonDto input)
    {
        var validationErrors = new List<string>();

        if (input.CountryId.HasValue)
        {
            var exists = await _context.Countries.AnyAsync(c => c.Id == input.CountryId.Value);
            if (!exists)
                validationErrors.Add($"No country found with ID = {input.CountryId}");
        }

        if (input.ProfessionId.HasValue)
        {
            var exists = await _context.Professions.AnyAsync(p => p.Id == input.ProfessionId.Value);
            if (!exists)
                validationErrors.Add($"No profession found with ID = {input.ProfessionId}");
        }

        if (input.HobbyIds != null && input.HobbyIds.Count > 0)
        {
            var hobbyIdSet = input.HobbyIds.ToHashSet();

            var existingHobbyIds = await _context.Hobbies
                .Select(h => h.Id)
                .ToListAsync();

            existingHobbyIds = existingHobbyIds.Where(id => hobbyIdSet.Contains(id)).ToList();

            var invalidHobbyIds = input.HobbyIds.Except(existingHobbyIds).ToList();
            if (invalidHobbyIds.Any())
                validationErrors.Add($"No hobbies found with IDs = {string.Join(", ", invalidHobbyIds)}");
        }

        if (validationErrors.Any())
            return NotFound(new { messages = validationErrors });

        var person = new Person
        {
            FirstName = input.FirstName,
            LastName = input.LastName,
            Email = input.Email,
            Phone = input.Phone,
            CountryId = input.CountryId,
            ProfessionId = input.ProfessionId,
            PersonHobbies = input.HobbyIds?
                .Select(hobbyId => new PersonHobby { HobbyId = hobbyId })
                .ToList() ?? new List<PersonHobby>()
        };

        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        var personDto = await _context.Persons
            .Where(p => p.Id == person.Id)
            .ProjectTo<PersonDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetPersonById), new { id = person.Id }, personDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePerson(int id, [FromBody] CreateUpdatePersonDto input)
    {
        var validationErrors = new List<string>();

        var person = await _context.Persons
            .Include(p => p.PersonHobbies)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
            return NotFound(new { message = $"No person found with ID = {id}" });

        if (input.CountryId.HasValue)
        {
            var exists = await _context.Countries.AnyAsync(c => c.Id == input.CountryId.Value);
            if (!exists)
                validationErrors.Add($"No country found with ID = {input.CountryId}");
        }

        if (input.ProfessionId.HasValue)
        {
            var exists = await _context.Professions.AnyAsync(p => p.Id == input.ProfessionId.Value);
            if (!exists)
                validationErrors.Add($"No profession found with ID = {input.ProfessionId}");
        }

        if (input.HobbyIds != null && input.HobbyIds.Count > 0)
        {
            var hobbyIdSet = input.HobbyIds.ToHashSet();

            var existingHobbyIds = await _context.Hobbies
                .Select(h => h.Id)
                .ToListAsync();

            existingHobbyIds = existingHobbyIds
                .Where(id => hobbyIdSet.Contains(id))
                .ToList();

            var invalidHobbyIds = input.HobbyIds.Except(existingHobbyIds).ToList();
            if (invalidHobbyIds.Any())
                validationErrors.Add($"No hobbies found with IDs = {string.Join(", ", invalidHobbyIds)}");
        }

        if (validationErrors.Any())
            return NotFound(new { messages = validationErrors });

        person.FirstName = input.FirstName;
        person.LastName = input.LastName;
        person.Email = input.Email;
        person.Phone = input.Phone;
        person.CountryId = input.CountryId;
        person.ProfessionId = input.ProfessionId;

        person.PersonHobbies.Clear();
        if (input.HobbyIds != null && input.HobbyIds.Count > 0)
        {
            person.PersonHobbies = input.HobbyIds
                .Select(hobbyId => new PersonHobby
                {
                    HobbyId = hobbyId,
                    PersonId = person.Id
                })
                .ToList();
        }

        await _context.SaveChangesAsync();

        var personDto = await _context.Persons
            .Where(p => p.Id == person.Id)
            .ProjectTo<PersonDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return Ok(personDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePerson(int id)
    {
        var person = await _context.Persons.FindAsync(id);

        if (person == null)
            return NotFound(new { message = $"No person found with Id {id}." });

        var deletedHobbies = await _context.PersonHobbies
            .Where(h => h.PersonId == id)
            .ExecuteDeleteAsync();

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();

        return NoContent();
    }

}