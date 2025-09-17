using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessionController(ContactsDbContext context, IMapper mapper) : ControllerBase
    {

        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<IdNameDto>>> GetProfessions()
        {
            var professions = await _context.Professions
                .AsNoTracking()
                .ProjectTo<IdNameDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(professions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdNameDto>> GetProfessionById(int id)
        {
            var profession= await _context.Professions
                .AsNoTracking()
                .Where(p => p.Id == id)
                .ProjectTo<IdNameDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

            if (profession == null)
                return NotFound(new { message = $"There is no profession with id {id}" });

            return Ok(profession);
        }

        [HttpPost]
        public async Task<ActionResult> CreateProfession([FromBody] CreateUpdateProfessionDto input)
        {
            var existingProfession = await _context.Professions
                .AnyAsync(c => c.Name.ToLower() == input.Name.ToLower());

            if (existingProfession)
                return Conflict(new { message = $"{input.Name} already exists." });

            var newProfession = _mapper.Map<Profession>(input);
            _context.Professions.Add(newProfession);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<IdNameDto>(newProfession);

            return CreatedAtAction(nameof(GetProfessionById), new {id = newProfession.Id}, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProfession(int id, [FromBody] CreateUpdateProfessionDto input)
        {
            var profession = await _context.Professions.FindAsync(id);

            if (profession == null)
                return NotFound(new { message = $"There is no profession with id {id}" });

            var existingProfession = await _context.Professions
                .AnyAsync(p => p.Id != id && p.Name.ToLower() == input.Name.ToLower());

            if (existingProfession)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            profession.Name = input.Name;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Profession updated successfully with name '{input.Name}'" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfession(int id)
        {
            var profession = await _context.Professions.FindAsync(id);

            if (profession == null)
                return NotFound(new { message = $"No profession with Id {id}." });

            var personWithProfession = await _context.Persons
                .AnyAsync(p => p.ProfessionId == id);

            if (personWithProfession)
                return Conflict(new { message = $"Cannot delete the profession '{profession.Name}' because there is at least one person associated with it." });

            _context.Professions.Remove(profession);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
