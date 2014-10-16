// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Net.Sockets;
using System.Text;

namespace AirVPN.Core
{
    public static class TorControl
    {
		public static string GetControlAuthCookiePath()
		{
			System.Diagnostics.Process[] processes = Process.GetProcessesByName("tor");
			if (processes.Length > 0)
			{
				string path = Platform.Instance.NormalizePath(new FileInfo(processes[0].MainModule.FileName).Directory.Parent.FullName + "/Data/Tor/control_auth_cookie");
				if (File.Exists(path))
					return path;
			}
			return "";
		}
		public static List<string> GetGuardIps()
		{
			try
			{
				string controlHost = Engine.Instance.Storage.Get("mode.tor.host").ToLowerInvariant().Trim();

				if ((controlHost != "127.0.0.1") && (controlHost.ToLowerInvariant() != "localhost"))
				{
					// Guard IPS are used to avoid routing loop, that occur only if the TOR host is the same machine when OpenVPN run.
					return new List<string>();
				}

				int controlPort = Engine.Instance.Storage.GetInt("mode.tor.control.port");
				string controlPassword = Engine.Instance.Storage.Get("mode.tor.control.password");
				bool controlAuthenticate = Engine.Instance.Storage.GetBool("mode.tor.control.auth");

				byte[] password = System.Text.Encoding.ASCII.GetBytes(controlPassword);

				if ((controlAuthenticate) && (controlPassword == ""))
				{
					string path = GetControlAuthCookiePath();

					if (path == "")
						throw new Exception(Messages.TorControlNoPath);

					Engine.Instance.Log(Engine.LogType.Verbose, Messages.Format(Messages.TorControlAuth, "Cookie, from " + path));							

					password = File.ReadAllBytes(path);
				}

				List<string> ips = new List<string>();

				TcpClient s = new TcpClient();
				s.Connect(controlHost, controlPort);

				if (controlAuthenticate)
				{					
					Write(s, "AUTHENTICATE \"");
					Write(s, password);
					Write(s, "\"\n");
				}
				Write(s, "getinfo circuit-status\n");
				Flush(s);
				string circuits = Read(s);
				
				string[] circuitsLines = circuits.Split('\n');
				foreach (string circuit in circuitsLines)
				{
					string[] circuitItems = circuit.Split(' ');
					if (circuitItems.Length < 3)
						continue;
					if (circuitItems[1] != "BUILT")
						continue;
					string id = circuitItems[2];
					id = id.Substring(1, id.IndexOf('~') - 1);
					
					Write(s, "getinfo ns/id/" + id + "\n");
					string nodeInfo = Read(s);

					string[] nodeLines = nodeInfo.Split('\n');
					foreach (string line in nodeLines)
					{
						string[] lineItems = line.Split(' ');
						if (lineItems.Length < 7)
							continue;
						if (lineItems[0] != "r")
							continue;
						string ip = lineItems[6];

						if (ips.Contains(ip) == false)
						{
							Engine.Instance.Log(Engine.LogType.Verbose, Messages.Format(Messages.TorControlGuardIp, ip, id));							
							ips.Add(ip);
						}
					}
				}

				s.Close();

				if (ips.Count == 0)
					throw new Exception(Messages.TorControlNoIps);

				return ips;
			}
			catch (Exception e)
			{
				throw new Exception(Messages.Format(Messages.TorControlException, e.Message));
			}
		}

		public static string Read(TcpClient s)
		{
			int bufSize = s.ReceiveBufferSize;
			if (bufSize < 1024)
				bufSize = 1024;
			byte[] inStream = new byte[bufSize + 1];
			s.GetStream().Read(inStream, 0, bufSize);
			string result = System.Text.Encoding.ASCII.GetString(inStream);
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
