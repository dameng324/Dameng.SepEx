using System.Text;
using AwesomeAssertions;
using nietras.SeparatedValues;

namespace Dameng.SepEx.Tests;

public class ReadTests
{
    [Test]
    public void SepParsableRead_ShouldWorkCorrect()
    {
        RunReadTest(reader=> reader.GetRecords<Level1.Level2.Class>());
    }
    [Test]
    public void SepTypeInfoRead_ShouldWorkCorrect()
    {
        RunReadTest(reader=> reader.GetRecords<Level1.Level2.Class>(TestSepTypeInfo.Class));
    }
    void RunReadTest(Func<SepReader, IEnumerable<Level1.Level2.Class>> readFaction)
    {
        // Arrange
        var text = $""""
                   String,Bool,PlatformID,Int,Ignore,OptionalPlatformID,Double,OptionalBoolean,DateTime
                   "  B,b""","True","Win32NT",42,50,"Unix","0.001",,20240101 11:11:11
                   "A""l{Environment.NewLine}ice",False,Unix,100,50,,2e4,,2024-01-01 11:11:11.333
                   """";

        // Act - Read back large dataset
        using var reader = Sep.Reader().FromText(text);
        var readRecords = readFaction(reader).ToList();

        // Assert
        int index = 0;
        (readRecords).Count.Should().Be(2);
        (readRecords[index].String).Should().Be("  B,b\"");
        (readRecords[index].Boolean).Should().Be(true);
        (readRecords[index].PlatformID).Should().Be(PlatformID.Win32NT);
        (readRecords[index].Int).Should().Be(42);
        (readRecords[index].Ignore).Should().Be(0);
        (readRecords[index].OptionalBoolean).Should().Be(null);
        (readRecords[index].OptionalPlatformID).Should().Be(PlatformID.Unix);
        (readRecords[index].OptionalDouble).Should().Be(null);
        (readRecords[index].DateTime).Should().HaveValue();

        index++;
        
        (readRecords[index].String).Should().Be($"A\"l{Environment.NewLine}ice");
        (readRecords[index].Boolean).Should().Be(false);
        (readRecords[index].PlatformID).Should().Be(PlatformID.Unix);
        (readRecords[index].Int).Should().Be(100);
        (readRecords[index].Ignore).Should().Be(0);
        (readRecords[index].OptionalBoolean).Should().Be(null);
        (readRecords[index].OptionalPlatformID).Should().Be(null);
        (readRecords[index].OptionalDouble).Should().Be(null);
        (readRecords[index].DateTime).Should().HaveValue();
    }

}