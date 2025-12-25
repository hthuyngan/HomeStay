using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AmenitiesController : Controller
{
    private readonly HomestayContext _context;

    public AmenitiesController(HomestayContext context)
    {
        _context = context;
    }

    // GET: /Amenities
    public async Task<IActionResult> Index()
    {
        var amenities = await _context.Amenities
            .OrderBy(a => a.AmenityName)
            .ToListAsync();

        return View(amenities);
    }
}
