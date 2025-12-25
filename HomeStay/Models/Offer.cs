using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class Offer
{
    public int OfferId { get; set; }

    public string? Title { get; set; }

    public int? DiscountPercent { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }
}
