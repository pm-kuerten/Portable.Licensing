using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Portable.Licensing.Security.Cryptography;

namespace Portable.Licensing;

/// <summary>
/// A software license
/// </summary>
public class License
{
    #region Fields
    private readonly string _signatureAlgorithm = X9ObjectIdentifiers.ECDsaWithSha512.Id;
    private readonly XElement _xmlData;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="License"/> class.
    /// </summary>
    internal License()
    {
        _xmlData = new XElement("License");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="License"/> class
    /// with the specified content.
    /// </summary>
    /// <remarks>This constructor is only used for loading from XML.</remarks>
    /// <param name="xmlData">The initial content of this <see cref="License"/>.</param>
    internal License(XElement xmlData)
    {
        _xmlData = xmlData;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the unique identifier of this <see cref="License"/>.
    /// </summary>
    public Guid Id
    {
        get => new(GetTag("Id") ?? Guid.Empty.ToString());
        set
        {
            if (!IsSigned) SetTag("Id", value.ToString());
        }
    }

    /// <summary>
    /// Gets or set the <see cref="LicenseType"/> or this <see cref="License"/>.
    /// </summary>
    public LicenseType Type
    {
        get => (LicenseType)Enum.Parse(typeof(LicenseType), GetTag("Type") ?? LicenseType.Trial.ToString(), false);
        set
        {
            if (!IsSigned) SetTag("Type", value.ToString());
        }
    }

    /// <summary>
    /// Get or sets the quantity of this license.
    /// E.g. the count of per-developer-licenses.
    /// </summary>
    public int Quantity
    {
        get => int.Parse(GetTag("Quantity") ?? "0");
        set
        {
            if (!IsSigned) SetTag("Quantity", value.ToString());
        }
    }

    /// <summary>
    /// Gets or sets the product features of this <see cref="License"/>.
    /// </summary>
    public LicenseAttributes? ProductFeatures
    {
        get
        {
            if (_xmlData.Element("ProductFeatures") is { } xmlElement) return new LicenseAttributes(xmlElement, "Feature");
            if (IsSigned) return null;
            _xmlData.Add(new XElement("ProductFeatures"));
            return new LicenseAttributes(_xmlData.Element("ProductFeatures"), "Feature");
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="Customer"/> of this <see cref="License"/>.
    /// </summary>
    public Customer? Customer
    {
        get
        {
            if (_xmlData.Element("Customer") is { } xmlElement) return new Customer(xmlElement);
            if (IsSigned) return null;
            _xmlData.Add(new XElement("Customer"));
            return new Customer(_xmlData.Element("Customer"));
        }
    }

    /// <summary>
    /// Gets or sets the additional attributes of this <see cref="License"/>.
    /// </summary>
    public LicenseAttributes? AdditionalAttributes
    {
        get
        {
            if (_xmlData.Element("LicenseAttributes") is { } xmlElement) return new LicenseAttributes(xmlElement, "Attribute");
            if (IsSigned) return null;
            _xmlData.Add(new XElement("LicenseAttributes"));
            return new LicenseAttributes(_xmlData.Element("LicenseAttributes"), "Attribute");
        }
    }

    /// <summary>
    /// Gets or sets the expiration date of this <see cref="License"/>.
    /// Use this property to set the expiration date for a trial license
    /// or the expiration of support & subscription updates for a standard license.
    /// </summary>
    public DateTime Expiration
    {
        get => DateTime.ParseExact(GetTag("Expiration") ?? DateTime.MaxValue.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture), "r",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        set
        {
            if (!IsSigned) SetTag("Expiration", value.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Gets the digital signature of this license.
    /// </summary>
    /// <remarks>Use the <see cref="License.Sign"/> method to compute a signature.</remarks>
    public string? Signature => GetTag("Signature");

    /// <summary>
    /// Gets a value indicating whether this <see cref="License"/> is already signed.
    /// </summary>
    private bool IsSigned => !string.IsNullOrEmpty(Signature);
    #endregion

    #region Methods
    /// <summary>
    /// Compute a signature and sign this <see cref="License"/> with the provided key.
    /// </summary>
    /// <param name="privateKey">The private key in xml string format to compute the signature.</param>
    /// <param name="passPhrase">The pass phrase to decrypt the private key.</param>
    public void Sign(string privateKey, string passPhrase)
    {
        XElement signTag = _xmlData.Element("Signature") ?? new XElement("Signature");

        try
        {
            if (signTag.Parent is not null)
                signTag.Remove();

            AsymmetricKeyParameter privKey = KeyFactory.FromEncryptedPrivateKeyString(privateKey, passPhrase);

            var documentToSign = Encoding.UTF8.GetBytes(_xmlData.ToString(SaveOptions.DisableFormatting));
            ISigner? signer = SignerUtilities.GetSigner(_signatureAlgorithm);
            signer.Init(true, privKey);
            signer.BlockUpdate(documentToSign, 0, documentToSign.Length);
            var signature = signer.GenerateSignature();
            signTag.Value = Convert.ToBase64String(signature);
        }
        finally
        {
            _xmlData.Add(signTag);
        }
    }

    /// <summary>
    /// Determines whether the <see cref="License.Signature"/> property verifies for the specified key.
    /// </summary>
    /// <param name="publicKey">The public key in xml string format to verify the <see cref="License.Signature"/>.</param>
    /// <returns>true if the <see cref="License.Signature"/> verifies; otherwise false.</returns>
    public bool VerifySignature(string publicKey)
    {
        XElement? signTag = _xmlData.Element("Signature");

        if (signTag is null) return false;

        try
        {
            signTag.Remove();

            AsymmetricKeyParameter pubKey = KeyFactory.FromPublicKeyString(publicKey);

            var documentToSign = Encoding.UTF8.GetBytes(_xmlData.ToString(SaveOptions.DisableFormatting));
            ISigner? signer = SignerUtilities.GetSigner(_signatureAlgorithm);
            signer.Init(false, pubKey);
            signer.BlockUpdate(documentToSign, 0, documentToSign.Length);

            return signer.VerifySignature(Convert.FromBase64String(signTag.Value));
        }
        finally
        {
            _xmlData.Add(signTag);
        }
    }

    /// <summary>
    /// Create a new <see cref="License"/> using the <see cref="ILicenseBuilder"/>
    /// fluent api.
    /// </summary>
    /// <returns>An instance of the <see cref="ILicenseBuilder"/> class.</returns>
    public static ILicenseBuilder New() => new LicenseBuilder();

    /// <summary>
    /// Loads a <see cref="License"/> from a string that contains XML.
    /// </summary>
    /// <param name="xmlString">A <see cref="string"/> that contains XML.</param>
    /// <returns>A <see cref="License"/> populated from the <see cref="string"/> that contains XML.</returns>
    public static License Load(string xmlString) => new(XElement.Parse(xmlString, LoadOptions.None));

    /// <summary>
    /// Loads a <see cref="License"/> by using the specified <see cref="Stream"/>
    /// that contains the XML.
    /// </summary>
    /// <param name="stream">A <see cref="Stream"/> that contains the XML.</param>
    /// <returns>A <see cref="License"/> populated from the <see cref="Stream"/> that contains XML.</returns>
    public static License Load(Stream stream) => new(XElement.Load(stream, LoadOptions.None));

    /// <summary>
    /// Loads a <see cref="License"/> by using the specified <see cref="TextReader"/>
    /// that contains the XML.
    /// </summary>
    /// <param name="reader">A <see cref="TextReader"/> that contains the XML.</param>
    /// <returns>A <see cref="License"/> populated from the <see cref="TextReader"/> that contains XML.</returns>
    public static License Load(TextReader reader) => new(XElement.Load(reader, LoadOptions.None));

    /// <summary>
    /// Loads a <see cref="License"/> by using the specified <see cref="XmlReader"/>
    /// that contains the XML.
    /// </summary>
    /// <param name="reader">A <see cref="XmlReader"/> that contains the XML.</param>
    /// <returns>A <see cref="License"/> populated from the <see cref="TextReader"/> that contains XML.</returns>
    public static License Load(XmlReader reader) => new(XElement.Load(reader, LoadOptions.None));

    /// <summary>
    /// Serialize this <see cref="License"/> to a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">A <see cref="Stream"/> that the 
    /// <see cref="License"/> will be written to.</param>
    public void Save(Stream stream) => _xmlData.Save(stream);

    /// <summary>
    /// Serialize this <see cref="License"/> to a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="textWriter">A <see cref="TextWriter"/> that the 
    /// <see cref="License"/> will be written to.</param>
    public void Save(TextWriter textWriter) => _xmlData.Save(textWriter);

    /// <summary>
    /// Serialize this <see cref="License"/> to a <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="xmlWriter">A <see cref="XmlWriter"/> that the 
    /// <see cref="License"/> will be written to.</param>
    public void Save(XmlWriter xmlWriter) => _xmlData.Save(xmlWriter);

    /// <summary>
    /// Returns the indented XML for this <see cref="License"/>.
    /// </summary>
    /// <returns>A string containing the indented XML.</returns>
    public override string ToString() => _xmlData.ToString();

    private void SetTag(string name, string? value)
    {
        XElement? element = _xmlData.Element(name);

        if (element is null)
        {
            element = new XElement(name);
            _xmlData.Add(element);
        }

        if (value is not null) element.Value = value;
    }

    private string? GetTag(string name) => _xmlData.Element(name)?.Value;
    #endregion
}