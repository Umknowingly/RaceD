using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Race.DTOs;
using Race.Models;
using System.Security.Claims;
using static Race.DTOs.UserDtos;

namespace Race.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UsersController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(new UserSummaryDto(user.Id, user.Name, user.Email!, user.Role, user.ContactNumber));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentId != id) return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.ContactNumber = dto.ContactNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new UserSummaryDto(user.Id, user.Name, user.Email!, user.Role, user.ContactNumber));
        }
    }
}