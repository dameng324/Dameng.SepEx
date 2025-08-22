using nietras.SeparatedValues;
#pragma warning disable CS9113 // Parameter is unread.

namespace Dameng.SepEx;
/// <summary>
/// An attribute that marks class generate SepTypeInfo.
/// </summary>
[AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
public class GenSepTypeInfoAttribute<T> : Attribute;

/// <summary>
/// An attribute that marks class to generate ISepParsable&lt;TSelf&gt; implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class GenSepParsableAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SepColumnNameAttribute(string name) : Attribute;

/// <summary>
/// When reading, is used to get the field at the given index. When writing, the fields will be written in the order of the field indexes. 
/// </summary>
/// <param name="index"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SepColumnIndexAttribute(int index) : Attribute;

/// <summary>
/// set default value for column when reading.
/// The value is used when the column is not present in the row
/// </summary>
/// <param name="value"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SepDefaultValueAttribute(object? value) : Attribute;

/// <summary>
/// set ignore column.
/// </summary>
[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
public class SepIgnoreAttribute : Attribute;

/// <summary>
/// set format for column.
/// The format is used when writing the column.
/// </summary>
/// <param name="format"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SepColumnFormatAttribute(string format) : Attribute;