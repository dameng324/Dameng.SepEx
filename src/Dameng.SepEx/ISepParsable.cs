namespace Dameng.SepEx;

public interface ISepParsable<TSelf> where TSelf : ISepParsable<TSelf>
{
    public static abstract TSelf Read(nietras.SeparatedValues.SepReader.Row readRow);
    public static abstract void Write(nietras.SeparatedValues.SepWriter.Row writeRow, TSelf value);
}