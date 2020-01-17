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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.Platform.Windows.Elevated
{
	public class IpcBase
	{
		public Dictionary<string, string> m_cmdline;
		public bool m_serviceMode = false;
		public string m_sessionKey = "";
		
		public ManualResetEvent ClientDisconnectSignal = new ManualResetEvent(false);		

		public delegate void CommandHandler(Dictionary<string, string> parameters);
		public event CommandHandler CommandEvent;

		public void ReplyPID(int pid)
		{
			SendMessage("ee:pid:" + Conversions.StringToBase64(pid.ToString(System.Globalization.CultureInfo.InvariantCulture)));
		}

		public void ReplyCommand(string id, string data)
		{
			SendMessage("ee:data:" + id + ":" + Conversions.StringToBase64(data));
		}

		public void ReplyException(string id, string message)
		{
			SendMessage("ee:exception:" + id + ":" + Conversions.StringToBase64(message));
		}

		public void EndCommand(string id)
		{
			SendMessage("ee:end:" + id);
		}

		public void LogRemote(string msg)
		{
			SendMessage("ee:log:" + Conversions.StringToBase64(msg));
		}

		public void LogFatal(string msg)
		{
			SendMessage("ee:fatal:" + Conversions.StringToBase64(msg));
		}

		public void LogLocal(string msg)
		{
			LogDebug(msg);
		}

		public void LogDebug(string msg)
		{
			Utils.LogDebug(msg);
		}

		public void DoCommand(Dictionary<string, string> parameters)
		{
			CommandEvent(parameters);
		}

		public virtual void MainLoop()
		{
			//Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
		}

		public virtual void SendMessage(string line)
		{
			throw new Exception("Not implemented");
		}

		public virtual void Close(string reason)
		{
			m_sessionKey = "";
		}

		private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			LogLocal(e.Exception.Message);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			// Log the exception, display it, etc
			LogLocal((e.ExceptionObject as Exception).Message);
		}
	}
}
