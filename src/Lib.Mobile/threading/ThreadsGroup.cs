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
