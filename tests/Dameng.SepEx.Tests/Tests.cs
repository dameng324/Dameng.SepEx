using System.Text.Json.Serialization;
using Dameng.SepEx;
using TUnit.Assertions;

namespace Dameng.Sep.Gen.Tests;

public class Record
{
    [SepColumnIndex(0)]
    [CsvHelper.Configuration.Attributes.Index(0)]
    [CsvHelper.Configuration.Attributes.Name("AA")]
    [SepDefaultValue("111")]
    public string A { get; set; } = string.Empty;
    
    [SepColumnName("B")]
    [CsvHelper.Configuration.Attributes.Name("B")]
    public string B { get; set; } = string.Empty;
    [SepDefaultValue(10)]
    public int C;
    [SepColumnFormat("0.000000")]
    public double D { get; set; }
    [SepIgnore]
    public float E { get; set; }
}

public struct Record2
{
    public string A { get; set; }
    public string B { get; set; }
    public int C;
    public double D { get; set; }
    public float E { get; set; }
}

public record Record3
{
    public string A { get; set; } = string.Empty;
    public string B { get; set; } = string.Empty;
    public double D { get; set; }
    public float E { get; set; }
}

public record struct Record4
{
    public string A { get; set; }
    public string B { get; set; }
    public double D { get; set; }
    public float E { get; set; }
}

public readonly struct Record5
{
    public string A { get; init; }
    public string B { get; init; }
    public double D { get; init; }
    public float E { get; init; }
}
public record Record6(string A,string B,double D,float E);
public class Record7(string A,string B,double D,float E);

// Test record for nullable types
public class SimpleNullableRecord
{
    // Read-only nullable property to avoid the Format<T> write issue
    public int? NullableInt { get; init; }
}

// Test enums
public enum Status
{
    Active,
    Inactive,
    Pending
}

public enum Priority
{
    Low = 1,
    Medium = 5,
    High = 10
}

// Test record with enums
public class RecordWithEnums
{
    public string Name { get; set; } = string.Empty;
    public Status Status { get; set; }
    public Priority Priority { get; set; }
    public Priority? OptionalPriority { get; set; }
}

[GenSepTypeInfo<Record>()]
[GenSepTypeInfo<Record2>()]
[GenSepTypeInfo<Record3>()]
[GenSepTypeInfo<Record4>()]
[GenSepTypeInfo<Record5>()]
[GenSepTypeInfo<Record6>()]
[GenSepTypeInfo<SimpleNullableRecord>()]
[GenSepTypeInfo<RecordWithEnums>()] 
public partial class TestSepTypeInfo;
