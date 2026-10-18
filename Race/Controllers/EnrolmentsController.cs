using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race.Data;
using Race.DTOs;
using Race.Models;
using System.Security.Claims;
using static Race.DTOs.EnrolmentDtos;

namespace Race.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EnrolmentsController(AppDbContext db) => _db = db;
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost("categories/{categoryId}/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol(int categoryId, [FromBody] CreateEnrolmentDto dto)
        {
            var cat = await _db.Categories.FindAsync(categoryId);
            if (cat == null) return NotFound(new { message = "Category not found." });

            var count = await _db.Enrolments.CountAsync(e => e.CategoryId == categoryId);
            if (count >= cat.MaxParticipants) return BadRequest(new { message = "Category is full." });

            var exists = await _db.Enrolments.AnyAsync(e => e.CategoryId == categoryId && e.ParticipantId == CurrentUserId);
            if (exists) return Conflict(new { message = "Already enrolled." });

            var en = new Enrolment
            {
                CategoryId = categoryId,
                EventId = cat.EventId,
                ParticipantId = CurrentUserId,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactNumber = dto.EmergencyContactNumber
            };
            _db.Enrolments.Add(en);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = en.Id },
                new EnrolmentResponseDto(en.Id, en.EventId, en.CategoryId, en.ParticipantId,
                    en.EmergencyContactName, en.EmergencyContactNumber, en.EnrolledAt));
        }

        [HttpGet("users/{userId}/enrolments")]
        [Authorize]
        public async Task<IActionResult> GetUserEnrolments(string userId)
        {
            if (CurrentUserId != userId && !User.IsInRole("Organiser")) return Forbid();

            var list = await _db.Enrolments.Where(e => e.ParticipantId == userId)
                .Select(e => new EnrolmentResponseDto(e.Id, e.EventId, e.CategoryId, e.ParticipantId,
                    e.EmergencyContactName, e.EmergencyContactNumber, e.EnrolledAt))
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("enrolments/{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var e = await _db.Enrolments.FindAsync(id);
            if (e == null) return NotFound();
            return Ok(new EnrolmentResponseDto(e.Id, e.EventId, e.CategoryId, e.ParticipantId,
                e.EmergencyContactName, e.EmergencyContactNumber, e.EnrolledAt));
        }

        [HttpDelete("enrolments/{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Cancel(int id)
        {
            var e = await _db.Enrolments.FindAsync(id);
            if (e == null) return NotFound();
            if (e.ParticipantId != CurrentUserId) return Forbid();

            _db.Enrolments.Remove(e);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}