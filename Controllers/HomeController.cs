using Assignment1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Assignment1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private static List<Event> _events = new List<Event>
        {
            new Event {
                Id = 1,
                Title = "Career Fair",
                Date = new DateTime(2026, 02, 01),
                Location = "Gym",
            },

            new Event
            {
                Id = 2,
                Title = "Tech Talk",
                Date = new DateTime(2026, 02, 08),
                Location = "Auditorium"
            },

            new Event
            {
                Id = 3,
                Title = "Hack Night",
                Date = new DateTime(2026, 02, 15),
                Location = "Library"
            },

            new Event
            {
                Id = 4,
                Title = "Gonzalo Genek",
                Date = new DateTime(2026, 06, 21),
                Location = "Estadio Nacional"
            },

            new Event
            {
                Id = 5,
                Title = "Jaze",
                Date= new DateTime(2026, 10, 15),
                Location = "Costa 21"
            },

            new Event
            {
                Id = 6,
                Title = "Grupo 5",
                Date = new DateTime(2026, 07, 11),
                Location = "San Marcos"
            }
        };

        [HttpGet]
        public IActionResult EventManager()
        {
            ViewData["Title"] = "Event Manager";
            var model = new DashboardViewModels { Events = _events };
            return View(model);
        }

        [HttpPost]
        public IActionResult ManageAttendees(int id)
        {
            ViewData["Title"] = "Manage Attendees";
            var model = new DashboardViewModels
            {
                Events = _events,
                SelectedEvent = _events.FirstOrDefault(e => e.Id == id)
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult AddAttendee(string name, string email, int eventId)
        {
            ViewData["Title"] = "Manage Attendees";
            var selectedEvent = _events.FirstOrDefault(e => e.Id == eventId);
            selectedEvent?.Attendees.Add(new Attendee { Name = name, Email = email });

            var model = new DashboardViewModels
            {
                Events = _events,
                SelectedEvent = selectedEvent,
                AttendeeRegistered = true
            };

            return View("ManageAttendees", model);
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
