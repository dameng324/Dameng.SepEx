namespace Dameng.SepEx.Tests;

public partial class Level1
{
    public partial class Level2
    {
        [GenSepParsable]
        public partial class Class : ITestDataGen<Class>
        {
            [SepColumnIndex(0)]
            [CsvHelper.Configuration.Attributes.Index(0)]
            [CsvHelper.Configuration.Attributes.Name("AA")]
            [SepDefaultValue("111")]
            public string String { get; set; } = string.Empty;

            [SepDefaultValue(10)] public int Int;
            [SepColumnFormat("0.000000")] public double Double { get; set; }
            [SepColumnFormat("0.000000")] public double? OptionalDouble { get; set; }
            [SepColumnIgnore] public float Ignore { get; set; }
            [SepColumnName("Bool")] public bool Boolean { get; set; }
            public bool? OptionalBoolean { get; set; }
            public PlatformID PlatformID { get; set; }
            public PlatformID? OptionalPlatformID { get; set; }

            public static Class Create()
            {
                return new Bogus.Faker<Class>().Generate();
            }
        }
    }
}

[GenSepTypeInfo<Level1.Level2.Class>()]
[GenSepTypeInfo<Struct>()]
[GenSepTypeInfo<Record>()]
[GenSepTypeInfo<RecordStruct>()]
[GenSepTypeInfo<ReadonlyStruct>()]
[GenSepTypeInfo<RecordPrimaryConstructor>()]
internal partial class TestSepTypeInfo;