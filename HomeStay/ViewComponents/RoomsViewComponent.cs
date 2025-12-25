using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeStay.ViewComponents
{
    public class RoomsViewComponent : ViewComponent
    {
        private readonly HomestayContext _context;

        public RoomsViewComponent(HomestayContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var rooms = await _context.Rooms
                .Include(r => r.RoomType)             
                .Where(r => r.Status == 1)  
                .OrderBy(r => r.RoomName)           
                .ToListAsync();

            return View(rooms);
        }
    }
}
