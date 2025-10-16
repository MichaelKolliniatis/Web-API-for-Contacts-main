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
    public class ProfessionController(IGenericRepository<Profession> repo, IUnitOfWork uow, ContactsDbContext context, IMapper mapper) : ControllerBase
    {

        private readonly IGenericRepository<Profession> _repo = repo;
        private readonly IUnitOfWork _uow = uow;
        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<IdNameDto>>> GetProfessions(CancellationToken ct)
        {
            var dtos = await _repo.GetAllProjectedAsync<IdNameDto>(_mapper.ConfigurationProvider, ct: ct);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdNameDto>> GetProfessionById(int id, CancellationToken ct)
        {
            var dto = await _repo.GetByIdProjectedAsync<IdNameDto>(id, _mapper.ConfigurationProvider, ct);
            if (dto == null)
                return NotFound(new { message = $"There is no profession with id {id}" });

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> CreateProfession([FromBody] CreateUpdateProfessionDto input, CancellationToken ct)
        {
            var exists = await _repo.ExistsAsync(c => c.Name.ToLower() == input.Name.ToLower(), ct);

            if (exists)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            var newProfession = _mapper.Map<Profession>(input);

            await _repo.AddAsync(newProfession, ct);
            await _uow.SaveChangesAsync(ct);

            var dto = _mapper.Map<IdNameDto>(newProfession);

            return CreatedAtAction(nameof(GetProfessionById), new { id = newProfession.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProfession(int id, [FromBody] CreateUpdateProfessionDto input, CancellationToken ct)
        {
            var profession = await _repo.GetByIdAsync(id, asNoTracking: false, ct);

            if (profession is null)
                return NotFound(new { message = $"There is no profession with id {id}" });

            var exists = await _repo.ExistsAsync(c => c.Id != id && c.Name.ToLower() == input.Name.ToLower(), ct);

            if (exists)
                return Conflict(new { message = $"'{input.Name}' already exists." });

            profession.Name = input.Name;

            await _repo.UpdateAsync(profession, ct);
            await _uow.SaveChangesAsync(ct);

            return Ok(new { message = $"Profession updated successfully with name '{input.Name}'" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfession(int id, CancellationToken ct)
        {
            var exists = await _repo.ExistsAsync(c => c.Id == id, ct);
            if (!exists)
                return NotFound(new { message = $"No profession with Id {id}." });

            var personWithProfession = await _context.Persons
                .AnyAsync(p => p.ProfessionId == id, ct);
            if (personWithProfession)
                return Conflict(new { message = $"Cannot delete the profession with Id {id} because there is at least one person associated with it." });

            await _repo.DeleteByIdAsync(id, ct);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}