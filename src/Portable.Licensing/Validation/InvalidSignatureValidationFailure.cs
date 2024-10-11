﻿namespace Portable.Licensing.Validation;

/// <summary>
/// Represents a failure when the <see cref="License.Signature"/> is invalid.
/// </summary>
public class InvalidSignatureValidationFailure(string message, string howToResolve) : IValidationFailure
{
    #region Properties
    /// <summary>
    /// Gets or sets a message that describes the validation failure.
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// Gets or sets a message that describes how to recover from the validation failure.
    /// </summary>
    public string HowToResolve { get; set; } = howToResolve;
    #endregion
}