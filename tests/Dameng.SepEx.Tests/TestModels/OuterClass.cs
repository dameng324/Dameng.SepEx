namespace Dameng.SepEx.Tests.TestModels;

public partial class OuterClass
{
    [GenSepParsable]
    public partial class InnerClass
    {
        public int Field { get; set; }
    }
}