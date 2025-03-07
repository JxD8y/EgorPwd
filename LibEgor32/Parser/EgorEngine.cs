using LibEgor32.Crypto;
using LibEgor32.EgorModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibEgor32.Parser
{
    /// <summary>
    /// This class will load a .egor file and verify it and also perform data serialization
    /// </summary>

    public static class EgorEngine
    {   
        public static EgorRepository CreateNewRepository(EgorKey key,string name,string filePath,EgorVersion version = EgorVersion.V1)
        {
            EgorRepository repo = new EgorRepository(version, name);
            repo.FilePath = filePath;
            repo.KeySlot.Add(key);
            EgorEngineWriter.UpdateRepository(repo,key,filePath);
            return repo;
        }
        public static EgorKey CreateKey(byte[] key,EgorVersion version = EgorVersion.V1)
        {
            EgorKey egorKey = new EgorKey(version);
            byte[] masterKey = CryptoEngine.GenerateRandomBytes(16);
            byte[] encryptedMasterKey = CryptoEngine.EncryptData(masterKey, key, null, version);
            byte[] keyHash = HashUtil.ComputeHash(version, key);

            egorKey.KeyHash = keyHash;
            egorKey.EncryptedMasterKey = encryptedMasterKey;

            Array.Clear(masterKey, 0, masterKey.Length);
            return egorKey;
        }

        /// <summary>
        /// Create new key for repository
        /// </summary>
        /// <param name="key"></param>
        /// <param name="currentKey">currentKey's SecuredPassword parameter shouldn't be null</param>
        /// <returns></returns>
        public static EgorKey CreateKey(byte[] key, EgorKey currentKey,EgorVersion version = EgorVersion.V1)
        {
            EgorKey eKey = new EgorKey(version);
            eKey.KeyHash = HashUtil.ComputeHash(version, key);

            //Decrypting masterKey
            byte[] cKey = ProtectedData.Unprotect(currentKey.SecuredKey ?? throw new NullReferenceException("Current key secureKey was null"), null, DataProtectionScope.CurrentUser);

            byte[] masterKey = CryptoEngine.DecryptData(currentKey.EncryptedMasterKey ?? throw new NullReferenceException("Current key MasterKey was null"), cKey, null, version);
            byte[] encryptedMasterKey = CryptoEngine.EncryptData(masterKey, key, null, version);

            //Clearing the masterKey and cKey
            Array.Clear(masterKey, 0, masterKey.Length);
            Array.Clear(cKey, 0, cKey.Length);

            eKey.EncryptedMasterKey = encryptedMasterKey;
            return eKey;
        }
        public static EgorEncryptedRepository LoadEgorFile(string filePath)
        {
            return EgorEngineReader.ReadRepository(filePath);
        }
        public static EgorRepository DecryptRepository(EgorEncryptedRepository repo , EgorKey currentKey)
        {
             
        }
        public static void WriteEgorFile(EgorRepository repo,EgorKey key,string filePath)
        {
            EgorEngineWriter.UpdateRepository(repo, key, filePath);
        }
        public static void AddKeySlotEntry(EgorRepository repo, EgorKey currentKey,EgorKey newKey,string filePath)
        {
            if(!repo.KeySlot.Contains(currentKey))
                throw new Exception("Current key is not in the key slot");

            if (newKey.KeyHash?.Length < 256)
                throw new Exception("Invalid hash data in newKey");

            if(newKey.EncryptedMasterKey?.Length < 256)
                throw new Exception("Invalid master key data in newKey");

            repo.KeySlot.Add(newKey);

            EgorEngineWriter.UpdateRepository(repo, currentKey, filePath);
        }
        /// <summary>
        /// RemoveKeySlotEntry and AddKeySlotEntry does not need to decrypt data from repository so EgorWriterEngine shouldn't decrypt the data 
        public static void RemoveKeySlotEntry(EgorRepository repo, EgorKey currentKey, EgorKey keyToRemove, string filePath)
        {
            if (!repo.KeySlot.Contains(currentKey))
                throw new Exception("Current key is not in the key slot");

            if (!repo.KeySlot.Contains(keyToRemove))
                throw new Exception("KeyToRemove is not in the key slot");

            if(currentKey == keyToRemove)
                throw new Exception("Cannot remove current key");

            repo.KeySlot.Remove(keyToRemove);
            EgorEngineWriter.UpdateRepository(repo, currentKey, filePath);
        }
    }
}
