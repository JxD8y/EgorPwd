using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibEgor32.EgorModels
{
    /// <summary>
    /// This class represent actual encrypted data, structure of this class can change in different versions.
    /// </summary>
    public class EgorKeyData
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";

        public EgorKeyData() { }

        public EgorKeyData(int id , string name)
        {
            this.Name = name;
            this.ID = id;
        }
    }
}
