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
       .Include(r => r.RoomsReviews)
           .ThenInclude(rr => rr.User)
       .FirstOrDefaultAsync(r => r.RoomId == id);


        if (room == null)
        {
            return NotFound();
        }

       
     
        return View(room);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int roomId, int rating, string comment)
    {
        // Kiểm tra user đã đăng nhập chưa (bạn cần implement authentication)
        var userId = GetCurrentUserId(); // Implement method này

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var review = new RoomsReview
        {
            RoomId = roomId,
            UserId = userId.Value,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.Now,
            IsActive = true
        };

        _context.RoomsReviews.Add(review);
        await _context.SaveChangesAsync();

        // Redirect về trang chi tiết phòng
        var room = await _context.Rooms.FindAsync(roomId);
        return RedirectToAction("Details", new { id = roomId, alias = room.Alias });
    }

 
    private int? GetCurrentUserId()
    {
        
        return HttpContext.Session.GetInt32("UserId");
    }
}
