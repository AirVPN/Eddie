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

using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Xml;
using Eddie.Core;
using Microsoft.Win32;

namespace Eddie.Platforms
{
	public class NetworkLockWfp : NetworkLockPlugin
	{
		public override string GetCode()
		{
			return "windows_wfp";
		}

		public override string GetName()
		{
			return "Windows Filtering Platform";
		}

		public override void Init()
		{
			base.Init();

			
		}
			
		public override void Activation()
		{
			base.Activation();

			
		}

		public override void Deactivation()
		{
			base.Deactivation();

			
		}
        
        public override void OnUpdateIps()
		{
			  
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			base.OnRecoveryLoad(root);
            
		}

		public override void OnRecoverySave(XmlElement root)
		{
			base.OnRecoverySave(root);

			
		}
	}
}
