using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? RoomId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateOnly? CheckInDate { get; set; }

    public DateOnly? CheckOutDate { get; set; }

    public int? NumberOfGuests { get; set; }

    public string? SpecialRequest { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Room? Room { get; set; }
}
