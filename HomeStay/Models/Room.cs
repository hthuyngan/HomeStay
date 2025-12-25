using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string? RoomName { get; set; }

    public int? RoomTypeId { get; set; }

    public string? Image { get; set; }

    public string? Description { get; set; }

    public int Status { get; set; }

    public string? Alias { get; set; }

    public decimal? Price { get; set; }

    public int? Capacity { get; set; }

    public string? Size { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();

    public virtual RoomType? RoomType { get; set; }

    public virtual ICollection<RoomsReview> RoomsReviews { get; set; } = new List<RoomsReview>();
}
