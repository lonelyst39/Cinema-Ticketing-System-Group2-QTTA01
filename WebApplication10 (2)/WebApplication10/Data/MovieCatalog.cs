using WebApplication10.Models;

namespace WebApplication10.Data;

/// <summary>
/// Danh mục phim bổ sung (12 phim đang chiếu, 6 phim sắp chiếu) ngoài 8 phim mẫu ban đầu.
/// Ngày công chiếu tính theo ngày hiện tại nên danh sách luôn có phim đang chiếu lẫn sắp chiếu.
/// </summary>
public static class MovieCatalog
{
    public static List<Movie> Extra(DateOnly todayDate) => new()
    {
            new() { Title = "Ngọn Hải Đăng Cuối Trời", Genre = "Phiêu lưu", Poster = "posters/ngon-hai-dang-cuoi-troi.svg", ReleaseDate = todayDate.AddDays(-45), Language = "Phụ đề tiếng Việt", AgeRating = "T13", Description = "Nhóm bạn trẻ lên đường tìm ngọn hải đăng huyền thoại nơi tận cùng đại dương." },
            new() { Title = "Mẹ Ơi, Con Về Rồi", Genre = "Gia đình", Poster = "posters/me-oi-con-ve-roi.svg", ReleaseDate = todayDate.AddDays(-42), Language = "Tiếng Việt", AgeRating = "P", Description = "Người con xa xứ trở về quê nhà và tìm lại những điều giản dị đã bị lãng quên." },
            new() { Title = "Bóng Ma Trường Cũ", Genre = "Kinh dị", Poster = "posters/bong-ma-truong-cu.svg", ReleaseDate = todayDate.AddDays(-38), Language = "Lồng tiếng", AgeRating = "T18", Description = "Đêm cuối cùng trước khi ngôi trường bị phá bỏ, những tiếng thì thầm bắt đầu vang lên." },
            new() { Title = "Thám Tử Mèo Mướp", Genre = "Hoạt hình", Poster = "posters/tham-tu-meo-muop.svg", ReleaseDate = todayDate.AddDays(-35), Language = "Lồng tiếng", AgeRating = "P", Description = "Chú mèo thám tử cùng đội bạn bốn chân phá vụ mất trộm ở khu chợ cá." },
            new() { Title = "Ngày Mai Sẽ Khác", Genre = "Tâm lý", Poster = "posters/ngay-mai-se-khac.svg", ReleaseDate = todayDate.AddDays(-32), Language = "Tiếng Việt", AgeRating = "T16", Description = "Một người đàn ông ba mươi lăm tuổi quyết định làm lại cuộc đời sau biến cố bất ngờ." },
            new() { Title = "Điệp Vụ Bóng Đêm", Genre = "Hành động", Poster = "posters/diep-vu-bong-dem.svg", ReleaseDate = todayDate.AddDays(-30), Language = "Phụ đề tiếng Việt", AgeRating = "T16", Description = "Đặc vụ hai mang phải chọn giữa nhiệm vụ và người mình yêu thương nhất." },
            new() { Title = "Hẹn Em Ở Hội An", Genre = "Tình cảm", Poster = "posters/hen-em-o-hoi-an.svg", ReleaseDate = todayDate.AddDays(-28), Language = "Tiếng Việt", AgeRating = "T13", Description = "Chuyến đi phố cổ đưa hai người xa lạ vào một mối duyên không ngờ." },
            new() { Title = "Cú Đúp Vàng", Genre = "Thể thao", Poster = "posters/cu-dup-vang.svg", ReleaseDate = todayDate.AddDays(-24), Language = "Tiếng Việt", AgeRating = "P", Description = "Đội bóng phủi của khu phố bước vào giải đấu lớn nhất trong lịch sử câu lạc bộ." },
            new() { Title = "Kẻ Săn Ký Ức", Genre = "Khoa học viễn tưởng", Poster = "posters/ke-san-ky-uc.svg", ReleaseDate = todayDate.AddDays(-21), Language = "Phụ đề tiếng Việt", AgeRating = "T16", Description = "Trong tương lai, ký ức có thể mua bán, và một thợ săn ký ức tìm thấy thứ không nên có." },
            new() { Title = "Quán Cà Phê Số 9", Genre = "Hài", Poster = "posters/quan-ca-phe-so-9.svg", ReleaseDate = todayDate.AddDays(-18), Language = "Tiếng Việt", AgeRating = "T13", Description = "Quán cà phê nhỏ ở Hà Nội và những vị khách kỳ quặc làm nên chuỗi ngày bất ổn." },
            new() { Title = "Đảo Rồng Thức Giấc", Genre = "Phiêu lưu", Poster = "posters/dao-rong-thuc-giac.svg", ReleaseDate = todayDate.AddDays(-14), Language = "Lồng tiếng", AgeRating = "T13", Description = "Khi con rồng ngủ yên suốt ngàn năm trở mình, cả hòn đảo phải đứng lên." },
            new() { Title = "Dấu Vết Trên Tuyết", Genre = "Hình sự", Poster = "posters/dau-vet-tren-tuyet.svg", ReleaseDate = todayDate.AddDays(-9), Language = "Phụ đề tiếng Việt", AgeRating = "T16", Description = "Một vụ mất tích tại trạm nghỉ vùng núi cao hé lộ bí mật của cả ngôi làng." },
            new() { Title = "Sắc Màu Tuổi Thơ", Genre = "Gia đình", Poster = "posters/sac-mau-tuoi-tho.svg", ReleaseDate = todayDate.AddDays(7), Language = "Tiếng Việt", AgeRating = "P", Description = "Những ngày hè rực rỡ của ba đứa trẻ và chiếc xe đạp cũ của ông ngoại." },
            new() { Title = "Hồ Sơ Mật 404", Genre = "Hình sự", Poster = "posters/ho-so-mat-404.svg", ReleaseDate = todayDate.AddDays(12), Language = "Phụ đề tiếng Việt", AgeRating = "T16", Description = "Nhà báo điều tra lần theo một tệp tin bí ẩn dẫn tới đường dây quyền lực." },
            new() { Title = "Mùa Gió Chướng", Genre = "Tâm lý", Poster = "posters/mua-gio-chuong.svg", ReleaseDate = todayDate.AddDays(17), Language = "Tiếng Việt", AgeRating = "T13", Description = "Một gia đình miền Tây đối mặt mùa nước nổi và những điều chưa nói thành lời." },
            new() { Title = "Siêu Anh Hùng Tập Sự", Genre = "Hài", Poster = "posters/sieu-anh-hung-tap-su.svg", ReleaseDate = todayDate.AddDays(25), Language = "Lồng tiếng", AgeRating = "T13", Description = "Cậu học sinh vô tình nhận siêu năng lực và biến mọi chuyện thành thảm họa hài hước." },
            new() { Title = "Trận Chiến Thiên Hà", Genre = "Khoa học viễn tưởng", Poster = "posters/tran-chien-thien-ha.svg", ReleaseDate = todayDate.AddDays(32), Language = "Phụ đề tiếng Việt", AgeRating = "T13", Description = "Liên minh các hành tinh đứng trước cuộc chiến cuối cùng quyết định số phận thiên hà." },
            new() { Title = "Đêm Trước Giao Thừa", Genre = "Tình cảm", Poster = "posters/dem-truoc-giao-thua.svg", ReleaseDate = todayDate.AddDays(40), Language = "Tiếng Việt", AgeRating = "T13", Description = "Một đêm cuối năm, hai con người cô đơn tình cờ gặp nhau giữa phố đông." }
    };
}
