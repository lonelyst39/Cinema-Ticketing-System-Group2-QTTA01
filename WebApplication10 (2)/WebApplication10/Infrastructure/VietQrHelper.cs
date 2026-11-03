using System.Globalization;
using System.Text;

namespace WebApplication10.Infrastructure
{
    /// <summary>
    /// Tạo chuỗi VietQR (chuẩn EMVCo / NAPAS 247) để app ngân hàng quét là tự điền
    /// số tài khoản, số tiền và nội dung chuyển khoản.
    /// </summary>
    public static class VietQrHelper
    {
        public const string VietcombankBin = "970436";

        public static string BuildPayload(string bankBin, string accountNo, long amount, string content)
        {
            // Tag 38: thông tin tài khoản thụ hưởng (NAPAS)
            var bank = Tlv("00", bankBin) + Tlv("01", accountNo);
            var merchant = Tlv("00", "A000000727")   // GUID NAPAS
                         + Tlv("01", bank)
                         + Tlv("02", "QRIBFTTA");    // chuyển nhanh tới số tài khoản

            var sb = new StringBuilder();
            sb.Append(Tlv("00", "01"));              // phiên bản
            sb.Append(Tlv("01", "12"));              // 12 = QR động (có số tiền)
            sb.Append(Tlv("38", merchant));
            sb.Append(Tlv("53", "704"));             // VND
            sb.Append(Tlv("54", amount.ToString(CultureInfo.InvariantCulture)));
            sb.Append(Tlv("58", "VN"));
            sb.Append(Tlv("62", Tlv("08", NormalizeContent(content))));
            sb.Append("6304");                       // CRC tính trên toàn bộ chuỗi gồm cả "6304"
            sb.Append(Crc16(sb.ToString()).ToString("X4"));
            return sb.ToString();
        }

        private static string Tlv(string id, string value) => id + value.Length.ToString("00") + value;

        // Nội dung chuyển khoản: bỏ dấu, chỉ giữ chữ/số/khoảng trắng, tối đa 25 ký tự
        private static string NormalizeContent(string text)
        {
            var d = (text ?? "").Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in d)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                if (ch == 'đ') sb.Append('d');
                else if (ch == 'Đ') sb.Append('D');
                else if (char.IsAsciiLetterOrDigit(ch) || ch == ' ') sb.Append(ch);
            }
            var s = sb.ToString().Trim();
            return s.Length > 25 ? s[..25] : s;
        }

        // CRC-16/CCITT-FALSE (poly 0x1021, init 0xFFFF)
        private static ushort Crc16(string data)
        {
            ushort crc = 0xFFFF;
            foreach (var b in Encoding.ASCII.GetBytes(data))
            {
                crc ^= (ushort)(b << 8);
                for (var i = 0; i < 8; i++)
                    crc = (crc & 0x8000) != 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1);
            }
            return crc;
        }
    }
}
