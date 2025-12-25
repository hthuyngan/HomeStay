using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class Gallery
{
    public int ImageId { get; set; }

    public string? ImageUrl { get; set; }

    public string? Category { get; set; }

    public string? Description { get; set; }
}
