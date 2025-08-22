using System.Text;
using Microsoft.CodeAnalysis;

namespace Dameng.SepEx.Generator;

public static class Utils
{
    internal static (string ReadCode, string WriteCode) GeneratePropertyCode(
        INamedTypeSymbol targetType,
        INamedTypeSymbol spanParsableInterface,
        GeneratorExecutionContext context
    )
    {
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
                        valueString = $"Set(value.{memberName}?.ToString() ?? string.Empty)";
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
                                        .Equals("Dameng.SepEx.SepColumnFormatAttribute") == true
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