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
using System.Text;
using System.Xml;

namespace Eddie.Core
{
	public class ProgramScope
	{
		private string m_path;
        private string m_name;

		public ProgramScope(string path, string name)
		{
			Start(path, name);
		}
				
		~ProgramScope()
		{
			End();
		}

		public void Start(string path, string name)
		{
			m_path = path;
            m_name = name;
			if (Engine.Instance.NetworkLockManager != null)
				Engine.Instance.NetworkLockManager.AllowProgram(m_path, m_name);							
		}

		public void End()
		{
            if(m_path != "")
            { 
			    if (Engine.Instance.NetworkLockManager != null)
				    Engine.Instance.NetworkLockManager.DeallowProgram(m_path, m_name);
                m_path = "";
            }
        }
	}
}
