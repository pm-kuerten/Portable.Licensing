namespace Portable.Licensing;

/// <summary>
/// Implementation of the <see cref="ILicenseBuilder"/>, a fluent api
/// to create new licenses.
/// </summary>
internal class LicenseBuilder : ILicenseBuilder
{
    #region Fields
    private readonly License _license;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseBuilder"/> class.
    /// </summary>
    public LicenseBuilder()
    {
        _license = new License();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Sets the unique identifier of the <see cref="License"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder WithUniqueIdentifier(Guid id)
    {
        _license.Id = id;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="LicenseType"/> of the <see cref="License"/>.
    /// </summary>
    /// <param name="type">The <see cref="LicenseType"/> of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder As(LicenseType type)
    {
        _license.Type = type;
        return this;
    }

    /// <summary>
    /// Sets the expiration date of the <see cref="License"/>.
    /// </summary>
    /// <param name="date">The expiration date of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder ExpiresAt(DateTime date)
    {
        _license.Expiration = date.ToUniversalTime();
        return this;
    }

    /// <summary>
    /// Sets the maximum utilization of the <see cref="License"/>.
    /// This can be the quantity of developers for a "per-developer-license".
    /// </summary>
    /// <param name="utilization">The maximum utilization of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder WithMaximumUtilization(int utilization)
    {
        _license.Quantity = utilization;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="Customer">license holder</see> of the <see cref="License"/>.
    /// </summary>
    /// <param name="name">The name of the license holder.</param>
    /// <param name="email">The email of the license holder.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder LicensedTo(string name, string email)
    {
        Customer customer = _license.Customer ?? throw new Exception("Cannot set customer");
        customer.Name = name;
        customer.Email = email;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="Customer">license holder</see> of the <see cref="License"/>.
    /// </summary>
    /// <param name="name">The name of the license holder.</param>
    /// <param name="email">The email of the license holder.</param>
    /// <param name="configureCustomer">A delegate to configure the license holder.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder LicensedTo(string name, string email, Action<Customer> configureCustomer)
    {
        Customer customer = _license.Customer ?? throw new Exception("Cannot set customer");
        customer.Name = name;
        customer.Email = email;
        configureCustomer(customer);
        return this;
    }

    /// <summary>
    /// Sets the <see cref="Customer">license holder</see> of the <see cref="License"/>.
    /// </summary>
    /// <param name="configureCustomer">A delegate to configure the license holder.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder LicensedTo(Action<Customer> configureCustomer)
    {
        Customer customer = _license.Customer ?? throw new Exception("Cannot set customer");
        configureCustomer(customer);
        return this;
    }

    /// <summary>
    /// Sets the licensed product features of the <see cref="License"/>.
    /// </summary>
    /// <param name="productFeatures">The licensed product features of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder WithProductFeatures(IDictionary<string, string> productFeatures)
    {
        LicenseAttributes features = _license.ProductFeatures ?? throw new Exception("Cannot set product features");
        features.AddAll(productFeatures);
        return this;
    }

    /// <summary>
    /// Sets the licensed product features of the <see cref="License"/>.
    /// </summary>
    /// <param name="configureProductFeatures">A delegate to configure the product features.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder WithProductFeatures(Action<LicenseAttributes> configureProductFeatures)
    {
        LicenseAttributes features = _license.ProductFeatures ?? throw new Exception("Cannot set product features");
        configureProductFeatures(features);
        return this;
    }

    /// <summary>
    /// Sets the licensed additional attributes of the <see cref="License"/>.
    /// </summary>
    /// <param name="additionalAttributes">The additional attributes of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder WithAdditionalAttributes(IDictionary<string, string> additionalAttributes)
    {
        LicenseAttributes attributes = _license.AdditionalAttributes ?? throw new Exception("Cannot set additional attributes");
        attributes.AddAll(additionalAttributes);
        return this;
    }

    /// <summary>
    /// Sets the licensed additional attributes of the <see cref="License"/>.
    /// </summary>
    /// <param name="configureAdditionalAttributes">A delegate to configure the additional attributes.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder WithAdditionalAttributes(Action<LicenseAttributes> configureAdditionalAttributes)
    {
        LicenseAttributes attributes = _license.AdditionalAttributes ?? throw new Exception("Cannot set additional attributes");
        configureAdditionalAttributes(attributes);
        return this;
    }

    /// <summary>
    /// Create and sign a new <see cref="License"/> with the specified
    /// private encryption key.
    /// </summary>
    /// <param name="privateKey">The private encryption key for the signature.</param>
    /// <param name="passPhrase">The pass phrase to decrypt the private key.</param>
    /// <returns>The signed <see cref="License"/>.</returns>
    public License CreateAndSignWithPrivateKey(string privateKey, string passPhrase)
    {
        _license.Sign(privateKey, passPhrase);
        return _license;
    }
    #endregion
}