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
				if (Loop) // Service edition
					port = Constants.PortService;

				IPAddress ipAddress = IPAddress.Loopback;
				m_localEndPoint = new IPEndPoint(ipAddress, port);

				DebugLog("Listen on localhost, port " + port.ToString() + ", " + (Loop ? "Loop mode" : "Spot mode"));

				Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				listener.Bind(m_localEndPoint);
				listener.Listen(100);

				while (true)
				{
					m_socket = null;
					m_sb.Clear();
					m_closePending = false;

					DebugLog("Waiting connection");
					
					ClientDisconnectSignal.Reset();

					listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

					ClientDisconnectSignal.WaitOne();

					if (Loop == false)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Close(ex.Message);
			}

			DebugLog("Clean end");
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
						FatalLog(reason);
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
					throw new Exception("A connection is already running");
									
				m_socket = socket;

				DebugLog("Accepted connection");

				Process clientProcess = Utils.GetProcessOfMatchingIPEndPoint(m_socket.RemoteEndPoint as IPEndPoint, m_localEndPoint);
				if (clientProcess == null)
					throw new Exception("Unable to detect client process");

#if DEBUG
				// Never release a signed debug.
#else
			
				bool match = false;
				try
				{
					System.Security.Cryptography.X509Certificates.X509Certificate c1 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(System.Reflection.Assembly.GetEntryAssembly().Location);
					System.Security.Cryptography.X509Certificates.X509Certificate c2 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(clientProcess.MainModule.FileName);

					match = (
						(c1.Issuer == c2.Issuer) &&
						(c1.Subject == c2.Subject) &&
						(c1.GetCertHashString() == c2.GetCertHashString()) &&
						(c1.GetEffectiveDateString() == c2.GetEffectiveDateString()) &&
						(c1.GetPublicKeyString() == c2.GetPublicKeyString()) &&
						(c1.GetRawCertDataString() == c2.GetRawCertDataString()) &&
						(c1.GetSerialNumberString() == c2.GetSerialNumberString())
					);
				}
				catch
				{

				}

				if (Constants.Debug)
					match = true;
				
				if (match == false)
				{
					throw new Exception("Client not allowed, signature mismatch.");
				}
#endif

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
