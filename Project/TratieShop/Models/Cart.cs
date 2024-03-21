using System;
using System.Collections.Generic;

namespace TratieShop.Models;

public partial class Cart
{
    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? DateInsert { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
