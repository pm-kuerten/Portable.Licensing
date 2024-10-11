namespace Portable.Licensing.Validation;

internal class ValidationChainBuilder(License license) : IStartValidationChain, IValidationChain
{
    #region Fields
    private readonly Queue<ILicenseValidator> _validators = new();
    private ILicenseValidator? _currentValidatorChain;
    #endregion

    #region Methods
    public ILicenseValidator StartValidatorChain() => _currentValidatorChain = new LicenseValidator();

    public void CompleteValidatorChain()
    {
        if (_currentValidatorChain is null)
            return;

        _validators.Enqueue(_currentValidatorChain);
        _currentValidatorChain = null;
    }

    public ICompleteValidationChain When(Predicate<License> predicate)
    {
        if (_currentValidatorChain is null) throw new Exception("No validation chain");
        _currentValidatorChain.ValidateWhen = predicate;
        return this;
    }

    public IStartValidationChain And()
    {
        CompleteValidatorChain();
        return this;
    }

    public IEnumerable<IValidationFailure> AssertValidLicense()
    {
        CompleteValidatorChain();

        while (_validators.Count > 0)
        {
            ILicenseValidator validator = _validators.Dequeue();
            if (validator.ValidateWhen is not null && !validator.ValidateWhen(license))
                continue;
            if (validator.Validate is null) throw new Exception("No validation function set");
            if (!validator.Validate(license)) yield return validator.FailureResult ?? new GeneralValidationFailure("License validation failed!");
        }
    }
    #endregion
}