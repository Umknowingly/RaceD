namespace Race.DTOs
{
    public class CategoryDtos
    {
        public record CreateCategoryDto(string Name, double DistanceKm, int MaxParticipants);
        public record CategoryResponseDto(int Id, int EventId, string Name, double DistanceKm, int MaxParticipants);
    }
}
