using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string PromotionCode { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
