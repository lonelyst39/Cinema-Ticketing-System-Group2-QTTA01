using System;
using System.Collections.Generic;

namespace WebApplication10.Models;

public partial class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Genre { get; set; } = null!;

    public string? Poster { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public string Language { get; set; } = null!;

    public string AgeRating { get; set; } = null!;

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
