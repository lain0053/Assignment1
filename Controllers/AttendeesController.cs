using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.Controllers
{
    [Route("events/{eventId}/attendees")]
    public class AttendeesController : Controller
    {
        private readonly AppDbContext _context;

        public AttendeesController(AppDbContext context)
        {
            _context = context;
        }

        // GET /events/{eventId}/attendees
        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> Index(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null) return NotFound();

            ViewData["EventId"] = eventId;
            ViewData["EventTitle"] = ev.Title;
            return View(ev.Attendees);
        }

        // GET /events/{eventId}/attendees/create
        [Authorize(Roles = "Organizer")] //Organizer only 
        [HttpGet("create")]
        public async Task<IActionResult> Create(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound();

            ViewData["EventId"] = eventId;
            ViewData["EventTitle"] = ev.Title;
            return View();
        }

        // POST /events/{eventId}/attendees/create
        [Authorize(Roles = "Organizer")] //Organizer only 
        [HttpPost("create")]
        public async Task<IActionResult> Create(int eventId, Attendee attendee)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound();

            attendee.EventId = eventId;

            _context.Attendees.Add(attendee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { eventId });
        }

        // GET /events/{eventId}/attendees/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int eventId, string id)
        {
            var attendee = await _context.Attendees.FindAsync(id);
            if (attendee == null) return NotFound();

            ViewData["EventId"] = eventId;
            ViewData["EventTitle"] = (await _context.Events.FindAsync(eventId))?.Title;
            return View(attendee);
        }

        // POST /events/{eventId}/attendees/edit/{id}
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int eventId, string id, Attendee attendee)
        {
            if (id != attendee.Id) return BadRequest();

            attendee.EventId = eventId;
            _context.Attendees.Update(attendee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { eventId });
        }

        // GET /events/{eventId}/attendees/delete/{id}
        [Authorize(Roles = "Organizer")] //Organizer only 
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int eventId, string id)
        {
            var attendee = await _context.Attendees.FindAsync(id);
            if (attendee == null) return NotFound();

            ViewData["EventId"] = eventId;
            ViewData["EventTitle"] = (await _context.Events.FindAsync(eventId))?.Title;
            return View(attendee);
        }

        // POST /events/{eventId}/attendees/delete/{id}
        [Authorize(Roles = "Organizer")] //Organizer only 
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int eventId, string id)
        {
            var attendee = await _context.Attendees.FindAsync(id);
            if (attendee != null)
            {
                _context.Attendees.Remove(attendee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { eventId });
        }
    }
}