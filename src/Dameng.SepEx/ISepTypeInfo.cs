using nietras.SeparatedValues;

namespace Dameng.SepEx;

/// <summary>
/// for external class, use can not put `GenSepParsable` to the class.
/// <br/>
/// so you can use `GenSepTypeInfo` to generate a type info for the class. just like `System.Text.Json` source generator.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISepTypeInfo<T>
{
    /// <summary>
    /// parse SepReader.Row data to record Type
    /// </summary>
    /// <param name="reader">SepReader</param>
    /// <param name="readRow">SepReader.Row</param>
    /// <returns>record</returns>
    public T Read(SepReader reader, SepReader.Row readRow);

    /// <summary>
    /// write record data to SepWriter.Row.
    /// </summary>
    /// <param name="writer">SepWriter</param>
    /// <param name="writeRow">SepWriter.Row</param>
    /// <param name="value">record</param>
    public void Write(SepWriter writer, SepWriter.Row writeRow, T value);

    /// <summary>
    /// This method provides headers for writer when empty records need be written.
    /// </summary>
    public IEnumerable<string> GetHeaders();
}
