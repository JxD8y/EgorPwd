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
        public static EgorKey CreateNewKey(byte[] key,EgorVersion version = EgorVersion.V1)
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
        public static EgorRepository DecryptRepository(EgorEncryptedRepository eRepo, EgorKey currentKey)
        {
            EgorRepository repo = new EgorRepository(eRepo.Version, eRepo.Name);
            repo.KeySlot = eRepo.EncryptedKeySlot;
            repo.FilePath = eRepo.FilePath;

            byte[] cKey = ProtectedData.Unprotect(currentKey.SecuredKey ?? throw new NullReferenceException("Current key secureKey was null"), null, DataProtectionScope.CurrentUser);

            byte[] masterKey = CryptoEngine.DecryptData(currentKey.EncryptedMasterKey ?? throw new NullReferenceException("Current key MasterKey was null"), cKey, null, repo.Version);

            //Decrypting each entry in EER using masterKey
            List<byte[]> decrypted = new List<byte[]>();
            foreach (byte[]? entry in eRepo.EncryptedDataSlot)
            {
                if (entry is null)
                    continue;

                EgorKeyData data = new EgorKeyData();

                byte[] decryptedData = CryptoEngine.DecryptData(entry, masterKey, null, repo.Version);

                decrypted.Add(decryptedData);
                //Doing deserialization in another loop to prevent master key exposed in memory for long time
            }

            Array.Clear(masterKey, 0, masterKey.Length);
            Array.Clear(cKey, 0, cKey.Length);
            foreach (byte[] data in decrypted)
            {
                EgorKeyData keyData = EgorEngineReader.ParseKeyDataBytes(data, repo.Version);
                repo.KeyData.Add(keyData);
            }

            return repo;

        }
        public static void WriteEgorFile(EgorRepository repo,EgorKey key,string filePath)
        {
            EgorEngineWriter.UpdateRepository(repo, key, filePath);
        }
        public static void AddKeySlotEntry(EgorRepository repo, EgorKey currentKey,EgorKey newKey,string filePath)
        {
            if(!repo.KeySlot.Contains(currentKey))
                throw new Exception("Current key is not in the key slot");

            if (newKey.KeyHash?.Length < 32)
                throw new Exception("Invalid hash data in newKey");

            if(newKey.EncryptedMasterKey?.Length < 32)
                throw new Exception("Invalid master key data in newKey");

            repo.KeySlot.Add(newKey);

            EgorEngineWriter.UpdateRepository(repo, currentKey, filePath);
        }
        /// <summary>
        /// RemoveKeySlotEntry and AddKeySlotEntry does not need to decrypt data from repository so EgorWriterEngine shouldn't decrypt the data 
        /// </summary>
        public static void RemoveKeySlotEntry(EgorRepository repo, EgorKey currentKey, EgorKey keyToRemove, string filePath)
        {
            if (keyToRemove.EncryptedMasterKey is null || keyToRemove.KeyHash is null)
                throw new NullReferenceException("Key contains null masterKey and hash");

            if (repo.KeySlot.Count((EgorKey k)=> { return k.KeyHash is not null && k.KeyHash.SequenceEqual(currentKey.KeyHash ?? throw new NullReferenceException("Key hash was null")); }) == 0)
                throw new Exception("Current key is not in the key slot");

            if (repo.KeySlot.Count((EgorKey k) => { return k.KeyHash is not null && k.KeyHash.SequenceEqual(keyToRemove.KeyHash ?? throw new NullReferenceException("Key hash was null")); }) == 0)
                throw new Exception("KeyToRemove is not in the key slot");

            if(currentKey == keyToRemove)
                throw new Exception("Cannot remove current key");

            repo.KeySlot.RemoveAll((EgorKey k) => { return k.KeyHash is not null && k.KeyHash.SequenceEqual(keyToRemove.KeyHash); });
            EgorEngineWriter.UpdateRepository(repo, currentKey, filePath);

            //Preventing from reusing key if repo is not reloaded
            Array.Clear(keyToRemove.EncryptedMasterKey, 0, keyToRemove.EncryptedMasterKey.Length);
            Array.Clear(keyToRemove.KeyHash, 0, keyToRemove.KeyHash.Length);
        }

        public static void AddDataKeySlotEntry(EgorRepository repo, EgorKey currentKey , EgorKeyData data ,string filePath)
        {
            if (repo.KeySlot.Count((EgorKey k) => { return k.KeyHash is not null && k.KeyHash.SequenceEqual(currentKey.KeyHash ?? throw new NullReferenceException("Key hash was null")); }) == 0)
                throw new Exception("Current key is not in the key slot");

            if (repo.KeyData.Exists((EgorKeyData _d) =>  _d.ID == data.ID))
                throw new Exception("Data with same ID already exists");

            repo.KeyData.Add(data);

            EgorEngineWriter.UpdateRepository(repo, currentKey, filePath);
        }
        public static void RemoveDataKeySlotEntry(EgorRepository repo, EgorKey currentKey, EgorKeyData data, string filePath)
        {
            if (repo.KeySlot.Count((EgorKey k) => { return k.KeyHash is not null && k.KeyHash.SequenceEqual(currentKey.KeyHash ?? throw new NullReferenceException("Key hash was null")); }) == 0)
                throw new Exception("Current key is not in the key slot");

            if (!repo.KeyData.Exists((EgorKeyData _d) => _d.ID == data.ID))
                throw new Exception("Data is not in the key slot");

            repo.KeyData.RemoveAll((EgorKeyData d) => { return d.ID == data.ID; });
            EgorEngineWriter.UpdateRepository(repo, currentKey, filePath);
        }
    }
}
