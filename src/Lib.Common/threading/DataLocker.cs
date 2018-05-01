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
