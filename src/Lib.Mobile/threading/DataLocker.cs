// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2018 AirVPN (support@airvpn.org) / https://airvpn.org
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

using System;
using System.Threading;

namespace Eddie.Common.Threading
{   
    /*
    Exception safe usage:
    
    using(Locker<data_type> l = new Locker<data_type>(<data>, <sync>))
    {
        ...
        data_type data = l.Data
        ...
    }           
    */ 
    
    public sealed class DataLocker<T> : IDisposable where T : class
    {
        private T m_data;
        private object m_sync;

        public DataLocker(T data, object sync)
        {
            Monitor.Enter(sync);

            m_data = data;
            m_sync = sync;            
        }

        ~DataLocker()
        {
            Dispose(false);
        }

        public T Data
        {
            get
            {
                return m_data;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(disposing)
              GC.SuppressFinalize(this);                
            
            if(m_sync != null)
            {
                try
                {
                    Monitor.Exit(m_sync);
                }
                catch // Catch everything
                {
                    
                }
                finally
                {
                    m_sync = null;
                    m_data = null;
                }
            }
        }
    }
}
