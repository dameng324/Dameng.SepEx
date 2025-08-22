using System.Text;
using Dameng.SepEx.Tests.TestModels;
using nietras.SeparatedValues;
using AwesomeAssertions;

namespace Dameng.SepEx.Tests;

public class NestedClassTests
{
    [Test]
    public void NestedClass_Read_ShouldWorkCorrectly()
    {
        // Arrange
        var csv = "Field\n42";
        
        // Act
        using var reader = Sep.Reader().FromText(csv);
        var records = reader.GetRecords<OuterClass.InnerClass>().ToList();
        
        // Assert
        records.Count.Should().Be(1);
        records[0].Field.Should().Be(42);
    }
    
    [Test]
    public void NestedClass_Write_ShouldGenerateCorrectCsv()
    {
        // Arrange
        var records = new[] { new OuterClass.InnerClass { Field = 42 } };
        
        // Act
        using var writer = Sep.New(',').Writer().ToText();
        writer.WriteRecords(records);
        
        // Assert
        var result = writer.ToString();
        result.Should().Contain("Field");
        result.Should().Contain("42");
    }
    
    [Test]
    public void NestedClass_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalRecord = new OuterClass.InnerClass { Field = 123 };
        
        // Act - Write
        using var writer = Sep.New(',').Writer().ToText();
        writer.WriteRecords(new[] { originalRecord });
        var csv = writer.ToString();
        
        // Act - Read
        using var reader = Sep.Reader().FromText(csv);
        var readRecords = reader.GetRecords<OuterClass.InnerClass>().ToList();
        
        // Assert
        readRecords.Count.Should().Be(1);
        readRecords[0].Field.Should().Be(originalRecord.Field);
    }
    
    [Test]
    public void IssueExample_NestedClass_ShouldWorkCorrectly()
    {
        // This test uses the exact pattern from the GitHub issue
        // Arrange
        var csv = "Field\n100";
        
        // Act - Read
        using var reader = Sep.Reader().FromText(csv);
        var records = reader.GetRecords<MyNamespace.A.B>().ToList();
        
        // Assert
        records.Count.Should().Be(1);
        records[0].Field.Should().Be(100);
        
        // Act - Write round trip
        using var writer = Sep.New(',').Writer().ToText();
        writer.WriteRecords(records);
        var result = writer.ToString();
        
        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("100");
    }
    
    [Test]
    public void DeepNestedClass_ShouldWorkCorrectly()
    {
        // Test multiple levels of nesting: Level1.Level2.Level3
        // Arrange
        var csv = "Name,Value\nTest,999";
        
        // Act - Read
        using var reader = Sep.Reader().FromText(csv);
        var records = reader.GetRecords<ComplexNesting.Level1.Level2.Level3>().ToList();
        
        // Assert
        records.Count.Should().Be(1);
        records[0].Name.Should().Be("Test");
        records[0].Value.Should().Be(999);
        
        // Round trip test
        using var writer = Sep.New(',').Writer().ToText();
        writer.WriteRecords(records);
        var result = writer.ToString();
        
        result.Should().Contain("Name");
        result.Should().Contain("Value");
        result.Should().Contain("Test");
        result.Should().Contain("999");
    }
}