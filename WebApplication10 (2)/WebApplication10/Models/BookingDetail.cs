using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class BookingDetail
{
    public int BookingDetailId { get; set; }

    public decimal Price { get; set; }

    public int BookingId { get; set; }

    public int SeatId { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Seat Seat { get; set; } = null!;

    public virtual Ticket? Ticket { get; set; }
}
