#if !EDDIENET2
using Eddie.Common.Validators;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Eddie.Common.Collections
{
    public class MultiOrderedDictionary : OrderedDictionary
    {
        public MultiOrderedDictionary()
        {
            
        }

        public bool TryGetValue(object key, out OrderedSet<object> value)
        {
            if(Contains(key))
            {
                value = (OrderedSet<object>) this[key];
                return true;
            }

            value = null;
            return false;
        }

        public new bool Add(object key, object value)
        {
            ArgumentVerifier.CantBeNull(key, "key");

            OrderedSet<object> container = null;
            if(!TryGetValue(key, out container))
            {
                container = new OrderedSet<object>();
                base.Add(key, container);
            }

            return container.Add(value);
        }

        public void AddRange(object key, IEnumerable<object> values)
        {
            if(values == null)
                return;
         
            foreach(object value in values)
            {
                Add(key, value);
            }
        }
        
        public bool ContainsValue(object key, object value)
        {
            ArgumentVerifier.CantBeNull(key, "key");

            bool toReturn = false;
            OrderedSet<object> values = null;
            if(TryGetValue(key, out values))
                toReturn = values.Contains(value);
            
            return toReturn;
        }

        public bool Remove(object key, object value)
        {
            ArgumentVerifier.CantBeNull(key, "key");

            OrderedSet<object> container = null;
            if(TryGetValue(key, out container))
            {
                if(container.Remove(value))
                {
                    if(container.Count <= 0)                    
                        Remove(key);

                    return true;
                }
            }

            return false;
        }

        public void Merge(MultiOrderedDictionary toMergeWith)
        {
            if(toMergeWith == null)
                return;

            foreach(KeyValuePair<object, OrderedSet<object>> pair in toMergeWith)
            {
                foreach(object value in pair.Value)
                {
                    Add(pair.Key, value);
                }
            }
        }
        
        public OrderedSet<object> GetValues(object key, bool returnEmptySet = true)
        {
            OrderedSet<object> toReturn = null;
            if(!TryGetValue(key, out toReturn) && returnEmptySet)
                toReturn = new OrderedSet<object>();
        
            return toReturn;
        }
    }
}
#endif