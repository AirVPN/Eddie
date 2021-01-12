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
using System.IO;
using System.Reflection;
using System.Net.Sockets;
using System.Text;

namespace Eddie.Core
{
	public static class TorControl
	{
		public static IpAddresses m_lastGuardIps;
		public static Int64 m_lastGuardTime;

		public static bool m_torDetection = false;
		public static string m_torProcessName;
		public static string m_torProcessPath;
		public static string m_torCookiePath;
		public static byte[] m_torCookiePassword;

		public static void Init()
		{
			if (m_torDetection)
				return;

			Elevated.Command c = new Elevated.Command();
			c.Parameters["command"] = "tor-get-info";

			c.Parameters["name"] = "tor";			
			string customPath = Engine.Instance.Options.Get("proxy.tor.path");
			if (customPath != "")
				c.Parameters["path"] = customPath;
            c.Parameters["username"] = Environment.UserName;

			string torInfo = Engine.Instance.Elevated.DoCommandSync(c);

			foreach (string line in torInfo.Split('\n'))
			{ 
				if (line.StartsWithInv("Name:"))
					m_torProcessName = line.Substring("Name:".Length);
				if (line.StartsWithInv("Path:"))
					m_torProcessPath = line.Substring("Path:".Length);
				if (line.StartsWithInv("CookiePath:"))
					m_torCookiePath = line.Substring("CookiePath:".Length);
				if (line.StartsWithInv("CookiePasswordHex:"))
					m_torCookiePassword = ExtensionsString.HexToBytes(line.Substring("CookiePasswordHex:".Length));
			} 

			m_torDetection = true; 
		} 

		public static string GetTorExecutablePath()
		{
			Init();

			return m_torProcessPath;
		}

		/*
		public static string GetTorExecutablePath()
		{
			string customPath = Engine.Instance.Options.Get("proxy.tor.path");
			if (customPath != "")
			{
				if (Platform.Instance.FileExists(customPath))
					return customPath;
			}

			System.Diagnostics.Process[] processes = Process.GetProcessesByName("tor");
			if (processes.Length > 0)
			{
				return processes[0].MainModule.FileName;
			}
			return "";
		}
		
		public static string GetControlAuthCookiePath()
		{			
			string executablePath = GetTorExecutablePath();

			if (executablePath != "")
			{
				// Tor Browser Bundle, Unix and Windows at 10/16/2014				
				string path1 = Platform.Instance.NormalizePath(new FileInfo(executablePath).Directory.Parent.FullName + "/Data/Tor/control_auth_cookie");
				if (Platform.Instance.FileExists(path1))
					return path1;

				string path2 = Platform.Instance.NormalizePath(Environment.GetEnvironmentVariable("APPDATA") + "/tor/control_auth_cookie");
				if (Platform.Instance.FileExists(path2))
					return path2;
			}

			{
				// Unix
				string path = "/var/run/tor/control.authcookie";
				if (Platform.Instance.FileExists(path))
					return path;
			}

			{
				// Unix
				string path = "/var/lib/tor/control_auth_cookie";
				if (Platform.Instance.FileExists(path))
					return path;
			} 

			{
				// macOS, TorBrowser 4.0 and above
				string path = "/Applications/TorBrowser.app/TorBrowser/Data/Tor/control_auth_cookie";
				if (Platform.Instance.FileExists(path))
					return path;
			}

			{
				// macOS, TorBrowser 6.0
				string path = "/Users/" + Environment.UserName + "/Library/Application Support/TorBrowser-Data/Tor/control_auth_cookie";
				if (Platform.Instance.FileExists(path))
					return path;
			}

			// Not found
			return "";			
		}
		*/

		public static void Connect(TcpClient client)
		{
			string controlHost = Engine.Instance.Options.Get("proxy.host").ToLowerInvariant().Trim();
			int controlPort = Engine.Instance.Options.GetInt("proxy.tor.control.port");
			string controlPassword = Engine.Instance.Options.Get("proxy.tor.control.password");

			Connect(client, controlHost, controlPort, controlPassword);
		}

		public static void Connect(TcpClient client, string host, int controlPort, string controlPassword)
		{
			if (client == null)
				throw new Exception("Internal error (client is null)");

			bool controlAuthenticate = Engine.Instance.Options.GetBool("proxy.tor.control.auth");

			byte[] password = System.Text.Encoding.ASCII.GetBytes(controlPassword);

			if (controlAuthenticate) 
			{
				if (controlPassword == "")
				{
					Init();

					if(m_torCookiePath == "")					
						throw new Exception(LanguageManager.GetText("TorControlNoPath"));

					Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("TorControlAuth", "Cookie, from " + m_torCookiePath));

					password = m_torCookiePassword; 
				}
				else
				{
					Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("TorControlAuth", "Password"));
				}
			}

			client.Connect(host, controlPort);

			if (controlAuthenticate)
			{
				Write(client, "AUTHENTICATE ");
				Write(client, ExtensionsString.BytesToHex(password));
				Write(client, "\n");

				string result = Read(client);

				if (result != "250 OK")
					throw new Exception(result);

			}

