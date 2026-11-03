using Microsoft.AspNetCore.Identity;
using WebApplication10.Models;

namespace WebApplication10.Data;

public static class DbSeeder
{
    public static void Seed(CinemaTicketContext db)
    {
        if (db.Roles.Any()) return; // đã có dữ liệu thì bỏ qua

        var today = DateTime.Today;
        var todayDate = DateOnly.FromDateTime(today);

        // 1. Vai trò
        var adminRole = new Role { RoleName = "Admin", Description = "Quản trị viên" };
        var customerRole = new Role { RoleName = "Customer", Description = "Khách hàng" };

        // 2. Người dùng (mật khẩu được băm)
        var hasher = new PasswordHasher<User>();
        var admin = new User
        {
            FullName = "Quản trị viên",
            Email = "admin@cinema.vn",
            Phone = "0900000001",
            Address = "Hà Nội",
            Role = adminRole,
            Password = ""
        };
        admin.Password = hasher.HashPassword(admin, "Admin@123");

        var customer = new User
        {
            FullName = "Khách Hàng Mẫu",
            Email = "user@cinema.vn",
            Phone = "0900000002",
            Address = "Hà Nội",
            Role = customerRole,
            Password = ""
        };
        customer.Password = hasher.HashPassword(customer, "User@123");

        // 3. Rạp, phòng chiếu
        var cinemaA = new Cinema { CinemaName = "AOV Cinemas Hà Nội", Address = " Hai Bà Trưng, Hà Nội", Phone = "02439741888" };
        var cinemaB = new Cinema { CinemaName = "AOV Cinemas TPHCM", Address = "Quận 7, TPHCM ", Phone = "02473001888" };

        var rooms = new List<CinemaRoom>();
        foreach (var cinema in new[] { cinemaA, cinemaB })
        {
            rooms.Add(new CinemaRoom { Cinema = cinema, RoomName = "Phòng 1", RoomType = "2D", Capacity = 50 });
            rooms.Add(new CinemaRoom { Cinema = cinema, RoomName = "Phòng 2", RoomType = "3D", Capacity = 50 });
        }

        // 4. Ghế: mỗi phòng 5 hàng x 10 ghế (A-B Standard, C-D VIP, E Couple)
        var seats = new List<Seat>();
        foreach (var room in rooms)
        {
            foreach (var row in new[] { "A", "B", "C", "D", "E" })
            {
                for (int n = 1; n <= 10; n++)
                {
                    seats.Add(new Seat
                    {
                        Room = room,
                        RowNumber = row,
                        SeatNumber = $"{row}{n}",
                        SeatType = row switch { "C" or "D" => "VIP", "E" => "Couple", _ => "Standard" }
                    });
                }
            }
        }

        // 5. Phim gốc (6 đang chiếu, 2 sắp chiếu); phim bổ sung nằm trong MovieCatalog
        var movies = new List<Movie>
        {
            new() { Title = "Cuộc Săn Đuổi Cuối Cùng", Genre = "Hành động", Poster = "posters/cuoc-san-duoi-cuoi-cung.svg", ReleaseDate = todayDate.AddDays(-30), Language = "Phụ đề tiếng Việt", AgeRating = "T16", Description = "Một cựu đặc vụ buộc phải quay lại để ngăn chặn âm mưu đe dọa cả thành phố." },
            new() { Title = "Mùa Hoa Anh Đào Trở Lại", Genre = "Tình cảm", Poster = "posters/mua-hoa-anh-dao-tro-lai.svg", ReleaseDate = todayDate.AddDays(-25), Language = "Phụ đề tiếng Việt", AgeRating = "T13", Description = "Câu chuyện tình lãng mạn giữa hai người trẻ gặp lại sau mười năm xa cách." },
            new() { Title = "Căn Nhà Số 13", Genre = "Kinh dị", Poster = "posters/can-nha-so-13.svg", ReleaseDate = todayDate.AddDays(-20), Language = "Lồng tiếng", AgeRating = "T18", Description = "Gia đình nhỏ chuyển đến căn nhà cũ và dần phát hiện bí mật rùng rợn." },
            new() { Title = "Hành Tinh Bị Lãng Quên", Genre = "Khoa học viễn tưởng", Poster = "posters/hanh-tinh-bi-lang-quen.svg", ReleaseDate = todayDate.AddDays(-15), Language = "Phụ đề tiếng Việt", AgeRating = "T13", Description = "Phi hành đoàn lạc vào một hành tinh bí ẩn nơi thời gian chảy ngược." },
            new() { Title = "Vương Quốc Phép Thuật", Genre = "Hoạt hình", Poster = "posters/vuong-quoc-phep-thuat.svg", ReleaseDate = todayDate.AddDays(-10), Language = "Lồng tiếng", AgeRating = "P", Description = "Hành trình phiêu lưu của cô bé tìm lại viên đá phép thuật cho vương quốc." },
            new() { Title = "Tiếng Cười Ngày Tết", Genre = "Hài", Poster = "posters/tieng-cuoi-ngay-tet.svg", ReleaseDate = todayDate.AddDays(-5), Language = "Tiếng Việt", AgeRating = "T13", Description = "Những tình huống dở khóc dở cười của một đại gia đình ngày Tết." },
            new() { Title = "Bí Mật Phố Cổ", Genre = "Hình sự", Poster = "posters/bi-mat-pho-co.svg", ReleaseDate = todayDate.AddDays(10), Language = "Tiếng Việt", AgeRating = "T16", Description = "Vụ án bí ẩn giữa lòng Hà Nội cổ kính đưa viên thanh tra vào cuộc truy tìm." },
            new() { Title = "Đường Đua Sinh Tử", Genre = "Hành động", Poster = "posters/duong-dua-sinh-tu.svg", ReleaseDate = todayDate.AddDays(20), Language = "Phụ đề tiếng Việt", AgeRating = "T13", Description = "Tay đua bị gạt khỏi giải đấu quyết tâm giành lại vinh quang." }
        };

        movies.AddRange(MovieCatalog.Extra(todayDate)); // thêm 18 phim bổ sung

        // 6. Suất chiếu 7 ngày tới (chỉ cho phim đã công chiếu)
        var nowShowing = movies.Where(m => m.ReleaseDate <= todayDate).ToList();
        var slots = new[] { new TimeSpan(9, 30, 0), new TimeSpan(12, 30, 0), new TimeSpan(15, 30, 0), new TimeSpan(18, 30, 0), new TimeSpan(21, 30, 0) };
        var showtimes = new List<Showtime>();
        for (int d = 0; d < 7; d++)
        {
            for (int r = 0; r < rooms.Count; r++)
            {
                for (int s = 0; s < slots.Length; s++)
                {
                    var movie = nowShowing[(d + r * 2 + s) % nowShowing.Count];
                    var start = today.AddDays(d) + slots[s];
                    showtimes.Add(new Showtime
                    {
                        Movie = movie,
                        Room = rooms[r],
                        StartTime = start,
                        EndTime = start.AddMinutes(120),
                        Price = rooms[r].RoomType == "3D" ? 110000 : 90000
                    });
                }
            }
        }

        // 7. Khuyến mãi
        var promotions = new List<Promotion>
        {
            new() { PromotionCode = "GIAM10", DiscountType = "Percentage", DiscountValue = 10, StartDate = todayDate.AddDays(-30), EndDate = todayDate.AddDays(365), Quantity = 100, Status = "Active" },
            new() { PromotionCode = "GIAM20K", DiscountType = "Fixed", DiscountValue = 20000, StartDate = todayDate.AddDays(-30), EndDate = todayDate.AddDays(365), Quantity = 100, Status = "Active" }
        };

        db.Roles.AddRange(adminRole, customerRole);
        db.Users.AddRange(admin, customer);
        db.Cinemas.AddRange(cinemaA, cinemaB);
        db.CinemaRooms.AddRange(rooms);
        db.Seats.AddRange(seats);
        db.Movies.AddRange(movies);
        db.Showtimes.AddRange(showtimes);
        db.Promotions.AddRange(promotions);
        db.SaveChanges();
    }

