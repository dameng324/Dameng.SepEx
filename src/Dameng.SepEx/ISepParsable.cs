using nietras.SeparatedValues;

namespace Dameng.SepEx;

/// <summary>
/// for types that can be read and written using SepReader and SepWriter.
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public interface ISepParsable<TSelf> where TSelf : ISepParsable<TSelf>
{
    public static abstract TSelf Read(SepReader reader,SepReader.Row readRow);
    public static abstract void Write(SepWriter writer,SepWriter.Row writeRow, TSelf value);
}