using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Race.DTOs;

namespace Race.Tests
{
    public class AuthTests : IClassFixture<CustomWebApplicationFactory>
    {
        // The HttpClient is used to send HTTP requests to the API endpoints for testing purposes.
        private readonly HttpClient _client;
        public AuthTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

        // This test method checks the registration endpoint with valid data. It creates a new RegisterDto object with valid user information,
        // sends a POST request to the /api/auth/register endpoint, and asserts that the response status code is 201 Created.
        public async Task Register_WithValidData_Returns201()
        {
            var dto = new RegisterDto("Test User", $"u{Guid.NewGuid()}@test.com", "Password1", "Participant", null);
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // This test method checks the registration endpoint with an invalid role. It creates a new RegisterDto object with an invalid role ("Admin"),
        public async Task Register_WithInvalidRole_Returns400()
        {
            var dto = new RegisterDto("Bad", $"b{Guid.NewGuid()}@test.com", "Password1", "Admin", null);
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        
        // This test method checks the login endpoint with a wrong password. It creates a new user, attempts to log in with an incorrect password,
        // and asserts that the response status code is 401 Unauthorized.
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