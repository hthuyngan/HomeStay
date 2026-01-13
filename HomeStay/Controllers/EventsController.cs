using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeStay.Controllers
{
    public class EventsController : Controller
    {
        private readonly HomestayContext _context;

        public EventsController(HomestayContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            return View(events);
        }

        [Route("/event/{id}.html")]
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id && e.IsActive);

            if (ev == null)
                return NotFound();

            return View(ev);
        }
    }
}
