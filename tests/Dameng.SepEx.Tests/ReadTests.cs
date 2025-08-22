using System.Text;
using AwesomeAssertions;
using nietras.SeparatedValues;

namespace Dameng.SepEx.Tests;

public class ReadTests
{
    [Test]
    public void SepParsableRead_ShouldWorkCorrect()
    {
        RunReadTest(reader=> reader.GetRecords<Record>());
    }
    [Test]
    public void SepTypeInfoRead_ShouldWorkCorrect()
    {
        RunReadTest(reader=> reader.GetRecords<Record>(TestSepTypeInfo.Record));
    }
    void RunReadTest(Func<SepReader, IEnumerable<Record>> readFaction)
    {
        // Arrange
        var text = $""""
                   String,Bool,PlatformID,Int,Ignore,OptionalPlatformID,Double,OptionalBoolean
                   "  B,b""","True","Win32NT",42,50,"Unix","0.001",
                   "A""l{Environment.NewLine}ice",False,Unix,100,50,,2e4,
                   """";

        // Act - Read back large dataset
        using var reader = Sep.Reader().FromText(text);
        var readRecords = readFaction(reader).ToList();

        // Assert
        (readRecords).Count.Should().Be(2);
        (readRecords[0].String).Should().Be("  B,b\"");
        (readRecords[0].Boolean).Should().Be(true);
        (readRecords[0].PlatformID).Should().Be(PlatformID.Win32NT);
        (readRecords[0].Int).Should().Be(42);
        (readRecords[0].Ignore).Should().Be(0);
        (readRecords[0].OptionalBoolean).Should().Be(null);
        (readRecords[0].OptionalPlatformID).Should().Be(PlatformID.Unix);
        (readRecords[0].OptionalDouble).Should().Be(null);
        
        (readRecords[1].String).Should().Be($"A\"l{Environment.NewLine}ice");
        (readRecords[1].Boolean).Should().Be(false);
        (readRecords[1].PlatformID).Should().Be(PlatformID.Unix);
        (readRecords[1].Int).Should().Be(100);
        (readRecords[1].Ignore).Should().Be(0);
        (readRecords[1].OptionalBoolean).Should().Be(null);
        (readRecords[1].OptionalPlatformID).Should().Be(null);
        (readRecords[0].OptionalDouble).Should().Be(null);
    }

}