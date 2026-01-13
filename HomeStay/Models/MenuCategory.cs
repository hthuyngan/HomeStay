using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace HomeStay.Models
{
    [Table("MenuCategories")]
    public class MenuCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }

        [StringLength(100)]
        [Display(Name = "Tên tiếng Anh")]
        public string CategoryNameEn { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int? DisplayOrder { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<MenuItem> MenuItems { get; set; }
    }

}