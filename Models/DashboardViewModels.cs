namespace Assignment1.Models
{
    public class DashboardViewModels
    {
        public List<Event> Events { get; set; } = new List<Event>();

        public Event? SelectedEvent { get; set; }
        
        public bool AttendeeRegistered { get; set; }
    }
}
