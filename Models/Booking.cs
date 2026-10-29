using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public DateTime BookingDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string BookingStatus { get; set; } = null!;

    public int UserId { get; set; }

    public int ShowtimeId { get; set; }

    public int? PromotionId { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();

    public virtual Payment? Payment { get; set; }

    public virtual Promotion? Promotion { get; set; }

    public virtual Showtime Showtime { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
