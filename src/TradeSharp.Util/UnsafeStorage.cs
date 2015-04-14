using System.Collections.Generic;
using System.Linq;

namespace TradeSharp.Util
{
    public class UnsafeStorage<K, V> : IStorage<K, V>
    {
        private Dictionary<K, V> dictionary =
            new Dictionary<K, V>();
        
        public void UpdateValues(K[] keys, V[] _values)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                if (dictionary.ContainsKey(keys[i]))
                    dictionary[keys[i]] = _values[i];
                else
                    dictionary.Add(keys[i], _values[i]);
            }           
        }

        public void UpdateValues(K key, V _value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = _value;
            else dictionary.Add(key, _value);
        }

        public V ReceiveValue(K key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : default(V);
        }

        public Dictionary<K, V> ReceiveAllData()
        {            
            return dictionary.ToDictionary(pair => pair.Key, pair => pair.Value);            
        }

        public void Copy(IStorage<K, V> sourceDic)
        {
            dictionary = sourceDic.ReceiveAllData();
        }

        public void Copy(Dictionary<K, V> sourceDic)
        {
            dictionary = sourceDic;
        }

        public void Clear()
        {
            dictionary.Clear();
        }
    }
}
