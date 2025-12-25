using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class RoomType
{
    public int RoomTypeId { get; set; }

    public string? TypeName { get; set; }

    public decimal? PricePerNight { get; set; }

    public int? MaxPeople { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
