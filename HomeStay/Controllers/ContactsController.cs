using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeStay.Controllers
{
    public class ContactsController : Controller
    {
        private readonly HomestayContext _context;

        public ContactsController(HomestayContext context)
        {
            _context = context;
        }

        // GET: /Contact
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Contact/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(ContactMessage model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin!";
                return RedirectToAction("Index");
            }

            model.CreatedDate = DateTime.Now;
            _context.ContactMessages.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Gửi liên hệ thành công! Chúng tôi sẽ phản hồi sớm.";
            return RedirectToAction("Index");
        }
    }
}
