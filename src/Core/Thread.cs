// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace AirVPN.Core
{
    public class Thread
    {
        public volatile bool CancelRequested = false;
        protected System.Threading.Thread m_Thread;

        public Thread(bool start = true)
        {
            m_Thread = new System.Threading.Thread(this.DoRun);
            if(NeedApartmentState())
                m_Thread.SetApartmentState(ApartmentState.STA);  
            m_Thread.Priority = GetPriority();
            if(start)
                m_Thread.Start();        
        }

        public static Core.Engine Engine
        {
            get
            {
                return Engine.Instance;
            }
        }

        public virtual void Start()
        {
            m_Thread.Start();
        }
                
        public virtual void RequestStop()
        {
            CancelRequested = true;
        }

        public void RequestStopSync()
        {
            RequestStop();
            m_Thread.Join();
        }

        public void Sleep(int msec)
        {
            System.Threading.Thread.Sleep(msec);
        }

        public void DoRun()
        {
            OnRun();
        }

        public virtual ThreadPriority GetPriority()
        {
            return ThreadPriority.Normal;
        }

        public virtual bool NeedApartmentState()
        {
            return false;
        }

        public virtual void OnRun()
        {
        }
    }
}
