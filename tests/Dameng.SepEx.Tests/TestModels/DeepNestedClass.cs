using Dameng.SepEx;

namespace ComplexNesting;

public partial class Level1
{
    public partial class Level2
    {
        [GenSepParsable]
        public partial class Level3
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }
    }
}