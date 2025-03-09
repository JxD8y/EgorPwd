using LibEgor32.Parser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibEgor32.EgorModels
{
    /// <summary>
    /// This Class is descriptor for any egor repository file.
    /// </summary>
    public class EgorRepository
    {
        public EgorVersion Version { get; set; }
        public string Name { get; set; } = "";
        public List<EgorKey> KeySlot { get; internal set; } = new List<EgorKey>();
        public List<EgorKeyData> KeyData { get; } = new List<EgorKeyData>();
        
        public event Action<EgorKey, DataAction>? OnKeySlotChanged;
        public event Action<EgorKeyData, DataAction>? OnDataSlotChanged;

        internal string FilePath = "";
        
        public EgorRepository(EgorVersion version,string name)
        {
            this.Version = version;
            this.Name = name;
        }

        public void AddNewData(EgorKeyData keyData,EgorKey currentKey)
        {
            EgorEngine.AddDataKeySlotEntry(this, currentKey, keyData, this.FilePath);
        }
        public void RemoveData(EgorKeyData keyData, EgorKey currentKey)
        {
            EgorEngine.RemoveDataKeySlotEntry(this, currentKey, keyData, this.FilePath);
        }
        
        public void AddNewKeyToKeySlot(EgorKey key, EgorKey currentKey)
        {
            EgorEngine.AddKeySlotEntry(this, currentKey, key, this.FilePath);
        }
        public void RemoveKeyFromKeySlot(EgorKey key,EgorKey currentKey)
        {
            EgorEngine.RemoveKeySlotEntry(this, currentKey, key,this.FilePath);
        }
    }
}
