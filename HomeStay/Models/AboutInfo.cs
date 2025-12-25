using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class AboutInfo
{
    public int AboutId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? Image { get; set; }
}
