using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class LocationInfo
{
    public int LocationId { get; set; }

    public string? Address { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public string? MapEmbed { get; set; }
}
