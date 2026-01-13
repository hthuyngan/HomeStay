using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HomeStay.Models
{
    [Table("MenuItems")]
    public class MenuItem
    {
        [Key]
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tên món ăn")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Giá (VNĐ)")]
        [Range(0, 999999999, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [StringLength(500)]
        [Display(Name = "Hình ảnh")]
        public string Image { get; set; }

        [Display(Name = "Món đặc biệt")]
        public bool IsSpecial { get; set; } = false;

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        [Display(Name = "Nhãn (Tags)")]
        public string Tags { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int? DisplayOrder { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("CategoryId")]
        public virtual MenuCategory Category { get; set; }

        // Helper properties
        [NotMapped]
        [Display(Name = "Danh sách nhãn")]
        public List<string> TagList
        {
            get
            {
                if (string.IsNullOrEmpty(Tags))
                    return new List<string>();

                return new List<string>(Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim()));
            }
        }

        [NotMapped]
        [Display(Name = "Giá hiển thị")]
        public string PriceFormatted => $"{Price:N0}đ";
    }
}