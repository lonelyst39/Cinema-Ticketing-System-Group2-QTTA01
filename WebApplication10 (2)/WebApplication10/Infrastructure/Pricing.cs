using WebApplication10.Models;

namespace WebApplication10.Infrastructure
{
    /// <summary>Một nơi duy nhất quy định giá, để giao diện và máy chủ luôn tính giống nhau.</summary>
    public static class Pricing
    {
        public const decimal VipSurcharge = 15000;
        public const decimal CoupleSurcharge = 30000;
        public const int MaxSeats = 8;
        public const int HoldMinutes = 10;
        public const int CancelBeforeHours = 2;

        public static decimal Surcharge(string? seatType) => seatType switch
        {
            "VIP" => VipSurcharge,
            "Couple" => CoupleSurcharge,
            _ => 0
        };

        public static decimal SeatPrice(decimal basePrice, string? seatType) => basePrice + Surcharge(seatType);

        public static decimal Discount(decimal subtotal, Promotion promo)
        {
            var d = promo.DiscountType == "Percentage"
                ? Math.Round(subtotal * promo.DiscountValue / 100m, 0)
                : promo.DiscountValue;
            return Math.Min(Math.Max(d, 0), subtotal);
        }
    }
}
