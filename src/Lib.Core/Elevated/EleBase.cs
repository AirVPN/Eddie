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
using System.Diagnostics;
using System.Text;
using System.Threading;

/*
This is the interface class for communication with helper executable that require root/admin privileges.
Derivated class (different in each platform) perform the launch, security checks and manage communication.
Every command requested from Eddie to Helper is identified by a random ID and have a dictionary of parameters.
Command can be executed in async (event-driven for data) or sync mode.
*/

namespace Eddie.Core.Elevated
{
	public class EleBase
	{
		public Dictionary<UInt32, Command> PendingCommands = new Dictionary<UInt32, Command>();

        protected string m_sessionKey = "";
		protected string m_failedReason = "";
		protected string m_bufferReceive = "";
		protected bool m_started = false;

		public bool ServiceEdition = false;
		public bool ServiceUninstallAtEnd = false;

        private System.Diagnostics.Process m_process;
        private UInt32 m_id = 0;
        
		private bool m_shutdownInProgress = false;
				
		public virtual void Start()
		{
			ServiceEdition = false;
			ServiceUninstallAtEnd = false;
		}
		
		public virtual void Stop()
		{
			m_shutdownInProgress = true;

			try
            {
                if (m_process != null)
                {
                    try
                    {
                        this.DoCommandASync("exit");
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (ServiceEdition == false)
                        {
                            if (m_process.HasExited == false)
                            {
                                m_process.WaitForExit();
                            }
                        }
                    }
                    catch
                    {
                    }

                    m_process.Dispose();
                    m_process = null;
                }
            }
            catch
            {
            }

			// See comment in C++ ServiceUninstallSupportRealtime
			if ( (ServiceUninstallAtEnd) && (Platform.Instance.GetServiceUninstallSupportRealtime() == false) )
				Platform.Instance.SetService(false, true);
		}

		public bool ShutdownInProgress
		{
			get
			{
				return m_shutdownInProgress;
			}
		}
        
		public void FatalError(string reason)
		{
			if (m_failedReason != "") // Already catched
				return; 

			m_failedReason = reason;
			lock (PendingCommands)
			{
				foreach (KeyValuePair<UInt32, Command> kp in PendingCommands)
				{
					kp.Value.Abort();
				}
			}

			if (m_started)
			{
				Engine.Instance.Logs.LogFatal(LanguageManager.GetText("HelperPrivilegesCrash") + ":" + reason);
				Environment.Exit(0); // Brutal, but correct.
			}
		}

		// Async
		public void DoCommandASync(Command c)
		{
			lock (PendingCommands)
			{
				c.Id = (m_id++);
				PendingCommands[c.Id] = c;
			}

			c.Parameters["_id"] = c.Id.ToString();
			c.Parameters["_token"] = m_sessionKey;
            c.Parameters["_debug"] = ((Engine.Instance != null) && (Engine.Instance.Storage != null) && (Engine.Instance.Storage.GetBool("log.level.debug")) ? "1" : "0");
            string line = "";
			foreach (KeyValuePair<string, string> kp in c.Parameters)
			{
				line += kp.Key + ":" + Conversions.StringToBase64(kp.Value) + ";";
			}
			line += "\n";

			SendLine(line);
		}

		// Sync
		public string DoCommandSync(Command c)
		{
			c.ReceiveEvent += DoCommandSyncReceive;
			DoCommandASync(c);
			for (; ; )
			{
				if (m_failedReason != "")
					throw new Exception(m_failedReason);
				if (c.Complete.WaitOne(100))
					break;
			}

			return c.GetSyncResult();
		}

		// Sync Helper
		public Command DoCommandASync(string command)
		{
			Command c = new Command();
			c.Parameters["command"] = command;
			DoCommandASync(c);
			return c;
		}

		public string DoCommandSync(string command)
		{
			Command c = new Command();
			c.Parameters["command"] = command;
			return DoCommandSync(c);
		}

		public string DoCommandSync(string command, string key1, string val1)
		{
			Command c = new Command();
			c.Parameters["command"] = command;
			c.Parameters[key1] = val1;
			return DoCommandSync(c);
		}

