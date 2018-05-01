using System.Collections.Generic;
using System.Threading;

namespace Eddie.Common.Threading
{
    public class ThreadsGroup
    {
        private List<Thread> m_threads = new List<Thread>();
        private object m_threadsSync = new object();
   
        public ThreadsGroup()
        {

        }

        public void Add(Thread backupThread, bool start = true)
        {
            lock(m_threadsSync)
            {
                m_threads.Add(backupThread);

                if(start)
                    backupThread.Start();
            }
        }

        private Thread[] GetThreads()
        {
            lock(m_threadsSync)
            {
                return m_threads.ToArray();
            }
        }

        public void Remove(Thread backupThread)
        {
            lock(m_threadsSync)
            {
                m_threads.Remove(backupThread);
            }
        }

        public void Join(int ?timeout = null)
        {
            Thread[] threads = null;

            lock(m_threadsSync)
            {
                threads = m_threads.ToArray();
                m_threads.Clear();
            }

            foreach(Thread thread in threads)
            {
                if(timeout != null)
                    thread.Join(timeout.Value);
                else
                    thread.Join();
            }
        }
    }
}
