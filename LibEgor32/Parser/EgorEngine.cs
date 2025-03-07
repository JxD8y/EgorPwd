using LibEgor32.EgorModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibEgor32.Parser
{
    /// <summary>
    /// This class will load a .egor file and verify it and also perform data serialization
    /// </summary>

    public static class EgorEngine
    {    
        
        public static EgorEncryptedRepository LoadEgorFile(string filePath)
        {
            return EgorEngineReader.ReadRepository(filePath);
        }
        public static void WriteEgorFile(EgorRepository repo,string filePath)
        {
            return EgorEngineWriter.UpdateRepository(repo,filePath)
        }

    }
}
