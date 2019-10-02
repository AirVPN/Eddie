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
using Eddie.Common.Threading;
using System;
using System.Collections.Generic;

namespace Eddie.Common
{
    public class Options
    {
        private object m_sync = new object();
        private Dictionary<string, string> m_options = new Dictionary<string, string>();

        public Options()
        {

        }

        public DataLocker<Dictionary<string, string>> Values
        {
            get
            {
                return new DataLocker<Dictionary<string, string>>(m_options, m_sync);
            }
        }

        public int Count
        {
            get
            {
                lock(m_sync)
                {
                    return m_options.Count;
                }
            }
        }

        public bool Contains(string name)
        {
            lock(m_sync)
            {
                return m_options.ContainsKey(name);
            }
        }

        public string Get(string name, string defValue = null)
        {
            lock(m_sync)
            {
                if(m_options.ContainsKey(name))
                {
                    string value = m_options[name];
                    if(Utils.Empty(value) == false)
                        return value;
                }

                return defValue;
            }
        }

        public T Get<T>(string name, T? defValue = null) where T : struct
        {
            string value = Get(name, defValue != null ? Convert.ToString(defValue) : null);
            if(value != null)
            {
                if(typeof(T).Equals(typeof(bool)))
                    return (T) Convert.ChangeType(Convert.ToBoolean(value), typeof(T));
                else if(typeof(T).Equals(typeof(short)))
                    return (T) Convert.ChangeType(Convert.ToInt16(value), typeof(T));
                else if(typeof(T).Equals(typeof(int)))
                    return (T) Convert.ChangeType(Convert.ToInt32(value), typeof(T));
                else if(typeof(T).Equals(typeof(long)))
                    return (T) Convert.ChangeType(Convert.ToInt64(value), typeof(T));
                else if(typeof(T).Equals(typeof(ushort)))
                    return (T) Convert.ChangeType(Convert.ToUInt16(value), typeof(T));
                else if(typeof(T).Equals(typeof(uint)))
                    return (T) Convert.ChangeType(Convert.ToUInt32(value), typeof(T));
                else if(typeof(T).Equals(typeof(ulong)))
                    return (T) Convert.ChangeType(Convert.ToUInt64(value), typeof(T));
                else if(typeof(T).Equals(typeof(float)))
                    return (T) Convert.ChangeType(Convert.ToSingle(value), typeof(T));
                else if(typeof(T).Equals(typeof(double)))
                    return (T) Convert.ChangeType(Convert.ToDouble(value), typeof(T));
                else
                    throw new Exception("Unsupported conversion type '" + typeof(T).ToString() + "'");
            }

            return defValue != null ? defValue.Value : new T();
        }

        public Options Set(string name, string value)
        {
            lock(m_sync)
            {
                m_options[name] = value;
            }

            return this;
        }

        public Options Set<T>(string name, T? value) where T : struct
        {
            return Set(name, value != null ? Convert.ToString(value.Value) : null);
        }

        public void Clear()
        {
            lock(m_sync)
            {
                m_options.Clear();
            }
        }
    }
}
#endif