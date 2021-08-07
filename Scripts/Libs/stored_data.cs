//C#
// Store Data
//  This Library let you store and read data (classes) into a json file located into Data folder
//  All is sotores as Global or "Per Character"
// SimonSoft 2021
//
// To use this script add Newtonsoft.Json.dll to Assemblies.cfg file
// To edit this script without VS errors add referene to Newtonsoft.Json.dll from RE folder
//
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorEnhanced;

namespace Scripts.Libs
{
    public class StoredData
    {
        /// <summary>
        /// This Class is only for test
        /// </summary>
        private class TestClass
        {
            public string A { get; set; }
            public int B { get; set; }
            public double C { get; set; }
        }

        /// <summary>
        /// This TestRun is only for test
        /// </summary>
        public void TestRun()
        {
            StoreData(new List<int>() { 1, 2, 3, 4, 5, 6 }, "global1", true);
            StoreData("mystring", "global2", true);
            StoreData(new TestClass() { A = "1", B = 2, C = 3.0 }, "global3", true);

            List<int> global1 = GetData<List<int>>("global1", true);
            string global2 = GetData<string>("global2", true);
            TestClass global3 = GetData<TestClass>("global3", true);

            StoreData(new List<bool>() { true, true, false }, "acnt1", false);
            StoreData(12.44, "acnt2", false);
            StoreData(new TestClass() { A = "zz", B = 3, C = 4.5 }, "acnt3", false);

            List<bool> acnt1 = GetData<List<bool>>("acnt1", false);
            double acnt2 = GetData<double>("acnt2", true);
            TestClass acnt3 = GetData<TestClass>("acnt3", true);
        }

        /// <summary>
        /// JSON Stucture
        /// </summary>
        private class Storage
        {
            public Storage()
            {
                Global = new Dictionary<string, object>();
                Accounts = new Dictionary<string, Account>();
            }

            [System.Serializable()]
            public class Account
            {
                public Account ()
                {
                    Data = new Dictionary<string, object>();
                }
                public Dictionary<string, object> Data { get; set; }
            }

            public Dictionary<string, object> Global { get; set; }
            public Dictionary<string, Account> Accounts { get; set; }
        }

        /// <summary>
        /// Store an object as Json
        /// </summary>
        /// <param name="data">objet to be stored</param>
        /// <param name="keyName">Name of the object stored</param>
        /// <param name="global">If true this will be visible to all chars. If false will be specific per character</param>
        public static void StoreData(object data, string keyName, bool global)
        {
            string dataFolder = Path.GetFullPath(Path.Combine(Assistant.Engine.RootPath, "Data"));
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            string jsonText = "";
            string dataFile = Path.Combine(dataFolder, "StoredData.json");
            if (File.Exists(dataFile))
            {
                jsonText = File.ReadAllText(dataFile);
            }

            Storage jsonObj = JsonConvert.DeserializeObject<Storage>(jsonText);
            if (jsonObj == null)
            {
                jsonObj = new Storage();
            }

            if (global)
            {
                jsonObj.Global[keyName] = data;
            }
            else
            {
                if (jsonObj.Accounts.ContainsKey(Player.Name))
                {
                    jsonObj.Accounts[Player.Name].Data[keyName] = data;
                } 
                else
                {
                    Storage.Account newAccount = new Storage.Account();
                    newAccount.Data[keyName] = data;
                    jsonObj.Accounts.Add(Player.Name, newAccount);
                }
                
            }

            jsonText = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

            File.WriteAllText(dataFile, jsonText);
        }

        /// <summary>
        /// Gets data from JSON
        /// </summary>
        /// <typeparam name="T">Returned object type</typeparam>
        /// <param name="keyName">>Name of the object required</param>
        /// <param name="global">If true this will be visible to all chars. If false will be specific per character</param>
        /// <returns>Requested object</returns>
        public static T GetData<T>(string keyName, bool global)
        {
            string dataFolder = Path.GetFullPath(Path.Combine(Assistant.Engine.RootPath, "Data"));
            if (!Directory.Exists(dataFolder))
            {
                return default;
            }

            string dataFile = Path.Combine(dataFolder, "StoredData.json");
            if (!File.Exists(dataFile))
            {
                return default;
            }

            string jsonText = File.ReadAllText(dataFile);
            Storage jsonObj = JsonConvert.DeserializeObject<Storage>(jsonText);
            if (jsonObj == null)
            {
                return default;
            }

            object retVal;

            if (global)
            {
                if (!jsonObj.Global.ContainsKey(keyName)) return default;
                retVal = jsonObj.Global[keyName];
            } 
            else
            {
                if (!jsonObj.Accounts.ContainsKey(Player.Name)) return default;
                if (!jsonObj.Accounts[Player.Name].Data.ContainsKey(keyName)) return default;
                retVal = jsonObj.Accounts[Player.Name].Data[keyName];
            }

            if (retVal is Newtonsoft.Json.Linq.JObject)
            {
                JObject j1 = retVal as JObject;
                return j1.ToObject<T>();
            }

            if (retVal is Newtonsoft.Json.Linq.JArray)
            {
                JArray j1 = retVal as JArray;
                return j1.ToObject<T>();
            }

            // Need help C# to convert the object to requested type
            // Without this, if retVal = 0, there is an exception.
            return (T)Convert.ChangeType(retVal, typeof(T));
        }
    }

}
