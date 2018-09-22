// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Eddie.Common.Collections
{
    public class OrderedSet<T> : ICollection<T>
    {
        private readonly IDictionary<T, LinkedListNode<T>> m_dictionary = null;
        private readonly LinkedList<T> m_list = null;

        public OrderedSet() : this(EqualityComparer<T>.Default)
        {

        }

        public OrderedSet(IEqualityComparer<T> comparer)
        {
            m_dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
            m_list = new LinkedList<T>();
        }

        public int Count
        {
            get
            {
                return m_dictionary.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                int i = 0;
                foreach(T t in this)
                {
                    if(i == index)
                        return t;

                    i++;
                }

                throw new IndexOutOfRangeException();
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return m_dictionary.IsReadOnly;
            }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public bool Add(T item)
        {
            if(m_dictionary.ContainsKey(item))
                return false;

            LinkedListNode<T> node = m_list.AddLast(item);
            m_dictionary.Add(item, node);
            return true;
        }

        public void Clear()
        {
            m_list.Clear();
            m_dictionary.Clear();
        }

        public bool Remove(T item)
        {
            LinkedListNode<T> node;
            if(!m_dictionary.TryGetValue(item, out node))
                return false;

            m_dictionary.Remove(item);
            m_list.Remove(node);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return m_dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }     
    }
}
#endif