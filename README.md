# Dameng.SepEx

A high-performance, **Native AOT compatible** source generator for working with separated values (CSV/TSV) files in C#. This library extends the excellent [Sep](https://github.com/nietras/Sep) library by providing compile-time code generation for strongly-typed reading and writing of CSV data.

**Perfect for developers migrating from CSVHelper** who need Native AOT support or want better performance through source generation.

## Features

- **Source Generator**: Automatically generates type-safe CSV readers and writers at compile time
- **High Performance**: Built on top of the fast Sep library
- **Attribute-Based Configuration**: Use attributes to configure column mapping, formatting, and behavior
- **Flexible Column Mapping**: Support for column names, indexes, default values, and formatting
- **Type Safety**: Compile-time type checking and IntelliSense support
- **NativeAOT Compatible**: Full support for Native AOT compilation, unlike CSVHelper

## Installation

Install the NuGet packages:

```bash
dotnet add package Dameng.SepEx
dotnet add package Dameng.SepEx.Generator
```

## Usage & Migration

### Basic Usage

Define your data model as a partial class with `[GenSepParsable]` and use attributes to configure column mapping:

```csharp
[GenSepParsable]
public partial class Person
{
    [SepColumnName("FullName")]
    [SepColumnIndex(0)]
    public string Name { get; set; }
    
    [SepColumnName("Age")]
    public int Age { get; set; }
    
    [SepDefaultValue("Unknown")]
    public string City { get; set; }
    
    [SepColumnIgnore]
    public string InternalId { get; set; }
}

// Reading CSV
using var reader = Sep.Reader().FromFile("people.csv");
foreach (var person in reader.GetRecords<Person>())
{
    Console.WriteLine($"{person.Name} is {person.Age} years old");
}

// Writing CSV
var people = new List<Person> { /* ... */ };
using var writer = Sep.Writer().ToFile("output.csv");
writer.WriteRecords(people);
```

### Migrating from CSVHelper

If you're migrating from CSVHelper, simply update your attributes and make classes partial:

| CSVHelper | Dameng.SepEx |
|-----------|--------------|
| `[Name("column")]` | `[SepColumnName("column")]` |
| `[Index(0)]` | `[SepColumnIndex(0)]` |
| `[Ignore]` | `[SepColumnIgnore]` |
| `[Default("value")]` | `[SepDefaultValue("value")]` |
| `[Format("format")]` | `[SepColumnFormat("format")]` |

**Key changes:**
- Add `[GenSepParsable]` attribute and make class `partial`
- Replace CSVHelper attributes with Dameng.SepEx equivalents
- Use `Sep.Reader()` and `Sep.Writer()` instead of CSVHelper APIs

### External Data Models

For classes you can't modify (e.g., from external libraries), use type info generation:

```csharp
[GenSepTypeInfo<ExternalPerson>()]
public partial class ExternalPersonTypeInfo;

// Usage
using var reader = Sep.Reader().FromFile("people.csv");
foreach (var person in reader.GetRecords<ExternalPerson>(ExternalPersonTypeInfo.ExternalPerson))
{
    // Process person
}
```

## Available Attributes

- `[GenSepTypeInfo<T>()]`: Generate type info for the specified type
- `[SepColumnName(string name)]`: Specify column name
- `[SepColumnIndex(int index)]`: Specify column index for reading/writing order
- `[SepDefaultValue(object value)]`: Set default value when column is missing
- `[SepColumnIgnore]`: Ignore property/field during CSV operations
- `[SepColumnFormat(string format)]`: Specify format for writing values

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.