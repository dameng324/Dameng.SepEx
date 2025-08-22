using System.Text;
using AwesomeAssertions;
using nietras.SeparatedValues;

namespace Dameng.SepEx.Tests;

public class WriteTests
{
    [Test]
    public void SepParsableWrite_ShouldWorkCorrect()
    {
        RunWriteTest((writer, records) => writer.WriteRecords<Level1.Level2.Class>(records));
    }

    [Test]
    public void SepTypeInfoWrite_ShouldWorkCorrect()
    {
        RunWriteTest((writer, records) => writer.WriteRecords<Level1.Level2.Class>(records, TestSepTypeInfo.Class));
    }

    void RunWriteTest(Action<SepWriter, IEnumerable<Level1.Level2.Class>> WriteFaction)
    {
        // Arrange
        var records = new List<Level1.Level2.Class>
        {
            new Level1.Level2.Class
            {
                String = $"B,b",
                Boolean = true,
                PlatformID = PlatformID.Win32NT,
                Int = 42,
                CustomProperty = new  CustomClass(){InternalString = "CustomValue"},
                DateTime = new DateTime(2020,1,3,4,5,6,7)
            },
            new Level1.Level2.Class
            {
                Ignore = 0,
                OptionalBoolean = null,
                OptionalPlatformID = PlatformID.Unix,
                String = $"Al{Environment.NewLine}ice",
                Boolean = false,
                PlatformID = PlatformID.Unix,
                Int = 100,
                CustomProperty = new  CustomClass(){InternalString = "CustomValue2"},
                DateTime = new DateTime(2020,4,5,4,5,6,7)
            }
        };

        // Act - Write back large dataset
        
        using var writer = Sep.New(',').Writer().ToText();
        WriteFaction(writer,records);

        // Assert
        var result = writer.ToString();
        Console.Write(result);
        var expected = """
            String,Int,Double,OptionalDouble,Bool,OptionalBoolean,PlatformID,OptionalPlatformID,DateTime,CustomProperty
            "B,b",42,0.000000,,True,,Win32NT,,20200103 04:05:06,CustomValue
            "Al
            ice",100,0.000000,,False,,Unix,Unix,20200405 04:05:06,CustomValue2

            """;
        result.Should().Be(expected);
    }
}