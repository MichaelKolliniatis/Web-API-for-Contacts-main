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
    public class HobbyController(IGenericRepository<Hobby> repo, IUnitOfWork uow, ContactsDbContext context, IMapper mapper) : ControllerBase
    {

        private readonly IGenericRepository<Hobby> _repo = repo;
        private readonly IUnitOfWork _uow = uow;
        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<IdNameDto>>> GetHobbies(CancellationToken ct)
        {
            var dtos = await _repo.GetAllProjectedAsync<IdNameDto>(_mapper.ConfigurationProvider, ct: ct);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdNameDto>> GetHobbyById(int id, CancellationToken ct)
        {
            var dto = await _repo.GetByIdProjectedAsync<IdNameDto>(id, _mapper.ConfigurationProvider, ct);
            if (dto == null)
                return NotFound(new { message = $"There is no hobby with id {id}" });

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> CreateHobby([FromBody] CreateUpdateHobbyDto input, CancellationToken ct)
        {
            var exists = await _repo.ExistsAsync(c => c.Name.ToLower() == input.Name.ToLower(), ct);

            if (exists)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            var newHobby = _mapper.Map<Hobby>(input);

            await _repo.AddAsync(newHobby, ct);
            await _uow.SaveChangesAsync(ct);

            var dto = _mapper.Map<IdNameDto>(newHobby);

            return CreatedAtAction(nameof(GetHobbyById), new { id = newHobby.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateHobby(int id, [FromBody] CreateUpdateHobbyDto input, CancellationToken ct)
        {
            var hobby = await _repo.GetByIdAsync(id, asNoTracking: false, ct);

            if (hobby is null)
                return NotFound(new { message = $"There is no hobby with id {id}" });

            var exists = await _repo.ExistsAsync(c => c.Id != id && c.Name.ToLower() == input.Name.ToLower(), ct);

            if (exists)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            hobby.Name = input.Name;

            await _repo.UpdateAsync(hobby, ct);
            await _uow.SaveChangesAsync(ct);

            return Ok(new { message = $"Hobby updated successfully with name '{input.Name}'" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHobby(int id, CancellationToken ct)
        {
            var hobby = await _repo.GetByIdAsync(id, asNoTracking: false, ct);
            if (hobby is null)
                return NotFound(new { message = $"No hobby with Id {id}." });

            var personWithHobby = await _context.PersonHobbies
                .AnyAsync(ph => ph.HobbyId == id, ct);
            if (personWithHobby)
                return Conflict(new { message = $"Cannot delete the hobby '{hobby.Name}' because there is at least one person associated with it." });

            await _repo.DeleteAsync(hobby, ct);
            await _uow.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}