using Dameng.SepEx;

namespace MyNamespace;

public partial class A
{
    [GenSepParsable]
    public partial class B
    {
        public int Field { get; set; }
    }
}