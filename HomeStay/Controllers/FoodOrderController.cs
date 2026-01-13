using HomeStay.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HomeStay.Controllers
{
    public class FoodOrderController : Controller
    {
        private readonly HomestayContext _context;

        public FoodOrderController(HomestayContext context)
        {
            _context = context;
        }

        // GET: FoodOrder/Cart
        public IActionResult Cart()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: Thêm món vào giỏ
        [HttpPost]
        public IActionResult AddToCart(int menuItemId, int quantity = 1, string? note = null)
        {
            var menuItem = _context.MenuItems.Find(menuItemId);
            if (menuItem == null || !menuItem.IsActive)
            {
                return Json(new { success = false, message = "Món ăn không tồn tại" });
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                if (!string.IsNullOrEmpty(note))
                    existingItem.Note = note;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MenuItemId = menuItemId,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = quantity,
                    Image = menuItem.Image,
                    Note = note
                });
            }

            SaveCart(cart);

            return Json(new
            {
                success = true,
                message = "Đã thêm vào giỏ hàng",
                cartCount = cart.Sum(c => c.Quantity),
                cartTotal = cart.Sum(c => c.Subtotal)
            });
        }

        // POST: Cập nhật số lượng
        [HttpPost]
        public IActionResult UpdateCartItem(int menuItemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);

            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Remove(item);
                }
                SaveCart(cart);
            }

            return Json(new
            {
                success = true,
                cartCount = cart.Sum(c => c.Quantity),
                cartTotal = cart.Sum(c => c.Subtotal)
            });
        }

        // POST: Xóa món khỏi giỏ
        [HttpPost]
        public IActionResult RemoveFromCart(int menuItemId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return Json(new
            {
                success = true,
                message = "Đã xóa khỏi giỏ hàng",
                cartCount = cart.Sum(c => c.Quantity),
                cartTotal = cart.Sum(c => c.Subtotal)
            });
        }

        // GET: Xóa toàn bộ giỏ hàng
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Đã xóa toàn bộ giỏ hàng";
            return RedirectToAction("Cart");
        }

        // GET: FoodOrder/Checkout
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Restaurant");
            }

            var model = new CreateOrderViewModel
            {
                CartItems = cart,
                OrderType = OrderType.TakeAway // Mặc định là mang về
            };

            // Tự động điền thông tin nếu user đã đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var user = _context.Users
                    .Include(u => u.UserProfile)
                    .FirstOrDefault(u => u.UserId == userId);

                if (user?.UserProfile != null)
                {
                    model.CustomerName = user.UserProfile.FullName ?? "";
                    model.Phone = user.UserProfile.Phone ?? "";
                    model.Email = user.UserProfile.Email ?? "";
                }
            }

            return View(model);
        }

        // POST: FoodOrder/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CreateOrderViewModel model)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Restaurant");
            }

            // XÓA LỖI CŨ - Xóa validation errors cho các trường không liên quan
            ModelState.Remove("TableNumber");
            ModelState.Remove("DeliveryAddress");

            // VALIDATE ĐỘNG theo OrderType
            if (model.OrderType == OrderType.DineIn)
            {
                // Tại chỗ: BẮT BUỘC có số bàn
                if (!model.TableNumber.HasValue || model.TableNumber.Value < 1 || model.TableNumber.Value > 50)
                {
                    ModelState.AddModelError("TableNumber", "Vui lòng chọn số bàn từ 1 đến 50");
                }
                // Set null cho các trường không dùng
                model.DeliveryAddress = null;
            }
            else if (model.OrderType == OrderType.Delivery)
            {
                // Giao hàng: BẮT BUỘC có địa chỉ
                if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
                {
                    ModelState.AddModelError("DeliveryAddress", "Vui lòng nhập địa chỉ giao hàng");
                }
                // Set null cho các trường không dùng
                model.TableNumber = null;
            }
            else // TakeAway - Mang về
            {
                // Mang về: KHÔNG CẦN gì cả
                model.TableNumber = null;
                model.DeliveryAddress = null;
            }

            if (!ModelState.IsValid)
            {
                model.CartItems = cart;
                return View(model);
            }

            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                // Tính tổng tiền (bao gồm phí ship nếu có)
                var totalAmount = cart.Sum(c => c.Subtotal);

                // Thêm phí ship nếu là giao hàng
                if (model.OrderType == OrderType.Delivery)
                {
                    totalAmount += 20000; // Phí ship cố định 20k
                }

                // Tạo đơn hàng
                var order = new FoodOrders
                {
                    UserId = userId,
                    CustomerName = model.CustomerName,
                    Phone = model.Phone,
                    Email = model.Email,
                    TableNumber = model.TableNumber, // Null nếu không phải tại chỗ
                    OrderDate = DateTime.Now,
                    DeliveryTime = model.DeliveryTime,
                    OrderType = model.OrderType,
                    DeliveryAddress = model.DeliveryAddress, // Null nếu không phải giao hàng
                    Note = model.Note,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Unpaid,
                    PaymentMethod = model.PaymentMethod,
                    CreatedAt = DateTime.Now
                };

                _context.FoodOrders.Add(order);
                await _context.SaveChangesAsync();

                // Thêm chi tiết đơn hàng
                foreach (var cartItem in cart)
                {
                    var orderItem = new FoodOrderItem
                    {
                        OrderId = order.OrderId,
                        MenuItemId = cartItem.MenuItemId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Price,
                        Subtotal = cartItem.Subtotal,
                        Note = cartItem.Note
                    };
                    _context.FoodOrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();

                // Xóa giỏ hàng
                HttpContext.Session.Remove("Cart");

                TempData["Success"] = "Đặt món thành công!";
                return RedirectToAction("OrderConfirmation", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                model.CartItems = cart;
                return View(model);
            }
        }

        // GET: FoodOrder/OrderConfirmation/{id}
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.FoodOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: FoodOrder/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem đơn hàng";
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.FoodOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: FoodOrder/OrderDetail/{id}
        public async Task<IActionResult> OrderDetail(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var order = await _context.FoodOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                        .ThenInclude(mi => mi!.Category)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền xem
            if (userId.HasValue && order.UserId != userId.Value)
            {
                TempData["Error"] = "Bạn không có quyền xem đơn hàng này";
                return RedirectToAction("MyOrders");
            }

            return View(order);
        }

        // POST: Hủy đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var order = await _context.FoodOrders.FindAsync(id);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Kiểm tra quyền
            if (userId.HasValue && order.UserId != userId.Value)
            {
                return Json(new { success = false, message = "Bạn không có quyền hủy đơn hàng này" });
            }

            // Chỉ được hủy đơn đang chờ
            if (order.Status != OrderStatus.Pending)
            {
                return Json(new { success = false, message = "Không thể hủy đơn hàng đã được xác nhận" });
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hủy đơn hàng" });
        }

        // Helper methods
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        // GET: Cart count
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCart();
            return Json(new
            {
                count = cart.Sum(c => c.Quantity),
                total = cart.Sum(c => c.Subtotal)
            });
        }
    }
}
