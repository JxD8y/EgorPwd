using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibEgor32;
using LibEgor32.EgorModels;
using LibEgor32.Parser;
namespace EgorPwd
{
    public static class GlobalObjects
    {
        public static readonly string DefaultDbName = "Database";
        public static MainWindow MainWindow { get; internal set; }
        public static EgorEncryptedRepository EncryptedRepository { get; internal set; }
        public static EgorRepository Repository { get; internal set; }
        public static EgorKey OpenedKey { get; internal set; }
    }
}
