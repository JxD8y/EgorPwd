using LibEgor32.EgorModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibEgor32.Parser
{
    /// <summary>
    /// This class is used after loading .egor file and before user input right password to hold egor file in memory
    /// </summary>
    public class EgorEncryptedRepository
    {
        public EgorVersion Version { get; set; }
        public string Name { get; set; }
        public List<EgorKey> EncryptedKeySlot = new List<EgorKey>();
        public List<byte[]?> EncryptedDataSlot = new List<byte[]?>();

        public EgorEncryptedRepository() { }
    }
}
