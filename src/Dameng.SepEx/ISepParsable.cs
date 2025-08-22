namespace Dameng.SepEx;

/// <summary>
/// for types that can be read and written using SepReader and SepWriter.
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public interface ISepParsable<TSelf> where TSelf : ISepParsable<TSelf>
{
    public static abstract TSelf Read(nietras.SeparatedValues.SepReader.Row readRow);
    public static abstract void Write(nietras.SeparatedValues.SepWriter.Row writeRow, TSelf value);
}