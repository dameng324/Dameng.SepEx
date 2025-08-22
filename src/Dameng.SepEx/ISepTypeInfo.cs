namespace Dameng.SepEx;

/// <summary>
/// for external class, use can not put `GenSepParsable` to the class.
/// <br/>
/// so you can use `GenSepTypeInfo` to generate a type info for the class. just like `System.Text.Json` source generator.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISepTypeInfo<T>
{
    public T Read(nietras.SeparatedValues.SepReader.Row readRow);
    public void Write(nietras.SeparatedValues.SepWriter.Row writeRow, T value);
}