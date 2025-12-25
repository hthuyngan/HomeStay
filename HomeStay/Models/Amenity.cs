using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class Amenity
{
    public int AmenityId { get; set; }

    public string? AmenityName { get; set; }

    public string? Icon { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }

    public string? OpenTime { get; set; }

    public string? Feature1 { get; set; }

    public string? Feature2 { get; set; }

    public string? Feature3 { get; set; }

    public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}
