using WebApplication10.Models;

namespace WebApplication10.Data;

/// <summary>
/// Seeder chỉ tạo suất chiếu 7 ngày đầu tiên. Lớp này tự bổ sung suất chiếu cho các ngày sắp tới
/// mỗi lần ứng dụng khởi động, để trang không bị trống sau một tuần.
/// </summary>
public static class ShowtimeRefresher
{
    private static readonly TimeSpan[] Slots =
    {
        new(9, 30, 0), new(12, 30, 0), new(15, 30, 0), new(18, 30, 0), new(21, 30, 0)
    };

    public static void EnsureUpcoming(CinemaTicketContext db, int days = 7)
    {
        var rooms = db.CinemaRooms.OrderBy(r => r.RoomId).ToList();
        if (rooms.Count == 0) return;

        var today = DateTime.Today;
        var changed = false;

        for (var d = 0; d < days; d++)
        {
            var day = today.AddDays(d);
            var next = day.AddDays(1);
            if (db.Showtimes.Any(s => s.StartTime >= day && s.StartTime < next)) continue;

            var dayDate = DateOnly.FromDateTime(day);
            var movies = db.Movies.Where(m => m.ReleaseDate <= dayDate).OrderBy(m => m.MovieId).ToList();
            if (movies.Count == 0) continue;

            for (var r = 0; r < rooms.Count; r++)
            {
                for (var s = 0; s < Slots.Length; s++)
                {
                    var start = day + Slots[s];
                    db.Showtimes.Add(new Showtime
                    {
                        MovieId = movies[(d + r * 2 + s) % movies.Count].MovieId,
                        RoomId = rooms[r].RoomId,
                        StartTime = start,
                        EndTime = start.AddMinutes(120),
                        Price = rooms[r].RoomType == "3D" ? 110000 : 90000
                    });
                    changed = true;
                }
            }
        }

        if (changed) db.SaveChanges();
    }
}
