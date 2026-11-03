using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class Cinema
{
    public int CinemaId { get; set; }

    public string CinemaName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public virtual ICollection<CinemaRoom> CinemaRooms { get; set; } = new List<CinemaRoom>();
}
