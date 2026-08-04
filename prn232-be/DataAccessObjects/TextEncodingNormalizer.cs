using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

internal static class TextEncodingNormalizer
{
    private static readonly string[] Markers =
    {
        "Ã", "â€”", "â€¢", "Ä", "áº", "á»", "ï¿½", "â€"
    };

    private static readonly Encoding Latin1 = Encoding.GetEncoding("ISO-8859-1");
    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    public static void NormalizePendingStrings(DbContext dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries()
                     .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            foreach (var property in entry.Properties.Where(p => p.Metadata.ClrType == typeof(string)))
            {
                var currentValue = property.CurrentValue as string;
                if (string.IsNullOrWhiteSpace(currentValue))
                {
                    continue;
                }

                var normalized = MaybeFixMojibake(currentValue);
                if (!string.Equals(normalized, currentValue, StringComparison.Ordinal))
                {
                    property.CurrentValue = normalized;
                }
            }
        }
    }

    public static string MaybeFixMojibake(string value)
    {
        if (!Markers.Any(value.Contains))
        {
            return value;
        }

        var current = value;
        for (var i = 0; i < 3; i++)
        {
            try
            {
                var bytes = Latin1.GetBytes(current);
                var fixedValue = Utf8.GetString(bytes);
                if (fixedValue == current)
                {
                    break;
                }

                current = fixedValue;
            }
            catch (EncoderFallbackException)
            {
                break;
            }
            catch (DecoderFallbackException)
            {
                break;
            }
            catch (ArgumentException)
            {
                break;
            }
        }

        return current;
    }
}
