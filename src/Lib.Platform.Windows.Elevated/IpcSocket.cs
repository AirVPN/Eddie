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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.Platform.Windows.Elevated
{
	public class IpcSocket : IpcBase
	{
		private IPEndPoint m_localEndPoint = null;
		//private IpcSocketClient m_client = null;

		private Socket m_socket = null;
		private const int m_bufferSize = 1024;
		private byte[] m_buffer = new byte[m_bufferSize];
		private StringBuilder m_sb = new StringBuilder();
		private bool m_closePending = false;

		public override void MainLoop()
		{
			base.MainLoop();

			try
			{
				int port = Constants.PortSpot;
				if (m_serviceMode)
					port = Constants.PortService;

				if (m_cmdline.ContainsKey("port"))
					port = Convert.ToInt32(m_cmdline["port"]);

				IPAddress ipAddress = IPAddress.Loopback;
				m_localEndPoint = new IPEndPoint(ipAddress, port);

				LogDebug("Listen on localhost, port " + port.ToString() + ", " + (m_serviceMode ? "Service mode" : "Spot mode"));

				Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				listener.Bind(m_localEndPoint);
				listener.Listen(100);

				while (true)
				{
					m_socket = null;
					m_sb.Clear();
					m_closePending = false;

					LogDebug("Waiting connection");
					
					ClientDisconnectSignal.Reset();

					listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

					ClientDisconnectSignal.WaitOne();

					if (m_serviceMode == false)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Close(ex.Message);
			}

			LogDebug("Clean end");
		}

		public override void SendMessage(string line)
		{
			try
			{
				byte[] byteData = Encoding.ASCII.GetBytes(line + "\n");

				m_socket.Send(byteData);
			}
			catch(Exception ex)
			{
				Close(ex.Message);
			}
		}

		public override void Close(string reason)
		{
			base.Close(reason);

			if (m_closePending)
				return;

			m_closePending = true; // Avoid a loop with FatalLog(reason)

			try
			{
				if (m_socket != null)
				{
					if(reason != "Disconnect")
						LogFatal(reason);
					m_socket.Shutdown(SocketShutdown.Both);
					m_socket.Close();
				}
			}
			catch
			{

			}

			ClientDisconnectSignal.Set();						
		}

		public void AcceptCallback(IAsyncResult ar)
		{
			try
			{
				Socket listener = (Socket)ar.AsyncState;
				Socket socket = listener.EndAccept(ar);

				if (m_socket != null)
					throw new Exception("Client not allowed: connection already active");
									
				m_socket = socket;

				LogDebug("Accepted connection");

				// Check allowed
				{
					Process clientProcess = Utils.GetProcessOfMatchingIPEndPoint(m_socket.RemoteEndPoint as IPEndPoint, m_localEndPoint);

					string clientPath = clientProcess.MainModule.FileName;

					if (clientProcess == null)
						throw new Exception("Client not allowed: Cannot detect client process");

					// If spot mode, must be the parent
					if(m_serviceMode == false)
					{
						int parentPID = Utils.GetParentProcess().Id;						
						if (clientProcess.Id != parentPID)
							throw new Exception("Client not allowed: Connection not from parent process (spot mode)");
					}

					// If service mode, hash must match
					if(m_serviceMode)
					{
						string allowedHash = "";
						if (m_cmdline.ContainsKey("allowed_hash"))
							allowedHash = m_cmdline["allowed_hash"];
						string clientHash = Utils.HashSHA512File(clientPath);
						if(allowedHash != clientHash)
							throw new Exception("Client not allowed: Hash mismatch (client " + clientHash + " != expected " + allowedHash + ") (service mode)");
					}

					// Check signature (optional)
					{
						try
						{
							System.Security.Cryptography.X509Certificates.X509Certificate c1 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(System.Reflection.Assembly.GetEntryAssembly().Location);

							// If above don't throw exception, Elevated it's signed, so it's mandatory that client is signed from same subject.
							try
							{
								System.Security.Cryptography.X509Certificates.X509Certificate c2 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(clientPath);

								bool match = (
									(c1.Issuer == c2.Issuer) &&
									(c1.Subject == c2.Subject) &&
									(c1.GetCertHashString() == c2.GetCertHashString()) &&
									(c1.GetEffectiveDateString() == c2.GetEffectiveDateString()) &&
									(c1.GetPublicKeyString() == c2.GetPublicKeyString()) &&
									(c1.GetRawCertDataString() == c2.GetRawCertDataString()) &&
									(c1.GetSerialNumberString() == c2.GetSerialNumberString())
								);

								if (match == false)
									throw new Exception("Client not allowed: digital signature mismatch");
							}
							catch (Exception)
							{
								throw new Exception("Client not allowed: digital signature not available");
							}
						}
						catch(Exception)
						{
							// Not signed, but maybe compiled from source, it's an optional check.
						}
					}
				}

				ReplyPID(Process.GetCurrentProcess().Id);

				m_socket.BeginReceive(m_buffer, 0, m_bufferSize, 0, new AsyncCallback(ReadCallback), m_socket);
			}
			catch(Exception ex)
			{
				Close(ex.Message);
			}			
		}

		public void ReadCallback(IAsyncResult ar)
		{
			try
			{
				//IpcSocketClient state = (IpcSocketClient)ar.AsyncState;
				//Socket handler = state.Socket;

				int bytesRead = m_socket.EndReceive(ar);

				if (bytesRead > 0)
				{
					m_sb.Append(Encoding.ASCII.GetString(m_buffer, 0, bytesRead));

					string content = m_sb.ToString();
					int posEndLine = content.IndexOf("\n");
					if (posEndLine != -1)
					{
						m_sb.Clear();
						string line = content.Substring(0, posEndLine);
						if (content.Length - (posEndLine + 1) > 0)
						{
							m_sb.Append(content.Substring(posEndLine + 1));
						}

						Dictionary<string, string> parameters = new Dictionary<string, string>();
						int pos = 0;
						for (; ; )
						{
							int posKeyEnd = line.IndexOf(':', pos);
							if (posKeyEnd == -1)
								break;
							string key = line.Substring(pos, posKeyEnd - pos);
							int posValueEnd = line.IndexOf(';', posKeyEnd + 1);
							if (posValueEnd == -1)
								break;
							string v = line.Substring(posKeyEnd + 1, posValueEnd - posKeyEnd - 1);
							v = Conversions.Base64ToString(v);
							parameters[key] = v;
							pos = posValueEnd + 1;
						}

						if (parameters.ContainsKey("_id") == false)
							return;
						if (parameters.ContainsKey("command") == false)
							return;

						DoCommand(parameters);
					}
				}
				else
					throw new Exception("Read fail");

				m_socket.BeginReceive(m_buffer, 0, IpcSocketClient.BufferSize, 0, new AsyncCallback(ReadCallback), m_socket);
			}
			catch(Exception ex)
			{
				Close(ex.Message);
			}
		}
	}

	public class IpcSocketClient
	{
		public Socket Socket = null;
		public const int BufferSize = 1024;
		public byte[] Buffer = new byte[BufferSize];
		public StringBuilder sb = new StringBuilder();
	}
}
