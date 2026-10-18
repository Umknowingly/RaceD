using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Race.DTOs;

namespace Race.Tests
{
    public class AuthTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        public AuthTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

        public async Task Register_WithValidData_Returns201()
        {
            var dto = new RegisterDto("Test User", $"u{Guid.NewGuid()}@test.com", "Password1", "Participant", null);
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        public async Task Register_WithInvalidRole_Returns400()
        {
            var dto = new RegisterDto("Bad", $"b{Guid.NewGuid()}@test.com", "Password1", "Admin", null);
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        public async Task Login_WithWrongPassword_Returns401()
        {
            var email = $"l{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/auth/register",
                new RegisterDto("L", email, "Password1", "Participant", null));

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto(email, "WrongPassword"));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            var email = $"v{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/auth/register",
                new RegisterDto("V", email, "Password1", "Participant", null));

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto(email, "Password1"));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            body!.Token.Should().NotBeNullOrEmpty();
        }
    }
}