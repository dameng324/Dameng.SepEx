using Dameng.SepEx;

namespace Dameng.Sep.Gen.Tests;

[GenSepParsable]
public partial class SimpleTestRecord
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}