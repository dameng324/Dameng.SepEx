namespace Dameng.SepEx.Tests;

[GenSepParsable]
public partial class Record
{
    [SepColumnIndex(0)]
    [CsvHelper.Configuration.Attributes.Index(0)]
    [CsvHelper.Configuration.Attributes.Name("AA")]
    [SepDefaultValue("111")]
    public string String { get; set; } = string.Empty;
    [SepDefaultValue(10)]
    public int Int;
    [SepColumnFormat("0.000000")]
    public double Double { get; set; }
    [SepColumnFormat("0.000000")]
    public double? OptionalDouble { get; set; }
    [SepIgnore]
    public float Ignore { get; set; }
    [SepColumnName("Bool")]
    public bool Boolean { get; set; }
    public bool? OptionalBoolean { get; set; }
    public PlatformID PlatformID { get; set; }
    public PlatformID? OptionalPlatformID { get; set; }
}

public struct Record2
{
    public string A { get; set; }
}

public record Record3
{
    public string A { get; set; } = string.Empty;
}

public record struct Record4
{
    public string A { get; set; }
}

public readonly struct Record5
{
    public string A { get; init; }
}
public record Record6(string A,string B,double D,float E);

[GenSepTypeInfo<Record>()]
[GenSepTypeInfo<Record2>()]
[GenSepTypeInfo<Record3>()]
[GenSepTypeInfo<Record4>()]
[GenSepTypeInfo<Record5>()]
[GenSepTypeInfo<Record6>()]
public partial class TestSepTypeInfo;