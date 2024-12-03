namespace Portable.Licensing.Validation
{
    /// <summary>
    /// Represents a <see cref="License"/> expired failure of a <see cref="ILicenseValidator"/>.
    /// </summary>
    public class LicenseExpiredValidationFailure : IValidationFailure
    {
        #region Constructors
        /// <summary>
        /// Represents a <see cref="License"/> expired failure of a <see cref="ILicenseValidator"/>.
        /// </summary>
        public LicenseExpiredValidationFailure(string message, string howToResolve)
        {
            Message = message;
            HowToResolve = howToResolve;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a message that describes the validation failure.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets a message that describes how to recover from the validation failure.
        /// </summary>
        public string HowToResolve { get; }
        #endregion
    }
}