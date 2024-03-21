using System;
using System.Collections.Generic;

namespace TratieShop.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? CategoryId { get; set; }

    public string? Description { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? UnitinStock { get; set; }

    public float? Discount { get; set; }

    public int? Status { get; set; }

    public string? Image { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
