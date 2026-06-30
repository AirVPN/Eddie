// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2026 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.IO.Pipes;
using System.Text;
using Eddie.Core;

namespace Eddie.Platform.Windows
{
	// Named-pipe transport implementation of IElevated (Windows).
	// The protocol (handshake, command framing) lives in IElevated; this class only
	// provides the transport: SendLine writes to the pipe stream, a reader thread feeds ReceiveData.
	public class IPipe : Eddie.Core.Elevated.IElevated
	{
		private NamedPipeClientStream m_pipe = null;
		private System.Threading.Thread m_readThread = null;
		private readonly object m_writeLock = new object();

		protected string Connect(int port, string mode)
		{
			return Connect(port, mode, 1000);
		}

		protected string Connect(int port, string mode, int connectTimeoutMs)
		{
			try
			{
				// Named pipe keyed by launch mode ("spot"/"service"); the port is only for the TCP fallback.
				string pipeName = "eddie-elevated-" + mode;

				m_bufferReceive = "";
				m_failedReason = "";

				NamedPipeClientStream pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
				pipe.Connect(connectTimeoutMs); // throws if the server pipe is not available in time

				m_pipe = pipe;

				m_readThread = new System.Threading.Thread(ReadLoop);
				m_readThread.IsBackground = true;
				m_readThread.Start();

				m_sessionKey = RandomGenerator.GetHash();

				DoCommandSync("session-key", "key", m_sessionKey, "version", Constants.ElevatedVersionExpected, "path", Platform.Instance.GetApplicationPath());

				if (m_failedReason != "")
					return m_failedReason;

				m_started = true;
			}
			catch (TimeoutException)
			{
				// Server pipe not available within the timeout: no listening service.
				return "No listening";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

			return "Ok";
		}

		private void ReadLoop()
		{
			byte[] buffer = new byte[4096];
			// Capture the stream: Stop() disposes and nulls m_pipe, so reading the field directly
			// would race into a NullReferenceException. A disposed read throws ObjectDisposedException, handled below.
			NamedPipeClientStream pipe = m_pipe;
			try
			{
				for (; ; )
				{
					int bytesRead = pipe.Read(buffer, 0, buffer.Length);
					if (bytesRead <= 0)
						throw new Exception("Elevated communication closed");

					ReceiveData(Encoding.ASCII.GetString(buffer, 0, bytesRead));
				}
			}
			catch (Exception ex)
			{
				if (ShutdownInProgress)
					return;

				FatalError(ex.Message);
			}
		}

		protected override void SendLine(string line)
		{
			try
			{
				byte[] bytes = Encoding.ASCII.GetBytes(line);
				lock (m_writeLock)
				{
					m_pipe.Write(bytes, 0, bytes.Length);
					m_pipe.Flush();
				}
			}
			catch (Exception ex)
			{
				FatalError(ex.Message);
			}
		}

		public override void Stop()
		{
			base.Stop();

			try
			{
				if (m_pipe != null)
				{
					m_pipe.Dispose();
					m_pipe = null;
				}
			}
			catch
			{
			}
		}
	}
}
