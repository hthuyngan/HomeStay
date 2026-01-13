using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeStay.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HomeStay.Controllers
{
    public class BookingController : Controller
    {
        private readonly HomestayContext _context;

        public BookingController(HomestayContext context)
        {
            _context = context;
        }

        // GET: /booking/{alias-roomId}
        [HttpGet]
        [Route("booking/{slug}")]
        public async Task<IActionResult> Index(string slug)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đặt phòng.";
                TempData["ReturnUrl"] = $"/booking/{slug}";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            // Tách roomId từ slug (format: alias-roomId)
            var parts = slug.Split('-');
            if (parts.Length < 2 || !int.TryParse(parts[^1], out int roomId))
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
            {
                return NotFound();
            }

            // Kiểm tra trạng thái phòng
            if (room.Status != 1) // Status = 1 là Available
            {
                TempData["Error"] = "Phòng này hiện không khả dụng để đặt.";
                return RedirectToAction("Details", "Rooms", new { slug });
            }

            // Lấy thông tin user để điền sẵn vào form
            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user?.UserProfile != null)
            {
                ViewBag.FullName = user.UserProfile.FullName;
                ViewBag.Email = user.UserProfile.Email;
                ViewBag.Phone = user.UserProfile.Phone;
            }
            else
            {
                ViewBag.FullName = "";
                ViewBag.Email = "";
                ViewBag.Phone = "";
            }

            return View(room);
        }

        // POST: Tạo booking mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking(int RoomID, string FullName, string Phone,
            string Email, DateTime CheckInDate, DateTime CheckOutDate, int NumberOfGuests, string SpecialRequest)
        {
            try
            {
                // Kiểm tra đăng nhập
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "Vui lòng đăng nhập để đặt phòng.";
                    return RedirectToAction("Login", "Account");
                }

                // Validate dates
                if (CheckInDate < DateTime.Now.Date)
                {
                    TempData["Error"] = "Ngày nhận phòng không được ở quá khứ.";
                    return RedirectToAction("Index", new { slug = await GetRoomSlug(RoomID) });
                }

                if (CheckOutDate <= CheckInDate)
                {
                    TempData["Error"] = "Ngày trả phòng phải sau ngày nhận phòng.";
                    return RedirectToAction("Index", new { slug = await GetRoomSlug(RoomID) });
                }

                // Kiểm tra phòng có tồn tại không
                var room = await _context.Rooms
                    .Include(r => r.RoomType)
                    .FirstOrDefaultAsync(r => r.RoomId == RoomID);

                if (room == null)
                {
                    TempData["Error"] = "Phòng không tồn tại.";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra số lượng khách
                if (NumberOfGuests > room.Capacity)
                {
                    TempData["Error"] = $"Số khách vượt quá sức chứa tối đa ({room.Capacity} khách).";
                    return RedirectToAction("Index", new { slug = await GetRoomSlug(RoomID) });
                }

                // Kiểm tra phòng có trống trong khoảng thời gian này không
                var isRoomAvailable = await CheckRoomAvailability(RoomID, CheckInDate, CheckOutDate);

                if (!isRoomAvailable)
                {
                    TempData["Error"] = "Phòng đã được đặt trong khoảng thời gian này. Vui lòng chọn ngày khác.";
                    return RedirectToAction("Index", new { slug = await GetRoomSlug(RoomID) });
                }

                // Tạo đối tượng Booking
                var booking = new Booking
                {
                    UserId = userId.Value, // Lưu UserId
                    RoomId = RoomID,
                    FullName = FullName,
                    Phone = Phone,
                    Email = Email,
                    CheckInDate = CheckInDate,
                    CheckOutDate = CheckOutDate,
                    NumberOfGuests = NumberOfGuests,
                    SpecialRequest = SpecialRequest,
                    CreatedDate = DateTime.Now
                };

                // Lưu booking
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Chuyển sang trang xác nhận
                TempData["Success"] = "Đặt phòng thành công!";
                return RedirectToAction("Confirmation", new { id = booking.BookingId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi đặt phòng. Vui lòng thử lại.";
                return RedirectToAction("Index", new { slug = await GetRoomSlug(RoomID) });
            }
        }

        // GET: Trang xác nhận đặt phòng
        [Route("booking/confirmation/{id}")]
        public async Task<IActionResult> Confirmation(int id)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem thông tin đặt phòng.";
                return RedirectToAction("Login", "Account");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đặt phòng.";
                return RedirectToAction("Index", "Home");
            }

            // Xóa TempData Success để tránh hiển thị lại khi refresh
            TempData.Remove("Success");

            // Tính tổng tiền
            if (booking.CheckInDate.HasValue && booking.CheckOutDate.HasValue &&
                booking.Room != null && booking.Room.Price.HasValue)
            {
                int nights = (booking.CheckOutDate.Value - booking.CheckInDate.Value).Days;
                ViewBag.Nights = nights;
                ViewBag.TotalAmount = nights * booking.Room.Price.Value;
            }
            else
            {
                ViewBag.Nights = 0;
                ViewBag.TotalAmount = 0;
            }

            return View(booking);
        }

        // Kiểm tra phòng có trống không
        private async Task<bool> CheckRoomAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var conflictingBookings = await _context.Bookings
                .Where(b => b.RoomId == roomId)
                .Where(b =>
                    (checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                    (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                    (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)
                )
                .AnyAsync();

            return !conflictingBookings;
        }

        // Lấy slug của phòng
        private async Task<string> GetRoomSlug(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            return room != null ? $"{room.Alias}-{room.RoomId}" : "";
        }

        // GET: Danh sách booking của user
        [Route("booking/my-bookings")]
        public async Task<IActionResult> MyBookings()
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem danh sách đặt phòng.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy bookings của user hiện tại theo UserId
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .Where(b => b.UserId == userId.Value) // Lọc theo UserId
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Chi tiết booking
        [Route("booking/detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem chi tiết đặt phòng.";
                return RedirectToAction("Login", "Account");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền xem (chỉ cho phép user xem booking của mình)
            if (booking.UserId != userId.Value)
            {
                TempData["Error"] = "Bạn không có quyền xem đặt phòng này.";
                return RedirectToAction("MyBookings");
            }

            // Tính số đêm và tổng tiền
            if (booking.CheckInDate.HasValue && booking.CheckOutDate.HasValue &&
                booking.Room != null && booking.Room.Price.HasValue)
            {
                int nights = (booking.CheckOutDate.Value - booking.CheckInDate.Value).Days;
                ViewBag.Nights = nights;
                ViewBag.TotalAmount = nights * booking.Room.Price.Value;
            }

            return View(booking);
        }

        // POST: Hủy booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("booking/cancel/{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy đặt phòng.";
                return RedirectToAction("MyBookings");
            }

            // Kiểm tra quyền hủy (chỉ cho phép user hủy booking của mình)
            if (booking.UserId != userId.Value)
            {
                TempData["Error"] = "Bạn không có quyền hủy đặt phòng này.";
                return RedirectToAction("MyBookings");
            }

            // Kiểm tra thời gian hủy (phải trước 24h)
            if (booking.CheckInDate.HasValue && booking.CheckInDate.Value.AddDays(-1) < DateTime.Now)
            {
                TempData["Error"] = "Không thể hủy phòng. Phải hủy trước 24 giờ nhận phòng.";
                return RedirectToAction("MyBookings");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hủy đặt phòng thành công!";
            return RedirectToAction("MyBookings");
        }

        // GET: Kiểm tra phòng trống (API cho AJAX)
        [HttpGet]
        [Route("booking/check-availability")]
        public async Task<IActionResult> CheckAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var isAvailable = await CheckRoomAvailability(roomId, checkIn, checkOut);

            return Json(new
            {
                available = isAvailable,
                message = isAvailable
                    ? "Phòng còn trống trong thời gian này."
                    : "Phòng đã được đặt trong thời gian này."
            });
        }
    }
}