using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Data.Repositories;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PersonController(
    IPersonRepository personRepo, 
    IGenericRepository<Country> countries, 
    IGenericRepository<Profession> professions, 
    IGenericRepository<Hobby> hobbies, 
    IGenericRepository<PersonHobby> personHobbies, 
    IUnitOfWork uow, 
    ContactsDbContext context, 
    IMapper mapper) : ControllerBase
{
    private readonly IPersonRepository _personRepo = personRepo;
    private readonly IGenericRepository<Country> _countries = countries;
    private readonly IGenericRepository<Profession> _professions = professions;
    private readonly IGenericRepository<Hobby> _hobbies = hobbies;
    private readonly IGenericRepository<PersonHobby> _personHobbies = personHobbies;
    private readonly IUnitOfWork _uow = uow;
    private readonly ContactsDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<List<PersonDto>>> GetPersons(int? countryId, int? professionId, int? hobbyId, CancellationToken ct)
    {
        var persons = await _personRepo.GetFilteredProjectedAsync(countryId, professionId, hobbyId, _mapper.ConfigurationProvider, ct);

        if (persons.Count == 0)
            return NotFound(new { message = "No persons found matching the given criteria." });

        return Ok(persons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PersonDto>> GetPersonById(int id, CancellationToken ct)
    {
        var personDto = await _personRepo.GetProjectedByIdAsync(
            id,
            _mapper.ConfigurationProvider,
            ct);

        if (personDto is null)
            return NotFound(new { message = $"No person found with ID = {id}" });

        return Ok(personDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePerson([FromBody] CreateUpdatePersonDto input, CancellationToken ct)
    {
        var validationErrors = new List<string>();

        if (input.CountryId.HasValue)
        {
            var countryExists = await _countries.ExistsAsync(c => c.Id == input.CountryId.Value, ct);
            if (!countryExists)
                validationErrors.Add($"No country found with ID = {input.CountryId}");
        }

        if (input.ProfessionId.HasValue)
        {
            var professionExists = await _professions.ExistsAsync(p => p.Id == input.ProfessionId.Value, ct);
            if (!professionExists)
                validationErrors.Add($"No profession found with ID = {input.ProfessionId}");
        }

        if (input.HobbyIds is { Count: > 0 })
        {
            var ids = input.HobbyIds;

            // 1 query: φέρνουμε ΟΛΑ τα Hobby Ids (lookup table = μικρός πίνακας)
            var allHobbyIds = await _hobbies.SelectWhereAsync(
                h => true,
                h => h.Id,
                asNoTracking: true,
                ct: ct);

            var invalidHobbyIds = ids.Except(allHobbyIds).ToList();
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
            PersonHobbies = input.HobbyIds?.Select(id => new PersonHobby { HobbyId = id }).ToList()
                            ?? new List<PersonHobby>()
        };

        await _personRepo.AddAsync(person, ct);
        await _uow.SaveChangesAsync(ct);

        var personDto = await _personRepo.GetProjectedByIdAsync(person.Id, _mapper.ConfigurationProvider, ct);

        return CreatedAtAction(nameof(GetPersonById), new { id = person.Id }, personDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePerson(int id, [FromBody] CreateUpdatePersonDto input, CancellationToken ct)
    {
        var validationErrors = new List<string>();

        var person = await _personRepo.GetByIdAsync(id, asNoTracking: false, ct);
        if (person is null)
            return NotFound(new { message = $"No person found with ID = {id}" });

        if (input.CountryId.HasValue)
        {
            var countryExists = await _countries.ExistsAsync(c => c.Id == input.CountryId.Value, ct);
            if (!countryExists)
                validationErrors.Add($"No country found with ID = {input.CountryId}");
        }

        if (input.ProfessionId.HasValue)
        {
            var professionExists = await _professions.ExistsAsync(p => p.Id == input.ProfessionId.Value, ct);
            if (!professionExists)
                validationErrors.Add($"No profession found with ID = {input.ProfessionId}");
        }

        if (input.HobbyIds is { Count: > 0 })
        {
            var ids = input.HobbyIds;

            var allHobbyIds = await _hobbies.SelectWhereAsync(
                h => true,
                h => h.Id,
                asNoTracking: true,
                ct: ct);

            var invalidHobbyIds = ids.Except(allHobbyIds).ToList();
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

        await _personHobbies.DeleteWhereAsync(ph => ph.PersonId == id, ct);

        if (input.HobbyIds is { Count: > 0 })
        {
            person.PersonHobbies = input.HobbyIds
                .Select(hobbyId => new PersonHobby
                {
                    PersonId = id,
                    HobbyId = hobbyId
                })
                .ToList();
        }
        else
        {
            person.PersonHobbies = new List<PersonHobby>();
        }

        await _personRepo.UpdateAsync(person, ct);
        await _uow.SaveChangesAsync(ct);

        var personDto = await _personRepo.GetProjectedByIdAsync(
            id,
            _mapper.ConfigurationProvider,
            ct);

        return Ok(personDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePerson(int id, CancellationToken ct)
    {
        var person = await _personRepo.GetByIdAsync(id, asNoTracking: false, ct);
        if (person is null)
            return NotFound(new { message = $"No person found with Id {id}." });

        await _personHobbies.DeleteWhereAsync(ph => ph.PersonId == id, ct);

        await _personRepo.DeleteAsync(person, ct);

        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }
}