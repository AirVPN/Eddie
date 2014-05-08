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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using AirVPN.Core;

namespace AirVPN.Core.Threads
{
    public class Manifest : AirVPN.Core.Thread
    {
		public static Manifest Instance;

        private string m_LastResult;
		public bool ForceUpdate = false;
		
		public AutoResetEvent Updated = new AutoResetEvent(false);


		public Manifest()
		{
			Instance = this;
		}

        public string GetLastResult()
        {
            lock(this)
            {
                return m_LastResult;
            }
        }

        public override ThreadPriority GetPriority()
        {
            return ThreadPriority.Lowest;
        }

        public override void OnRun()
        {
			for (; ; )
			{
				if ((ForceUpdate) || (Engine.Storage.UpdateManifestNeed(true)))
				{
					m_LastResult = Engine.Storage.UpdateManifest();
					if(m_LastResult != "")
						Engine.Instance.Log(Engine.LogType.Warning, Messages.Format(Messages.ManifestUpdate, m_LastResult));
					ForceUpdate = false;
					Updated.Set();
				}
				
				for (int i = 0; i < 600; i++) // Every minute
				{
					Sleep(100);

					if (ForceUpdate)
						break;

					if (CancelRequested)
						return;
				}
			}
            
        }
                
        
    }
}