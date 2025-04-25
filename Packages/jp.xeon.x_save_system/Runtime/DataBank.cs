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

        private XSaveSetting setting;

        private DataBank() { }

        public static DataBank Instance => instance;

        public void SetSetting(XSaveSetting setting) => this.setting = setting;

        public bool IsEmpty() => bank.Count == 0;

        public bool ExistsKey(string key) => bank.ContainsKey(key);

        public void Store(string key, object obj) => bank[key] = obj;

        public void Clear() => bank.Clear();

        public void Remove(string key) => bank.Remove(key);

        public DataType Get<DataType>(string key) => ExistsKey(key) ? (DataType)bank[key] : default;

        private void CreateDirectoryIfNeed()
        {
            var directory = new DirectoryInfo(setting.GetSaveDirectory());
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
            var filePath = setting.GetFullPath(key);

            var json = JsonUtility.ToJson(bank[key]);
            if (!setting.IsEncrypt)
            {
                using (var sw = new StreamWriter(filePath, false))
                    sw.Write(json);
                return true;
            }
            var data = Encoding.UTF8.GetBytes(json);
            data = Compressor.Compress(data);
            data = Cryptor.Encrypt(data, setting);

            CreateDirectoryIfNeed();

            using (var fileStream = File.Create(filePath))
                fileStream.Write(data, 0, data.Length);

            return true;
        }

        public bool Load<T>(string key) where T : class, new()
        {
            var filePath = setting.GetFullPath(key);

            if (!File.Exists(filePath)) return false;

            if (!setting.IsEncrypt)
            {
                using (var streamReader = new StreamReader(filePath, Encoding.UTF8))
                {
                    var text = streamReader.ReadToEnd();
                    bank[key] = JsonUtility.FromJson<T>(text);
                    return true;
                }
            }

            byte[] data = null;
            using (var fileStream = File.OpenRead(filePath))
            {
                data = new byte[fileStream.Length];
                fileStream.Read(data, 0, data.Length);
            }

            data = Cryptor.Decrypt(data, setting);
            data = Compressor.Decompress(data);

            var json = Encoding.UTF8.GetString(data);

            bank[key] = JsonUtility.FromJson<T>(json);

            return true;
        }

        public T GetOrCreate<T>(string key, Func<T> createFunc = null) where T : class, new()
        {
            Load<T>(key);
            return Get<T>(key) ?? Create(key, createFunc);
        }

        private T Create<T>(string key, Func<T> createFunc = null) where T : class, new()
        {
            var instance = createFunc == null ? new T() : createFunc();
            Store(key, instance);
            Save(key);
            return instance;
        }

        public void LoadOrCreate<T>(string key, Func<T> createFunc = null) where T : class, new()
        {
            if (!Load<T>(key))
            {
                var instance = createFunc == null ? new T() : createFunc();
                Store(key, instance);
            }
        }

        public bool ExistSaveFile(string key)
        {
            var filePath = setting.GetFullPath(key);
            return File.Exists(filePath);
        }

        public void DeleteSaveFile(string key)
        {
            var filePath = setting.GetFullPath(key);
            if (!ExistSaveFile(key)) return;
            File.Delete(filePath);
        }
    }
}
