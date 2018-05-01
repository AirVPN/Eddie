#if !EDDIENET2
using Eddie.Common.Validators;
using System.Collections.Generic;

namespace Eddie.Common.Collections
{
    public class MultiDictionary<K, V> : Dictionary<K, HashSet<V>>
    {
        private IEqualityComparer<V> m_comparer;

        public MultiDictionary() : this(EqualityComparer<V>.Default)
        {
            
        }

        public MultiDictionary(IEqualityComparer<V> comparer)
        {
            m_comparer = comparer;
        }

        public void Add(K key, V value)
        {
            ArgumentVerifier.CantBeNull(key, "key");

            HashSet<V> container = null;
            if(!TryGetValue(key, out container))
            {
                container = new HashSet<V>(m_comparer);
                base.Add(key, container);
            }

            container.Add(value);
        }

        public void AddRange(K key, IEnumerable<V> values)
        {
            if(values == null)
                return;
         
            foreach(V value in values)
            {
                Add(key, value);
            }
        }
        
        public bool ContainsValue(K key, V value)
        {
            ArgumentVerifier.CantBeNull(key, "key");

            bool toReturn = false;
            HashSet<V> values = null;
            if(TryGetValue(key, out values))
                toReturn = values.Contains(value);
            
            return toReturn;
        }

        public bool Remove(K key, V value)
        {
            ArgumentVerifier.CantBeNull(key, "key");

            HashSet<V> container = null;
            if(TryGetValue(key, out container))
            {
                if(container.Remove(value))
                {
                    if(container.Count <= 0)
                        return Remove(key);

                    return true;
                }
            }

            return false;
        }

        public void Merge(MultiDictionary<K, V> toMergeWith)
        {
            if(toMergeWith == null)
                return;

            foreach(KeyValuePair<K, HashSet<V>> pair in toMergeWith)
            {
                foreach(V value in pair.Value)
                {
                    Add(pair.Key, value);
                }
            }
        }
        
        public HashSet<V> GetValues(K key, bool returnEmptySet = true)
        {
            HashSet<V> toReturn = null;
            if(!TryGetValue(key, out toReturn) && returnEmptySet)
                toReturn = new HashSet<V>(m_comparer);
        
            return toReturn;
        }
    }
}
#endif