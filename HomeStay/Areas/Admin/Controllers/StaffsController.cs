using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HomeStay.Models;
using HomeStay.ViewModels;
using Microsoft.CodeAnalysis.Scripting;

namespace HomeStay.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StaffsController : Controller
    {
        private readonly HomestayContext _context;

        public StaffsController(HomestayContext context)
        {
            _context = context;
        }

        // GET: Admin/Staffs
        public async Task<IActionResult> Index()
        {
            var homestayContext = _context.Staff.Include(s => s.User);
            return View(await homestayContext.ToListAsync());
        }

        // GET: Admin/Staffs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Kiểm tra username tồn tại
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                return View(model);
            }

            // 2. Tạo User
            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.Password,

                Role = 1, // Staff
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // LẤY UserId

            // 3. Tạo Staff
            var staff = new Staff
            {
                UserId = user.UserId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Position = model.Position,
                Salary = model.Salary,
                HireDate = model.HireDate,
                Status = model.Status
            };

            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

      
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", staff.UserId);
            return View(staff);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Staff model)
        {
            if (id != model.StaffId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine(e.ErrorMessage);

                return View(model);
            }

            var staff = await _context.Staff.FindAsync(id);
            if (staff == null)
                return NotFound();

            // cập nhật từng field
            staff.FullName = model.FullName;
            staff.Email = model.Email;
            staff.Phone = model.Phone;
            staff.Position = model.Position;
            staff.Salary = model.Salary;
            staff.HireDate = model.HireDate;
            staff.Status = model.Status;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // POST: Admin/Staffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff != null)
            {
                _context.Staff.Remove(staff);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StaffExists(int id)
        {
            return _context.Staff.Any(e => e.StaffId == id);
        }
    }
}
