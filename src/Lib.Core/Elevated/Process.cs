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

namespace Eddie.Core.Elevated
{
    public class Process
	{
		public Elevated.Command Command = new Elevated.Command();

        public bool DeleteAfterStart = false;

		public delegate void EndHandler();
		public event EndHandler EndEvent;
        		
		private int m_pid;

		public StringWriterLine StdOut = new StringWriterLine();
		public StringWriterLine StdErr = new StringWriterLine();

		public bool ReallyExited
		{
			get
			{
                return Command.IsComplete;
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
            if (Platform.Instance.NeedExecuteOutsideAppPath(Command.Parameters["path"]))
			{
				// TOCLEAN string tempPathToDelete = Utils.GetTempPath() + "/eddie-external-process-" + RandomGenerator.GetHash();
				string tempPathToDelete = Platform.Instance.FileTempName(Platform.Instance.FileGetNameFromPath(Command.Parameters["path"]));

				if (Platform.Instance.FileExists(tempPathToDelete))
					Platform.Instance.FileDelete(tempPathToDelete);
				System.IO.File.Copy(Command.Parameters["path"], tempPathToDelete);

				Command.Parameters["path"] = tempPathToDelete;

				DeleteAfterStart = true;
			}

			Command.ExceptionEvent += delegate (Elevated.Command cmd, string message)
            {
                StdErr.Write("Error: " + message);
            };

			Command.ReceiveEvent += delegate (Elevated.Command cmd, string data)
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
                        Platform.Instance.FileDelete(Command.Parameters["path"]);
                    }
                }
                else if (feedbackType == "return")
                {
                }					
			};

			Command.CompleteEvent += delegate (Elevated.Command cmd)
			{
				StdOut.Stop();
				StdErr.Stop();
				if (EndEvent != null)
					EndEvent();
			};

			Command.DoASync();			
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
