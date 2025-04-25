using System.IO;
using UnityEngine;

namespace Xeon.SaveSystem
{
    [CreateAssetMenu(fileName = "XSaveSetting", menuName = "XSave/Setting file")]
    public class XSaveSetting : ScriptableObject
    {
        [SerializeField, Tooltip("Key used to encrypt saved data.\nMust be a single byte character and 32 digits.")]
        private string encryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
        [SerializeField, Tooltip("IV to be used for encryption for saved data.\nMust be a single byte character and 16 digits.")]
        private string encryptionIV = "0123456789ABCDEF";
        [SerializeField, Tooltip("Path for create save data files.")]
        private string savePath = "../SaveData";
        [SerializeField, Tooltip("Extension for save data files.")]
        private string saveFileExtension = "dat";
        [SerializeField, Tooltip("If this flag is True, the save data is encrypted, if False, the save data is JSON string")]
        private bool isEncrypt = true;
        [SerializeField, Tooltip("If this flag is True, the save data stored to PlayerPrefs and no save data file is created.")]
        private bool usePlayerPrefs = false;

        public string EncryptionKey => encryptionKey;
        public string EncryptionIV => encryptionIV;
        public string SavePath => savePath;
        public string SaveFileExtension => saveFileExtension;
        public bool IsEncrypt => isEncrypt;
        public bool UsePlayerPrefs => usePlayerPrefs;

        public string GetSaveDirectory()
            => Path.Join(Application.dataPath, savePath);

        public string GetFullPath(string key)
            => Path.Join(Application.dataPath, savePath, $"{key}.{saveFileExtension}");
    }
}
