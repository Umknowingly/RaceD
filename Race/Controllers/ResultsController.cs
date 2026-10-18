using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race.Data;
using Race.DTOs;
using Race.Models;
using System.Security.Claims;
using static Race.DTOs.ResultDtos;

namespace Race.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ResultsController(AppDbContext db) => _db = db;
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost("enrolments/{enrolmentId}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Capture(int enrolmentId, [FromBody] CreateResultDto dto)
        {
            var en = await _db.Enrolments.Include(e => e.Event).Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.Id == enrolmentId);
            if (en == null) return NotFound(new { message = "Enrolment not found." });
            if (en.Event!.OrganiserId != CurrentUserId) return Forbid();
            if (en.Result != null) return Conflict(new { message = "Result already captured." });

            var r = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                Status = dto.Status
            };
            _db.Results.Add(r);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = r.Id },
                new ResultResponseDto(r.Id, r.EnrolmentId, r.FinishTime, r.Position, r.Status));
        }

        [HttpGet("events/{eventId}/results")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var results = await _db.Results
                .Where(r => r.Enrolment!.EventId == eventId)
                .OrderBy(r => r.Position)
                .Select(r => new ResultResponseDto(r.Id, r.EnrolmentId, r.FinishTime, r.Position, r.Status))
                .ToListAsync();
            return Ok(results);
        }

        [HttpGet("users/{userId}/results")]
        [Authorize]
        public async Task<IActionResult> GetUserResults(string userId)
        {
            if (CurrentUserId != userId && !User.IsInRole("Organiser")) return Forbid();

            var results = await _db.Results
                .Where(r => r.Enrolment!.ParticipantId == userId)
                .Select(r => new ResultResponseDto(r.Id, r.EnrolmentId, r.FinishTime, r.Position, r.Status))
                .ToListAsync();
            return Ok(results);
        }

        [HttpGet("results/{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var r = await _db.Results.FindAsync(id);
            if (r == null) return NotFound();
            return Ok(new ResultResponseDto(r.Id, r.EnrolmentId, r.FinishTime, r.Position, r.Status));
        }
    }
}