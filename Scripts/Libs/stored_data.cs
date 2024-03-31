//C#
// Stored Data Library - CaporaleSimone 2021-2024
// This library allows you to store/read data in a JSON file based on the storage type (Global, Server, or Character).
//
// To edit this script without VS errors add referene to Newtonsoft.Json.dll from RE folder

//#assembly <Newtonsoft.Json.dll>

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace RazorEnhanced
{
    internal class StoredData
    {
        public enum StoreType
        {
            Global,
            Server,
            Character
        }

        private class Server
        {
            public Dictionary<string, object> ServerGlobal { get; set; }
            public Dictionary<string, Character> Characters { get; set; }
            public Server()
            {
                ServerGlobal = new Dictionary<string, object>();
                Characters = new Dictionary<string, Character>();
            }
        }

        private class Character : Dictionary<string, object>
        {
            public Character() { }
            public void AddSetting(string key, object value)
            {
                this[key] = value;
            }
        }

        private class JSONRoot
        {
            public Dictionary<string, object> Global { get; set; }
            public Dictionary<string, Server> Servers { get; set; }
            public JSONRoot()
            {
                Global = new Dictionary<string, object>();
                Servers = new Dictionary<string, Server>();
            }
        }

        private readonly FileInfo _fileName;    // path of the JSON file
        private readonly string _serverName;    // Name of the server
        private readonly string _characterName; // Name of the account

        /// <summary>
        /// Initializes a new instance of the StoredData class.
        /// </summary>
        /// <param name="storedDataFolder">The folder path where the JSON file will be stored.</param>
        /// <param name="serverName">The name of the server.</param>
        /// <param name="characterName">The name of the character.</param>
        public StoredData()
        {
            string _data_folder = Path.GetFullPath(Path.Combine(Assistant.Engine.RootPath, "Data"));
            _fileName = new FileInfo(Path.Combine(_data_folder, "StoredData.json"));
            _serverName = Misc.ShardName();
            _characterName = Player.Name;

            if (!Directory.Exists(_data_folder))
            {
                Directory.CreateDirectory(_data_folder);
            }

            if (!File.Exists(_fileName.FullName))
            {
                File.Create(_fileName.FullName).Close();
            }

            string jsonText = File.ReadAllText(_fileName.FullName);

            bool save = false;
            JSONRoot jsonObj = JsonConvert.DeserializeObject<JSONRoot>(jsonText);

            if (jsonObj == null)
            {
                jsonObj = new JSONRoot();
                save = true;
            }

            if (!jsonObj.Servers.ContainsKey(_serverName))
            {
                jsonObj.Servers[_serverName] = new Server();
                save = true;
            }

            if (!jsonObj.Servers[_serverName].Characters.ContainsKey(_characterName))
            {
                jsonObj.Servers[_serverName].Characters[_characterName] = new Character();
                save = true;
            }

            if (save)
            {
                jsonText = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(_fileName.FullName, jsonText);
            }
        }

        /// <summary>
        /// Returns a string representation of the JSON data.
        /// </summary>
        /// <returns>A string representation of the JSON data.</returns>
        public override string ToString()
        {
            string jsonText = File.ReadAllText(_fileName.FullName);
            JSONRoot jsonObj = JsonConvert.DeserializeObject<JSONRoot>(jsonText);
            if (jsonObj == null) return "";

            return JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
        }

        /// <summary>
        /// Stores data in the JSON file based on the specified storage type.
        /// </summary>
        /// <param name="data">The data to store.</param>
        /// <param name="keyName">The key name to associate with the data.</param>
        /// <param name="storage">The storage type (Global, Server, or Character).</param>
        public void StoreData(object data, string keyName, StoreType storage)
        {
            string jsonText = File.ReadAllText(_fileName.FullName);
            JSONRoot jsonObj = JsonConvert.DeserializeObject<JSONRoot>(jsonText) ?? throw new Exception("Error reading JSON file");

            switch (storage)
            {
                case StoreType.Global:
                    jsonObj.Global[keyName] = data;
                    break;
                case StoreType.Server:
                    jsonObj.Servers[_serverName].ServerGlobal[keyName] = data;
                    break;
                case StoreType.Character:
                default:
                    var character = jsonObj.Servers[_serverName].Characters[_characterName];
                    character.AddSetting(keyName, data);
                    break;
            }

            jsonText = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(_fileName.FullName, jsonText);
        }

        /// <summary>
        /// Retrieves data from the JSON file based on the specified storage type.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve.</typeparam>
        /// <param name="keyName">The key name associated with the data.</param>
        /// <param name="storage">The storage type (Global, Server, or Character).</param>
        /// <returns>The retrieved data or the default value if keyName doesn't exist.</returns>
        public T GetData<T>(string keyName, StoreType storage)
        {
            string jsonText = File.ReadAllText(_fileName.FullName);
            JSONRoot jsonObj = JsonConvert.DeserializeObject<JSONRoot>(jsonText);
            if (jsonObj == null) return default;

            object retVal;

            switch (storage)
            {
                case StoreType.Global:
                    if (!jsonObj.Global.ContainsKey(keyName)) return default;
                    retVal = jsonObj.Global[keyName];
                    break;
                case StoreType.Server:
                    if (!jsonObj.Servers[_serverName].ServerGlobal.ContainsKey(keyName)) return default;
                    retVal = jsonObj.Servers[_serverName].ServerGlobal[keyName];
                    break;
                case StoreType.Character:
                default:
                    if (!jsonObj.Servers[_serverName].Characters[_characterName].ContainsKey(keyName)) return default;
                    retVal = jsonObj.Servers[_serverName].Characters[_characterName][keyName];
                    break;
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

            return (T)Convert.ChangeType(retVal, typeof(T));
        }
    }
}
