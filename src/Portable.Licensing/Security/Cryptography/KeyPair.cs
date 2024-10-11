using Org.BouncyCastle.Crypto;

namespace Portable.Licensing.Security.Cryptography;

/// <summary>
/// Represents a private/public encryption key pair.
/// </summary>
public class KeyPair
{
    #region Fields
    private readonly AsymmetricCipherKeyPair _keyPair;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyPair"/> class
    /// with the provided asymmetric key pair.
    /// </summary>
    /// <param name="keyPair"></param>
    internal KeyPair(AsymmetricCipherKeyPair keyPair)
    {
        _keyPair = keyPair;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Gets the encrypted and DER encoded private key.
    /// </summary>
    /// <param name="passPhrase">The pass phrase to encrypt the private key.</param>
    /// <returns>The encrypted private key.</returns>
    public string ToEncryptedPrivateKeyString(string passPhrase) => KeyFactory.ToEncryptedPrivateKeyString(_keyPair.Private, passPhrase);

    /// <summary>
    /// Gets the DER encoded public key.
    /// </summary>
    /// <returns>The public key.</returns>
    public string ToPublicKeyString() => KeyFactory.ToPublicKeyString(_keyPair.Public);
    #endregion
}