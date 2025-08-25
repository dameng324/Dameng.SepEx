using nietras.SeparatedValues;
using System.Data;
using System.Runtime.CompilerServices;

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
        bool dataWritten = false;
        foreach (var value in values)
        {
            using var row = writer.NewRow();
            typeInfo.Write(writer, row, value);
            dataWritten = true;
        }
        if (!dataWritten && GetWriteHeader(writer))
        {
            foreach (var header in typeInfo.GetHeaders())
            {
                writer.Header.Add(header);
            }
            writer.Header.Write();
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
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_writeHeader")]
    static extern ref bool GetWriteHeader(SepWriter a);
    public static void WriteRecords<T>(this SepWriter writer, IEnumerable<T> values)
        where T : ISepParsable<T>
    {
        bool dataWritten = false;
        foreach (var value in values)
        {
            using var row = writer.NewRow();
            T.Write(writer, row, value);
            dataWritten = true;
        }
        if (!dataWritten && GetWriteHeader(writer))
        {
            foreach (var header in T.GetHeaders())
            {
                writer.Header.Add(header);
            }
            writer.Header.Write();
        }
    }
}