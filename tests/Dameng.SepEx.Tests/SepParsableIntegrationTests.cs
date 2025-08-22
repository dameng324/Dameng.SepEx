using System.Text;
using nietras.SeparatedValues;

namespace Dameng.SepEx.Tests;

public class SepParsableIntegrationTests
{
    [Test]
    public async Task LargeDataset_ShouldProcessEfficiently()
    {
        // Arrange
        var records = new List<Record>();
        for (int i = 0; i < 1000; i++)
        {
            records.Add(new Record
            {
                A = $"Item{i}",
                B = $"Value{i}",
                C = i,
                D = i * 1.5,
                E = i * 0.1f
            });
        }

        // Act - Write large dataset
        var stringBuilder = new StringBuilder();
        using (var writer = nietras.SeparatedValues.Sep.Writer().To(stringBuilder))
        {
            writer.WriteRecords(records);
        }

        // Act - Read back large dataset
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(stringBuilder.ToString());
        var readRecords = reader.GetRecords<Record>().ToList();

        // Assert
        await Assert.That(readRecords).HasCount().EqualTo(1000);
        await Assert.That(readRecords[0].A).IsEqualTo("Item0");
        await Assert.That(readRecords[999].A).IsEqualTo("Item999");
        await Assert.That(readRecords[500].C).IsEqualTo(500);
    }

    [Test]
    public async Task SpecialCharacters_ShouldBeHandledCorrectly()
    {
        // Arrange - Test proper CSV escaping for special characters
        var records = new List<Record>
        {
            new() { A = "SimpleText", B = "SimpleValue", C = 1, D = 1.5 },
            new() { A = "Unicode🚀", B = "Symbols@#$%", C = 2, D = 2.5 },
            new() { A = "Text_with_underscores", B = "Numbers123", C = 3, D = 3.5 }
        };

        // Act - Round trip
        var stringBuilder = new StringBuilder();
        using (var writer = nietras.SeparatedValues.Sep.Writer().To(stringBuilder))
        {
            writer.WriteRecords(records);
        }

        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(stringBuilder.ToString());
        var readRecords = reader.GetRecords<Record>().ToList();

        // Assert
        await Assert.That(readRecords).HasCount().EqualTo(3);
        await Assert.That(readRecords[0].A).IsEqualTo("SimpleText");
        await Assert.That(readRecords[1].A).IsEqualTo("Unicode🚀");
        await Assert.That(readRecords[2].A).IsEqualTo("Text_with_underscores");
    }

    [Test]
    public async Task EmptyAndNullValues_ShouldBeHandledCorrectly()
    {
        // Arrange
        var records = new List<Record>
        {
            new() { A = "", B = "NotEmpty", C = 0, D = 0.0 },
            new() { A = "NotEmpty", B = "", C = 1, D = 1.0 }
        };

        // Act - Round trip
        var stringBuilder = new StringBuilder();
        using (var writer = nietras.SeparatedValues.Sep.Writer().To(stringBuilder))
        {
            writer.WriteRecords(records);
        }

        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(stringBuilder.ToString());
        var readRecords = reader.GetRecords<Record>().ToList();

        // Assert
        await Assert.That(readRecords).HasCount().EqualTo(2);
        await Assert.That(readRecords[0].A).IsEqualTo("");
        await Assert.That(readRecords[0].B).IsEqualTo("NotEmpty");
        await Assert.That(readRecords[1].A).IsEqualTo("NotEmpty");
        await Assert.That(readRecords[1].B).IsEqualTo("");
    }
}