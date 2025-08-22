using nietras.SeparatedValues;

namespace Dameng.SepEx;

public static class SepReaderExtension
{
    public static IEnumerable<T> GetRecords<T>(this SepReader reader, ISepTypeInfo<T> typeInfo)
    {
        foreach (var row in reader)
        {
            yield return typeInfo.Read(reader, row);
        }
    }

    public static void WriteRecords<T>(
        this SepWriter writer,
        IEnumerable<T> values,
        ISepTypeInfo<T> typeInfo
    )
    {
        foreach (var value in values)
        {
            using var row = writer.NewRow();
            typeInfo.Write(writer, row, value);
        }
    }

    public static IEnumerable<T> GetRecords<T>(this SepReader reader)
        where T : ISepParsable<T>
    {
        foreach (var row in reader)
        {
            yield return T.Read(reader, row);
        }
    }

    public static void WriteRecords<T>(this SepWriter writer, IEnumerable<T> values)
        where T : ISepParsable<T>
    {
        foreach (var value in values)
        {
            using var row = writer.NewRow();
            T.Write(writer, row, value);
        }
    }
}