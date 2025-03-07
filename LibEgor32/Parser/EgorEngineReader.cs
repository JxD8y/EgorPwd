using LibEgor32.EgorModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibEgor32.Parser
{
    /// <summary>
    /// This class is used to read a egor file 
    /// I know this is egor engine responsibility but to make it maintain able, i had to separate it
    /// </summary>
    public static class EgorEngineReader
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

        /*
         * Single keySlot entry format:
         * KeyHash (32 byte)
         * Master Key (32 byte)
         * PADD
        */
        public static EgorEncryptedRepository ReadRepository(string filePath)
        {
            EgorEncryptedRepository repository = new EgorEncryptedRepository();
            repository.FilePath = filePath;

            using (var file = File.OpenRead(filePath))
            {
                byte[] header = new byte[5];
                file.Read(header, 0, 5);

                //Validating header

                if (header != EGOR_HEADER)
                {
                    throw new Exception("Selected file does not have a egor header");
                }

                //Reading database name and version

                List<byte> nameBytes = new List<byte>();
                byte lVal = 0xff;
                do
                {
                    lVal = (byte)file.ReadByte();
                    nameBytes.Add(lVal);
                } while (lVal != 0);
                string name = Encoding.UTF8.GetString(nameBytes.ToArray());
                repository.Name = name;

                byte version = (byte)file.ReadByte();
                // 1 = V1
                switch (version)
                {
                    case 1:
                        repository.Version = EgorVersion.V1;
                        break;
                    default:
                        throw new Exception("fail to parse version");
                }
                byte[] PADD = new byte[4];

                file.Read(PADD, 0, 4);
                if (PADD != EGOR_PADD)
                {
                    throw new Exception("Invalid byte alignment");
                }

                List<byte> _strBytes = new List<byte>();
                do
                {
                    lVal = (byte)file.ReadByte();
                    nameBytes.Add(lVal);
                } while (lVal != 0);

                if (Encoding.UTF8.GetString(nameBytes.ToArray()) != "KEYSLOTBEGIN")
                {
                    throw new Exception("Cannot find KeySlot");
                }
                //Getting all key slot bytes

                byte[] keySlotEndBytes = Encoding.UTF8.GetBytes("KEYSLOTEND");
                List<byte> _keySlot = new List<byte>();
                bool keySlotEnd = false;
                byte[] buffer = new byte[11];
                int i = 0;
                do
                {
                    byte value = (byte)file.ReadByte();
                    buffer[i++] = value;
                    if (i == 10)
                    {
                        if (buffer == keySlotEndBytes)
                        {
                            keySlotEnd = true;
                            break;
                        }
                        else
                        {
                            i = 0;
                            _keySlot.AddRange(buffer);
                        }
                    }

                } while (!keySlotEnd);

                List<EgorKey> Keys = ParseKeySlotBytes(_keySlot, repository.Version);
                if (Keys.Count == 0)
                {
                    throw new Exception("Cannot load database: no key found");
                }
                repository.EncryptedKeySlot = Keys;

                //Getting data

                _strBytes.Clear();
                do
                {
                    lVal = (byte)file.ReadByte();
                    nameBytes.Add(lVal);
                } while (lVal != 0);

                if (Encoding.UTF8.GetString(nameBytes.ToArray()) != "DATASLOTBEGIN")
                {
                    throw new Exception("Cannot find DataSlot");
                }

                byte[] dataSlotEndBytes = Encoding.UTF8.GetBytes("DATASLOTEND");
                List<byte> _dataSlot = new List<byte>();
                bool dataSlotEnd = false;
                buffer = new byte[11];

                i = 0;
                do
                {
                    byte value = (byte)file.ReadByte();
                    buffer[i++] = value;
                    if (i == 10)
                    {
                        if (buffer == dataSlotEndBytes)
                        {
                            dataSlotEnd = true;
                            break;
                        }
                        else
                        {
                            i = 0;
                            _dataSlot.AddRange(buffer);
                        }
                    }

                } while (!dataSlotEnd);

                List<byte[]?> data = ParseDataBytes(_dataSlot, repository.Version);
                repository.EncryptedDataSlot = data;
                return repository;

            }
        }
        private static List<EgorKey> ParseKeySlotBytes(List<byte> keySlotBytes, EgorVersion version)
        {
            List<EgorKey> keys = new List<EgorKey>();
            EgorKey key = new EgorKey(version);
            byte[] keyHash = new byte[32];
            byte[] masterKey = new byte[32];
            bool hashAssoc = false;

            for (int i = 0; i < keySlotBytes.Count; i += 32)
            {
                if (!hashAssoc)
                {
                    keyHash = keySlotBytes.Slice(i, i + 31).ToArray();
                    hashAssoc = true;
                }
                else
                {
                    masterKey = keySlotBytes.Slice(i, i + 31).ToArray();
                    i += 4; //ignoring paddings
                    key.KeyHash = keyHash;
                    key.EncryptedMasterKey = masterKey;
                    keys.Add(key);
                    key = new EgorKey(version);
                }
            }

            return keys;

        }
        private static List<byte[]?> ParseDataBytes(List<byte> dataBytes, EgorVersion version)
        {
            List<byte[]?> dataList = new List<byte[]?>();

            List<byte> data = new List<byte>();
            byte[] buffer = new byte[4];
            for (int i = 0; i < dataBytes.Count; i++)
            {
                buffer[i % 4] = dataBytes[i];
                if (i % 4 == 0)
                {
                    if (buffer == EGOR_PADD)
                    {
                        dataList.Add(data.ToArray());
                        data.Clear();
                    }
                    else
                    {
                        data.AddRange(buffer);
                    }
                }
            }
            return dataList;
        }
    }
}
