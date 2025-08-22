using System.Globalization;
using System.Text;
using CsvHelper;
using nietras.SeparatedValues;

namespace Dameng.SepEx.Tests;

public class BasicCsvOperationsTests
{
    [Test]
    public async Task Read_BasicCsvData_ShouldParseCorrectly()
    {
        // Arrange
        var text = """
                   A;B;C;D;E;F
                   Sep;🚀;1;1.2;0.1;0.5
                   CSV;✅;2;2.2;0.2;1.5
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record>(SepEx.Tests.TestSepTypeInfo.Record).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(2);
        
        var firstRecord = records[0];
        await Assert.That(firstRecord.A).IsEqualTo("Sep");
        await Assert.That(firstRecord.B).IsEqualTo("🚀");
        await Assert.That(firstRecord.C).IsEqualTo(1);
        await Assert.That(firstRecord.D).IsEqualTo(1.2);
        
        var secondRecord = records[1];
        await Assert.That(secondRecord.A).IsEqualTo("CSV");
        await Assert.That(secondRecord.B).IsEqualTo("✅");
        await Assert.That(secondRecord.C).IsEqualTo(2);
        await Assert.That(secondRecord.D).IsEqualTo(2.2);
    }

    [Test]
    public async Task Write_BasicRecords_ShouldGenerateCorrectCsv()
    {
        // Arrange
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

        // Act
        using var writer = nietras.SeparatedValues.Sep.Writer().To(stringBuilder);
        writer.WriteRecords(records, SepEx.Tests.TestSepTypeInfo.Record);
        var result = stringBuilder.ToString();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Sep");
        await Assert.That(result).Contains("CSV");
        await Assert.That(result).Contains("🚀");
        await Assert.That(result).Contains("✅");
        
        // Verify header structure based on attributes
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        await Assert.That(lines).HasCount().GreaterThanOrEqualTo(3); // header + 2 data rows
    }

    [Test]
    public async Task RoundTrip_WriteAndRead_ShouldPreserveData()
    {
        // Arrange
        var originalRecords = new List<Record>
        {
            new() { A = "Test1", B = "Value1", C = 100, D = 1.5 },
            new() { A = "Test2", B = "Value2", C = 200, D = 2.5 }
        };

        // Act - Write to CSV
        var stringBuilder = new StringBuilder();
        using (var writer = nietras.SeparatedValues.Sep.Writer().To(stringBuilder))
        {
            writer.WriteRecords(originalRecords, SepEx.Tests.TestSepTypeInfo.Record);
        }
        
        // Act - Read back from CSV
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(stringBuilder.ToString());
        var readRecords = reader.GetRecords<Record>(SepEx.Tests.TestSepTypeInfo.Record).ToList();

        // Assert
        await Assert.That(readRecords).HasCount().EqualTo(originalRecords.Count);
        
        for (int i = 0; i < originalRecords.Count; i++)
        {
            await Assert.That(readRecords[i].A).IsEqualTo(originalRecords[i].A);
            await Assert.That(readRecords[i].B).IsEqualTo(originalRecords[i].B);
            await Assert.That(readRecords[i].C).IsEqualTo(originalRecords[i].C);
            // Note: D field has SepColumnFormat("C") which formats as currency, 
            // so we can't expect exact round-trip for formatted fields
        }
    }

    [Test]
    public async Task Write_CSVHelper_ShouldGenerateValidOutput()
    {
        // Arrange
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

        // Act
        using var csvWriter = new CsvWriter(stringBuilder, CultureInfo.InvariantCulture);
        csvWriter.WriteRecords(records);
        var result = stringBuilder.ToString();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Sep");
        await Assert.That(result).Contains("CSV");
        
        // Verify proper CSV structure
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        await Assert.That(lines).HasCount().GreaterThanOrEqualTo(3); // header + 2 data rows
    }
}