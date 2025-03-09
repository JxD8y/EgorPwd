using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace LibEgor32.Crypto
{
    /// <summary>
    /// This class is used to Encrypt and Decrypt data blocks using key and a version specifier
    /// </summary>
    public static class CryptoEngine
    {
        //version 1 uses constant IV which is not secure but that was only meant for testing purposes.
        public static readonly byte[] V1_IV = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10};
        public static byte[] EncryptData(byte[] data , byte[] key , byte[]? iv ,EgorVersion version)
        {
            byte[] tKey = CreateKey(key, version);
            switch (version)
            {
                case EgorVersion.V1:

                    return AES256Encrypt(data, tKey, V1_IV);
                default:
                    throw new NotImplementedException("This version is not implemented");
            }
        }
        public static byte[] DecryptData(byte[] crypted, byte[] key, byte[]? iv,EgorVersion version)
        {
            byte[] tKey = CreateKey(key, version);
            switch (version)
            {
                case EgorVersion.V1:
                    return AES256Decrypt(crypted, tKey,V1_IV);
                default:
                    throw new NotImplementedException("This version is not implemented");
            }
        }
        private static byte[] AES256Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using(Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;

                var transform = aes.CreateEncryptor();
                
                using(MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
            }
        }
        private static byte[] AES256Decrypt(byte[] crypted, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;

                var transform = aes.CreateDecryptor();

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        cs.Write(crypted, 0, crypted.Length);
                    }
                    return ms.ToArray();
                }
            }
        }
        /// <summary>
        /// Create key to be used in Encryption and Decryption process different for each version
        /// </summary>
        public static byte[] CreateKey(byte[] key,EgorVersion version)
        {
            switch (version)
            {
                case EgorVersion.V1:
                    return Create16ByteKey(key);
                default:
                    throw new Exception("Invalid version");
            }
        }
        /// <summary>
        /// Version 1 keys have maximum length of 32 byte
        /// </summary>
        private static byte[] Create16ByteKey(byte[] key)
        {
            byte[] rKey = new byte[16];

            if (key.Length >= 32)
                throw new Exception("Key is larger than 32 byte");

            byte[] eKey = new byte[32];
            if (key.Length < 32)
            {
                //Padding the key
                int length = key.Length;
                
                for(int i = 0; i < key.Length; i++)
                {
                    if (i < length)
                        eKey[i] = key[i];
                    else
                        eKey[i] = 0x00;
                }
            }
            for (int i = 0; i < 32; i++)
            {
                rKey[i % 16] ^= eKey[i];
            }

            return rKey;
        }
        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using(RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes,0,length);
            }
            return randomBytes;
        }
    }
}
