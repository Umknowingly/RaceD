namespace Race.Models
{
    public class Category
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }

        public string Name { get; set; } = "";
        public double DistanceKm { get; set; }
        public int MaxParticipants { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
