namespace Race.DTOs
{
    public record RegisterDto(string Name, string Email, string Password, string Role, string? ContactNumber);
    public record LoginDto(string Email, string Password);
    public record AuthResponseDto(string Token, UserSummaryDto User);
    public record UserSummaryDto(string Id, string Name, string Email, string Role, string? ContactNumber);
}