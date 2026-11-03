using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string TransactionCode { get; set; } = null!;

    public int BookingId { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
