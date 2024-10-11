namespace Portable.Licensing.Validation;

/// <summary>
/// Interface for the fluent validation syntax.
/// </summary>
public interface IAddAdditionalValidationChain : IFluentInterface
{
    #region Methods
    /// <summary>
    /// Adds another validation chain.
    /// </summary>
    /// <returns>An instance of <see cref="IStartValidationChain"/>.</returns>
    IStartValidationChain And();
    #endregion
}