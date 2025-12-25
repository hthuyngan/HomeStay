using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeStay.ViewComponents
{
    public class MenuTopViewComponent : ViewComponent
    {
        private readonly HomestayContext _context;

        public MenuTopViewComponent(HomestayContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _context.Menus
                .Where(m => m.IsActive == true)       
                .OrderBy(m => m.DisplayOrder)          
                .ThenBy(m => m.Title)                  
                .ToListAsync();

            return View(items);
        }

    }
}
