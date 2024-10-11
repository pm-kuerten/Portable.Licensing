namespace Portable.Licensing.Validation;

/// <summary>
/// Represents a failure of a <see cref="ILicenseValidator"/>.
/// </summary>
public interface IValidationFailure
{
    #region Properties
    /// <summary>
    /// Gets or sets a message that describes the validation failure.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets or sets a message that describes how to recover from the validation failure.
    /// </summary>
    string HowToResolve { get; }
    #endregion
}