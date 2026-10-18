namespace Race.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public string Location { get; set; } = "";
        public string Description { get; set; } = "";
        public string? RouteInfo { get; set; }
        public string EventType { get; set; } = "run";

        public string OrganiserId { get; set; } = "";
        public ApplicationUser? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
