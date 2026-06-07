namespace Assignment1.Models
{
    public class DashboardViewModels
    {
        public List<EventManager> Events { get; set; } = new List<EventManager>();

        public EventManager SelectedEvent { get; set; }
        
        public bool AttendeeRegistered { get; set; }
    }
}
