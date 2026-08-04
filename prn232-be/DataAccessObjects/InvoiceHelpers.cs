using System;
using System.Linq;

namespace DataAccessObjects;

/// <summary>Tiện ích sinh &amp; đối chiếu mã captcha xác thực hóa đơn (dùng chung mọi module).</summary>
public static class CaptchaHelper
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // bỏ ký tự dễ nhầm (I,O,0,1)

    public static string Generate(int length = 6)
    {
        var rnd = Random.Shared;
        return new string(Enumerable.Range(0, length).Select(_ => Chars[rnd.Next(Chars.Length)]).ToArray());
    }

    public static bool Match(string? stored, string? input) =>
        !string.IsNullOrWhiteSpace(stored) &&
        string.Equals(stored.Trim(), input?.Trim(), StringComparison.OrdinalIgnoreCase);
}

/// <summary>Sinh số hóa đơn duy nhất dạng INV-yyyyMMdd-XXXX.</summary>
public static class InvoiceNumberHelper
{
    public static string Next(CarShowroomContext ctx)
    {
        string number;
        do
        {
            var suffix = Random.Shared.Next(0, 10000).ToString("D4");
            number = $"INV-{DateTime.Now:yyyyMMdd}-{suffix}";
        } while (ctx.MasterInvoices.Any(m => m.InvoiceNumber == number));
        return number;
    }
}
