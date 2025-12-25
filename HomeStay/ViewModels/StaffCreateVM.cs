namespace HomeStay.ViewModels
{
    public class StaffCreateVM
    {
        // User
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        // Staff
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public DateTime HireDate { get; set; }
        public int Status { get; set; } // 1 / 0
    }
}
