using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace HomeStay.Models
{
    public class FoodOrders
    {
        [Key]
        public int OrderId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        public int? TableNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryTime { get; set; }

        public OrderType OrderType { get; set; }

        [StringLength(255)]
        public string? DeliveryAddress { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<FoodOrderItem> OrderItems { get; set; } = new List<FoodOrderItem>();

        // Computed properties
        [NotMapped]
        public string StatusText => Status switch
        {
            OrderStatus.Pending => "Chờ xác nhận",
            OrderStatus.Confirmed => "Đã xác nhận",
            OrderStatus.Preparing => "Đang chuẩn bị",
            OrderStatus.Completed => "Hoàn thành",
            OrderStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };

        [NotMapped]
        public string OrderTypeText => OrderType switch
        {
            Models.OrderType.DineIn => "Tại chỗ",
            Models.OrderType.TakeAway => "Mang về",
            Models.OrderType.Delivery => "Giao hàng",
            _ => "Không xác định"
        };

        [NotMapped]
        public string PaymentStatusText => PaymentStatus switch
        {
            Models.PaymentStatus.Unpaid => "Chưa thanh toán",
            Models.PaymentStatus.Paid => "Đã thanh toán",
            _ => "Không xác định"
        };

        [NotMapped]
        public int TotalItems => OrderItems?.Sum(i => i.Quantity) ?? 0;
    }

    public class FoodOrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int MenuItemId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual FoodOrders? Order { get; set; }

        [ForeignKey("MenuItemId")]
        public virtual MenuItem? MenuItem { get; set; }
    }

    // Enums
    public enum OrderType
    {
        DineIn = 0,      // Tại chỗ
        TakeAway = 1,    // Mang về
        Delivery = 2     // Giao hàng
    }

    public enum OrderStatus
    {
        Pending = 0,     // Chờ xác nhận
        Confirmed = 1,   // Đã xác nhận
        Preparing = 2,   // Đang chuẩn bị
        Completed = 3,   // Hoàn thành
        Cancelled = 4    // Đã hủy
    }

    public enum PaymentStatus
    {
        Unpaid = 0,      // Chưa thanh toán
        Paid = 1         // Đã thanh toán
    }

    public enum PaymentMethod
    {
        Cash = 0,        // Tiền mặt
        BankTransfer = 1,// Chuyển khoản
        Card = 2         // Thẻ
    }

    // ViewModel cho giỏ hàng
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Image { get; set; }
        public string? Note { get; set; }

        public decimal Subtotal => Price * Quantity;
    }

    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức đặt món")]
        public OrderType OrderType { get; set; }

        // KHÔNG CÓ [Required] nữa
        public int? TableNumber { get; set; }

        // KHÔNG CÓ [Required] nữa
        [StringLength(255)]
        public string? DeliveryAddress { get; set; }

        public DateTime? DeliveryTime { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}