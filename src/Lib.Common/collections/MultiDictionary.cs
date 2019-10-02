// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

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