using AwesomeAssertions;
using nietras.SeparatedValues;

namespace Dameng.SepEx.Tests;

public class SupportTypesTests
{
    [Test]
    public void Struct_SepParsableTest()
    {
        RunSepParsableTest<Struct>();
    }
    [Test]
    public void Record_SepParsableTest()
    {
        RunSepParsableTest<Record>();
    }
    [Test]
    public void RecordStruct_SepParsableTest()
    {
        RunSepParsableTest<RecordStruct>();
    }
    [Test]
    public void ReadonlyStruct_SepParsableTest()
    {
        RunSepParsableTest<ReadonlyStruct>();
    }
    [Test]
    public void RecordPrimaryConstructor_SepParsableTest()
    {
        RunSepParsableTest<RecordPrimaryConstructor>();
    }
    
    [Test]
    public void Struct_SepTypeInfoTest()
    {
        RunSepTypeInfoTest<Struct>(TestSepTypeInfo.Struct);
    }
    [Test]
    public void Record_SepTypeInfoTest()
    {
        RunSepTypeInfoTest<Record>(TestSepTypeInfo.Record);
    }
    [Test]
    public void RecordStruct_SepTypeInfoTest()
    {
        RunSepTypeInfoTest<RecordStruct>(TestSepTypeInfo.RecordStruct);
    }
    [Test]
    public void ReadonlyStruct_SepTypeInfoTest()
    {
        RunSepTypeInfoTest<ReadonlyStruct>(TestSepTypeInfo.ReadonlyStruct);
    }
    [Test]
    public void RecordPrimaryConstructor_SepTypeInfoTest()
    {
        RunSepTypeInfoTest<RecordPrimaryConstructor>(TestSepTypeInfo.RecordPrimaryConstructor);
    }


    void RunSepParsableTest<T>() where T : ITestDataGen<T>, ISepParsable<T>
    {
        var original = Enumerable.Range(0, 100).Select(i => T.Create()).ToArray();
        T[] readResult;
        string csvString;
        {
            using var writer = Sep.New(',').Writer().ToText();
            writer.WriteRecords(original);
            csvString = writer.ToString();
        }
        {
            using var reader = Sep.New(',').Reader().FromText(csvString);
            readResult = reader.GetRecords<T>().ToArray();
        }
        
        
        readResult.Length.Should().Be(original.Length);
    }
    
    void RunSepTypeInfoTest<T>( ISepTypeInfo<T> typeInfo) where T : ITestDataGen<T>
    {
        var original = Enumerable.Range(0, 100).Select(i => T.Create()).ToArray();
        T[] readResult;
        string csvString;
        {
            using var writer = Sep.New(',').Writer().ToText();
            writer.WriteRecords(original,typeInfo);
            csvString = writer.ToString();
        }
        {
            using var reader = Sep.New(',').Reader().FromText(csvString);
            readResult = reader.GetRecords<T>(typeInfo).ToArray();
        }
        
        
        readResult.Length.Should().Be(original.Length);
    }
}

public interface ITestDataGen<TSelf> where TSelf : ITestDataGen<TSelf>
{
    public static abstract TSelf Create();
}

[GenSepParsable]
internal partial struct Struct : ITestDataGen<Struct>
{
    public string A { get; set; }

    public static Struct Create()
    {
        return new Struct()
        {
            A = Guid.NewGuid().ToString(),
        };
    }
}

[GenSepParsable]
public sealed partial record Record : ITestDataGen<Record>
{
    public string A { get; set; } = string.Empty;

    public static Record Create()
    {
        return new Record()
        {
            A = Guid.NewGuid().ToString(),
        };
    }
}

[GenSepParsable]
public partial record struct RecordStruct : ITestDataGen<RecordStruct>
{
    public string A { get; set; }

    public static RecordStruct Create()
    {
        return new RecordStruct()
        {
            A = Guid.NewGuid().ToString(),
        };
    }
}

[GenSepParsable]
public readonly partial struct ReadonlyStruct : ITestDataGen<ReadonlyStruct>
{
    public string A { get; init; }

    public static ReadonlyStruct Create()
    {
        return new ReadonlyStruct()
        {
            A = Guid.NewGuid().ToString(),
        };
    }
}

[GenSepParsable]
public partial record RecordPrimaryConstructor(string A, string B, double D): ITestDataGen<RecordPrimaryConstructor>
{
    public static RecordPrimaryConstructor Create()
    {
        return new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Random.Shared.NextDouble());
    }
}