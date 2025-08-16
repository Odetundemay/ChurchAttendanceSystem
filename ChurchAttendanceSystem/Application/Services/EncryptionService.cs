using System.Security.Cryptography;
using System.Text;
using ChurchAttendanceSystem.Application.Interfaces;

namespace ChurchAttendanceSystem.Application.Services;

public class EncryptionService : IEncryptionService
{
    private readonly string _key;

    public EncryptionService(IConfiguration configuration)
    {
        _key = configuration["Encryption:Key"] ?? "MyEncryptionKey1234567890123456";
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        
        var keyBytes = Encoding.UTF8.GetBytes(_key);
        // Ensure key is exactly 32 bytes for AES-256
        Array.Resize(ref keyBytes, 32);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        
        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
        Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
        
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;
        
        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(_key);
            // Ensure key is exactly 32 bytes for AES-256
            Array.Resize(ref keyBytes, 32);
            var fullCipher = Convert.FromBase64String(cipherText);
            
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];
            
            Array.Copy(fullCipher, 0, iv, 0, 16);
            Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);
            
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return cipherText;
        }
    }
}