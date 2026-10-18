using FluentAssertions;
using Race.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static Race.DTOs.CategoryDtos;
using static Race.DTOs.EnrolmentDtos;
using static Race.DTOs.EventDtos;

namespace Race.Tests
{
    // this inherits from IClassFixture<CustomWebApplicationFactory> to share the same factory instance across tests
    public class EnrolmentTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        public EnrolmentTests(CustomWebApplicationFactory factory) => _factory = factory;

        // helper method to create an authenticated HttpClient for a given role
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

    
        public async Task Participant_Can_Enrol_In_Category()
        {
            var organiser = await AuthedClient("Organiser");
            var evResp = await organiser.PostAsJsonAsync("/api/events",
                new CreateEventDto("E", DateTime.UtcNow.AddDays(3), "L", "D", null, "run"));
            var ev = await evResp.Content.ReadFromJsonAsync<EventResponseDto>();

            var catResp = await organiser.PostAsJsonAsync($"/api/events/{ev!.Id}/categories",
                new CreateCategoryDto("10km", 10, 50));
            var cat = await catResp.Content.ReadFromJsonAsync<CategoryResponseDto>();

            var participant = await AuthedClient("Participant");
            var enrolResp = await participant.PostAsJsonAsync($"/api/categories/{cat!.Id}/enrolments",
                new CreateEnrolmentDto("Mom", "0821234567"));

            enrolResp.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        
        public async Task Duplicate_Enrolment_Returns409()
        {
            var organiser = await AuthedClient("Organiser");
            var evResp = await organiser.PostAsJsonAsync("/api/events",
                new CreateEventDto("E2", DateTime.UtcNow.AddDays(3), "L", "D", null, "run"));
            var ev = await evResp.Content.ReadFromJsonAsync<EventResponseDto>();

            var catResp = await organiser.PostAsJsonAsync($"/api/events/{ev!.Id}/categories",
                new CreateCategoryDto("5km", 5, 50));
            var cat = await catResp.Content.ReadFromJsonAsync<CategoryResponseDto>();

            var participant = await AuthedClient("Participant");
            var dto = new CreateEnrolmentDto("Mom", "082");
            await participant.PostAsJsonAsync($"/api/categories/{cat!.Id}/enrolments", dto);
            var second = await participant.PostAsJsonAsync($"/api/categories/{cat.Id}/enrolments", dto);

            second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }

    public interface IClassFixture<T>
    {
    }
}