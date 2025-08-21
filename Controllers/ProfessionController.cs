using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API_for_Contacts_2._0.Data;
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
        public async Task<ActionResult<List<Profession>>> GetProfessions()
        {
            return Ok(await _context.Professions.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Profession>> GetProfessionById(int id)
        {
            var profession = await _context.Professions.FindAsync(id);

            if (profession == null)
                return NotFound(new { message = $"There is no profession with id {id}" });

            return Ok(profession);
        }

        [HttpPost]
        public async Task<ActionResult> CreateProfession([FromBody] CreateUpdateDeleteProfessionDto input)
        {
            var existingProfession = await _context.Professions
                .AnyAsync(c => c.Name.ToLower() == input.Name.ToLower());

            if (existingProfession)
                return Conflict(new { message = $"{input.Name} already exists." });

            var newProfession = _mapper.Map<Profession>(input);

            _context.Professions.Add(newProfession);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfessionById), new {id = newProfession.Id}, newProfession);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProfession(int id, [FromBody] CreateUpdateDeleteProfessionDto input)
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
        public async Task<IActionResult> DeleteProfession(int id, [FromBody] CreateUpdateDeleteProfessionDto input)
        {
            var profession = await _context.Professions.FindAsync(id);

            if (profession == null || profession.Name != input.Name)
                return NotFound(new { message = $"No profession with Id {id} and Name '{input.Name}'." });

            var personWithProfession = await _context.Persons
                .AnyAsync(p => p.ProfessionId == id);

            if (personWithProfession)
                return Conflict(new { message = $"Cannot delete the profession '{input.Name}' because there is at least one person associated with it." });

            _context.Professions.Remove(profession);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{profession.Name} deleted successfully." });
        }
    }
}
