using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using LibEgor32.Crypto;

namespace LibEgor32.EgorModels
{
    /// <summary>
    /// This class is a descriptor for key slots in egor file which the repository hold a list of them
    /// </summary>
    public class EgorKey
    {
        public EgorVersion Version { get; internal set; }
        public byte[]? KeyHash { get; internal set; }
        public byte[]? EncryptedMasterKey { get; set; }
        public byte[]? SecuredKey { get; set; } //Only will be set if want to write the Egor file.
        public EgorKey() { }
        public EgorKey(EgorVersion version)
        {
            this.Version = version;
        }
        public void OpenKey(byte[] key)
        {
            var hsh = HashUtil.ComputeHash(Version, key);
            if (!hsh.SequenceEqual(KeyHash ?? throw new NullReferenceException("Key hash was null")))
                throw new Exception("Key hash does not match");

            SecuredKey = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
        }
        public void CloseKey()
        {
            if (SecuredKey == null)
                throw new NullReferenceException("Secured key was not Open");

            Array.Clear(SecuredKey, 0, SecuredKey.Length);
        }
    }
}
