namespace Race.Models
{
    public class Enrolment
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

        public string ParticipantId { get; set; } = "";
        public ApplicationUser? Participant { get; set; }

        public string EmergencyContactName { get; set; } = "";
        public string EmergencyContactNumber { get; set; } = "";
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public Result? Result { get; set; }
    }
}
