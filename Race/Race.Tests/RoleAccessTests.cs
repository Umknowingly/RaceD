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
        // This factory is used to create HTTP clients for testing the API. 
        private readonly CustomWebApplicationFactory _factory;
        public RoleAccessTests(CustomWebApplicationFactory factory) => _factory = factory;

        // This method creates an authenticated HTTP client for a given role by registering and logging in a user.
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

        // This test checks that a user with the "Participant" role cannot create an event and receives a 403 Forbidden response. 
        public async Task Participant_Cannot_CreateEvent_Returns403()
        {
            var client = await AuthedClient("Participant");
            var dto = new CreateEventDto("X", DateTime.UtcNow.AddDays(1), "Loc", "Desc", null, "run");
            var response = await client.PostAsJsonAsync("/api/events", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // This test checks that a user with the "Organiser" role can successfully create an event and receives a 201 Created response.
        public async Task Organiser_Can_CreateEvent_Returns201()
        {
            var client = await AuthedClient("Organiser");
            var dto = new CreateEventDto("City Run", DateTime.UtcNow.AddDays(5), "Cape Town", "5km fun run", "Flat", "run");
            var response = await client.PostAsJsonAsync("/api/events", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // This test checks that an unauthenticated user can list events and receives a 200 OK response.
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