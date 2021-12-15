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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Eddie.Core.Elevated
{
	public class ISocket : IElevated
	{
		private ManualResetEvent connectDone = new ManualResetEvent(false);
		private ManualResetEvent sendDone = new ManualResetEvent(false);
		private ManualResetEvent receiveDone = new ManualResetEvent(false);
		private Socket m_socket = null;

		protected int GetPortSpot()
		{
			return RandomGenerator.GetInt(2048 + 128, 256 * 256 - 128);
		}

		protected override void SendLine(string line)
		{
			Send(m_socket, line);
		}

		protected string Connect(int port)
		{
			try
			{
				if (Platform.Instance.IsPortLocalListening(port) == false)
					return "No listening";

				m_bufferReceive = "";
				m_failedReason = "";

				IPAddress ipAddress = IPAddress.Loopback;
				IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

				Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				connectDone.Reset();
				socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), socket);
				connectDone.WaitOne();

				if (m_socket == null) // Connect failed
				{
					return "No socket";
				}

				StateObject state = new StateObject();
				state.Socket = socket;
				// Begin receiving the data from the remote device.  
				socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

				m_sessionKey = RandomGenerator.GetHash();

				DoCommandSync("session-key", "key", m_sessionKey, "version", Constants.ElevatedVersionExpected);

				if (m_failedReason != "")
					return m_failedReason;

				m_started = true;
			}
			catch (Exception ex)
			{
				// Debug hardening issue on Mojave
				// return ex.Message + "\n" + ex.StackTrace + "\n" + ex.ToString();
				return ex.Message;
			}

			return "Ok";
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				Socket client = (Socket)ar.AsyncState;

				client.EndConnect(ar);

				m_socket = client;
			}
			catch
			{
				m_socket = null;
			}

			connectDone.Set();
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				StateObject state = (StateObject)ar.AsyncState;
				Socket client = state.Socket;

				int bytesRead = client.EndReceive(ar);

				if (bytesRead > 0)
				{
					string data = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);

					ReceiveData(data);
				}
				else
				{
					throw new Exception("Elevated communication closed");
				}

				client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
			}
			catch (Exception ex)
			{
				if (ShutdownInProgress) // Expected: An existing connection was forcibly closed by the remote host
					return;

				FatalError(ex.Message);
			}
		}

		private void Send(Socket client, String data)
		{
			try
			{
				byte[] byteData = Encoding.ASCII.GetBytes(data);
				client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
			}
			catch (Exception ex)
			{
				FatalError(ex.Message);
			}
		}

		private void SendCallback(IAsyncResult ar)
		{
			try
			{
				Socket client = (Socket)ar.AsyncState;
				int bytesSent = client.EndSend(ar);
				sendDone.Set();
			}
			catch (Exception ex)
			{
				FatalError(ex.Message);
			}
		}

		public class StateObject
		{
			public Socket Socket = null;
			public const int BufferSize = 4096; // TOFIX, with 256 don't work as expected (test with Windows UI > Service that refuse)
			public byte[] Buffer = new byte[BufferSize];
		}
	}
}

