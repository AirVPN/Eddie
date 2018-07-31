// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Text;
using System.Xml;
using Eddie.Common;

namespace Eddie.Core
{	
	public class ConnectionActive
	{
		public OvpnBuilder OpenVpnProfileStartup;
		public OvpnBuilder OpenVpnProfileWithPush;

		public List<ConnectionActiveRoute> Routes = new List<ConnectionActiveRoute>();

		public bool TunnelIPv4 = true;
		public bool TunnelIPv6 = true;

		public bool BlockedIPv4 = false;
		public bool BlockedIPv6 = false;

		public IpAddress Address;
		public int SshLocalPort = 0;
		public int SshRemotePort = 0;
		public int SshPortDestination = 0;
		public int SslLocalPort = 0;
		public int SslRemotePort = 0;

		public DateTime TimeStart = DateTime.MinValue;
		public IpAddress EntryIP = new IpAddress();
		public IpAddresses ExitIPs = new IpAddresses();
		public int Port = 0;

		public string RealIp = Messages.NotAvailable;
		public string InterfaceName = "";
		public string InterfaceId = "";
		public string ControlChannel = Messages.NotAvailable;
		public Int64 TimeServer = 0;
		public Int64 TimeClient = 0;
		public Int64 BytesRead = 0;
		public Int64 BytesWrite = 0;
		public Int64 BytesLastDownloadStep = -1;
		public Int64 BytesLastUploadStep = -1;

		public List<string> PendingPushDetected = new List<string>();

		public string ManagementPassword = "";
		public TemporaryFile ManagementPasswordFile;
		public TemporaryFile ProxyAuthFile;
		public TemporaryFile PasswordAuthFile;

		public TemporaryFile OvpnFile;

		private string m_protocol = "";
		

		public string Protocol
		{
			get
			{
				if (m_protocol != "")
					return m_protocol;
				else
					return OpenVpnProfileStartup.GetOneDirectiveText("proto").ToUpperInvariant();
			}
			set
			{
				m_protocol = value;
			}
		}

		public void AddRoute(IpAddress address, string gateway, string notes)
		{
			ConnectionActiveRoute route = new ConnectionActiveRoute();
			route.Address = address;
			route.Gateway = gateway;
			route.Notes = notes;
			Routes.Add(route);
		}

		public void FillProfileWithPush()
		{
			OpenVpnProfileWithPush = OpenVpnProfileStartup.Clone();
		}

		public void InitStart()
		{
			OvpnFile = new TemporaryFile("ovpn");

			Platform.Instance.FileContentsWriteText(OvpnFile.Path, OpenVpnProfileStartup.Get(), Encoding.UTF8);
			Platform.Instance.FileEnsurePermission(OvpnFile.Path, "600");
		}

		public void CleanAfterStart()
		{
			if (OvpnFile != null)
			{
				OvpnFile.Close();
				OvpnFile = null;
			}

			if (ManagementPasswordFile != null)
			{
				ManagementPasswordFile.Close();
				ManagementPasswordFile = null;
			}

			if (ProxyAuthFile != null)
			{
				ProxyAuthFile.Close();
				ProxyAuthFile = null;
			}
		}

		public void Close()
		{
			CleanAfterStart();

            // Here because reneg keys require it, and we can't know when OpenVPN need it.
            if (PasswordAuthFile != null)
            {
                PasswordAuthFile.Close();
                PasswordAuthFile = null;
            }
		}

		public void SetAuthUserPass(string username, string password)
		{
			if (PasswordAuthFile != null)
			{
				PasswordAuthFile.Close();
				PasswordAuthFile = null;
			}

			PasswordAuthFile = new TemporaryFile("ppw");
			string fileNameAuthOvpn = PasswordAuthFile.Path;
			string fileNameData = username + "\n" + password + "\n";

            Platform.Instance.FileContentsWriteText(fileNameAuthOvpn, fileNameData, Encoding.Default); // TOFIX: Check if OpenVPN expect UTF-8
            Platform.Instance.FileEnsurePermission(fileNameAuthOvpn, "600");
            Platform.Instance.FileEnsureOwner(fileNameAuthOvpn);

			OpenVpnProfileStartup.AppendDirective("auth-user-pass", OpenVpnProfileStartup.EncodePath(fileNameAuthOvpn), "Auth");
		}
	}
}
