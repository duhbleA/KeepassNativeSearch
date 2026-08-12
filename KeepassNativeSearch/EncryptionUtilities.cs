using System.Security.Cryptography;
using System.Text;

namespace KeepassNativeSearch;

/*
 * <summary>
 * A utility class that contains functions to encrypt and decrypt strings of data using the
 * <see cref="System.Security.Cryptography.ProtectedData"/> package, scoped to the current user executing this session.
 * </summary>
 *
 */
public static class EncryptionUtilities
{
    private static readonly byte[] AdditionalEntropy = [3, 1, 4, 2, 8];

    /**
     * <summary>
     * Converts a string to UTF-8, encrypts it, and returns a hex string of cipher text. Only current user executing
     * the session can decrypt it.
     * </summary>
     *
     * <param name="plainText">The string to encrypt</param>
     * <returns>A Hex string of cipher text, or empty string if the encryption failed.</returns>
     */
    internal static string Encrypt(string plainText)
    {
        try
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherText = ProtectedData.Protect(plainTextBytes, AdditionalEntropy,
                DataProtectionScope.CurrentUser);
            return Convert.ToHexString(cipherText);
        }
        catch
        {
            return "";
        }
    }

    /**
     * <summary>Converts cipher text encoded using hex into a string.</summary>
     *
     * <param name="cipherText">Hex encoded cipher text.</param>
     * <returns>A plaintext string, or empty string if the decryption failed.</returns>
     */
    internal static string Decrypt(string cipherText)
    {
        try
        {
            var cipherBytes = Convert.FromHexString(cipherText);
            var plainBytes = ProtectedData.Unprotect(cipherBytes, AdditionalEntropy,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return "";
        }
    }
}