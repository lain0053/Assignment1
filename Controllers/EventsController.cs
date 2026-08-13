using Assignment1.Data;
using Assignment1.Models;
using Assignment1.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Assignment1.Controllers
{
    [Route("events")]
    [Authorize] //all actions will require login by default
    public class EventsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<EventHub> _hubContext;

        public EventsController(AppDbContext context, IConfiguration configuration, IHubContext<EventHub> hubContext)
        {
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        // GET /events
        [AllowAnonymous] //allow anonymous access to the index page
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.ToListAsync();
            return View(events);
        }

        //GET /events/details/5
        [AllowAnonymous] //allow anonymous access to the index page
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null)
            {
                return NotFound();
            }
            return View(ev);
        }

        // GET /events/create
        [Authorize(Roles = "Organizer")] //Organizer only 
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST /events/create
        [Authorize(Roles = "Organizer")] //Organizer only        
        [HttpPost("create")]
        public async Task<IActionResult> Create(Event ev, IFormFile? bannerFile)
        {
            if (ModelState.IsValid)
            {
                //Storing the organizer's user ID
                ev.OrganizerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (bannerFile != null && bannerFile.Length > 0)
                {
                    var connectionString = _configuration["ConnectionStrings:BlobStorage"];
                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient("banners");
                    await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                    var blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(bannerFile.FileName));

                    using var stream = bannerFile.OpenReadStream();
                    await blobClient.UploadAsync(stream, overwrite: true);
                    ev.BannerUrl = blobClient.Uri.ToString();
                }

                _context.Events.Add(ev);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(ev);
        }

        //GET /events/edit/5
        [Authorize(Roles = "Organizer")] //Organizer only 
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
            {
                return NotFound();
            }
            return View(ev);
        }

        //POST /events/edit/5
        [Authorize(Roles = "Organizer")] //Organizer only 
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int id, Event ev, IFormFile? bannerFile)
        {
            if (id != ev.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                if (bannerFile != null && bannerFile.Length > 0)
                {
                    var connectionString = _configuration["ConnectionStrings:BlobStorage"];
                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient("banners");
                    await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                    var blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(bannerFile.FileName));

                    using var stream = bannerFile.OpenReadStream();
                    await blobClient.UploadAsync(stream, overwrite: true);
                    ev.BannerUrl = blobClient.Uri.ToString();
                }

                _context.Events.Update(ev);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(ev);
        }

        //GET /events/delete/5
        [Authorize(Roles = "Organizer")] //Organizer only 
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
            {
                return NotFound();
            }
            return View(ev);
        }

        //POST /events/delete/5
        [Authorize(Roles = "Organizer")] //Organizer only 
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
            {
                return NotFound();
            }
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //POST /events/register/5
        [Authorize]
        [HttpPost("register/{id}")]
        public async Task<IActionResult> Register(int id)
        {
            var ev = await _context.Events.Include(e => e.Attendees).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null)
            {
                return NotFound();
            }
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);

            //Checking for duplicate registration
            var existingAttendee = await _context.Attendees.FirstOrDefaultAsync(a => a.EventId == id && a.UserId == userId);

            if (existingAttendee != null)
            {
                TempData["Error"] = "You are already registered for this event";
                return RedirectToAction("Details", new { id });
            }

            var attendee = new Attendee
            {
                Name = User.Identity?.Name,
                Email = email,
                EventId = id,
                UserId = userId
            };
            _context.Attendees.Add(attendee);
            await _context.SaveChangesAsync();

            // Reload attendees count after saving
            var attendeeCount = await _context.Attendees.CountAsync(a => a.EventId == id);

            // Broadcast to all users viewing this event
            await _hubContext.Clients.Group($"event-{id}")
                .SendAsync("AttendeeRegistered", attendee.Name, attendeeCount);

            // Send private notification to organizer
            if (!string.IsNullOrEmpty(ev.OrganizerUserId))
            {
                await _hubContext.Clients.User(ev.OrganizerUserId)
                    .SendAsync("OrganizerNotification", $"{email} just registered for your {ev.Title}.");
            }

            return RedirectToAction("Details", new { id });
        }

        //POST /events/unregister/5
        [Authorize]
        [HttpPost("unregister/{id}")]
        public async Task<IActionResult> Unregister(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var attendee = await _context.Attendees
                .FirstOrDefaultAsync(a => a.EventId == id && a.UserId == userId);

            if (attendee == null)
            {
                TempData["Error"] = "You are not registered for this event.";
                return RedirectToAction("Details", new { id });
            }

            _context.Attendees.Remove(attendee);
            await _context.SaveChangesAsync();

            var attendeeCount = await _context.Attendees.CountAsync(a => a.EventId == id);

            // Broadcast to all users viewing this event
            await _hubContext.Clients.Group($"event-{id}")
                .SendAsync("AttendeeUnregistered", attendee.Name, attendeeCount);

            return RedirectToAction("Details", new { id });
        }
    }
}
