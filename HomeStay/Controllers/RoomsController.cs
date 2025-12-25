using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class RoomsController : Controller
{
    private readonly HomestayContext _context;

    public RoomsController(HomestayContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _context.Rooms
            .Include(r => r.RoomType)
            .Where(r => r.Status == 1)
            .OrderBy(r => r.RoomName)
            .ToListAsync();

        return View(rooms);
    }


    [Route("/room/{alias}-{id}.html")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Rooms == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms
            .Include(r => r.RoomType)
            .Include(r => r.RoomAmenities)
                .ThenInclude(ra => ra.Amenity)
            .FirstOrDefaultAsync(r => r.RoomId == id);

        if (room == null)
        {
            return NotFound();
        }

       
     
        return View(room);
    }
}
