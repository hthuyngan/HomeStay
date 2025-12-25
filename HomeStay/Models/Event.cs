using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string? Title { get; set; }

    public DateOnly? EventDate { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }
}
