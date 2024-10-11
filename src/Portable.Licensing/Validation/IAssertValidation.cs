namespace Portable.Licensing.Validation;

/// <summary>
/// Interface for the fluent validation syntax.
/// </summary>
public interface IAssertValidation : IFluentInterface
{
    #region Methods
    /// <summary>
    /// Invokes the license assertion.
    /// </summary>
    /// <returns>An array is <see cref="IValidationFailure"/> when the validation fails.</returns>
    IEnumerable<IValidationFailure> AssertValidLicense();
    #endregion
}