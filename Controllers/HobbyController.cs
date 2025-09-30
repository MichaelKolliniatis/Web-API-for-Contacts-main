using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Models;
using Web_API_for_Contacts_2._0.Dtos;
using AutoMapper.QueryableExtensions;

namespace Web_API_for_Contacts_2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HobbyController(ContactsDbContext context, IMapper mapper) : ControllerBase
    {

        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<IdNameDto>>> GetHobbies()
        {
            var hobbies = await _context.Hobbies
                .AsNoTracking()
                .ProjectTo<IdNameDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(hobbies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdNameDto>> GetHobbyById(int id)
        {
            var hobby = await _context.Hobbies
                .AsNoTracking()
                .Where(h => h.Id == id)
                .ProjectTo<IdNameDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

            if (hobby == null)
                return NotFound(new { message = $"There is no hobby with id {id}" });

            return Ok(hobby);
        }

        [HttpPost]
        public async Task<ActionResult> CreateHobby([FromBody] CreateUpdateHobbyDto input)
        {
            var existingHobby = await _context.Hobbies
                .AnyAsync(c => c.Name.ToLower() == input.Name.ToLower());

            if (existingHobby)
                return Conflict(new { message = $"{input.Name} already exists." });

            var newHobby = _mapper.Map<Hobby>(input);
            _context.Hobbies.Add(newHobby);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<IdNameDto>(newHobby);

            return CreatedAtAction(nameof(GetHobbyById), new {id = newHobby.Id}, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateHobby(int id, [FromBody] CreateUpdateHobbyDto input)
        {
            var hobby = await _context.Hobbies.FindAsync(id);

            if (hobby == null)
                return NotFound(new { message = $"There is no hobby with id {id}" });

            var existingHobby = await _context.Hobbies
                .AnyAsync(h => h.Id != id && h.Name.ToLower() == input.Name.ToLower());

            if (existingHobby)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            hobby.Name = input.Name;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Hobby updated successfully with name '{input.Name}'" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHobby(int id)
        {
            var hobby = await _context.Hobbies.FindAsync(id);

            if (hobby == null)
                return NotFound(new { message = $"No hobby with Id {id}." });

            var personWithHobby = await _context.PersonHobbies
                .AnyAsync(p => p.HobbyId == id);

            if (personWithHobby)
                return Conflict(new { message = $"Cannot delete the hobby '{hobby.Name}' because there is at least one person associated with it." });

            _context.Hobbies.Remove(hobby);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}