using System;
using System.ComponentModel.DataAnnotations;

namespace HomeStay.ViewModels
{
    public class UserProfileCreateVM
    {
        [Required(ErrorMessage = "Tên đăng nhập bắt buộc")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu bắt buộc")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ tên bắt buộc")]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
        public int? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
    }
}
