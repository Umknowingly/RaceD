using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Race.DTOs;
using Race.Models;
using Race.Services;

namespace Race.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto.Role != "Organiser" && dto.Role != "Participant")
                return BadRequest(new { message = "Role must be Organiser or Participant." });

            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null) return Conflict(new { message = "Email already registered." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                Role = dto.Role,
                ContactNumber = dto.ContactNumber
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, dto.Role);

            return StatusCode(201,
                new UserSummaryDto(user.Id, user.Name, user.Email!, user.Role, user.ContactNumber));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized(new { message = "Invalid credentials." });

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid) return Unauthorized(new { message = "Invalid credentials." });

            var token = _tokenService.CreateToken(user);
            return Ok(new AuthResponseDto(token,
                new UserSummaryDto(user.Id, user.Name, user.Email!, user.Role, user.ContactNumber)));
        }
    }
}