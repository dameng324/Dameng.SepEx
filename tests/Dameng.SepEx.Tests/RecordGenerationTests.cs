using System.Text;
using AwesomeAssertions;
using nietras.SeparatedValues;

namespace Dameng.SepEx.Tests;

public class RecordGenerationTests
{
    [Test]
    public void StructRecord_ShouldWorkWithGenSepParsable()
    {
        // Arrange
        var text = """
                   A
                   Hello
                   World
                   """;

        // Act - Read using generated ISepParsable implementation
        using var reader = Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record2>().ToList();

        // Assert
        records.Count.Should().Be(2);
        records[0].A.Should().Be("Hello");
        records[1].A.Should().Be("World");
    }

    [Test]
    public void RecordClass_ShouldWorkWithGenSepParsable()
    {
        // Arrange
        var text = """
                   A
                   TestValue1
                   TestValue2
                   """;

        // Act - Read using generated ISepParsable implementation
        using var reader = Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record3>().ToList();

        // Assert
        records.Count.Should().Be(2);
        records[0].A.Should().Be("TestValue1");
        records[1].A.Should().Be("TestValue2");
    }

    [Test]
    public void RecordStruct_ShouldWorkWithGenSepParsable()
    {
        // Arrange
        var text = """
                   A
                   Value1
                   Value2
                   """;

        // Act - Read using generated ISepParsable implementation
        using var reader = Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record4>().ToList();

        // Assert
        records.Count.Should().Be(2);
        records[0].A.Should().Be("Value1");
        records[1].A.Should().Be("Value2");
    }

    [Test]
    public void RecordWithPrimaryConstructor_ShouldWorkWithGenSepParsable()
    {
        // Arrange
        var text = """
                   A,B,D,E
                   Test,Data,3.14,2.5
                   More,Info,1.23,4.5
                   """;

        // Act - Read using generated ISepParsable implementation
        using var reader = Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record6>().ToList();

        // Assert
        records.Count.Should().Be(2);
        records[0].A.Should().Be("Test");
        records[0].B.Should().Be("Data");
        records[0].D.Should().Be(3.14);
        records[0].E.Should().Be(2.5f);
        
        records[1].A.Should().Be("More");
        records[1].B.Should().Be("Info");
        records[1].D.Should().Be(1.23);
        records[1].E.Should().Be(4.5f);
    }

    [Test]
    public void WriteRecords_ShouldWorkWithGenSepParsable()
    {
        // Arrange
        var records = new List<Record2>
        {
            new Record2 { A = "First" },
            new Record2 { A = "Second" }
        };

        // Act - Write using generated ISepParsable implementation
        using var writer = Sep.New(',').Writer().ToText();
        writer.WriteRecords<Record2>(records);

        // Assert
        var result = writer.ToString();
        var expected = """
            A
            First
            Second

            """;
        result.Should().Be(expected);
    }
}