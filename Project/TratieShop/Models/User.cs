using System;
using System.Collections.Generic;

namespace TratieShop.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool? Gender { get; set; }

    public string? Email { get; set; }

    public string? UserName { get; set; }

    public string? PassWord { get; set; }

    public int? RoleId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Role? Role { get; set; }
}
