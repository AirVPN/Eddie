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
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using AirVPN.Core;

namespace AirVPN.Core
{
    public class RoutesManager
    {
        public static RoutesManager Instance = new RoutesManager();

		

		public RoutesManager()
		{
			Instance = this;
		}

        public bool GetLockActive()
        {
            return (DefaultGateway.Empty == false);
        }

        public bool LockActivate()
        {
            lock(this)
            {
				if (GetLockActive() == true)
                    return true;

                

                return true;
            }
        }

        public void LockDeactivate()
        {
            lock (this)
            {
				if (GetLockActive() == false)
                    return;

                
            }
        }

		public void OnRecoveryLoad(XmlElement root)
		{
			try
			{
				XmlElement node = Utils.XmlGetFirstElementByTagName(root, "NetworkLocking");
				if (node != null)
				{
					
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
			}
		}

		public void OnRecoverySave(XmlElement root)
		{
			if (GetLockActive() == false)
				return;

			try
			{
				
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
			}
		}


		
		
    }
}
