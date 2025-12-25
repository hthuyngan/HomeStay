using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HomeStay.Models;
using HomeStay.ViewModels;

namespace HomeStay.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserProfilesController : Controller
    {
        private readonly HomestayContext _context;

        public UserProfilesController(HomestayContext context)
        {
            _context = context;
        }

        // GET: Admin/UserProfiles
        public async Task<IActionResult> Index()
        {
            var homestayContext = _context.UserProfiles.Include(u => u.User);
            return View(await homestayContext.ToListAsync());
        }

        // GET: Admin/UserProfiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.ProfileId == id);
            if (userProfile == null)
            {
                return NotFound();
            }

            return View(userProfile);
        }

        // GET: Admin/UserProfiles/Create
        public IActionResult Create()
        {
            ViewBag.UserId = new SelectList(
                _context.Users
                    .Where(u => !_context.UserProfiles.Any(p => p.UserId == u.UserId)),
                "UserId",
                "Username"
            );

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserProfileCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Check username tồn tại
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                return View(model);
            }

            // 2. Tạo USER
            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.Password,
                Role = 0, // USER
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); 

            // 3. Tạo USER PROFILE
            var profile = new UserProfile
            {
                UserId = user.UserId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                Gender = model.Gender,
                BirthDate = model.BirthDate
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/UserProfiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userProfile = await _context.UserProfiles.FindAsync(id);
            if (userProfile == null)
                return NotFound();

            return View(userProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfile model)
        {

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Field: {state.Key} - Error: {error.ErrorMessage}");
                    }
                }

                return View(model);
            }


            var userProfile = await _context.UserProfiles
                .FindAsync(model.ProfileId);

            if (userProfile == null)
            {
                return NotFound();
            }

            userProfile.FullName = model.FullName;
            userProfile.Email = model.Email;
            userProfile.Phone = model.Phone;
            userProfile.Address = model.Address;
            userProfile.Gender = model.Gender;
            userProfile.BirthDate = model.BirthDate;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // GET: Admin/UserProfiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.ProfileId == id);
            if (userProfile == null)
            {
                return NotFound();
            }

            return View(userProfile);
        }
        // POST: Admin/UserProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Lấy UserProfile kèm User
            var userProfile = await _context.UserProfiles
                .Include(u => u.User)
                .FirstOrDefaultAsync(x => x.ProfileId == id);

            if (userProfile == null)
                return NotFound();

            // Xóa UserProfile trước
            _context.UserProfiles.Remove(userProfile);

            // Xóa User sau
            if (userProfile.User != null)
            {
                _context.Users.Remove(userProfile.User);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
