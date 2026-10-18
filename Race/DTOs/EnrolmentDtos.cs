namespace Race.DTOs
{
    public class EnrolmentDtos
    {
        public record CreateEnrolmentDto(string EmergencyContactName, string EmergencyContactNumber);
        public record EnrolmentResponseDto(int Id, int EventId, int CategoryId, string ParticipantId,
            string EmergencyContactName, string EmergencyContactNumber, DateTime EnrolledAt);
    }
}
