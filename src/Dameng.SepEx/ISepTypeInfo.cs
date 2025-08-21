using nietras.SeparatedValues;

namespace Dameng.SepEx;

public interface ISepTypeInfo<T>
{
    public T Read(nietras.SeparatedValues.SepReader.Row readRow);
    public void Write(nietras.SeparatedValues.SepWriter.Row writeRow, T value);
}

public static class SepReaderExtension
{
    public static IEnumerable<T> GetRecords<T>(this SepReader reader, ISepTypeInfo<T> typeInfo)
    {
        foreach (var row in reader)
        {
            yield return typeInfo.Read(row);
        }
    }

    public static void WriteRecord<T>(this SepWriter writer, IEnumerable<T> values, ISepTypeInfo<T> typeInfo)
    {
        foreach (var value in values)
        {
            using var row = writer.NewRow();
            typeInfo.Write(row, value);
        }
    }
}