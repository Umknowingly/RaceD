using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race.Data;
using Race.DTOs;
using Race.Models;
using System.Security.Claims;
using static Race.DTOs.CategoryDtos;

namespace Race.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db) => _db = db;
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost("events/{eventId}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var cat = new Category
            {
                EventId = eventId,
                Name = dto.Name,
                DistanceKm = dto.DistanceKm,
                MaxParticipants = dto.MaxParticipants
            };
            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(List), new { eventId },
                new CategoryResponseDto(cat.Id, cat.EventId, cat.Name, cat.DistanceKm, cat.MaxParticipants));
        }

        [HttpGet("events/{eventId}/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> List(int eventId)
        {
            if (!await _db.Events.AnyAsync(e => e.Id == eventId)) return NotFound();
            var cats = await _db.Categories.Where(c => c.EventId == eventId)
                .Select(c => new CategoryResponseDto(c.Id, c.EventId, c.Name, c.DistanceKm, c.MaxParticipants))
                .ToListAsync();
            return Ok(cats);
        }

        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            if (cat.Event!.OrganiserId != CurrentUserId) return Forbid();

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}