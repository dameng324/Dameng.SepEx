# Dameng.SepEx

A C# source generator that provides a **CsvHelper-like API** for the high-performance [Sep](https://github.com/nietras/Sep) CSV library. Combine the familiar developer experience of CsvHelper with the blazing-fast performance of Sep.

[![Build Status](https://img.shields.io/github/actions/workflow/status/dameng324/Dameng.SepEx/ci.yml?branch=main)](https://github.com/dameng324/Dameng.SepEx/actions)
[![NuGet Version](https://img.shields.io/nuget/v/Dameng.SepEx)](https://www.nuget.org/packages/Dameng.SepEx/)
[![License](https://img.shields.io/github/license/dameng324/Dameng.SepEx)](LICENSE)

## 🚀 Quick Start

### Installation

Install via NuGet Package Manager:

```bash
dotnet add package Dameng.SepEx
```

Or via Package Manager Console:

```powershell
Install-Package Dameng.SepEx
```

> **Note**: This package includes a source generator that requires .NET 6+ and C# 11.0+. The source generator runs at compile-time and generates the necessary type information automatically.

### Basic Usage

1. **Define your data model** with attributes:

```csharp
using Dameng.SepEx;

public class Person
{
    [SepColumnName("Name")]
    public string Name { get; set; } = "";
    
    [SepColumnName("Age")]
    public int Age { get; set; }
    
    [SepColumnFormat("C")] // Currency formatting
    public decimal Salary { get; set; }
    
    [SepIgnore] // This field won't be included in CSV
    public string InternalId { get; set; } = "";
}
```

2. **Generate the type info** class:

```csharp
[GenSepTypeInfo<Person>()]
public partial class MyTypeInfo;
```

3. **Write CSV data**:

```csharp
using nietras.SeparatedValues;

var people = new List<Person>
{
    new() { Name = "Alice", Age = 30, Salary = 50000m },
    new() { Name = "Bob", Age = 25, Salary = 45000m }
};

var output = new StringBuilder();
using var writer = Sep.Writer().To(output);
writer.WriteRecord(people, MyTypeInfo.Person);

Console.WriteLine(output.ToString());
// Output:
// Name;Age;Salary
// Alice;30;¤50,000.00
// Bob;25;¤45,000.00
```

4. **Read CSV data**:

```csharp
var csvData = """
    Name;Age;Salary
    Alice;30;50000
    Bob;25;45000
    """;

using var reader = Sep.Reader().FromText(csvData);
foreach (var person in reader.GetRecords<Person>(MyTypeInfo.Person))
{
    Console.WriteLine($"Name: {person.Name}, Age: {person.Age}, Salary: {person.Salary:C}");
}
```

## 📚 Features

### ✅ **Attribute-Based Configuration**

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `SepColumnName` | Map to specific column name | `[SepColumnName("FirstName")]` |
| `SepColumnIndex` | Map to column by index | `[SepColumnIndex(0)]` |
| `SepDefaultValue` | Set default value when column is missing | `[SepDefaultValue("Unknown")]` |
| `SepColumnFormat` | Format output (write only) | `[SepColumnFormat("C")]` |
| `SepIgnore` | Exclude from CSV processing | `[SepIgnore]` |

### ✅ **Type Support**

- All primitive types (`string`, `int`, `double`, `decimal`, etc.)
- Types implementing `ISpanParsable<T>` (.NET 6+)
- Properties and fields
- Classes, structs, and records
- Primary constructor records
- Nullable types

### ✅ **Modern C# Features**

- Source generators for zero-runtime overhead
- Nullable reference types
- Records and primary constructors
- Span-based parsing for maximum performance

## 🔥 Performance

Dameng.SepEx leverages the Sep library, which is one of the fastest CSV libraries for .NET:

- **Up to 3x faster** than CsvHelper for reading
- **Up to 2x faster** than CsvHelper for writing  
- **Zero allocations** for primitive type parsing
- **Memory efficient** with span-based operations

## 🔄 Migration from CsvHelper

Dameng.SepEx provides similar attributes to CsvHelper for easy migration:

| CsvHelper | Dameng.SepEx | Notes |
|-----------|--------------|--------|
| `[Name("ColumnName")]` | `[SepColumnName("ColumnName")]` | Direct mapping |
| `[Index(0)]` | `[SepColumnIndex(0)]` | Direct mapping |
| `[Ignore]` | `[SepIgnore]` | Direct mapping |
| `[Format("C")]` | `[SepColumnFormat("C")]` | Write-only formatting |
| `[Default("value")]` | `[SepDefaultValue("value")]` | Read-only defaults |

## 📖 Advanced Examples

### Working with the Existing Test Models

Here's a complete working example based on the test models:

```csharp
using System;
using System.Collections.Generic;
using System.Text;
using Dameng.SepEx;
using nietras.SeparatedValues;

public class Record
{
    [SepColumnIndex(0)]
    [SepDefaultValue("default")]
    public string A { get; set; } = "";
    
    [SepColumnName("B")]
    public string B { get; set; } = "";

    [SepDefaultValue(10)]
    public int C { get; set; }

    [SepColumnFormat("C")]
    public double D { get; set; }

    [SepIgnore]
    public float E { get; set; }
}

[GenSepTypeInfo<Record>()]
public partial class MyTypeInfo;

class Program
{
    static void Main()
    {
        // Writing CSV
        var records = new List<Record>
        {
            new() { A = "Sep", B = "🚀", C = 1, D = 1.2, E = 0.1f },
            new() { A = "CSV", B = "✅", C = 2, D = 2.2, E = 0.2f }
        };

        var output = new StringBuilder();
        using var writer = Sep.Writer().To(output);
        writer.WriteRecord(records, MyTypeInfo.Record);
        
        Console.WriteLine("CSV Output:");
        Console.WriteLine(output.ToString());
        
        // Reading CSV  
        var csvData = """
            A;B;C;D;E;F
            Sep;🚀;1;1.2;0.1;0.5
            CSV;✅;2;2.2;0.2;1.5
            """;

        using var reader = Sep.Reader().FromText(csvData);
        Console.WriteLine("\nParsed Records:");
        foreach (var record in reader.GetRecords<Record>(MyTypeInfo.Record))
        {
            Console.WriteLine($"A: {record.A}, B: {record.B}, C: {record.C}, D: {record.D:C}");
        }
    }
}
```

### Record Types with Primary Constructor

```csharp
public record Product(
    [property: SepColumnName("ProductName")] string Name,
    [property: SepColumnFormat("C")] decimal Price,
    [property: SepDefaultValue(0)] int Stock
);

[GenSepTypeInfo<Product>()]
public partial class ProductTypeInfo;
```

### Mixed Column Mapping

```csharp
public class Employee
{
    [SepColumnIndex(0)]           // Map to first column
    public string Id { get; set; }
    
    [SepColumnName("FullName")]   // Map by name
    public string Name { get; set; }
    
    [SepColumnIndex(2)]           // Map to third column
    [SepDefaultValue(0.0)]        // Default if missing
    public double Salary { get; set; }
}
```

### Multiple Types in One Generator

```csharp
[GenSepTypeInfo<Person>()]
[GenSepTypeInfo<Product>()]
[GenSepTypeInfo<Employee>()]
public partial class AllTypeInfo;

// Use like:
// AllTypeInfo.Person
// AllTypeInfo.Product  
// AllTypeInfo.Employee
```

## 🛠️ Building from Source

```bash
git clone https://github.com/dameng324/Dameng.SepEx.git
cd Dameng.SepEx
dotnet build
dotnet test
```

## 🔧 Troubleshooting

### Source Generation Issues

If the generated properties (like `MyTypeInfo.Person`) are not available:

1. **Clean and rebuild**: `dotnet clean && dotnet build`
2. **Check target framework**: Ensure you're targeting .NET 6.0+ 
3. **Check C# language version**: Ensure you're using C# 11.0+
4. **IDE restart**: Sometimes IDEs need a restart to recognize generated code
5. **Check build output**: Look for any source generator warnings/errors

### Common Issues

- **Missing properties on TypeInfo class**: Usually resolved by clean rebuild
- **Nullable warnings**: Use `= ""` for string properties or make them nullable
- **Column not found**: Ensure CSV headers match `SepColumnName` values

## 📄 Requirements

- .NET 6.0 or later (for `ISpanParsable<T>` support)
- C# 11.0 or later (for source generators)

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes with tests
4. Submit a pull request

## 📝 License

This project is licensed under the [MIT License](LICENSE).

## 🙏 Acknowledgments

- [nietras/Sep](https://github.com/nietras/Sep) - The high-performance CSV library that powers this project
- [CsvHelper](https://joshclose.github.io/CsvHelper/) - Inspiration for the API design

---

**Made with ❤️ by [dameng324](https://github.com/dameng324)**