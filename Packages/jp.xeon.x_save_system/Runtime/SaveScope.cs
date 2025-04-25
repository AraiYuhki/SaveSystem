using System;

namespace Xeon.SaveSystem
{
    public class SaveScope<T> : IDisposable where T : class, new()
    {
        private T instance = null;
        private string saveKey = string.Empty;

        public T Instance => instance;

        public SaveScope(string saveKey)
        {
            this.saveKey = saveKey;
            instance = DataBank.Instance.GetOrCreate<T>(saveKey);
        }

        public void Dispose()
        {
            DataBank.Instance.Store(saveKey, instance);
            DataBank.Instance.Save(saveKey);
        }
    }
}
