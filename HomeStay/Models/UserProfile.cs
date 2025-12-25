using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class UserProfile
{
    public int ProfileId { get; set; }

    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public int? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
