using LibEgor32.Crypto;
using LibEgor32.EgorModels;
using System.Security.Cryptography;
using System.Text;

namespace LibEgor32.Parser
{
    /// <summary>
    /// This class is used to serialize a egorRepository and write it into .egor file
    /// I did not tried to edit only a part of .egor file for each update i just rewrite it.
    /// </summary>
    public static class EgorEngineWriter
    {
        /// <summary>
        /// EGOR file structure:
        /// 
        /// {0x64 , 0xff, 0x33,0xf1,0xe1} Header
        /// [NAME (UTF-8)] [VERSION (1BYTE)]
        /// [PADD]
        /// [KEYSLOTBEGIN (STRING)]
        /// [KEY]
        /// [PADD]
        /// [KEYSLOTEND (STRING)]
        /// [DATASLOTBEGIN (STRING)]
        /// [DATA]
        /// [PADD]
        /// [DATASLOTEND (STRING)]
        /// </summary>

        public static readonly byte[] EGOR_HEADER = { 0x64, 0xff, 0x33, 0xf1, 0xe1 };
        public static readonly byte[] EGOR_PADD = { 0x00, 0x00, 0x00, 0x00 };
        public static void UpdateRepository(EgorRepository repo, EgorKey? currentKey,string filePath)
        {
            if(currentKey == null)
                throw new NullReferenceException("CurrentKey cannot be null");
            if(File.Exists(filePath))
                File.Copy(filePath, filePath + ".back");
            File.Delete(filePath);
            using (var file = File.OpenWrite(filePath))
            {
                byte[] serializedRepo = SerializeRepository(repo,currentKey);
                file.Write(serializedRepo, 0, serializedRepo.Length);
            }
            if (File.Exists(filePath))
                File.Delete(filePath + ".back");
        }
        private static byte[] SerializeRepository(EgorRepository repo, EgorKey key)
        { 
            List<byte> bRepo = new List<byte>();

            //Adding header

            bRepo.AddRange(EGOR_HEADER);

            if (repo.Name.Length == 0)
                throw new Exception("Repository name cannot be empty");
            byte[] nameBytes = Encoding.UTF8.GetBytes(repo.Name[repo.Name.Length - 1] == '\0' ? repo.Name : repo.Name + '\0');

            //version

            byte version = 0x1;

            bRepo.AddRange(nameBytes);
            bRepo.Add(version);
            bRepo.AddRange(EGOR_PADD);

            //Adding KeySlot
            bRepo.AddRange(Encoding.UTF8.GetBytes("KEYSLOTBEGIN\0"));

            if (repo.KeySlot.Count == 0)
                throw new Exception("KeySlot should contain atleast one key");

            /*
             * Single keySlot entry format:
             * KeyHash (32 byte)
             * Master Key (32 byte)
             * PADD
            */
            
            List<byte> bKey = new List<byte>();

            foreach (EgorKey keyEntry in repo.KeySlot)
            {
                bKey.AddRange(keyEntry.KeyHash ?? throw new NullReferenceException("KeyHash cannot be null"));
                bKey.AddRange(keyEntry.EncryptedMasterKey ?? throw new NullReferenceException("MasterKey cannot be null"));
                bKey.AddRange(EGOR_PADD);
            }
            bRepo.AddRange(bKey.ToArray());
            bRepo.AddRange(Encoding.UTF8.GetBytes("KEYSLOTEND\0"));

            bRepo.AddRange(Encoding.UTF8.GetBytes("DATASLOTBEGIN\0"));

            foreach (EgorKeyData data in repo.KeyData)
            {
                bRepo.AddRange(EncryptKeyData(data,key));
                bRepo.AddRange(EGOR_PADD);
            }
            return bRepo.ToArray();
        }

        /*
            * Single dataSlot entry format:
            * ID (4 byte)
            * Name (?? byte)\0
            * Password (?? byte)\0
        */
        public static byte[] EncryptKeyData(EgorKeyData data,EgorKey key)
        {
            List<byte> bKeyData = new List<byte>();
            
            //Adding ID

            bKeyData.AddRange(BitConverter.GetBytes(data.ID));

            //Adding name
            bKeyData.AddRange(Encoding.UTF8.GetBytes(data.Name[data.Name.Length - 1] == '\0' ? data.Name : data.Name + '\0'));
            
            //Adding Password
            bKeyData.AddRange(Encoding.UTF8.GetBytes(data.Password[data.Password.Length - 1] == '\0' ? data.Password : data.Password + '\0'));

            //Encrypting byte array using AES256

            byte[] enKey = ProtectedData.Unprotect(key.SecuredKey ?? throw new NullReferenceException("SecureKey was null"), null, DataProtectionScope.CurrentUser);
            
            //Decrypting masterKey
            byte[] masterKey = CryptoEngine.DecryptData(key.EncryptedMasterKey ?? throw new NullReferenceException("MasterKey was null"), enKey, null ,EgorVersion.V1);

            byte[] encryptedData = CryptoEngine.EncryptData(bKeyData.ToArray(), masterKey, null, EgorVersion.V1);

            //Disposing masterKey & enKey
            Array.Clear(enKey,0, enKey.Length);
            Array.Clear(masterKey, 0, masterKey.Length);

            return encryptedData;
        }
    }
}
