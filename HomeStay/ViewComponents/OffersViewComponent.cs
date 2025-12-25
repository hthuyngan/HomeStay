using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeStay.ViewComponents
{
    public class OffersViewComponent : ViewComponent
    {
        private readonly HomestayContext _context;

        public OffersViewComponent(HomestayContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            var offers = await _context.Offers
                .Where(o =>
                    o.StartDate.HasValue &&
                    o.EndDate.HasValue &&
                    o.StartDate.Value <= today &&
                    o.EndDate.Value >= today
                )
                .OrderByDescending(o => o.DiscountPercent)
                .ToListAsync();

            return View(offers);
        }
    }
}
