using Assignment1.Models;

namespace Assignment1.Data
{
    public class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();
            // Check if there are any events in the database
            if (context.Events.Any())
            {
                return; // Database has been seeded
            }
            var events = new List<Event>
            {
                new Event
                {
                    Title = "Career Fair",
                    Description = "Annual career fair with top companies.",
                    Date = new DateTime(2026, 02, 01),
                    Location = "Gym",
                    BannerUrl = "https://media.istockphoto.com/id/1415792420/vector/3d-isometric-flat-vector-conceptual-illustration-of-job-fair.jpg?s=612x612&w=0&k=20&c=Le81EPJGJPMGqH6E35b-sMh73XMuT9EMFD62YkmXFvA="
                },
                new Event
                {
                    Title = "Tech Talk",
                    Description = "Latest trends in technology.",
                    Date = new DateTime(2026, 02, 08),
                    Location = "Auditorium",
                    BannerUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ26eIQlKuQUqYwlvs_-OxeWrKBh2rT5JNjv08tmPzMqg&s=10"
                },
                new Event
                {
                    Title = "Hack Night",
                    Description = "A night of coding and fun.",
                    Date = new DateTime(2026, 02, 15),
                    Location = "Library",
                    BannerUrl = "https://thumbs.dreamstime.com/b/cartoon-granny-hacker-night-front-laptop-cartoon-grandma-character-hacker-glasses-sits-front-laptop-night-277865794.jpg"
                }
            };
            context.Events.AddRange(events);
            context.SaveChanges();

            //Seed attendees using the generated event IDs
            var attendees = new List<Attendee>
            {
                new Attendee { Name = "Alice Smith", Email = "alice@example.com", EventId = events[0].Id },
                new Attendee { Name = "Bob Jones",   Email = "bob@example.com",   EventId = events[0].Id },
                new Attendee { Name = "David Lainez", Email = "david@example.com", EventId = events[1].Id },
                new Attendee { Name = "Gabriel Aguirre",   Email = "gabs@example.com",   EventId = events[1].Id },
                new Attendee { Name = "Gonzalo Genek",   Email = "gonza@example.com",   EventId = events[2].Id },
                new Attendee { Name = "Juan Carlos",   Email = "jaze@example.com", EventId = events[2].Id }
            };
            context.Attendees.AddRange(attendees);
            context.SaveChanges();
        }
    }
}
