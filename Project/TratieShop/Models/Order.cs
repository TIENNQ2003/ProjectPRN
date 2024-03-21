using System;
using System.Collections.Generic;

namespace TratieShop.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? RequireDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public decimal? TotalPrice { get; set; }

    public int? StatusId { get; set; }

    public string? DeliverAddress { get; set; }

    public string? Phone { get; set; }

    public string? ReceiverName { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Status? Status { get; set; }

    public virtual User? User { get; set; }
}
