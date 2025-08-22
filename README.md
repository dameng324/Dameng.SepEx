# Dameng.SepEx

A high-performance source generator for working with separated values (CSV/TSV) files in C#. This library extends the excellent [Sep](https://github.com/nietras/Sep) library by providing compile-time code generation for strongly-typed reading and writing of CSV data.

## Features

- **Source Generator**: Automatically generates type-safe CSV readers and writers at compile time
- **High Performance**: Built on top of the fast Sep library
- **Attribute-Based Configuration**: Use attributes to configure column mapping, formatting, and behavior
- **Flexible Column Mapping**: Support for column names, indexes, default values, and formatting
- **Type Safety**: Compile-time type checking and IntelliSense support

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
    
    [SepIgnore]
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
writer.WriteRecord(people);
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
writer.WriteRecord(people, PersonTypeInfo.Person);
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