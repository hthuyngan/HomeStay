using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class RoomAmenity
{
    public int RoomAmenityId { get; set; }

    public int? RoomId { get; set; }

    public int? AmenityId { get; set; }

    public virtual Amenity? Amenity { get; set; }

    public virtual Room? Room { get; set; }
}