    // DB đã có dữ liệu từ trước: bổ sung các phim còn thiếu (so theo tên phim, không ghi đè phim đã có)
    // rồi chia lại phim cho các suất chiếu sắp tới để phim mới cũng có suất đặt vé.
    public static void SyncMovies(CinemaTicketContext db)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var titles = db.Movies.Select(m => m.Title).ToHashSet();
        var missing = MovieCatalog.Extra(today).Where(m => !titles.Contains(m.Title)).ToList();
        if (missing.Count == 0) return;

        db.Movies.AddRange(missing);
        db.SaveChanges();
        RebalanceShowtimes(db);
    }

    // Chỉ đụng tới suất chiếu sắp tới CHƯA có đơn đặt vé hay ghế đang giữ, nên không ảnh hưởng vé đã bán.
    private static void RebalanceShowtimes(CinemaTicketContext db)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var movieIds = db.Movies.Where(m => m.ReleaseDate <= today).OrderBy(m => m.MovieId).Select(m => m.MovieId).ToList();
        if (movieIds.Count == 0) return;

        var free = db.Showtimes
            .Where(s => s.StartTime >= now && !s.Bookings.Any() && !s.SeatLocks.Any())
            .OrderBy(s => s.StartTime).ThenBy(s => s.RoomId)
            .ToList();
        for (var i = 0; i < free.Count; i++)
            free[i].MovieId = movieIds[i % movieIds.Count];
        if (free.Count > 0) db.SaveChanges();
    }

    // Phim đã nằm trong DB từ trước vẫn trỏ tới ảnh mẫu cũ (product-0N.jpg): đổi sang poster mới.
    // Chỉ thay khi Poster còn trống hoặc còn là ảnh cũ, nên poster do quản trị viên tự đặt sẽ không bị ghi đè.
    public static void SyncPosters(CinemaTicketContext db)
    {
        var map = new Dictionary<string, string>
        {
            ["Cuộc Săn Đuổi Cuối Cùng"] = "posters/cuoc-san-duoi-cuoi-cung.svg",
            ["Mùa Hoa Anh Đào Trở Lại"] = "posters/mua-hoa-anh-dao-tro-lai.svg",
            ["Căn Nhà Số 13"] = "posters/can-nha-so-13.svg",
            ["Hành Tinh Bị Lãng Quên"] = "posters/hanh-tinh-bi-lang-quen.svg",
            ["Vương Quốc Phép Thuật"] = "posters/vuong-quoc-phep-thuat.svg",
            ["Tiếng Cười Ngày Tết"] = "posters/tieng-cuoi-ngay-tet.svg",
            ["Bí Mật Phố Cổ"] = "posters/bi-mat-pho-co.svg",
            ["Đường Đua Sinh Tử"] = "posters/duong-dua-sinh-tu.svg"
        };

        var changed = false;
        foreach (var movie in db.Movies.ToList())
        {
            var old = string.IsNullOrEmpty(movie.Poster) || movie.Poster.StartsWith("product-");
            if (old && map.TryGetValue(movie.Title, out var poster))
            {
                movie.Poster = poster;
                changed = true;
            }
        }
        if (changed) db.SaveChanges();
    }

    // Rạp đã có trong DB từ trước vẫn mang tên cũ "CGV ...": đổi sang thương hiệu AOV Cinemas.
    public static void SyncBranding(CinemaTicketContext db)
    {
        var changed = false;
        foreach (var cinema in db.Cinemas.ToList())
        {
            if (cinema.CinemaName.StartsWith("CGV ", StringComparison.OrdinalIgnoreCase))
            {
                cinema.CinemaName = "AOV Cinemas " + cinema.CinemaName.Substring(4);
                changed = true;
            }
        }
        if (changed) db.SaveChanges();
    }
}
