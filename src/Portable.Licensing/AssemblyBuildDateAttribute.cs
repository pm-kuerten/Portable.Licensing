﻿using System.Globalization;

namespace Portable.Licensing;

/// <summary>
/// Defines assembly build date information for an assembly manifest.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class AssemblyBuildDateAttribute : Attribute
{
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyBuildDateAttribute"/> class
    /// with the specified build date.
    /// </summary>
    /// <param name="buildDate">The build date of the assembly.</param>
    public AssemblyBuildDateAttribute(DateTime buildDate)
    {
        BuildDate = buildDate;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyBuildDateAttribute"/> class
    /// with the specified build date string.
    /// </summary>
    /// <param name="buildDateString">The build date of the assembly.</param>
    public AssemblyBuildDateAttribute(string buildDateString)
    {
        BuildDate = DateTime.Parse(buildDateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets the assembly build date.
    /// </summary>
    public DateTime BuildDate { get; }
    #endregion
}