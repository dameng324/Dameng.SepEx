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

## Migration from CSVHelper

Dameng.SepEx is designed as a high-performance, NativeAOT-compatible alternative to CSVHelper. If you're currently using CSVHelper and want to take advantage of Native AOT compilation or better performance, this migration guide will help you transition smoothly.

### Why Migrate?

- **Native AOT Support**: Unlike CSVHelper, Dameng.SepEx fully supports Native AOT compilation
- **Better Performance**: Built on the fast Sep library for superior performance
- **Compile-Time Safety**: Source generator provides compile-time type checking
- **Smaller Runtime Footprint**: No reflection overhead in Native AOT scenarios

### Attribute Migration Guide

| CSVHelper Attribute | Dameng.SepEx Equivalent | Notes |
|---------------------|-------------------------|-------|
| `[Name("ColumnName")]` | `[SepColumnName("ColumnName")]` | Direct replacement |
| `[Index(0)]` | `[SepColumnIndex(0)]` | Direct replacement |
| `[Ignore]` | `[SepColumnIgnore]` | Direct replacement |
| `[Default("value")]` | `[SepDefaultValue("value")]` | Direct replacement |
| `[Format("format")]` | `[SepColumnFormat("format")]` | Direct replacement |

### Code Migration Example

**Before (CSVHelper):**
```csharp
public class Person
{
    [Name("FullName")]
    [Index(0)]
    public string Name { get; set; }
    
    [Name("Age")]
    public int Age { get; set; }
    
    [Default("Unknown")]
    public string City { get; set; }
    
    [Ignore]
    public string InternalId { get; set; }
}

// Usage
using var reader = new StringReader(csvContent);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var records = csv.GetRecords<Person>().ToList();
```

**After (Dameng.SepEx):**
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

// Usage
using var reader = Sep.Reader().FromText(csvContent);
var records = reader.GetRecords<Person>().ToList();
```

### Key Differences

1. **Partial Classes**: Your data models must be declared as `partial` and marked with `[GenSepParsable]`
2. **Source Generation**: Code is generated at compile time instead of runtime reflection
3. **Sep Library**: Uses the Sep library instead of CSVHelper's internal parsing
4. **Native AOT Ready**: No reflection means full Native AOT compatibility

### External Data Models

If you can't modify existing classes (e.g., from external libraries), use the type info approach:

```csharp
// For external classes you can't modify
[GenSepTypeInfo<ExternalPerson>()]
public partial class ExternalPersonTypeInfo;

// Usage
using var reader = Sep.Reader().FromFile("people.csv");
foreach (var person in reader.GetRecords<ExternalPerson>(ExternalPersonTypeInfo.ExternalPerson))
{
    // Process person
}
```

## Installation

Install the NuGet packages:

```bash
dotnet add package Dameng.SepEx
dotnet add package Dameng.SepEx.Generator
```

## Quick Start

1. Define your data model with attributes:

```csharp
[GenSepParsable]
public partial class Person
{
    [SepColumnIndex(0)]
    public string Name { get; set; }
    
    [SepColumnName("Age")]
    public int Age { get; set; }
    
    [SepDefaultValue("Unknown")]
    public string City { get; set; }
    
    [SepColumnIgnore]
    public string InternalId { get; set; }
}
```

2. Read and write CSV data:

```csharp
// Reading
using var reader = Sep.Reader().FromFile("people.csv");
foreach (var person in reader.GetRecords<Person>())
{
    Console.WriteLine($"{person.Name} is {person.Age} years old");
}

// Writing
var people = new List<Person> { /* ... */ };
using var writer = Sep.Writer().ToFile("output.csv");
writer.WriteRecords(people);
```

## External Data Model

1. Generate type info using attributes:

```csharp
[GenSepTypeInfo<Person>()]
public partial class PersonTypeInfo;
```

2. Read and write CSV data:

```csharp
// Reading
using var reader = Sep.Reader().FromFile("people.csv");
foreach (var person in reader.GetRecords<Person>(PersonTypeInfo.Person))
{
    Console.WriteLine($"{person.Name} is {person.Age} years old");
}

// Writing
var people = new List<Person> { /* ... */ };
using var writer = Sep.Writer().ToFile("output.csv");
writer.WriteRecords(people, PersonTypeInfo.Person);
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