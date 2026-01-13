using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeStay.Models
{
    public partial class Booking
    {
        [Key]
        public int BookingId { get; set; }

        public int? RoomId { get; set; }

        public int? UserId { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CheckInDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CheckOutDate { get; set; }

        public int? NumberOfGuests { get; set; }

        [StringLength(255)]
        public string? SpecialRequest { get; set; }

        public DateTime? CreatedDate { get; set; }

        // Navigation properties
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}