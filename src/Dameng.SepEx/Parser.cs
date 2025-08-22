using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using nietras.SeparatedValues;

namespace Dameng.SepEx;

public class Parser
{
    /// <summary>
    /// add quote when needed
    /// </summary>
    public static string EscapeSepField(string? field, char sep)
    {
        if (field == null)
            return "";

        bool mustQuote = false;

        // 1. 含有分隔符
        if (field.Contains(sep))
            mustQuote = true;

        // 2. 含有换行符
        if (field.Contains('\r') || field.Contains('\n'))
            mustQuote = true;

        // 3. 含有双引号
        if (field.Contains('"'))
            mustQuote = true;

        // 4. 前后空格
        if (field.Length > 0 && (field[0] == ' ' || field[^1] == ' '))
            mustQuote = true;

        // 如果需要引号，就处理引号转义并包裹
        if (mustQuote)
        {
            field = field.Replace("\"", "\"\"");
            return $"\"{field}\"";
        }

        return field;
    }

    public static ReadOnlySpan<char> UnescapeSepField(ReadOnlySpan<char> field)
    {
        if (field.IsEmpty)
            return string.Empty;

        bool quoted = field.Length >= 2 && field[0] == '"' && field[^1] == '"';

        if (quoted)
        {
            field = field.Slice(1, field.Length - 2); // 去掉首尾引号

            if (
                field.IndexOf('"') is { } index and >= 0
                && index < field.Length - 1
                && field[index + 1] == '"'
            )
            {
                // 处理 "" → "
                return field.ToString().Replace("\"\"", "\"");
            }
        }

        return field;
    }

    public static bool TryReadSpanParsable<T>(
        SepReader reader,
        SepReader.Row row,
        int index,
        ReadOnlySpan<char> format,
        [NotNullWhen(true)] out T? value
    )
        where T : ISpanParsable<T>
    {
        if (index < 0 || index >= reader.Header.ColNames.Count)
        {
            value = default(T);
            return false;
        }

        var col = UnescapeSepField(row[index].Span);
        if (col.IsWhiteSpace())
        {
            value = default(T);
            return false;
        }

        if (TryParseExact(col, format,reader.Spec.CultureInfo, out value))
        {
            return true;
        }

        value = T.Parse(col, null);
        return true;
    }

    public static bool TryReadSpanParsable<T>(
        SepReader reader,
        SepReader.Row row,
        string columnName,
        ReadOnlySpan<char> format,
        [NotNullWhen(true)] out T? value
    )
        where T : ISpanParsable<T>
    {
        if (reader.Header.TryIndexOf(columnName, out var index) == false)
        {
            value = default(T);
            return false;
        }

        return TryReadSpanParsable(reader, row, index, format, out value);
    }

    public static bool TryReadEnum<T>(SepReader reader, SepReader.Row row, int index,
        ReadOnlySpan<char> _, out T value)
        where T : struct, Enum
    {
        if (index < 0 || index >= reader.Header.ColNames.Count)
        {
            value = default(T);
            return false;
        }

        var col = UnescapeSepField(row[index].Span);
        if (col.IsWhiteSpace())
        {
            value = default(T);
            return false;
        }

        value = Enum.Parse<T>(col, true);
        return true;
    }

    public static bool TryReadEnum<T>(
        SepReader reader,
        SepReader.Row row,
        string columnName,
        ReadOnlySpan<char> format,
        out T value
    )
        where T : struct, Enum
    {
        if (reader.Header.TryIndexOf(columnName, out var index) == false)
        {
            value = default(T);
            return false;
        }

        return TryReadEnum(reader, row, index,format, out value);
    }

    public static bool TryReadParsable<T>(
        SepReader reader,
        SepReader.Row row,
        int index,
        ReadOnlySpan<char> format,
        [NotNullWhen(true)] out T? value
    )
        where T : IParsable<T>
    {
        if (index < 0 || index >= reader.Header.ColNames.Count)
        {
            value = default(T);
            return false;
        }

        var col = UnescapeSepField(row[index].Span);
        if (col.IsWhiteSpace())
        {
            value = default(T);
            return false;
        }

        if (TryParseExact(col, format,reader.Spec.CultureInfo, out value))
        {
            return true;
        }

        value = T.Parse(col.ToString(), null);
        return true;
    }

    private static bool TryParseExact<T>(ReadOnlySpan<char> col, ReadOnlySpan<char> format,IFormatProvider? provider,[NotNullWhen(true)] out T? value)
    {
        if (typeof(T) == typeof(DateTime))
        {
            var ok = DateTime.TryParseExact(col, format, provider, DateTimeStyles.AssumeLocal, out var dt);
            value = Unsafe.As<DateTime, T>(ref dt);
            return ok;
        }
        if (typeof(T) == typeof(DateTimeOffset))
        {
            var ok = DateTimeOffset.TryParseExact(col, format, provider, DateTimeStyles.AssumeLocal, out var dt);
            value = Unsafe.As<DateTimeOffset, T>(ref dt);
            return ok;
        }
        if (typeof(T) == typeof(DateOnly))
        {
            var ok = DateOnly.TryParseExact(col, format, provider, DateTimeStyles.AssumeLocal, out var dt);
            value = Unsafe.As<DateOnly, T>(ref dt);
            return ok;
        }
        if (typeof(T) == typeof(TimeOnly))
        {
            var ok = TimeOnly.TryParseExact(col, format, provider, DateTimeStyles.AssumeLocal, out var dt);
            value = Unsafe.As<TimeOnly, T>(ref dt);
            return ok;
        }
        if (typeof(T) == typeof(TimeSpan))
        {
            var ok = TimeSpan.TryParseExact(col, format, provider, out var dt);
            value = Unsafe.As<TimeSpan, T>(ref dt);
            return ok;
        }
        
        if (typeof(T) == typeof(Guid))
        {
            var ok = Guid.TryParseExact(col, format,  out var dt);
            value = Unsafe.As<Guid, T>(ref dt);
            return ok;
        }

        value = default(T);
        return false;
    }

    public static bool TryReadParsable<T>(
        SepReader reader,
        SepReader.Row row,
        string columnName,
        ReadOnlySpan<char> format,
        [NotNullWhen(true)] out T? value
    )
        where T : IParsable<T>
    {
        if (reader.Header.TryIndexOf(columnName, out var index) == false)
        {
            value = default(T);
            return false;
        }

        return TryReadParsable(reader, row, index, format, out value);
    }
}