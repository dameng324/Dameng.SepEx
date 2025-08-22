# GenSepParsable Record/Struct Support Examples

This demonstrates the support for using `[GenSepParsable]` with various record and struct types.

## Usage Examples

### Struct
```csharp
[GenSepParsable]
public partial struct PersonStruct
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

### Record
```csharp
[GenSepParsable]
public partial record PersonRecord
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
```

### Record Struct
```csharp
[GenSepParsable]
public partial record struct PersonRecordStruct
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

### Readonly Struct
```csharp
[GenSepParsable]
public readonly partial struct PersonReadonlyStruct
{
    public string Name { get; init; }
    public int Age { get; init; }
}
```

### Record with Primary Constructor
```csharp
[GenSepParsable]
public partial record Person(string Name, int Age, string Email);
```

## Requirements

All types must be declared as `partial` to work with the source generator:

✅ `public partial record MyRecord` - Works  
❌ `public record MyRecord` - Does not work  

✅ `public partial struct MyStruct` - Works  
❌ `public struct MyStruct` - Does not work  

The generator will automatically implement `ISepParsable<T>` for the type, providing `Read` and `Write` methods for CSV serialization/deserialization.