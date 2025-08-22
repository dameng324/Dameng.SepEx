using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Dameng.Sep.Gen;

[Generator]
public class SepTypeInfoGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    private static void GeneratePropertyCode(
        INamedTypeSymbol classSymbol, 
        INamedTypeSymbol targetType, 
        INamedTypeSymbol spanParsableInterface,
        StringBuilder propertyReadCodeBuilder, 
        StringBuilder propertyWriteCodeBuilder, 
        GeneratorExecutionContext context)
    {
        bool hasPrimaryConstructor =
            targetType.Constructors.Any(c =>
                c.IsImplicitlyDeclared == false && c.Parameters.Length > 0
            );

        int propertyIndex = 0;

        foreach (var member in targetType.GetMembers())
        {
            if (member is not (IPropertySymbol or IFieldSymbol))
            {
                continue;
            }

            var memberName = member.Name;
            var memberType =
                member switch
                {
                    IPropertySymbol property => property.Type,
                    IFieldSymbol field => field.Type,
                    _ => null,
                };

            if (memberType is null)
            {
                continue;
            }

            var isWritable =
                member switch
                {
                    IPropertySymbol property => property.SetMethod is not null,
                    IFieldSymbol field => !field.IsReadOnly,
                    _ => false,
                };

            var isReadable =
                member switch
                {
                    IPropertySymbol property => property.GetMethod is not null,
                    IFieldSymbol field => true,
                    _ => false,
                };

            var ignoreAttribute = member.GetAttributes()
                .FirstOrDefault(o =>
                    o.AttributeClass?.ToDisplayString()
                        .Equals("Dameng.SepEx.SepIgnoreAttribute")
                    == true
                );
            if (ignoreAttribute is not null)
            {
                continue;
            }

            var isNullable =
                memberType.OriginalDefinition.SpecialType
                    == SpecialType.System_Nullable_T
                || memberType.CanBeReferencedByName;

            var underlyingType = isNullable
                ? memberType.OriginalDefinition.SpecialType
                    == SpecialType.System_Nullable_T
                    ? ((INamedTypeSymbol)memberType).TypeArguments[0]
                    : memberType
                : memberType;

            if (underlyingType.TypeKind == TypeKind.Enum)
            {
                var defaultValueAttributeValue = member
                    .GetAttributes()
                    .FirstOrDefault(o =>
                        o.AttributeClass?.ToDisplayString()
                            .Equals("Dameng.SepEx.SepDefaultValueAttribute")
                        == true
                    )
                    ?.ConstructorArguments.FirstOrDefault()
                    .Value;
                var defaultValue =
                    defaultValueAttributeValue is null
                        ? $"default({memberType.ToDisplayString()})"
                        : defaultValueAttributeValue.ToString();

                var columnName =
                    member
                        .GetAttributes()
                        .FirstOrDefault(o =>
                            o.AttributeClass?.ToDisplayString()
                                .Equals("Dameng.SepEx.SepColumnNameAttribute")
                            == true
                        )
                        ?.ConstructorArguments.FirstOrDefault()
                        .Value?.ToString() ?? memberName;
                var columnIndex = member
                    .GetAttributes()
                    .FirstOrDefault(o =>
                        o.AttributeClass?.ToDisplayString()
                            .Equals("Dameng.SepEx.SepColumnIndexAttribute")
                        == true
                    )
                    ?.ConstructorArguments.FirstOrDefault()
                    .Value?.ToString();

                string readColKey = columnIndex ?? $"\"{columnName}\"";
                string writeColKey = $"\"{columnName}\"";

                if (isWritable)
                {
                    if (hasPrimaryConstructor)
                    {
                        propertyReadCodeBuilder.AppendLine(
                            $"            System.Enum.TryParse<{underlyingType.ToDisplayString()}>(readRow[{readColKey}].ToString(), out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                        );
                    }
                    else
                    {
                        propertyReadCodeBuilder.AppendLine(
                            $"            {memberName} = System.Enum.TryParse<{underlyingType.ToDisplayString()}>(readRow[{readColKey}].ToString(), out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                        );
                    }
                }

                if (isReadable)
                {
                    string valueString;
                    if (isNullable)
                    {
                        valueString =
                            $"Set(value.{memberName}?.ToString() ?? string.Empty)";
                    }
                    else
                    {
                        valueString = $"Set(value.{memberName}.ToString())";
                    }

                    propertyWriteCodeBuilder.AppendLine(
                        $"        writeRow[{writeColKey}].{valueString};"
                    );
                }
            }
            else
            {
                var spanParsableDefine =
                    underlyingType.AllInterfaces.FirstOrDefault(i =>
                        SymbolEqualityComparer.Default.Equals(
                            i.OriginalDefinition,
                            spanParsableInterface
                        )
                    );

                if (
                    spanParsableDefine is not null
                    && spanParsableDefine.TypeArguments.Length == 1
                    && SymbolEqualityComparer.Default.Equals(
                        spanParsableDefine.TypeArguments[0],
                        underlyingType
                    )
                )
                {
                    propertyIndex++;
                    var defaultValueAttributeValue = member
                        .GetAttributes()
                        .FirstOrDefault(o =>
                            o.AttributeClass?.ToDisplayString()
                                .Equals("Dameng.SepEx.SepDefaultValueAttribute")
                            == true
                        )
                        ?.ConstructorArguments.FirstOrDefault()
                        .Value;
                    var defaultValue =
                        memberType.SpecialType == SpecialType.System_String
                            ? defaultValueAttributeValue is null
                                ? "string.Empty"
                                : $"\"{defaultValueAttributeValue}\""
                            : isNullable
                                ? defaultValueAttributeValue is null
                                    ? "null"
                                    : $"({underlyingType.ToDisplayString()}?){defaultValueAttributeValue}"
                                : defaultValueAttributeValue is null
                                    ? "default("
                                        + memberType.ToDisplayString()
                                        + ")"
                                    : defaultValueAttributeValue.ToString();

                    var columnName =
                        member
                            .GetAttributes()
                            .FirstOrDefault(o =>
                                o.AttributeClass?.ToDisplayString()
                                    .Equals("Dameng.SepEx.SepColumnNameAttribute")
                                == true
                            )
                            ?.ConstructorArguments.FirstOrDefault()
                            .Value?.ToString() ?? memberName;
                    var columnIndex = member
                        .GetAttributes()
                        .FirstOrDefault(o =>
                            o.AttributeClass?.ToDisplayString()
                                .Equals("Dameng.SepEx.SepColumnIndexAttribute")
                            == true
                        )
                        ?.ConstructorArguments.FirstOrDefault()
                        .Value?.ToString();

                    string readColKey = columnIndex ?? $"\"{columnName}\"";
                    string writeColKey = $"\"{columnName}\"";

                    if (isWritable)
                    {
                        if (hasPrimaryConstructor)
                        {
                            propertyReadCodeBuilder.AppendLine(
                                $"            readRow[{readColKey}].TryParse<{underlyingType.ToDisplayString()}>(out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                            );
                        }
                        else
                        {
                            propertyReadCodeBuilder.AppendLine(
                                $"            {memberName} = readRow[{readColKey}].TryParse<{underlyingType.ToDisplayString()}>(out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                            );
                        }
                    }

                    if (isReadable)
                    {
                        string valueString;
                        if (
                            (isNullable ? underlyingType : memberType).SpecialType
                            == SpecialType.System_String
                        )
                        {
                            if (isNullable)
                            {
                                valueString =
                                    $"Set(value.{memberName}?.ToString() ?? string.Empty)";
                            }
                            else
                            {
                                valueString = $"Set(value.{memberName})";
                            }
                        }
                        else
                        {
                            var format = member
                                .GetAttributes()
                                .FirstOrDefault(o =>
                                    o.AttributeClass?.ToDisplayString()
                                        .Equals(
                                            "Dameng.SepEx.SepColumnFormatAttribute"
                                        ) == true
                                )
                                ?.ConstructorArguments.FirstOrDefault()
                                .Value?.ToString();

                            if (isNullable)
                            {
                                // For now, always use Set with ToString for nullable types to avoid Format constraints
                                valueString = string.IsNullOrWhiteSpace(format)
                                    ? $"Set(value.{memberName}?.ToString() ?? string.Empty)"
                                    : $"Set(value.{memberName}?.ToString(\"{format}\") ?? string.Empty)";
                            }
                            else
                            {
                                valueString = string.IsNullOrWhiteSpace(format)
                                    ? $"Format(value.{memberName})"
                                    : $"Set(value.{memberName}.ToString(\"{format}\"))";
                            }
                        }

                        propertyWriteCodeBuilder.AppendLine(
                            $"        writeRow[{writeColKey}].{valueString};"
                        );
                    }
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "SP002",
                                "Unsupported Property Type",
                                $"Property '{member.Name}' of type '{memberType.ToDisplayString()}' is not supported. Only ISpanParsable<T> and enum types are supported.",
                                "Usage",
                                DiagnosticSeverity.Error,
                                true
                            ),
                            Location.None
                        )
                    );
                }
            }
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // #if DEBUG
        //         if(!Debugger.IsAttached)
        //         {
        //             Debugger.Launch();
        //         }
        // #endif
        var spanParsableInterface = context.Compilation.GetTypeByMetadataName(
            "System.ISpanParsable`1"
        );
        if (spanParsableInterface is null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "SP001",
                        "Missing ISpanParsable",
                        "The ISpanParsable interface is not available. Ensure you are targeting .NET 6 or later.",
                        "Usage",
                        DiagnosticSeverity.Error,
                        true
                    ),
                    Location.None
                )
            );

            return;
        }

        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
            foreach (var typeDeclaration in syntaxTree.GetRoot().DescendantNodesAndSelf())
            {
                if (typeDeclaration is not ClassDeclarationSyntax and not StructDeclarationSyntax)
                    continue;
                var classSymbol =
                    semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;

                if (classSymbol is null)
                {
                    continue;
                }

                var attributes = classSymbol.GetAttributes();
                
                // Handle GenSepTypeInfo attributes
                var typeInfoAttributes = attributes
                    .Where(attribute =>
                        attribute
                            .AttributeClass!.ToDisplayString()
                            .StartsWith("Dameng.SepEx.GenSepTypeInfoAttribute<")
                    )
                    .ToArray();
                    
                // Handle GenSepParsable attribute
                var genParsableAttribute = attributes
                    .FirstOrDefault(attribute =>
                        attribute
                            .AttributeClass!.ToDisplayString()
                            .Equals("Dameng.SepEx.GenSepParsableAttribute")
                    );
                
                // Skip if no relevant attributes found
                if (typeInfoAttributes.Length <= 0 && genParsableAttribute is null)
                {
                    continue;
                }

                try
                {
                    StringBuilder genClassCodeBuilder = new StringBuilder();
                    genClassCodeBuilder.AppendLine(
                        $$"""
                        // <auto-generated at {{DateTimeOffset.Now:O}}/>
                        using System;
                        using Dameng.SepEx;

                        namespace {{classSymbol.ContainingNamespace.ToDisplayString()}};

                        """
                    );

                    List<INamedTypeSymbol> targetTypeNames = new();
                    foreach (var attribute in typeInfoAttributes)
                    {
                        var targetType =
                            attribute.AttributeClass!.TypeArguments[0] as INamedTypeSymbol;
                        if (targetType is null)
                        {
                            continue;
                        }

                        StringBuilder propertyReadCodeBuilder = new StringBuilder();
                        StringBuilder propertyWriteCodeBuilder = new StringBuilder();

                        bool hasPrimaryConstructor =
                            targetType.InstanceConstructors.FirstOrDefault(c =>
                                c.IsImplicitlyDeclared == false && c.Parameters.Length > 0
                            )
                                is not null;

                        int propertyIndex = 0;
                        foreach (var member in targetType.GetMembers())
                        {
                            string memberName;
                            bool isReadable = true;
                            bool isWritable = true;
                            ITypeSymbol memberType;
                            if (
                                member is IPropertySymbol
                                {
                                    IsStatic: false,
                                    IsImplicitlyDeclared: false
                                } property
                            )
                            {
                                memberName = property.Name;
                                memberType = property.Type;
                                isReadable = !property.IsWriteOnly;
                                isWritable = !property.IsReadOnly;
                            }
                            else if (
                                member is IFieldSymbol
                                {
                                    IsStatic: false,
                                    IsImplicitlyDeclared: false
                                } field
                            )
                            {
                                memberName = field.Name;
                                memberType = field.Type;
                            }
                            else
                            {
                                continue;
                            }

                            if (
                                member
                                    .GetAttributes()
                                    .FirstOrDefault(o =>
                                        o.AttributeClass?.ToDisplayString()
                                            .Contains("Dameng.SepEx.SepIgnoreAttribute") == true
                                    )
                                is not null
                            )
                            {
                                propertyReadCodeBuilder.AppendLine(
                                    "            // Ignored: " + memberName
                                );
                                propertyWriteCodeBuilder.AppendLine(
                                    "        // Ignored: " + memberName
                                );
                                continue;
                            }

                            // Check if it's a nullable type first
                            bool isNullable = false;
                            ITypeSymbol underlyingType = memberType;
                            if (
                                memberType.OriginalDefinition?.SpecialType
                                == SpecialType.System_Nullable_T
                            )
                            {
                                isNullable = true;
                                underlyingType = ((INamedTypeSymbol)memberType).TypeArguments[0];
                            }

                            // Check if it's an enum type
                            bool isEnum = underlyingType.TypeKind == TypeKind.Enum;

                            if (isEnum)
                            {
                                // Handle enum types specially
                                propertyIndex++;

                                var defaultValueAttributeValue = member
                                    .GetAttributes()
                                    .FirstOrDefault(o =>
                                        o.AttributeClass?.ToDisplayString()
                                            .Equals("Dameng.SepEx.SepDefaultValueAttribute") == true
                                    )
                                    ?.ConstructorArguments.FirstOrDefault()
                                    .Value;

                                var defaultValue = isNullable
                                    ? defaultValueAttributeValue is null
                                        ? "null"
                                        : $"({underlyingType.ToDisplayString()}?){defaultValueAttributeValue}"
                                    : defaultValueAttributeValue is null
                                        ? "default(" + memberType.ToDisplayString() + ")"
                                        : defaultValueAttributeValue.ToString();

                                var columnName =
                                    member
                                        .GetAttributes()
                                        .FirstOrDefault(o =>
                                            o.AttributeClass?.ToDisplayString()
                                                .Equals("Dameng.SepEx.SepColumnNameAttribute")
                                            == true
                                        )
                                        ?.ConstructorArguments.FirstOrDefault()
                                        .Value?.ToString() ?? memberName;
                                var columnIndex = member
                                    .GetAttributes()
                                    .FirstOrDefault(o =>
                                        o.AttributeClass?.ToDisplayString()
                                            .Equals("Dameng.SepEx.SepColumnIndexAttribute") == true
                                    )
                                    ?.ConstructorArguments.FirstOrDefault()
                                    .Value?.ToString();

                                string readColKey = columnIndex ?? $"\"{columnName}\"";
                                string writeColKey = $"\"{columnName}\"";

                                if (isWritable)
                                {
                                    if (hasPrimaryConstructor)
                                    {
                                        propertyReadCodeBuilder.AppendLine(
                                            $"            System.Enum.TryParse<{underlyingType.ToDisplayString()}>(readRow[{readColKey}].ToString(), out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                                        );
                                    }
                                    else
                                    {
                                        propertyReadCodeBuilder.AppendLine(
                                            $"            {memberName} = System.Enum.TryParse<{underlyingType.ToDisplayString()}>(readRow[{readColKey}].ToString(), out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                                        );
                                    }
                                }

                                if (isReadable)
                                {
                                    string valueString;
                                    if (isNullable)
                                    {
                                        valueString =
                                            $"Set(value.{memberName}?.ToString() ?? string.Empty)";
                                    }
                                    else
                                    {
                                        valueString = $"Set(value.{memberName}.ToString())";
                                    }

                                    propertyWriteCodeBuilder.AppendLine(
                                        $"        writeRow[{writeColKey}].{valueString};"
                                    );
                                }
                            }
                            else
                            {
                                var spanParsableDefine =
                                    underlyingType.AllInterfaces.FirstOrDefault(i =>
                                        SymbolEqualityComparer.Default.Equals(
                                            i.OriginalDefinition,
                                            spanParsableInterface
                                        )
                                    );

                                if (
                                    spanParsableDefine is not null
                                    && spanParsableDefine.TypeArguments.Length == 1
                                    && SymbolEqualityComparer.Default.Equals(
                                        spanParsableDefine.TypeArguments[0],
                                        underlyingType
                                    )
                                )
                                {
                                    propertyIndex++;

                                    var defaultValueAttributeValue = member
                                        .GetAttributes()
                                        .FirstOrDefault(o =>
                                            o.AttributeClass?.ToDisplayString()
                                                .Equals("Dameng.SepEx.SepDefaultValueAttribute")
                                            == true
                                        )
                                        ?.ConstructorArguments.FirstOrDefault()
                                        .Value;
                                    var defaultValue =
                                        memberType.SpecialType == SpecialType.System_String
                                            ? defaultValueAttributeValue is null
                                                ? "string.Empty"
                                                : $"\"{defaultValueAttributeValue}\""
                                            : isNullable
                                                ? defaultValueAttributeValue is null
                                                    ? "null"
                                                    : $"({underlyingType.ToDisplayString()}?){defaultValueAttributeValue}"
                                                : defaultValueAttributeValue is null
                                                    ? "default("
                                                        + memberType.ToDisplayString()
                                                        + ")"
                                                    : defaultValueAttributeValue.ToString();

                                    var columnName =
                                        member
                                            .GetAttributes()
                                            .FirstOrDefault(o =>
                                                o.AttributeClass?.ToDisplayString()
                                                    .Equals("Dameng.SepEx.SepColumnNameAttribute")
                                                == true
                                            )
                                            ?.ConstructorArguments.FirstOrDefault()
                                            .Value?.ToString() ?? memberName;
                                    var columnIndex = member
                                        .GetAttributes()
                                        .FirstOrDefault(o =>
                                            o.AttributeClass?.ToDisplayString()
                                                .Equals("Dameng.SepEx.SepColumnIndexAttribute")
                                            == true
                                        )
                                        ?.ConstructorArguments.FirstOrDefault()
                                        .Value?.ToString();

                                    string readColKey = columnIndex ?? $"\"{columnName}\"";
                                    string writeColKey = $"\"{columnName}\"";

                                    if (isWritable)
                                    {
                                        if (hasPrimaryConstructor)
                                        {
                                            propertyReadCodeBuilder.AppendLine(
                                                $"            readRow[{readColKey}].TryParse<{underlyingType.ToDisplayString()}>(out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                                            );
                                        }
                                        else
                                        {
                                            propertyReadCodeBuilder.AppendLine(
                                                $"            {memberName} = readRow[{readColKey}].TryParse<{underlyingType.ToDisplayString()}>(out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                                            );
                                        }
                                    }

                                    if (isReadable)
                                    {
                                        string valueString;
                                        if (
                                            (isNullable ? underlyingType : memberType).SpecialType
                                            == SpecialType.System_String
                                        )
                                        {
                                            if (isNullable)
                                            {
                                                valueString =
                                                    $"Set(value.{memberName}?.ToString() ?? string.Empty)";
                                            }
                                            else
                                            {
                                                valueString = $"Set(value.{memberName})";
                                            }
                                        }
                                        else
                                        {
                                            var format = member
                                                .GetAttributes()
                                                .FirstOrDefault(o =>
                                                    o.AttributeClass?.ToDisplayString()
                                                        .Equals(
                                                            "Dameng.SepEx.SepColumnFormatAttribute"
                                                        ) == true
                                                )
                                                ?.ConstructorArguments.FirstOrDefault()
                                                .Value?.ToString();

                                            if (isNullable)
                                            {
                                                // For now, always use Set with ToString for nullable types to avoid Format constraints
                                                valueString = string.IsNullOrWhiteSpace(format)
                                                    ? $"Set(value.{memberName}?.ToString() ?? string.Empty)"
                                                    : $"Set(value.{memberName}?.ToString(\"{format}\") ?? string.Empty)";
                                            }
                                            else
                                            {
                                                valueString = string.IsNullOrWhiteSpace(format)
                                                    ? $"Format(value.{memberName})"
                                                    : $"Set(value.{memberName}.ToString(\"{format}\"))";
                                            }
                                        }

                                        propertyWriteCodeBuilder.AppendLine(
                                            $"        writeRow[{writeColKey}].{valueString};"
                                        );
                                    }
                                }
                                else
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            new DiagnosticDescriptor(
                                                "SP002",
                                                "Unsupported Property Type",
                                                $"The property '{memberName}' of type '{memberType}' is not support ISpanParsable<TSelf>.",
                                                "Usage",
                                                DiagnosticSeverity.Error,
                                                true
                                            ),
                                            Location.None
                                        )
                                    );
                                }
                            }
                        }

                        targetTypeNames.Add(targetType);

                        var instanceInitCode = hasPrimaryConstructor
                            ? $$"""
                                        // PrimaryConstructor:{{hasPrimaryConstructor}}
                                        return new {{targetType.ToDisplayString()}}(
                                {{propertyReadCodeBuilder.ToString().TrimEnd().TrimEnd(',')}}
                                        );
                                """
                            : $$"""
                                        // PrimaryConstructor:{{hasPrimaryConstructor}}
                                        return new {{targetType.ToDisplayString()}}() 
                                        {
                                {{propertyReadCodeBuilder.ToString().TrimEnd().TrimEnd(',')}}
                                        };
                                """;

                        genClassCodeBuilder.AppendLine(
                            $$"""
                            file sealed class {{targetType.Name}}SepTypeInfo : ISepTypeInfo<{{targetType.ToDisplayString()}}>
                            {
                                public {{targetType.ToDisplayString()}} Read(nietras.SeparatedValues.SepReader.Row readRow) 
                                {
                            {{instanceInitCode}}
                                }

                                public void Write(nietras.SeparatedValues.SepWriter.Row writeRow, {{targetType.ToDisplayString()}} value)
                                {
                            {{propertyWriteCodeBuilder.ToString().TrimEnd()}}
                                }
                            }
                            """
                        );
                    }

                    var accessibility =
                        classSymbol.DeclaredAccessibility is Accessibility.Public
                            ? "public"
                            : "internal";

                    StringBuilder staticTypeInfoPropertyCodeBuilder = new();

                    foreach (var targetTypeName in targetTypeNames)
                    {
                        staticTypeInfoPropertyCodeBuilder.AppendLine(
                            $$"""
                                public static ISepTypeInfo<{{targetTypeName.ToDisplayString()}}> {{targetTypeName.Name}} { get; } = new {{targetTypeName.Name}}SepTypeInfo();
                            """
                        );
                    }

                    // Generate ISepParsable implementation if GenSepParsable attribute is present
                    if (genParsableAttribute is not null)
                    {
                        var propertyReadCodeBuilder = new StringBuilder();
                        var propertyWriteCodeBuilder = new StringBuilder();
                        GeneratePropertyCode(classSymbol, classSymbol, spanParsableInterface, 
                                           propertyReadCodeBuilder, propertyWriteCodeBuilder, context);

                        bool hasPrimaryConstructor =
                            classSymbol.Constructors.Any(c =>
                                c.IsImplicitlyDeclared == false && c.Parameters.Length > 0
                            );

                        var instanceInitCode = hasPrimaryConstructor
                            ? $$"""
                                            return new {{classSymbol.ToDisplayString()}}(
                                {{propertyReadCodeBuilder.ToString().TrimEnd().TrimEnd(',')}}
                                            );
                                """
                            : $$"""
                                            return new {{classSymbol.ToDisplayString()}}() 
                                            {
                                {{propertyReadCodeBuilder.ToString().TrimEnd().TrimEnd(',')}}
                                            };
                                """;

                        genClassCodeBuilder.Append(
                            $$"""
                            {{accessibility}} partial class {{classSymbol.Name}} : ISepParsable<{{classSymbol.ToDisplayString()}}>
                            {
                                public static {{classSymbol.ToDisplayString()}} Read(nietras.SeparatedValues.SepReader.Row readRow) 
                                {
                            {{instanceInitCode}}
                                }

                                public static void Write(nietras.SeparatedValues.SepWriter.Row writeRow, {{classSymbol.ToDisplayString()}} value)
                                {
                            {{propertyWriteCodeBuilder.ToString().TrimEnd()}}
                                }
                            }
                            
                            """
                        );
                    }
                    else
                    {
                        genClassCodeBuilder.Append(
                            $$"""
                            {{accessibility}} partial class {{classSymbol.Name}}
                            {
                            {{staticTypeInfoPropertyCodeBuilder.ToString().TrimEnd()}}
                            }
                            """
                        );
                    }

                    context.AddSource(
                        classSymbol.Name + ".g.cs",
                        SourceText.From(genClassCodeBuilder.ToString(), Encoding.UTF8)
                    );
                }
                catch (Exception e)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "SP003",
                                "Code Generation Error",
                                $"An error occurred while generating code for '{classSymbol.Name}': {e}",
                                "Usage",
                                DiagnosticSeverity.Error,
                                true
                            ),
                            Location.None
                        )
                    );
                }
            }
        }
    }
}
