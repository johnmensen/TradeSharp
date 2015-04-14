using System.Collections.Generic;

namespace TradeSharp.Util
{
    public interface IStorage <K, V>
    {
        void UpdateValues(K[] keys, V[] _values);

        void UpdateValues(K key, V _value);

        V ReceiveValue(K key);

        Dictionary<K, V> ReceiveAllData();

        void Copy(IStorage<K, V> sourceDic);

        void Copy(Dictionary<K, V> sourceDic);

        void Clear();
    }
}
