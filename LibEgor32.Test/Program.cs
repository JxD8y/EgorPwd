using System;
using LibEgor32;
using LibEgor32.EgorModels;
using LibEgor32.Parser;
using System.Text;
using LibEgor32.Crypto;

namespace LibEgor32.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initiating test....");

            string fileName = "Test.egor";

            Console.WriteLine($"Test file path : {fileName}");

            Console.WriteLine("Creating new repository....");

            string keyStr = "Tesu23@!#fsdPass";
            string key2Str = "dfaasdf34#@FDsdf2fsdfv";

            byte[] key = Encoding.UTF8.GetBytes(keyStr);
            byte[] key2 = Encoding.UTF8.GetBytes(key2Str);

            Console.WriteLine($"Created new key: {keyStr}");

            var eKey = EgorEngine.CreateNewKey(key);
            eKey.OpenKey(key);

            var repo = EgorEngine.CreateNewRepository(eKey, "Test Repo", fileName);

            Console.WriteLine($"Repo with name {repo.Name} created");

            Console.WriteLine("Adding Data ...");

            var data1 = new EgorKeyData() { ID = 0 , Name="Google.com" , Password = "PWS134r5fsdfvhnj!@" };
            var data2= new EgorKeyData() { ID = 1 , Name="Instagram.com" , Password = "sgsdfa23r4fewf!@" };
            var data3 = new EgorKeyData() { ID = 2 , Name="yahoo.com" , Password = "PWS134r5fsddsfawfvhnj!@" };
            var data4 = new EgorKeyData() { ID = 3 , Name="pwn.com" , Password = "asdf4r5wef" };
            var data5 = new EgorKeyData() { ID = 4 , Name="Egor.uk.com" , Password = "asdfw4erfnewfwef" };
            var data6 = new EgorKeyData() { ID = 5 , Name="randomName" , Password = "sdf243S134wefwwe@" };

            repo.AddNewData(data1,eKey);
            repo.AddNewData(data2,eKey);
            repo.AddNewData(data3,eKey);
            repo.AddNewData(data4,eKey);
            repo.AddNewData(data5,eKey);
            repo.AddNewData(data6,eKey);

            Console.WriteLine($"Creating new key ....");
            var eKey2 = EgorEngine.CreateKey(key2, eKey);

            Console.WriteLine("Adding key to Repository....");
            repo.AddNewKeyToKeySlot(eKey2, eKey);

            repo.RemoveData(data5, eKey);

            Console.WriteLine("Reloading repository with new key .....");
            var erp = EgorEngine.LoadEgorFile(fileName);
            eKey2 = erp.EncryptedKeySlot[1];
            eKey2.OpenKey(key2);
            var repo2 = EgorEngine.DecryptRepository(erp, eKey2);

            Console.WriteLine("Removing first key ...");
            repo2.RemoveKeyFromKeySlot(eKey, eKey2);

            Console.WriteLine("Iterating over data ...");

            foreach(var data in repo2.KeyData)
            {
                Console.WriteLine($"DATA_TEST(1): {data.ID} {data.Name} {data.Password}");
            }

            Console.WriteLine("Removing data 2 from list and reloading ....");
            repo2.RemoveData(data2, eKey2);

            erp = EgorEngine.LoadEgorFile(fileName);
            eKey2 = erp.EncryptedKeySlot[0];
            eKey2.OpenKey(key2);
            repo2 = EgorEngine.DecryptRepository(erp, eKey2);

            Console.WriteLine("Iterating over data .....");

            foreach (var data in repo2.KeyData)
            {
                Console.WriteLine($"DATA_TEST(1): {data.ID} {data.Name} {data.Password}");
            }

            Console.WriteLine("Testing scalability by adding 100+ data and 100+ keys to repository and then ensure the data integrity ....");

            EgorKey lastKey = eKey2;
            for(int i =1; i < 100; i++)
            {
                var dataN = new EgorKeyData() { ID = 5 + i, Name = $"randomName{i}", Password = $"wefsd#{new string('*',10)}" };


                Console.WriteLine($"Adding data by key({i}) to repository...");
                repo2.AddNewData(dataN, lastKey);
                Console.WriteLine($"***********************Executing: {i} DONE**************************\n");
            }
            erp = EgorEngine.LoadEgorFile(fileName);
            var Mkey = erp.EncryptedKeySlot.Where((EgorKey k) => { return HashUtil.ComputeHash(EgorVersion.V1, key2).SequenceEqual(k.KeyHash); }).First();
            Mkey.OpenKey(key2);

            var repo3 = EgorEngine.DecryptRepository(erp, Mkey);

            foreach(var data in repo3.KeyData)
            {
                Console.WriteLine($"({data.ID} {data.Name})");
            }



            Console.WriteLine("Test is done");
        }
    }
}