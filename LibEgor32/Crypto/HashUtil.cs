using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace LibEgor32.Crypto
{
    /// <summary>
    /// This class is used to compute a unified hash to be used in various versions of libEgor.
    /// </summary>
    public static class HashUtil
    {
        public static byte[] ComputeHash(EgorVersion version, byte[] data)
        {
            switch (version)
            {
                case EgorVersion.V1:
                    return ComputeSha256Hash(data);
                default:
                    throw new ArgumentException("Unrecognized version");

            }
        }
        public static byte[] ComputeSha256Hash(byte[] data)
        {
            return SHA256.HashData(data);
        }
        public static byte[] ComputeMD5Hash(byte[] data)
        {
            return MD5.HashData(data);
        }
        public static bool Equal(byte[] data1,byte[] data2)
        {
            if (data1.Length != data2.Length)
                return false;
            for(int i = 0;i < data1.Length; i++)
            {
                if (data1[i] != data2[i])
                    return false;
            }
            return true;
        }
    }
}
