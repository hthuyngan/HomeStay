using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeStay.ViewComponents
{
    public class AmenitiesViewComponent : ViewComponent
    {
        private readonly HomestayContext _context;

        public AmenitiesViewComponent(HomestayContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var amenities = await _context.Amenities
                .OrderBy(a => a.AmenityName)
                .ToListAsync();

            return View(amenities);
        }
    }
}
