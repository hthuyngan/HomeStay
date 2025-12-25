using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeStay.ViewComponents
{
    public class EventsViewComponent : ViewComponent
    {
        private readonly HomestayContext _context;

        public EventsViewComponent(HomestayContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var events = await _context.Events
                .OrderByDescending(e => e.EventDate) 
                .Take(5)                           
                .ToListAsync();

            return View(events);
        }
    }
}
