namespace Race.DTOs
{
    public class UserDtos
    {
        public record UpdateUserDto(string Name, string Email, string? ContactNumber);
    }
}
