using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class Seat
{
    public int SeatId { get; set; }

    public string SeatNumber { get; set; } = null!;

    public string RowNumber { get; set; } = null!;

    public string SeatType { get; set; } = null!;

    public int RoomId { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();

    public virtual CinemaRoom Room { get; set; } = null!;

    public virtual ICollection<SeatLock> SeatLocks { get; set; } = new List<SeatLock>();
}
