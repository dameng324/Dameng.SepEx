using System.Text;
using Microsoft.CodeAnalysis;

namespace Dameng.SepEx.Generator;

public static class Utils
{
    internal static (string ReadCode, string WriteCode) GeneratePropertyCode(
        INamedTypeSymbol targetType,
        GeneratorExecutionContext context
    )
    {
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

            throw new Exception(
                "The ISpanParsable interface is not available. Ensure you are targeting .NET 6 or later."
            );
        }

        var spanFormattableInterface = context.Compilation.GetTypeByMetadataName(
            "System.ISpanFormattable"
        );
        if (spanFormattableInterface is null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "SP001",
                        "Missing ISpanFormattable",
                        "The ISpanFormattable interface is not available. Ensure you are targeting .NET 6 or later.",
                        "Usage",
                        DiagnosticSeverity.Error,
                        true
                    ),
                    Location.None
                )
            );

            throw new Exception(
                "The ISpanFormattable interface is not available. Ensure you are targeting .NET 6 or later."
            );
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
            if (member is IPropertySymbol { IsStatic: false, IsImplicitlyDeclared: false } property)
            {
                memberName = property.Name;
                memberType = property.Type;
                isReadable = !property.IsWriteOnly;
                isWritable = !property.IsReadOnly;
            }
            else if (member is IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false } field)
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
                propertyReadCodeBuilder.AppendLine("            // Ignored: " + memberName);
                propertyWriteCodeBuilder.AppendLine("        // Ignored: " + memberName);
                continue;
            }

            // Check if it's a nullable type first
            bool isNullable = false;
            ITypeSymbol underlyingType = memberType;
            if (memberType.OriginalDefinition?.SpecialType == SpecialType.System_Nullable_T)
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
                                .Equals("Dameng.SepEx.SepColumnNameAttribute") == true
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
                            $"            System.Enum.TryParse<{underlyingType.ToDisplayString()}>(Dameng.SepEx.Parser.UnescapeSepField(readRow[{readColKey}].Span), out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                        );
                    }
                    else
                    {
                        propertyReadCodeBuilder.AppendLine(
                            $"            {memberName} = System.Enum.TryParse<{underlyingType.ToDisplayString()}>(Dameng.SepEx.Parser.UnescapeSepField(readRow[{readColKey}].Span), out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue},"
                        );
                    }
                }

                if (isReadable)
                {
                    string valueCode;
                    if (isNullable)
                    {
                        valueCode = $"Set(value.{memberName}?.ToString() ?? string.Empty)";
                    }
                    else
                    {
                        valueCode = $"Set(value.{memberName}.ToString())";
                    }

                    propertyWriteCodeBuilder.AppendLine(
                        $"        writeRow[{writeColKey}].{valueCode};"
                    );
                }
            }
            else
            {
                var spanParsableDefine = underlyingType.AllInterfaces.FirstOrDefault(i =>
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
                                .Equals("Dameng.SepEx.SepDefaultValueAttribute") == true
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
                                    ? "default(" + memberType.ToDisplayString() + ")"
                                    : defaultValueAttributeValue.ToString();

                    var columnName =
                        member
                            .GetAttributes()
                            .FirstOrDefault(o =>
                                o.AttributeClass?.ToDisplayString()
                                    .Equals("Dameng.SepEx.SepColumnNameAttribute") == true
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
                        var valueCode =
                            underlyingType.SpecialType == SpecialType.System_String
                                ? $"Dameng.SepEx.Parser.UnescapeSepField(readRow[{readColKey}].Span).ToString()"
                                : $"{underlyingType.ToDisplayString()}.TryParse(Dameng.SepEx.Parser.UnescapeSepField(readRow[{readColKey}].Span),out var v{propertyIndex}) ? v{propertyIndex} : {defaultValue}";
                        propertyReadCodeBuilder.AppendLine(
                            hasPrimaryConstructor
                                ? $"            {valueCode},"
                                : $"            {memberName} = {valueCode},"
                        );
                    }

                    if (isReadable)
                    {
                        string valueString;
                        if (underlyingType.SpecialType == SpecialType.System_String)
                        {
                            if (isNullable)
                            {
                                valueString = $"value.{memberName}?.ToString() ?? string.Empty";
                            }
                            else
                            {
                                valueString = $"value.{memberName}";
                            }
                        }
                        else
                        {
                            var format = member
                                .GetAttributes()
                                .FirstOrDefault(o =>
                                    o.AttributeClass?.ToDisplayString()
                                        .Equals("Dameng.SepEx.SepColumnFormatAttribute") == true
                                )
                                ?.ConstructorArguments.FirstOrDefault()
                                .Value?.ToString();

                            if (isNullable)
                            {
                                // For now, always use Set with ToString for nullable types to avoid Format constraints
                                valueString = string.IsNullOrWhiteSpace(format)
                                    ? $"value.{memberName}?.ToString() ?? string.Empty"
                                    : $"value.{memberName}?.ToString(\"{format}\") ?? string.Empty";
                            }
                            else
                            {
                                var spanFormattableDefine =
                                    underlyingType.AllInterfaces.FirstOrDefault(i =>
                                        SymbolEqualityComparer.Default.Equals(
                                            i.OriginalDefinition,
                                            spanFormattableInterface
                                        )
                                    );
                                valueString = string.IsNullOrWhiteSpace(format)
                                    ? $"value.{memberName}.ToString()"
                                    : $"value.{memberName}.ToString(\"{format}\")";
                            }
                        }

                        propertyWriteCodeBuilder.AppendLine(
                            $"        writeRow[{writeColKey}].Set(Dameng.SepEx.Parser.EscapeSepField({valueString},writer.Spec.Sep.Separator));"
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
        return (instanceInitCode, propertyWriteCodeBuilder.ToString().TrimEnd());
    }
}
