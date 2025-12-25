using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Staff? Staff { get; set; }

    public virtual UserProfile? UserProfile { get; set; }
}