		public string DoCommandSync(string command, string key1, string val1, string key2, string val2)
		{
			Command c = new Command();
			c.Parameters["command"] = command;
			c.Parameters[key1] = val1;
			c.Parameters[key2] = val2;
			return DoCommandSync(c);
		}

		public string DoCommandSync(string command, string key1, string val1, string key2, string val2, string key3, string val3)
		{
			Command c = new Command();
			c.Parameters["command"] = command;
			c.Parameters[key1] = val1;
			c.Parameters[key2] = val2;
			c.Parameters[key3] = val3;
			return DoCommandSync(c);
		}

		public string DoCommandSync(string command, string key1, string val1, string key2, string val2, string key3, string val3, string key4, string val4)
		{
			Command c = new Command();
			c.Parameters["command"] = command;
			c.Parameters[key1] = val1;
			c.Parameters[key2] = val2;
			c.Parameters[key3] = val3;
			c.Parameters[key4] = val4;
			return DoCommandSync(c);
		}


		public void WaitAllCommands()
		{
			for (; ; )
			{
				if (PendingCommands.Count == 0)
					break;
				System.Threading.Thread.Sleep(100);
			}
		}

		private void DoCommandSyncReceive(Command c, string data)
		{
			c.AddSyncResult(data);
		}

		protected virtual void SendLine(string line)
		{
			// Must be implemented in derivated
		}

		protected void ReceiveData(string data)
		{
			m_bufferReceive += data;

			for (; ; )
			{
				int posEndLine = m_bufferReceive.IndexOf('\n');
				if (posEndLine == -1)
					break;
				else
				{
					string line = m_bufferReceive.Substring(0, posEndLine);
					m_bufferReceive = m_bufferReceive.Substring(posEndLine + 1);

					if (line.StartsWith("ee:", StringComparison.InvariantCulture) == false)
					{
						Engine.Instance.Logs.LogVerbose("Elevated unexpected log: " + line);
					}
					else
					{
						int posEndKind = line.IndexOf(':', 3);
						if (posEndKind == -1)
							return;
						string packetKind = line.Substring(3, posEndKind - 3);

						if (packetKind == "log")
						{
							string logB = line.Substring(posEndKind + 1);
							string log = Conversions.Base64ToString(logB);                            
                            Engine.Instance.Logs.LogVerbose(log); // For example, DNS flush messages
                        }
						else if (packetKind == "fatal")
						{
							string logB = line.Substring(posEndKind + 1);
							string log = Conversions.Base64ToString(logB);
							FatalError(log);
						}
                        else if (packetKind == "pid")
                        {
							// For example under MacOS, it's not possible to obtain PID with AuthorizationExecuteWithPrivileges.
							// So, it's the elevated that inform the launcher of his PID.
							string pidB = line.Substring(posEndKind + 1);
                            string pidS = Conversions.Base64ToString(pidB);
                            int pid = Conversions.ToInt32(pidS);
                            m_process = System.Diagnostics.Process.GetProcessById(pid);                            
                        }
						else
						{
							int posEndId = line.IndexOf(":", posEndKind + 1, StringComparison.InvariantCulture);
							string packetIdS = "";
							string packetData = "";
							if (posEndId == -1)
							{
								packetIdS = line.Substring(posEndKind + 1);
								packetData = "";
							}
							else
							{
								packetIdS = line.Substring(posEndKind + 1, posEndId - posEndKind - 1);
								packetData = line.Substring(posEndId + 1);
								packetData = Conversions.Base64ToString(packetData);
							}

							UInt32 packetId = Convert.ToUInt32(packetIdS);

							if (PendingCommands.ContainsKey(packetId) == false)
								return;

							Command c = PendingCommands[packetId];
							if (packetKind == "data")
								c.Data(packetData);
							else if (packetKind == "exception")
							{
								c.Exception(packetData);
								PendingCommands.Remove(packetId);
							}
							else if (packetKind == "end")
							{
								c.End();
								PendingCommands.Remove(packetId);
							}
						}
					}
				}
			}
		}
	}
}
