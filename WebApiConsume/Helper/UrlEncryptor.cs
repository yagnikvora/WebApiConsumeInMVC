using System.Security.Cryptography;
using System.Text;

namespace WebApiConsume.Helper
{
    public static class UrlEncryptor
    {
        private static readonly string EncryptionKey = "pjsGLNYrMqU6wny4"; // Change this key
        public static string Encrypt(string text)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aesAlg.IV = new byte[16]; // Initialize the IV to 0s

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }

                    // Convert to Base64 and make it URL safe
                    string base64 = Convert.ToBase64String(msEncrypt.ToArray());
                    return base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
                }
            }
        }
        public static string Decrypt(string encryptedText)
        {
            try
            {
                // Make Base64 URL-safe again
                string base64 = encryptedText.Replace("-", "+").Replace("_", "/");
                int mod4 = base64.Length % 4;
                if (mod4 > 0)
                {
                    base64 += new string('=', 4 - mod4); // Pad with '='
                }

                byte[] encryptedBytes = Convert.FromBase64String(base64);

                using (var aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                    aesAlg.IV = new byte[16]; // Initialize the IV to 0s

                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (var msDecrypt = new MemoryStream(encryptedBytes))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new Exception("Decryption failed: Invalid Base64 string.", ex);
            }
        }
    }
}
