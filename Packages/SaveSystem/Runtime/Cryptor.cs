using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Xeon.SaveSystem
{
    public class Cryptor
    {
        private const int KeySize = 256;
        private const int BlockSize = 128;
        private const string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
        private const string EncryptionIV = "0123456789ABCDEF";

        public static byte[] Encrypt(byte[] rawData) => Encrypt(rawData, EncryptionKey, EncryptionIV);

        public static byte[] Encrypt(byte[] rawData, string key, string iv)
        {
            using (var aes = new AesManaged())
            {
                SetAesParams(aes, key, iv);

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var encryptedStream = new MemoryStream())
                {
                    using (CryptoStream cryptStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write))
                        cryptStream.Write(rawData, 0, rawData.Length);
                    return encryptedStream.ToArray();
                }
            }
        }

        public static byte[] Decrypt(byte[] encryptedData) => Decrypt(encryptedData, EncryptionKey, EncryptionIV);

        public static byte[] Decrypt(byte[] encryptedData, string key, string iv)
        {
            using (var aes = new AesManaged())
            {
                SetAesParams(aes, key, iv);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var encryptedStream = new MemoryStream(encryptedData))
                using (var decryptedStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read))
                        cryptoStream.CopyTo(decryptedStream);
                    return decryptedStream.ToArray();
                }
            }
        }

        private static void SetAesParams(AesManaged aes, string key, string iv)
        {
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = Encoding.UTF8.GetBytes(CreateKeyFromString(key));
            aes.IV = Encoding.UTF8.GetBytes(CreateIVFromString(iv));
        }

        private static string CreateKeyFromString(string str) => PaddingString(str, KeySize / 8);

        private static string CreateIVFromString(string str) => PaddingString(str, BlockSize / 8);

        private static string PaddingString(string str, int len)
        {
            const char PaddingCharacter = '.';

            if (str.Length < len)
            {
                var key = str;
                for (int i = 0; i < len - str.Length; ++i)
                    key += PaddingCharacter;
                return key;
            }
            if (str.Length > len)
                return str.Substring(0, len);
            return str;
        }
    }
}
