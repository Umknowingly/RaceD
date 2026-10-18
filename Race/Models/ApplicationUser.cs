using Microsoft.AspNetCore.Identity;

namespace Race.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = "";
        public string? ContactNumber { get; set; }
        public string Role { get; set; } = "Participant";

        public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
    

    
}
