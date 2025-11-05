namespace PortalForge.Application.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data such as API keys.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plaintext string using AES encryption.
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <returns>The encrypted text as a Base64-encoded string.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted string using AES encryption.
    /// </summary>
    /// <param name="encryptedText">The Base64-encoded encrypted text.</param>
    /// <returns>The decrypted plaintext string.</returns>
    string Decrypt(string encryptedText);
}
