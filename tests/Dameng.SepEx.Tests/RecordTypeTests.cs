using Dameng.SepEx;
using nietras.SeparatedValues;

namespace Dameng.Sep.Gen.Tests;

public class RecordTypeTests
{
    [Test]
    public async Task ClassRecord_ShouldWorkCorrectly()
    {
        // Arrange
        var text = """
                   A;B;C;D;E;F
                   ClassTest;Value;1;1.5;0.1;0.5
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record>(TestSepTypeInfo.Record).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        await Assert.That(records[0].A).IsEqualTo("ClassTest");
        await Assert.That(records[0].B).IsEqualTo("Value");
    }

    [Test]
    public async Task StructRecord_ShouldWorkCorrectly()
    {
        // Arrange
        var text = """
                   A;B;C;D;E
                   StructTest;Value;1;1.5;0.1
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record2>(TestSepTypeInfo.Record2).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        await Assert.That(records[0].A).IsEqualTo("StructTest");
        await Assert.That(records[0].B).IsEqualTo("Value");
        await Assert.That(records[0].C).IsEqualTo(1);
    }

    [Test]
    public async Task RecordClass_ShouldWorkCorrectly()
    {
        // Arrange
        var text = """
                   A;B;D;E
                   RecordTest;Value;1.5;0.1
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record3>(TestSepTypeInfo.Record3).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        await Assert.That(records[0].A).IsEqualTo("RecordTest");
        await Assert.That(records[0].B).IsEqualTo("Value");
        await Assert.That(records[0].D).IsEqualTo(1.5);
    }

    [Test]
    public async Task RecordStruct_ShouldWorkCorrectly()
    {
        // Arrange
        var text = """
                   A;B;D;E
                   RecordStructTest;Value;1.5;0.1
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record4>(TestSepTypeInfo.Record4).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        await Assert.That(records[0].A).IsEqualTo("RecordStructTest");
        await Assert.That(records[0].B).IsEqualTo("Value");
        await Assert.That(records[0].D).IsEqualTo(1.5);
    }

    [Test]
    public async Task ReadonlyStruct_ShouldWorkCorrectly()
    {
        // Arrange
        var text = """
                   A;B;D;E
                   ReadonlyTest;Value;1.5;0.1
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record5>(TestSepTypeInfo.Record5).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        await Assert.That(records[0].A).IsEqualTo("ReadonlyTest");
        await Assert.That(records[0].B).IsEqualTo("Value");
        await Assert.That(records[0].D).IsEqualTo(1.5);
    }

    [Test]
    public async Task RecordWithPrimaryConstructor_ShouldWorkCorrectly()
    {
        // Arrange
        var text = """
                   A;B;D;E
                   PrimaryTest;Value;1.5;0.1
                   """;

        // Act
        using var reader = nietras.SeparatedValues.Sep.Reader().FromText(text);
        var records = reader.GetRecords<Record6>(TestSepTypeInfo.Record6).ToList();

        // Assert
        await Assert.That(records).HasCount().EqualTo(1);
        await Assert.That(records[0].A).IsEqualTo("PrimaryTest");
        await Assert.That(records[0].B).IsEqualTo("Value");
        await Assert.That(records[0].D).IsEqualTo(1.5);
    }
}