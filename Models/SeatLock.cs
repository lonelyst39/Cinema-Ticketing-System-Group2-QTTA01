using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class SeatLock
{
    public int SeatLockId { get; set; }

    public DateTime LockedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string LockStatus { get; set; } = null!;

    public int UserId { get; set; }

    public int SeatId { get; set; }

    public int ShowtimeId { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual Showtime Showtime { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
