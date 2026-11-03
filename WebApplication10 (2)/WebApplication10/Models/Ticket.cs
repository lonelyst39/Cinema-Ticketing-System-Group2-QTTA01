using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public string TicketCode { get; set; } = null!;

    public string? Qrcode { get; set; }

    public DateTime IssuedDate { get; set; }

    public string TicketStatus { get; set; } = null!;

    public int BookingDetailId { get; set; }

    public virtual BookingDetail BookingDetail { get; set; } = null!;
}
