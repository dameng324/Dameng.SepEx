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

        bool mustQuote=false;

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
        if (field.Length > 0 && 
            (field[0] == ' ' || field[^1] == ' '))
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

            if (field.IndexOf('"') is {} index and >= 0 && index <field.Length-1 && field[index+1]=='"')
            {
                // 处理 "" → "
                return field.ToString().Replace("\"\"", "\"");
            }
        }

        return field;
    }

}