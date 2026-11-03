using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class CinemaRoom
{
    public int RoomId { get; set; }

    public string RoomName { get; set; } = null!;

    public string RoomType { get; set; } = null!;

    public int Capacity { get; set; }

    public int CinemaId { get; set; }

    public virtual Cinema Cinema { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
