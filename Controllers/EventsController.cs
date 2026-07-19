using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;

namespace Assignment1.Controllers
{
    [Route("events")]
    public class EventsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public EventsController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET /events
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.ToListAsync();
            return View(events);
        }

        //GET /events/details/5
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
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST /events/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(Event ev, IFormFile? bannerFile)
        {
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

                _context.Events.Add(ev);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(ev);
        }

        //GET /events/edit/5
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
    }
}
