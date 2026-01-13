using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HomeStay.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly HomestayContext _context;

        public RestaurantController(HomestayContext context)
        {
            _context = context;
        }

        // GET: Restaurant
        public async Task<IActionResult> Index()
        {
            var categories = await _context.MenuCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Include(c => c.MenuItems.Where(m => m.IsActive))
                .ToListAsync();

            // Lấy các món đặc biệt
            var specialItems = await _context.MenuItems
                .Where(m => m.IsSpecial && m.IsActive)
                .Include(m => m.Category)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();

            ViewBag.SpecialItems = specialItems;

            return View(categories);
        }

        // GET: Restaurant/Menu (Trang menu chi tiết)
        public async Task<IActionResult> Menu(int? categoryId)
        {
            var query = _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
            }

            var menuItems = await query
                .OrderBy(m => m.Category.DisplayOrder)
                .ThenBy(m => m.DisplayOrder)
                .ToListAsync();

            ViewBag.Categories = await _context.MenuCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            ViewBag.SelectedCategoryId = categoryId;

            return View(menuItems);
        }


        // API: Lấy menu items theo category (cho filtering)
        [HttpGet]
        public async Task<IActionResult> GetMenuByCategory(int categoryId)
        {
            var menuItems = await _context.MenuItems
                .Where(m => m.CategoryId == categoryId && m.IsActive)
                .Include(m => m.Category)
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new
                {
                    m.MenuItemId,
                    m.Name,
                    m.Description,
                    m.Price,
                    m.Image,
                    m.Tags,
                    m.IsSpecial,
                    CategoryName = m.Category.CategoryName
                })
                .ToListAsync();

            return Json(menuItems);
        }
    }
}