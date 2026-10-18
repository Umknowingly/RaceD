using FluentAssertions;
using Race.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static Race.DTOs.EventDtos;

namespace Race.Tests
{
    // This test class checks role-based access control for the Race API.
    public class RoleAccessTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        public RoleAccessTests(CustomWebApplicationFactory factory) => _factory = factory;

        private async Task<HttpClient> AuthedClient(string role)
        {
            var client = _factory.CreateClient();
            var email = $"{role}{Guid.NewGuid()}@test.com";
            await client.PostAsJsonAsync("/api/auth/register", new RegisterDto("U", email, "Password1", role, null));
            var login = await client.PostAsJsonAsync("/api/auth/login", new LoginDto(email, "Password1"));
            var body = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
            return client;
        }

        public async Task Participant_Cannot_CreateEvent_Returns403()
        {
            var client = await AuthedClient("Participant");
            var dto = new CreateEventDto("X", DateTime.UtcNow.AddDays(1), "Loc", "Desc", null, "run");
            var response = await client.PostAsJsonAsync("/api/events", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        public async Task Organiser_Can_CreateEvent_Returns201()
        {
            var client = await AuthedClient("Organiser");
            var dto = new CreateEventDto("City Run", DateTime.UtcNow.AddDays(5), "Cape Town", "5km fun run", "Flat", "run");
            var response = await client.PostAsJsonAsync("/api/events", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        public async Task Unauthenticated_Can_ListEvents()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/events");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public async Task Unauthenticated_Cannot_GetUser()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/users/any-id");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}