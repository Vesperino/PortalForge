using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using PortalForge.Application.Interfaces;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Service for encrypting and decrypting sensitive data using AES encryption.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _encryptionKey;
    private const int KeySize = 256; // AES-256
    private const int BlockSize = 128;

    public EncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "Encryption key not configured. Please set 'Encryption:Key' in appsettings or environment variables.");

        // Ensure key is 32 bytes for AES-256
        _encryptionKey = DeriveKeyFromString(keyString, 32);
    }

    /// <summary>
    /// Encrypts a plaintext string using AES-256 encryption.
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Key = _encryptionKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV and encrypted data
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts an encrypted string using AES-256 encryption.
    /// </summary>
    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentNullException(nameof(encryptedText));

        var fullBytes = Convert.FromBase64String(encryptedText);

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Key = _encryptionKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Extract IV from the beginning
        var iv = new byte[aes.IV.Length];
        var encryptedBytes = new byte[fullBytes.Length - iv.Length];
        Buffer.BlockCopy(fullBytes, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// Derives a cryptographic key from a string using SHA-256.
    /// </summary>
    private static byte[] DeriveKeyFromString(string keyString, int keyLength)
    {
        using var sha256 = SHA256.Create();
        var keyBytes = Encoding.UTF8.GetBytes(keyString);
        var hash = sha256.ComputeHash(keyBytes);

        if (hash.Length >= keyLength)
        {
            var result = new byte[keyLength];
            Array.Copy(hash, result, keyLength);
            return result;
        }

        // If hash is shorter than required, repeat it
        var extended = new byte[keyLength];
        for (int i = 0; i < keyLength; i++)
        {
            extended[i] = hash[i % hash.Length];
        }
        return extended;
    }
}
