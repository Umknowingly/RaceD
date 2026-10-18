namespace Race.DTOs
{
    public class ResultDtos
    {
        public record CreateResultDto(TimeSpan FinishTime, int Position, string Status);
        public record ResultResponseDto(int Id, int EnrolmentId, TimeSpan FinishTime, int Position, string Status);
    }
}
