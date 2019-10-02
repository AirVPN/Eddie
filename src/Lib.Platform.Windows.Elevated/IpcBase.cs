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
		public bool Loop = false; // If false, exit when first connection end.
		public string m_sessionKey = "";
		//public bool m_shutdown = false;
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

		public void RemoteLog(string msg)
		{
			SendMessage("ee:log:" + Conversions.StringToBase64(msg));
		}

		public void FatalLog(string msg)
		{
			SendMessage("ee:fatal:" + Conversions.StringToBase64(msg));
		}

		public void LocalLog(string msg)
		{
			DebugLog(msg);
		}

		public void DebugLog(string msg)
		{
			Utils.DebugLog(msg);
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
			DebugLog(e.Exception.Message);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			// Log the exception, display it, etc
			DebugLog((e.ExceptionObject as Exception).Message);
		}
	}
}
