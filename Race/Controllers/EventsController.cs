using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race.Data;
using Race.DTOs;
using Race.Models;
using System.Security.Claims;
using static Race.DTOs.EventDtos;

namespace Race.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EventsController(AppDbContext db) => _db = db;
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> List([FromQuery] string? location, [FromQuery] DateTime? date)
        {
            var q = _db.Events.AsQueryable().Where(e => e.Date >= DateTime.UtcNow);
            if (!string.IsNullOrEmpty(location)) q = q.Where(e => e.Location.Contains(location));
            if (date.HasValue) q = q.Where(e => e.Date.Date == date.Value.Date);

            var events = await q.Select(e => new EventResponseDto(
                e.Id, e.Name, e.Date, e.Location, e.Description, e.RouteInfo, e.EventType, e.OrganiserId)).ToListAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            var e = await _db.Events.FindAsync(id);
            if (e == null) return NotFound();
            return Ok(new EventResponseDto(e.Id, e.Name, e.Date, e.Location, e.Description, e.RouteInfo, e.EventType, e.OrganiserId));
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            var ev = new Event
            {
                Name = dto.Name,
                Date = dto.Date,
                Location = dto.Location,
                Description = dto.Description,
                RouteInfo = dto.RouteInfo,
                EventType = dto.EventType,
                OrganiserId = CurrentUserId
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = ev.Id },
                new EventResponseDto(ev.Id, ev.Name, ev.Date, ev.Location, ev.Description, ev.RouteInfo, ev.EventType, ev.OrganiserId));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            ev.Name = dto.Name;
            ev.Date = dto.Date;
            ev.Location = dto.Location;
            ev.Description = dto.Description;
            ev.RouteInfo = dto.RouteInfo;
            ev.EventType = dto.EventType;
            await _db.SaveChangesAsync();

            return Ok(new EventResponseDto(ev.Id, ev.Name, ev.Date, ev.Location, ev.Description, ev.RouteInfo, ev.EventType, ev.OrganiserId));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}