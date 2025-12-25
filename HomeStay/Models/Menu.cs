using System;
using System.Collections.Generic;

namespace HomeStay.Models;

public partial class Menu
{
    public int MenuId { get; set; }

    public string Title { get; set; } = null!;

    public string Url { get; set; } = null!;

    public int? ParentId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Menu> InverseParent { get; set; } = new List<Menu>();

    public virtual Menu? Parent { get; set; }
}
