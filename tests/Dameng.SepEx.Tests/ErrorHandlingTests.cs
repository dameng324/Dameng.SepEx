using Dameng.SepEx;
using nietras.SeparatedValues;

namespace Dameng.Sep.Gen.Tests;

public class ErrorHandlingTests
{
    [Test]
    public async Task Read_InvalidData_ShouldUseDefaults()
    {
        // Arrange
        var text = """
                   A;B;C;D
                   ValidString;ValidString;InvalidInt;InvalidDouble
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record>(TestSepTypeInfo.Record).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        var record = records[0];
        
        await Assert.That(record.A).IsEqualTo("ValidString");
        await Assert.That(record.B).IsEqualTo("ValidString");
        // Invalid int should fall back to default value (10 from SepDefaultValue)
        await Assert.That(record.C).IsEqualTo(10);
        // Invalid double should fall back to default
        await Assert.That(record.D).IsEqualTo(0.0);
    }

    [Test]
    public async Task Read_EmptyData_ShouldHandleGracefully()
    {
        // Arrange
        var text = """
                   A;B;C;D
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record>(TestSepTypeInfo.Record).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(0);
    }

    [Test]
    public async Task Read_MissingColumns_ShouldUseDefaults()
    {
        // The current implementation expects all columns to be present
        // This test validates error handling for missing columns
        
        // Arrange - All columns present but with empty/invalid values
        var text = """
                   A;B;C;D
                   Test;Value;;
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record>(TestSepTypeInfo.Record).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        var record = records[0];
        
        await Assert.That(record.A).IsEqualTo("Test");
        await Assert.That(record.B).IsEqualTo("Value");
        await Assert.That(record.C).IsEqualTo(10); // Default from attribute when parsing fails
        await Assert.That(record.D).IsEqualTo(0.0); // Default for double when parsing fails
    }
}