using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Xeon.SaveSystem
{
    public class DataBank
    {
        private static DataBank instance = new DataBank();
        private static Dictionary<string, object> bank = new();

        private const string path = "SaveData";
        private const string extension = "dat";
        private static readonly string fullPath = Path.Combine(Application.dataPath, "../", path);

        public static bool IsEncrypt { get; set; }
        public string SavePath => fullPath;

        private DataBank() { }

        public static DataBank Instance => instance;

        public bool IsEmpty() => bank.Count == 0;

        public bool ExistsKey(string key) => bank.ContainsKey(key);

        public void Store(string key, object obj) => bank[key] = obj;

        public void Clear() => bank.Clear();

        public void Remove(string key) => bank.Remove(key);

        public DataType Get<DataType>(string key) => ExistsKey(key) ? (DataType)bank[key] : default;

        private void CreateDirectoryIfNeed()
        {
            var directory = new DirectoryInfo(fullPath);
            if (!directory.Exists) directory.Create();
        }

        public void SaveAll()
        {
            CreateDirectoryIfNeed();
            foreach (var key in bank.Keys)
                Save(key);
        }

        public bool Save(string key)
        {
            if (!ExistsKey(key)) return false;
            CreateDirectoryIfNeed();
            var filePath = $"{fullPath}/{key}.{extension}";

            var json = JsonUtility.ToJson(bank[key]);
            if (!IsEncrypt)
            {
                using (var sw = new StreamWriter(filePath, false))
                    sw.Write(json);
                return true;
            }
            var data = Encoding.UTF8.GetBytes(json);
            data = Compressor.Compress(data);
            data = Cryptor.Encrypt(data);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            using (var fileStream = File.Create(filePath))
                fileStream.Write(data, 0, data.Length);

            return true;
        }

        public bool Load<DataType>(string key)
        {
            var filePath = $"{fullPath}/{key}.{extension}";

            if (!File.Exists(filePath)) return false;

            if (!IsEncrypt)
            {
                using (var streamReader = new StreamReader(filePath, Encoding.UTF8))
                {
                    var text = streamReader.ReadToEnd();
                    bank[key] = JsonUtility.FromJson<DataType>(text);
                    return true;
                }
            }

            byte[] data = null;
            using (var fileStream = File.OpenRead(filePath))
            {
                data = new byte[fileStream.Length];
                fileStream.Read(data, 0, data.Length);
            }

            data = Cryptor.Decrypt(data);
            data = Compressor.Decompress(data);

            var json = Encoding.UTF8.GetString(data);

            bank[key] = JsonUtility.FromJson<DataType>(json);

            return true;
        }

        public T GetOrCreate<T>(string key, Func<T> createFunc = null) where T : new()
        {
            Load<T>(key);
            return Get<T>(key) ?? Create(key, createFunc);
        }

        private T Create<T>(string key, Func<T> createFunc = null) where T : new()
        {
            var instance = createFunc == null ? new T() : createFunc();
            Store(key, instance);
            Save(key);
            return instance;
        }

        public void LoadOrCreate<T>(string key, Func<T> createFunc = null) where T : new()
        {
            if (!Load<T>(key))
            {
                var instance = createFunc == null ? new T() : createFunc();
                Store(key, instance);
            }
        }

        public bool ExistSaveFile(string key)
        {
            var filePath = $"{fullPath}/{key}.{extension}";
            return File.Exists(filePath);
        }

        public void DeleteSaveFile(string key)
        {
            var filePath = $"{fullPath}/{key}.{extension}";
            if (!ExistSaveFile(key)) return;
            File.Delete(filePath);
        }
    }
}
