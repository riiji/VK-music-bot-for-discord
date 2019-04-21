using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disaudiobot.Modules
{
    interface IDataStorage
    {
        void StoreObject(string key, object obj);

        T RestoreObject<T>(string key);
    }

    class StorageData : IDataStorage
    {
        private Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        public void StoreObject(string key, object obj)
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key] = obj;
                return;
            }
            _dictionary.Add(key, obj);

        }

        public T RestoreObject<T>(string key)
        {
            if (!_dictionary.ContainsKey(key))
                throw new ArgumentException($"{key} not founded!");
            return (T)_dictionary[key];
        }

        public void RemoveObject(string key)
        {
            _dictionary.Remove(key);
        }
        
    }
}
