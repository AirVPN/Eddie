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