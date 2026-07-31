using Assignment1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Assignment1.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(AppDbContext context, IServiceProvider serviceProvider)
        {
            context.Database.EnsureCreated();

            // Seed roles
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Organizer", "Attendee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed users
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Organizer user
            if (await userManager.FindByEmailAsync("organizer@test.com") == null)
            {
                var organizer = new IdentityUser
                {
                    UserName = "organizer@test.com",
                    Email = "organizer@test.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(organizer, "Organizer123!");
                await userManager.AddToRoleAsync(organizer, "Organizer");
            }

            // Attendee user
            if (await userManager.FindByEmailAsync("attendee@test.com") == null)
            {
                var attendee = new IdentityUser
                {
                    UserName = "attendee@test.com",
                    Email = "attendee@test.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(attendee, "Attendee123!");
                await userManager.AddToRoleAsync(attendee, "Attendee");
            }

            // Seed events
            if (context.Events.Any())
                return;

            var events = new List<Event>
            {
                new Event { Title = "Career Fair",   Description = "Annual career fair.", Date = new DateTime(2026, 02, 01), Location = "Gym",        BannerUrl = "https://picsum.photos/800/400" },
                new Event { Title = "Tech Talk",     Description = "Latest in tech.",     Date = new DateTime(2026, 02, 08), Location = "Auditorium", BannerUrl = "https://picsum.photos/800/401" },
                new Event { Title = "Hack Night",    Description = "A night of coding.",  Date = new DateTime(2026, 02, 15), Location = "Library",    BannerUrl = "https://picsum.photos/800/402" }
            };

            context.Events.AddRange(events);
            context.SaveChanges();

            var attendees = new List<Attendee>
            {
                new Attendee { Name = "Alice Smith", Email = "alice@example.com", EventId = events[0].Id },
                new Attendee { Name = "Bob Jones",   Email = "bob@example.com",   EventId = events[0].Id },
                new Attendee { Name = "Carol White", Email = "carol@example.com", EventId = events[1].Id },
                new Attendee { Name = "Dan Brown",   Email = "dan@example.com",   EventId = events[1].Id },
                new Attendee { Name = "Eve Davis",   Email = "eve@example.com",   EventId = events[2].Id },
                new Attendee { Name = "Frank Lee",   Email = "frank@example.com", EventId = events[2].Id }
            };

            context.Attendees.AddRange(attendees);
            context.SaveChanges();
        }
    }
}
