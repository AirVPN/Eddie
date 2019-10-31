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

namespace Eddie.Core
{
    // TOFIX: must become a generic "Process that need to run as elevated (wg in future?)".
	public class OpenVpnProcess
	{
        public string ExePath = "";
		public bool AirBuild = false;
		public string ConfigPath = "";

        public bool DeleteAfterStart = false;

		public delegate void EndHandler();
		public event EndHandler EndEvent;
        		
		private ElevatedProcess.Command m_command;
		private int m_pid;

		public StringWriterLine StdOut = new StringWriterLine();
		public StringWriterLine StdErr = new StringWriterLine();

		public bool ReallyExited
		{
			get
			{
                return m_command.IsComplete;
			}
		}

		public void KillSoft()
		{
            Engine.Instance.Elevated.DoCommandSync("kill", "signal", "sigint", "pid", m_pid.ToString());
        }

		public void Kill()
		{
            Engine.Instance.Elevated.DoCommandSync("kill", "signal", "sigterm", "pid", m_pid.ToString());
		}

		public void Start()
		{
            m_command = new ElevatedProcess.Command();

            if (Platform.Instance.NeedExecuteOutsideAppPath(ExePath))
            {
                string tempPathToDelete = UtilsCore.GetTempPath() + "/openvpn-" + RandomGenerator.GetHash();
                if (Platform.Instance.FileExists(tempPathToDelete))
                    Platform.Instance.FileDelete(tempPathToDelete);
                System.IO.File.Copy(ExePath, tempPathToDelete);

                ExePath = tempPathToDelete;

                DeleteAfterStart = true; 
            }

			m_command.Parameters["command"] = "process_openvpn";
            m_command.Parameters["path"] = ExePath;
			m_command.Parameters["airbuild"] = (AirBuild ? "y":"n");
            m_command.Parameters["gui-version"] = Constants.Name + Constants.VersionDesc;
            m_command.Parameters["config"] = ConfigPath;

            m_command.ExceptionEvent += delegate (ElevatedProcess.Command cmd, string message)
            {
                StdErr.Write("Error: " + message);
            };

            m_command.ReceiveEvent += delegate (ElevatedProcess.Command cmd, string data)
			{
				string feedbackType = data.Substring(0, 6);
                string feedbackData = data.Substring(7);

                if (feedbackType == "stdout")
                    StdOut.Write(feedbackData);
                else if (feedbackType == "stderr")
                    StdErr.Write(feedbackData);
                else if (feedbackType == "procid")
                {
                    m_pid = Conversions.ToInt32(feedbackData);
                    if(DeleteAfterStart)
                    {
                        Platform.Instance.FileDelete(ExePath);
                    }
                }
                else if (feedbackType == "return")
                {
                }					
			};

			m_command.CompleteEvent += delegate (ElevatedProcess.Command cmd)
			{
				StdOut.Stop();
				StdErr.Stop();
				if (EndEvent != null)
					EndEvent();
			};
			m_command.DoASync();			
		}
				
		private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
				StdOut.Write(e.Data + "\n");				
		}

		private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
				StdErr.Write(e.Data + "\n");
		}

		private void Process_Exited(object sender, EventArgs e)
		{
			if (EndEvent != null)
				EndEvent();
		}

		public void Stop()
		{			
		}
	}
}
