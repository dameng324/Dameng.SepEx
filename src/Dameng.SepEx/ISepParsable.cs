using nietras.SeparatedValues;

namespace Dameng.SepEx;

/// <summary>
/// for types that can be read and written using SepReader and SepWriter.
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public interface ISepParsable<TSelf>
    where TSelf : ISepParsable<TSelf>
{
    /// <summary>
    /// parse SepReader.Row data to TSelf instance.
    /// </summary>
    /// <param name="reader">SepReader</param>
    /// <param name="readRow">SepReader.Row</param>
    /// <returns>TSelf instance</returns>
    public static abstract TSelf Read(SepReader reader, SepReader.Row readRow);

    /// <summary>
    /// write TSelf instance data to SepWriter.Row.
    /// </summary>
    /// <param name="writer">SepWriter</param>
    /// <param name="writeRow">SepWriter.Row</param>
    /// <param name="value">TSelf instance</param>
    public static abstract void Write(SepWriter writer, SepWriter.Row writeRow, TSelf value);

    /// <summary>
    /// This method provides headers for writer when empty records need be written.
    /// </summary>
    public static abstract IEnumerable<string> GetHeaders();
}
