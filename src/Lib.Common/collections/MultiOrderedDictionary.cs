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