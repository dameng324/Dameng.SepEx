using System.Text;
using Dameng.SepEx;
using nietras.SeparatedValues;

namespace Dameng.Sep.Gen.Tests;

public class NullableTypeTests
{
    [Test]
    public async Task NullableInt_ShouldHandleNullAndValues()
    {
        // Arrange
        var text = """
                   NullableInt
                   123

                   456
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<SimpleNullableRecord>(TestSepTypeInfo.SimpleNullableRecord).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(3);
        
        await Assert.That(records[0].NullableInt).IsEqualTo(123);
        await Assert.That(records[1].NullableInt).IsNull(); // Empty string should result in null
        await Assert.That(records[2].NullableInt).IsEqualTo(456);
    }

    [Test]
    public async Task NullableInt_RoundTrip_ShouldPreserveNullValues()
    {
        // Arrange
        var originalRecords = new List<SimpleNullableRecord>
        {
            new() { NullableInt = 100 },
            new() { NullableInt = null },
            new() { NullableInt = 200 }
        };

        // Act - Write
        var stringBuilder = new StringBuilder();
        using (var writer = nietras.SeparatedValues.Sep.Writer().To(stringBuilder))
        {
            writer.WriteRecords(originalRecords, TestSepTypeInfo.SimpleNullableRecord);
        }

        // Act - Read back
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(stringBuilder.ToString());
        var readRecords = reader.GetRecords<SimpleNullableRecord>(TestSepTypeInfo.SimpleNullableRecord).ToList();

        // Assert
        await Assert.That(readRecords).HasCount().EqualTo(3);
        await Assert.That(readRecords[0].NullableInt).IsEqualTo(100);
        await Assert.That(readRecords[1].NullableInt).IsNull();
        await Assert.That(readRecords[2].NullableInt).IsEqualTo(200);
    }
}