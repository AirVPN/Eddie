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

using System;
using System.Collections.Generic;
using System.Threading;

/*
This is the interface class for communication with helper executable that require root/admin privileges.
Derivated class (different in each platform) perform the launch, security checks and manage a stdio/stdout line-based communication.
Every command requested from Eddie to Helper is identified by a random ID and have a dictionary of parameters.
Command can be executed in async (event-driven for data) or sync mode.
*/

namespace Eddie.Core.Elevated
{
	public class Command
	{
		public UInt32 Id = 0;
		public Dictionary<string, string> Parameters = new Dictionary<string, string>();
		public AutoResetEvent Complete = new AutoResetEvent(false);

		public delegate void ReceiveEventHandler(Command c, string data);
		public event ReceiveEventHandler ReceiveEvent;

		public delegate void ExceptionEventHandler(Command c, string message);
		public event ExceptionEventHandler ExceptionEvent;

		public delegate void CompleteEventHandler(Command c);
		public event CompleteEventHandler CompleteEvent;

		private bool m_complete = false;
		private string m_syncResult = "";
		private string m_syncException = "";

		public bool IsComplete
		{
			get
			{
				return m_complete;
			}
		}

		public void Data(string data)
		{
			if (ReceiveEvent != null)
				ReceiveEvent(this, data);
		}

		public void End()
		{
			m_complete = true;
			if (CompleteEvent != null)
				CompleteEvent(this);

			Complete.Set();
		}

		public void Abort()
		{
			End();
		}

		public void Exception(string ex)
		{
			m_syncException = ex;

			if (ExceptionEvent != null)
				ExceptionEvent(this, ex);

			Abort();
		}

		public void AddSyncResult(string data)
		{
			m_syncResult += data + "\n";
		}

		public void DoSync()
		{
			Engine.Instance.Elevated.DoCommandSync(this);
		}

		public void DoASync()
		{
			Engine.Instance.Elevated.DoCommandASync(this);
		}

		public string GetSyncResult()
		{
			if (m_syncException != "")
				throw new Exception(m_syncException);
			else
				return m_syncResult.Trim();
		}
	}
}
