using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WebApplication10.Data;
using WebApplication10.Infrastructure;
using WebApplication10.Models;

// Hiển thị tiền và ngày theo định dạng Việt Nam (90.000 đ, dd/MM/yyyy)
var vi = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = vi;
CultureInfo.DefaultThreadCurrentUICulture = vi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CinemaTicketContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Tài khoản nhận tiền (Payment:Bank trong appsettings.json), dùng để tạo mã QR VietQR
builder.Services.Configure<BankOptions>(builder.Configuration.GetSection("Payment:Bank"));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Tự tạo database theo Migration và nạp dữ liệu mẫu (chỉ nạp khi DB còn trống)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CinemaTicketContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
    DbSeeder.SyncBranding(db); // đổi tên rạp cũ sang AOV Cinemas
    DbSeeder.SyncMovies(db); // bổ sung phim mới cho DB đã có dữ liệu
    DbSeeder.SyncPosters(db); // đổi ảnh mẫu cũ của phim đã có sang poster mới
    ShowtimeRefresher.EnsureUpcoming(db); // luôn có suất chiếu cho 7 ngày tới
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();