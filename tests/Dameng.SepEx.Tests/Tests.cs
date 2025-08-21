using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using CsvHelper;
using Dameng.SepEx;

namespace Dameng.Sep.Gen.Tests;

using nietras.SeparatedValues;

public class Tests
{
    [Test]
    public void Read()
    {
        var text = """
            A;B;C;D;E;F
            Sep;🚀;1;1.2;0.1;0.5
            CSV;✅;2;2.2;0.2;1.5
            """;

        using var reader = Sep.Reader().FromText(text);
        foreach (var record in reader.GetRecords<Record>(TestSepTypeInfo.Record))
        {
            Console.WriteLine($"A: {record.A}, B: {record.B}, C: {record.C}");
        }
    }

    [Test]
    public void Write()
    {
        var stringBuilder = new StringBuilder();

        var records = new List<Record>
        {
            new()
            {
                A = "Sep",
                B = "🚀",
                C = 1,
                D = 1.2,
                E = 0.1f,
            },
            new()
            {
                A = "CSV",
                B = "✅",
                C = 2,
                D = 2.2,
                E = 0.2f,
            },
        };
        {
            using var writer = Sep.Writer().To(stringBuilder);
            writer.WriteRecord(records, TestSepTypeInfo.Record);
        }
        Console.WriteLine(stringBuilder.ToString());
    }
    [Test]
    public void Write_CSVHelper()
    {
        var stringBuilder = new StringWriter();

        var records = new List<Record>
        {
            new()
            {
                A = "Sep",
                B = "🚀",
                C = 1,
                D = 1.2,
                E = 0.1f,
            },
            new()
            {
                A = "CSV",
                B = "✅",
                C = 2,
                D = 2.2,
                E = 0.2f,
            },
        };
        {
            using var csvWriter = new CsvWriter(stringBuilder,CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(records);
        }
        Console.WriteLine(stringBuilder.ToString());
    }
}

public class Record
{
    [SepColumnIndex(0)]
    [CsvHelper.Configuration.Attributes.Index(0)]
    [CsvHelper.Configuration.Attributes.Name("AA")]
    [SepDefaultValue("111")]
    public string A { get; set; }
    
    [SepColumnName("B")]
    [CsvHelper.Configuration.Attributes.Name("B")]
    public System.String B { get; set; }

    
    [SepDefaultValue(10)]
    public int C;

    [SepColumnFormat("C")]
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
    public string A { get; set; }
    public string B { get; set; }
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

[GenSepTypeInfo<Record>()]
[GenSepTypeInfo<Record2>()]
[GenSepTypeInfo<Record3>()]
[GenSepTypeInfo<Record4>()]
[GenSepTypeInfo<Record5>()]
[GenSepTypeInfo<Record6>()]
public partial class TestSepTypeInfo;

file sealed class RecordSepTypeInfo : ISepTypeInfo<Dameng.Sep.Gen.Tests.Record>
{
    public Dameng.Sep.Gen.Tests.Record Read(nietras.SeparatedValues.SepReader.Row readRow)
    {
        return new Dameng.Sep.Gen.Tests.Record()
        {
            A = readRow[0].TryParse<string>(out var v1)?v1:default(string),
            B = readRow["B"].TryParse<string>(out var v2)?v2:default(string),
            C = readRow["C"].TryParse<int>(out var v3)?v3:10,
            D = readRow["D"].TryParse<double>(out var v4)?v4:default(double),
// Ignored: E
        };
    }

    public void Write(nietras.SeparatedValues.SepWriter.Row writeRow, Dameng.Sep.Gen.Tests.Record value)
    {
        writeRow[0].Set(value.A);
        writeRow["B"].Set(value.B);
        writeRow["C"].Format(value.C);
        writeRow["D"].Set(value.D.ToString("C"));
    }
}