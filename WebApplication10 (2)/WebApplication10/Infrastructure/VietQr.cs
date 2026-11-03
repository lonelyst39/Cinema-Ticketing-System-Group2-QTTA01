using System.Globalization;
using System.Text;
using QRCoder;

namespace WebApplication10.Infrastructure
{
    /// <summary>Thông tin tài khoản nhận tiền, đọc từ appsettings.json (mục Payment:Bank).</summary>
    public class BankOptions
    {
        public string BankName { get; set; } = "Vietcombank";
        public string BankBin { get; set; } = "970436";          // mã BIN của Vietcombank theo Napas
        public string AccountNumber { get; set; } = "1050556154";
        public string AccountName { get; set; } = "";            // tên chủ tài khoản (hiển thị cho khách đối chiếu)
    }

    /// <summary>Dữ liệu một lần thanh toán: số tiền và nội dung chuyển khoản (thường là mã đơn).</summary>
    public record QrPayment(decimal Amount, string Memo);

    /// <summary>Tạo mã VietQR (chuẩn EMVCo/Napas) hoàn toàn trên máy chủ, không gọi dịch vụ bên ngoài.</summary>
    public static class VietQr
    {
        public static string BuildPayload(BankOptions bank, decimal amount, string memo)
        {
            var account = Tlv("00", bank.BankBin) + Tlv("01", bank.AccountNumber);
            var merchant = Tlv("00", "A000000727") + Tlv("01", account) + Tlv("02", "QRIBFTTA"); // QRIBFTTA: chuyển đến tài khoản

            var sb = new StringBuilder();
            sb.Append(Tlv("00", "01"));
            sb.Append(Tlv("01", amount > 0 ? "12" : "11")); // 12: mã động (có số tiền), 11: mã tĩnh
            sb.Append(Tlv("38", merchant));
            sb.Append(Tlv("53", "704"));                    // VND
            if (amount > 0)
                sb.Append(Tlv("54", ((long)Math.Round(amount, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture)));
            sb.Append(Tlv("58", "VN"));
            var note = CleanMemo(memo);
            if (note.Length > 0) sb.Append(Tlv("62", Tlv("08", note)));
            sb.Append("6304");
            sb.Append(Crc16(sb.ToString()));
            return sb.ToString();
        }

        /// <summary>Nội dung chuyển khoản: bỏ dấu, chỉ giữ chữ-số-khoảng trắng, tối đa 25 ký tự.</summary>
        public static string CleanMemo(string? memo)
        {
            if (string.IsNullOrWhiteSpace(memo)) return "";
            var d = memo.Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in d)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                if (ch < 128 && (char.IsLetterOrDigit(ch) || ch == ' ')) sb.Append(ch);
            }
            var s = string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return s.Length > 25 ? s[..25].TrimEnd() : s;
        }

        public static string ToPngDataUri(string payload, int pixelsPerModule = 8)
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
            var bytes = new PngByteQRCode(data).GetGraphic(pixelsPerModule);
            return "data:image/png;base64," + Convert.ToBase64String(bytes);
        }

        private static string Tlv(string id, string value) =>
            id + value.Length.ToString("00", CultureInfo.InvariantCulture) + value;

        // CRC-16/CCITT-FALSE (đa thức 0x1021, khởi tạo 0xFFFF) tính trên toàn chuỗi, gồm cả "6304"
        private static string Crc16(string s)
        {
            int crc = 0xFFFF;
            foreach (var b in Encoding.ASCII.GetBytes(s))
            {
                crc ^= b << 8;
                for (var i = 0; i < 8; i++)
                    crc = (crc & 0x8000) != 0 ? ((crc << 1) ^ 0x1021) & 0xFFFF : (crc << 1) & 0xFFFF;
            }
            return crc.ToString("X4", CultureInfo.InvariantCulture);
        }
    }
}
