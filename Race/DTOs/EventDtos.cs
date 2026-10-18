namespace Race.DTOs
{
    public class EventDtos
    {
        public record CreateEventDto(string Name, DateTime Date, string Location, string Description, string? RouteInfo, string EventType);
        public record UpdateEventDto(string Name, DateTime Date, string Location, string Description, string? RouteInfo, string EventType);
        public record EventResponseDto(int Id, string Name, DateTime Date, string Location, string Description, string? RouteInfo, string EventType, string OrganiserId);
    }
}
