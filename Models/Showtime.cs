using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class Showtime
{
    public int ShowtimeId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal Price { get; set; }

    public int MovieId { get; set; }

    public int RoomId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Movie Movie { get; set; } = null!;

    public virtual CinemaRoom Room { get; set; } = null!;

    public virtual ICollection<SeatLock> SeatLocks { get; set; } = new List<SeatLock>();
}