			Flush(client);
		}

		public static string Test(string host, int controlPort, string controlPassword)
		{
			string result = "";
			try
			{
				using (TcpClient s = new TcpClient())
				{
					Connect(s, host, controlPort, controlPassword);

					Write(s, "getinfo version\n");

					result = Read(s);

					if ((result.IndexOfInv("250 OK") != -1) && (result.IndexOfInv("version=") != -1))
					{
						result = result.Replace("250-", "").Trim();
						result = result.Replace("250 OK", "");
						result = result.Replace("version=", "");
						result = LanguageManager.GetText("TorControlTest", result.Trim());
					}
				}
			}
			catch (Exception e)
			{
				result = LanguageManager.GetText("TorControlException", e.Message);
			}

			Engine.Instance.Logs.Log(LogType.Verbose, "Tor Test: " + result);
			return result;
		}

		public static void SendNEWNYM()
		{
			using (TcpClient s = new TcpClient())
			{
				Connect(s);

				Write(s, "SIGNAL NEWNYM\n");
				Flush(s);

				Engine.Instance.Logs.LogVerbose(LanguageManager.GetText("TorControlNEWNYM"));
			}
		}

		public static string SendCommand(string cmd)
		{
			string result = "";
			try
			{
				using (TcpClient s = new TcpClient())
				{
					Connect(s);

					Write(s, cmd + "\n");

					result = Read(s);

					return result;
				}
			}
			catch (Exception e)
			{
				result = LanguageManager.GetText("TorControlException", e.Message);
			}

			Engine.Instance.Logs.Log(LogType.Verbose, "Tor Test: " + result);
			return result;
		}

		public static IpAddresses GetGuardIps(bool force)
		{
			// This is called a lots of time.	
			Int64 now = Utils.UnixTimeStamp();
			if ((force == false) && ((now - m_lastGuardTime < 60)))
				return m_lastGuardIps;

			IpAddresses ips = new IpAddresses();

			try
			{
				string controlHost = Engine.Instance.Options.Get("proxy.host").ToLowerInvariant().Trim();

				if ((controlHost != "127.0.0.1") && (controlHost.ToLowerInvariant() != "localhost"))
				{
					// Guard IPS are used to avoid routing loop, that occur only if the Tor host is the same machine when OpenVPN run.
					return ips;
				}

				List<string> ipsMessages = new List<string>();

				using (TcpClient s = new TcpClient())
				{
					Connect(s);

					Write(s, "getinfo circuit-status\n");
					Flush(s);
					string circuits = Read(s);

					string[] circuitsLines = circuits.Split('\n');
					foreach (string circuit in circuitsLines)
					{
						string id = circuit.ToLowerInvariant().RegExMatchOne("\\d+\\sbuilt\\s\\$([0-9a-f]+)");

						if (id != "")
						{
							Write(s, "getinfo ns/id/" + id.ToUpperInvariant() + "\n");
							string nodeInfo = Read(s);

							string[] nodeLines = nodeInfo.Split('\n');
							foreach (string line in nodeLines)
							{
								string ip = line.RegExMatchOne("r\\s.+?\\s.+?\\s.+?\\s.+?\\s.+?\\s(.+?)\\s");

								if ((IpAddress.IsIP(ip)) && (!ips.Contains(ip)))
								{
									ips.Add(ip);
									ipsMessages.Add(ip + " (circuit)");
								}
							}
						}
					}

					Write(s, "getconf bridge\n");
					Flush(s);
					string bridges = Read(s);

					if (bridges.IndexOf("meek") == -1) //Panic if we have meek enabled, don't yet know what to do :-(
					{
						string[] bridgeLines = bridges.Split('\n');
						foreach (string bridge in bridgeLines)
						{
							List<string> matches = bridge.ToLowerInvariant().RegExMatchSingle("250.bridge=(.+?)\\s([0-9a-f\\.\\:]+?):\\d+\\s");
							if ((matches != null) && (matches.Count == 2))
							{
								string bridgeType = matches[0];
								string ip = matches[1];

								if ((IpAddress.IsIP(ip)) && (!ips.Contains(ip)))
								{
									ips.Add(matches[1]);
									ipsMessages.Add(matches[1] + " (" + bridgeType + ")");
								}
							}
						}
					}
					else
					{
						Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("TorControlMeekUnsupported"));
					}

					if (ips.Count == 0)
					{
						Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("TorControlNoIps"));
						//throw new Exception(Messages.TorControlNoIps);
					}
					else
					{
						string list = String.Join("; ", ipsMessages.ToArray());
						Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("TorControlGuardIps", list));
					}
				}
			}
			catch (Exception e)
			{
				//throw new Exception(LanguageManager.GetText("TorControlException, e.Message));
				Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("TorControlException", e.Message));
			}

			m_lastGuardIps = ips;
			m_lastGuardTime = now;

			return ips;
		}

		public static string Read(TcpClient s)
		{
			int bufSize = s.ReceiveBufferSize;
			if (bufSize < 1024)
				bufSize = 1024;
			byte[] inStream = new byte[bufSize + 1];
			s.GetStream().Read(inStream, 0, bufSize);
			string result = System.Text.Encoding.ASCII.GetString(inStream);
			result = result.Trim('\0').Trim(); // 2.10.1
			return result;
		}

		public static void Write(TcpClient s, string v)
		{
			byte[] v2 = System.Text.Encoding.ASCII.GetBytes(v);
			s.GetStream().Write(v2, 0, v2.Length);
		}

		public static void Write(TcpClient s, byte[] v)
		{
			s.GetStream().Write(v, 0, v.Length);
		}

		public static void Flush(TcpClient s)
		{
			s.GetStream().Flush();
		}
	}
}
