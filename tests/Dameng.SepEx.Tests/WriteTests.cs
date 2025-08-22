using System.Text;
using AwesomeAssertions;
using nietras.SeparatedValues;

namespace Dameng.SepEx.Tests;

public class WriteTests
{
    [Test]
    public void SepParsableWrite_ShouldWorkCorrect()
    {
        RunWriteTest((writer, records) => writer.WriteRecords<Record>(records));
    }

    [Test]
    public void SepTypeInfoWrite_ShouldWorkCorrect()
    {
        RunWriteTest((writer, records) => writer.WriteRecords<Record>(records, TestSepTypeInfo.Record));
    }

    void RunWriteTest(Action<SepWriter, IEnumerable<Record>> WriteFaction)
    {
        // Arrange
        var records = new List<Record>
        {
            new Record
            {
                String = $"B,b",
                Boolean = true,
                PlatformID = PlatformID.Win32NT,
                Int = 42
            },
            new Record
            {
                Ignore = 0,
                OptionalBoolean = null,
                OptionalPlatformID = PlatformID.Unix,
                String = $"Al{Environment.NewLine}ice",
                Boolean = false,
                PlatformID = PlatformID.Unix,
                Int = 100,
            }
        };

        // Act - Write back large dataset
        
        using var writer = Sep.New(',').Writer().ToText();
        WriteFaction(writer,records);

        // Assert
        var result = writer.ToString();
        Console.Write(result);
        var expected = """
            String,Int,Double,OptionDouble,Bool,OptionalBoolean,PlatformID,OptionalPlatformID
            "B,b",42,0.000000,,True,,Win32NT,
            "Al
            ice",100,0.000000,,False,,Unix,Unix

            """;
        result.Should().Be(expected);
    }
}