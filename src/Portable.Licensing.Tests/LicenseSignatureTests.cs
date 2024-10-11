using System.Globalization;
using System.Xml.Linq;
using Portable.Licensing.Security.Cryptography;

namespace Portable.Licensing.Tests;

[TestFixture]
public class LicenseSignatureTests
{
    [SetUp]
    public void Init()
    {
        _passPhrase = Guid.NewGuid().ToString();
        KeyGenerator keyGenerator = KeyGenerator.Create();
        KeyPair keyPair = keyGenerator.GenerateKeyPair();
        _privateKey = keyPair.ToEncryptedPrivateKeyString(_passPhrase);
        _publicKey = keyPair.ToPublicKeyString();
    }

    private string _passPhrase;
    private string _privateKey;
    private string _publicKey;

    private static DateTime ConvertToRfc1123(DateTime dateTime) =>
        DateTime.ParseExact(
            dateTime.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture)
            , "r", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

    [Test]
    public void Can_Generate_And_Validate_Signature_With_Empty_License()
    {
        License license = License.New()
            .CreateAndSignWithPrivateKey(_privateKey, _passPhrase);

        Assert.That(license, Is.Not.Null);
        Assert.That(license.Signature, Is.Not.Null);

        // validate xml
        XElement xmlElement = XElement.Parse(license.ToString(), LoadOptions.None);
        Assert.Multiple(() =>
        {
            Assert.That(xmlElement.HasElements, Is.True);

            // validate default values when not set
            Assert.That(license.Id, Is.EqualTo(Guid.Empty));
            Assert.That(license.Type, Is.EqualTo(LicenseType.Trial));
            Assert.That(license.Quantity, Is.EqualTo(0));
            Assert.That(license.ProductFeatures, Is.Null);
            Assert.That(license.Customer, Is.Null);
            Assert.That(license.Expiration, Is.EqualTo(ConvertToRfc1123(DateTime.MaxValue)));

            // verify signature
            Assert.That(license.VerifySignature(_publicKey), Is.True);
        });
    }

    [Test]
    public void Can_Generate_And_Validate_Signature_With_Standard_License()
    {
        Guid licenseId = Guid.NewGuid();
        const string customerName = "Max Mustermann";
        const string customerEmail = "max@mustermann.tld";
        DateTime expirationDate = DateTime.Now.AddYears(1);
        Dictionary<string, string> productFeatures = new()
        {
            { "Sales Module", "yes" },
            { "Purchase Module", "yes" },
            { "Maximum Transactions", "10000" }
        };

        License license = License.New()
            .WithUniqueIdentifier(licenseId)
            .As(LicenseType.Standard)
            .WithMaximumUtilization(10)
            .WithProductFeatures(productFeatures)
            .LicensedTo(customerName, customerEmail)
            .ExpiresAt(expirationDate)
            .CreateAndSignWithPrivateKey(_privateKey, _passPhrase);

        Assert.That(license, Is.Not.Null);
        Assert.That(license.Signature, Is.Not.Null);

        // validate xml
        XElement xmlElement = XElement.Parse(license.ToString(), LoadOptions.None);
        Assert.Multiple(() =>
        {
            Assert.That(xmlElement.HasElements, Is.True);

            // validate default values when not set
            Assert.That(license.Id, Is.EqualTo(licenseId));
            Assert.That(license.Type, Is.EqualTo(LicenseType.Standard));
            Assert.That(license.Quantity, Is.EqualTo(10));
        });
        Assert.That(license.ProductFeatures, Is.Not.Null);
        Assert.That(license.ProductFeatures.GetAll(), Is.EquivalentTo(productFeatures));
        Assert.That(license.Customer, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(license.Customer.Name, Is.EqualTo(customerName));
            Assert.That(license.Customer.Email, Is.EqualTo(customerEmail));
            Assert.That(license.Expiration, Is.EqualTo(ConvertToRfc1123(expirationDate)));

            // verify signature
            Assert.That(license.VerifySignature(_publicKey), Is.True);
        });
    }

    [Test]
    public void Can_Detect_Hacked_License()
    {
        Guid licenseId = Guid.NewGuid();
        const string customerName = "Max Mustermann";
        const string customerEmail = "max@mustermann.tld";
        DateTime expirationDate = DateTime.Now.AddYears(1);
        Dictionary<string, string> productFeatures = new()
        {
            { "Sales Module", "yes" },
            { "Purchase Module", "yes" },
            { "Maximum Transactions", "10000" }
        };

        License license = License.New()
            .WithUniqueIdentifier(licenseId)
            .As(LicenseType.Standard)
            .WithMaximumUtilization(10)
            .WithProductFeatures(productFeatures)
            .LicensedTo(customerName, customerEmail)
            .ExpiresAt(expirationDate)
            .CreateAndSignWithPrivateKey(_privateKey, _passPhrase);

        Assert.That(license, Is.Not.Null);
        Assert.That(license.Signature, Is.Not.Null);

        // verify signature
        Assert.That(license.VerifySignature(_publicKey), Is.True);

        // validate xml
        XElement xmlElement = XElement.Parse(license.ToString(), LoadOptions.None);
        Assert.That(xmlElement.HasElements, Is.True);

        // manipulate xml
        Assert.That(xmlElement.Element("Quantity"), Is.Not.Null);
        xmlElement.Element("Quantity").Value = "11"; // now we want to have 11 licenses

        // load license from manipulated xml
        License hackedLicense = License.Load(xmlElement.ToString());

        Assert.Multiple(() =>
        {
            // validate default values when not set
            Assert.That(hackedLicense.Id, Is.EqualTo(licenseId));
            Assert.That(hackedLicense.Type, Is.EqualTo(LicenseType.Standard));
            Assert.That(hackedLicense.Quantity, Is.EqualTo(11)); // now with 10+1 licenses
        });
        Assert.That(hackedLicense.ProductFeatures, Is.Not.Null);
        Assert.That(hackedLicense.ProductFeatures.GetAll(), Is.EquivalentTo(productFeatures));
        Assert.That(hackedLicense.Customer, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(hackedLicense.Customer.Name, Is.EqualTo(customerName));
            Assert.That(hackedLicense.Customer.Email, Is.EqualTo(customerEmail));
            Assert.That(hackedLicense.Expiration, Is.EqualTo(ConvertToRfc1123(expirationDate)));

            // verify signature
        });
        Assert.That(hackedLicense.VerifySignature(_publicKey), Is.False);
    }
}