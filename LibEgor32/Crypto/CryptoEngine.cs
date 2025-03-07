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

        public static byte[] EncryptData(byte[] data , byte[] key , EgorVersion version)
        {
            switch (version)
            {
                case EgorVersion.V1:
                    return AES256Encrypt(data, key);
                default:
                    throw new NotImplementedException("This version is not implemented");
            }
        }
        public static byte[] DecryptData(byte[] crypted, byte[] key, EgorVersion version)
        {
            switch (version)
            {
                case EgorVersion.V1:
                    return AES256Decrypt(crypted, key);
                default:
                    throw new NotImplementedException("This version is not implemented");
            }
        }
        private static byte[] AES256Encrypt(byte[] data, byte[] key)
        {

        }
        private static byte[] AES256Decrypt(byte[] crypted, byte[] key)
        {

        }
    }
}
