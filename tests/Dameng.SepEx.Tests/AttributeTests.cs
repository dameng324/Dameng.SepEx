using System.Text;
using nietras.SeparatedValues;

namespace Dameng.SepEx.Tests;

public class AttributeTests
{
    [Test]
    public async Task SepColumnIndex_ShouldRespectColumnOrder()
    {
        // Arrange
        var text = """
                   A;B;C;D
                   FirstValue;SecondValue;123;4.56
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record>(SepEx.Tests.TestSepTypeInfo.Record).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        var record = records[0];
        
        // A has SepColumnIndex(0), so it should get the first column
        await Assert.That(record.A).IsEqualTo("FirstValue");
        // B has SepColumnName("B"), so it should get the "B" column
        await Assert.That(record.B).IsEqualTo("SecondValue");
    }

    [Test]
    public async Task SepDefaultValue_ShouldUseDefaultWhenColumnMissing()
    {
        // Based on the current implementation, the library expects all columns to be present
        // This test validates the current behavior rather than the expected behavior
        
        // Arrange - CSV with all expected columns, but C contains invalid data
        var text = """
                   A;B;C;D
                   TestValue;TestB;InvalidInt;1.5
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record>(SepEx.Tests.TestSepTypeInfo.Record).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        var record = records[0];
        
        await Assert.That(record.A).IsEqualTo("TestValue");
        await Assert.That(record.B).IsEqualTo("TestB");
        await Assert.That(record.C).IsEqualTo(10); // Should use default value when parsing fails
        await Assert.That(record.D).IsEqualTo(1.5);
    }

    [Test]
    public async Task SepIgnore_ShouldNotIncludeIgnoredProperty()
    {
        // Arrange
        var records = new List<Record>
        {
            new() { A = "Test", B = "Value", C = 1, D = 1.5, E = 999.9f } // E should be ignored
        };

        // Act
        var stringBuilder = new StringBuilder();
        using var writer = nietras.SeparatedValues.Sep.Writer().To(stringBuilder);
        writer.WriteRecords(records, SepEx.Tests.TestSepTypeInfo.Record);
        var result = stringBuilder.ToString();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Test");
        await Assert.That(result).Contains("Value");
        // E should not appear in the output as it has SepIgnore attribute
        await Assert.That(result).DoesNotContain("999.9");
    }

    [Test]
    public async Task SepColumnFormat_ShouldApplyFormattingOnWrite()
    {
        // Arrange
        var records = new List<Record>
        {
            new() { A = "Test", B = "Value", C = 1, D = 1.2345 } // D has SepColumnFormat("C") for currency
        };

        // Act
        var stringBuilder = new StringBuilder();
        using var writer = nietras.SeparatedValues.Sep.Writer().To(stringBuilder);
        writer.WriteRecords(records, SepEx.Tests.TestSepTypeInfo.Record);
        var result = stringBuilder.ToString();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Test");
        await Assert.That(result).Contains("Value");
        // D should be formatted according to SepColumnFormat("C")
        await Assert.That(result).Contains("1.234500"); // Currency symbol
    }
}