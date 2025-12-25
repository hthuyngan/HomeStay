using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Position { get; set; }

    public decimal? Salary { get; set; }

    public DateTime? HireDate { get; set; }

    public int Status { get; set; }


    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
