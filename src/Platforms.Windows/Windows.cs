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
using System.Net;
using System.Net.NetworkInformation;
using System.Management;
using System.Xml;
using System.Text;
using AirVPN.Core;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace AirVPN.Platforms
{
    public class Windows : Platform
    {
		private string m_architecture;
		private List<NetworkManagerDhcpEntry> ListOldDhcp = new List<NetworkManagerDhcpEntry>();
		private List<NetworkManagerDnsEntry> ListOldDns = new List<NetworkManagerDnsEntry>();
		
        // Override
		public Windows()
		{
			m_architecture = NormalizeArchitecture(System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"));
		}

		public override string GetCode()
		{
			return "Windows";
		}

		public override string GetArchitecture()
		{
			return m_architecture;
		}

        public override bool IsAdmin()
        {
            return true; // Manifest ensure that
        }

        public override bool IsTraySupported()
        {
            return true;
        }

		public override bool GetAutoStart()
		{
			try
			{
				TaskService ts = new TaskService();
				if (ts.RootFolder.Tasks.Exists("AirVPN"))
					return true;				
			}
			catch (NotV1SupportedException)
			{
				//Ignore, not supported on XP
			}

			return false;
		}
		public override bool SetAutoStart(bool value)
		{
			try
			{
				TaskService ts = new TaskService();
				if (value == false)
				{
					if(ts.RootFolder.Tasks.Exists("AirVPN"))
						ts.RootFolder.DeleteTask("AirVPN");
				}
				else
				{
					if(ts.RootFolder.Tasks.Exists("AirVPN"))
						ts.RootFolder.DeleteTask("AirVPN");

					string path = Platform.Instance.GetExecutablePath();

					// Create a new task definition and assign properties
					TaskDefinition td = ts.NewTask();
					td.Principal.RunLevel = TaskRunLevel.Highest;					
					//td.Settings.RunOnlyIfLoggedOn = true;
					td.Settings.DisallowStartIfOnBatteries = false;
					td.Settings.StopIfGoingOnBatteries = false;
					td.Settings.RunOnlyIfNetworkAvailable = false;
					td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
					td.Settings.DeleteExpiredTaskAfter = TimeSpan.Zero;
					td.Settings.ExecutionTimeLimit = TimeSpan.Zero;

					td.RegistrationInfo.Description = "AirVPN Client";					
					td.Triggers.Add(new LogonTrigger());				
					td.Actions.Add(new ExecAction(GetExecutablePath(), "", null));

					// Register the task in the root folder
					ts.RootFolder.RegisterTaskDefinition(@"AirVPN", td);
				}
			}
			catch (NotV1SupportedException)
            {
                //Ignore, not supported on XP
            }

			return true;
		}

		public override string NormalizeString(string val)
        {
            return val.Replace("\n", "\r\n");
        }

        public override string DirSep
        {
            get
            {
                return "\\";
            }
        }

        public override string GetUserFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AirVPN";
        }

        public override string ShellCmd(string Command)
        {
            return Shell("cmd.exe", String.Format("/c {0}", Command));
        }

        public override void FlushDNS()
        {
            ShellCmd("ipconfig /flushdns");
        }

		public override void RouteAdd(RouteEntry r)
		{
			string cmd = "";
			cmd += "route add " + r.Address.Value + " mask " + r.Mask.Value + " " + r.Gateway.Value;
			if(r.Metrics != "")
				cmd += " metric " + r.Metrics;
			if (r.Interface != "")
				cmd += " if " + r.Interface;
			ShellCmd(cmd);
		}

		public override void RouteRemove(RouteEntry r)
		{
			string cmd = "route delete " + r.Address.Value + " mask " + r.Mask.Value + " " + r.Gateway.Value;
			ShellCmd(cmd);
		}

		public override bool WaitTunReady()
		{
			int tickStart = Environment.TickCount;
			string lastStatus = "";

			for (; ; )
			{
				NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface adapter in interfaces)
				{
					if (adapter.Description.StartsWith("TAP-Win"))
					{
						if (adapter.OperationalStatus == OperationalStatus.Up)
							return true;
						else
						{
							lastStatus = adapter.OperationalStatus.ToString();							
						}
					}
				}

				if (Environment.TickCount - tickStart > 10000)
				{
					Engine.Instance.Log(Engine.LogType.Warning, "Tunnel not ready in 10 seconds, contact our support. Last interface status: " + lastStatus);
					return false;
				}

				Engine.Instance.Log(Engine.LogType.Warning, "Waiting TUN interface");

				System.Threading.Thread.Sleep(2000);
			}
		}

		public override List<RouteEntry> RouteList()
		{			
			List<RouteEntry> entryList = new List<RouteEntry>();

			// 'route print' show IP in Interface fields, but 'route add' need the interface ID. We use a map.
			Dictionary<string, string> InterfacesIp2Id = new Dictionary<string, string>();

			ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

			foreach (ManagementObject objMO in objMOC)
			{				
				if (!((bool)objMO["IPEnabled"]))
					continue;

				string[] ips = Conversions.ToString(objMO["IPAddress"]).Trim().Split(',');
				string id = Conversions.ToString(objMO["InterfaceIndex"]).Trim();

				foreach (string ip in ips)
				{
					InterfacesIp2Id[ip.Trim()] = id;
				}
			}

			string cmd = "route PRINT";
			string result = ShellCmd(cmd);

			string[] lines = result.Split('\n');
			foreach (string line in lines)
			{
				string[] fields = Utils.StringCleanSpace(line).Split(' ');

				if(fields.Length == 5)
				{
					// Route line.
					RouteEntry e = new RouteEntry();
					e.Address = fields[0];
					e.Mask = fields[1];
					e.Gateway = fields[2];
					e.Interface = fields[3];
					e.Metrics = fields[4];
			 
					if(e.Address.Valid == false)
						continue;
					if(e.Mask.Valid == false)
						continue;
					if(e.Gateway.Valid == false)
						continue;
					
					if (InterfacesIp2Id.ContainsKey(e.Interface))
					{
						e.Interface = InterfacesIp2Id[e.Interface];
						if (e.Gateway.Value != "On-link")
							entryList.Add(e);
					}
					else
					{
						Engine.Instance.LogDebug("Unexpected.");
					}
				}
			}
			
			return entryList;
		}
		
		public override string GenerateSystemReport()
		{
			string t = base.GenerateSystemReport();
			
			t += "\n\n-- Windows-Only informations\n";

			ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

			foreach (ManagementObject objMO in objMOC)
			{				
				 if (!((bool)objMO["IPEnabled"]))
					continue;

				t += "\n";
				t += "Network Adapter: " + Conversions.ToString(objMO.Properties["Caption"].Value) + "\n";
				t += "DNS: " + Conversions.ToString(objMO.Properties["DNSServerSearchOrder"].Value) + "\n";
				
				t += "Details:\n";
				foreach (PropertyData prop in objMO.Properties)
				{
					t += "\t" + prop.Name + ": " + Conversions.ToString(prop.Value) + "\n";					
				}				
			}			

			return t;
		}

		public override void OnNetworkLockManagerInit()
		{
			base.OnNetworkLockManagerInit();

			Engine.Instance.NetworkLockManager.AddPlugin(new NetworkLockWindowsFirewall());
		}

		public override void OnSessionStart()
		{
			// https://airvpn.org/topic/11162-airvpn-client-advanced-features/ -> Switch DHCP to Static			
			if (Engine.Instance.Storage.GetBool("advanced.windows.dhcp_disable"))
				SwitchToStaticDo();

			// https://airvpn.org/topic/11162-airvpn-client-advanced-features/ -> Force DNS	
			if (Engine.Instance.Storage.GetBool("advanced.windows.dns_force"))
				DnsForceDo();

			FlushDNS();

			Recovery.Save();
		}

		public override void OnSessionStop()
		{
			DnsForceRestore();
			SwitchToStaticRestore();

			FlushDNS();

			Recovery.Save();
		}

		public override void OnDaemonOutput(string source, string message)
		{
			if (message.IndexOf("Waiting for TUN/TAP interface to come up") != -1)
			{
				if (Engine.Instance.Storage.GetBool("advanced.windows.tap_up"))
				{
					HackWindowsInterfaceUp();
				}
			}
		}

		public override void OnRecoveryLoad(XmlElement root)
		{
			XmlElement nodeDhcp = Utils.XmlGetFirstElementByTagName(root, "DhcpSwitch");
			if (nodeDhcp != null)
			{
				foreach (XmlElement nodeEntry in nodeDhcp.ChildNodes)
				{
					NetworkManagerDhcpEntry entry = new NetworkManagerDhcpEntry();
					entry.ReadXML(nodeEntry);
					ListOldDhcp.Add(entry);
				}
			}

			XmlElement nodeDns = Utils.XmlGetFirstElementByTagName(root, "DnsSwitch");
			if (nodeDns != null)
			{
				foreach (XmlElement nodeEntry in nodeDns.ChildNodes)
				{
					NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();
					entry.ReadXML(nodeEntry);
					ListOldDns.Add(entry);
				}
			}

			SwitchToStaticRestore();
			DnsForceRestore();
		}

		public override void OnRecoverySave(XmlElement root)
		{
			XmlDocument doc = root.OwnerDocument;

			if (ListOldDhcp.Count != 0)
			{
				XmlElement nodeDhcp = (XmlElement)root.AppendChild(doc.CreateElement("DhcpSwitch"));
				foreach (NetworkManagerDhcpEntry entry in ListOldDhcp)
				{
					XmlElement nodeEntry = nodeDhcp.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}

			if (ListOldDns.Count != 0)
			{
				XmlElement nodeDns = (XmlElement)root.AppendChild(doc.CreateElement("DnsSwitch"));
				foreach (NetworkManagerDnsEntry entry in ListOldDns)
				{
					XmlElement nodeEntry = nodeDns.AppendChild(doc.CreateElement("entry")) as XmlElement;
					entry.WriteXML(nodeEntry);
				}
			}
		}

		private string GetDriverUninstallPath()
		{
			// Note: 32 bit uninstaller can't be viewed by 64 bit app and viceversa.
			// http://www.rhyous.com/2011/01/24/how-read-the-64-bit-registry-from-a-32-bit-application-or-vice-versa/
			
			object objUninstallPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\TAP-Windows", "UninstallString", "");
			if (objUninstallPath != null)
			{
				string uninstallPath = objUninstallPath as string;

				if (File.Exists(uninstallPath))
					return uninstallPath;
			}

			return "";
		}

		public override string GetDriverAvailable()
		{
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if(adapter.Description.StartsWith("TAP-Windows Adapter"))
					return adapter.Description;
			}
			return "";
		}

		public override bool CanInstallDriver()
		{
			return true;
		}

		public override bool CanUnInstallDriver()
		{
			if (GetDriverUninstallPath() != "")
				return true;
			
			return false;
		}

		public override void InstallDriver()
		{
			string driverPath = Software.FindResource("tap-windows.exe");
			if (driverPath == "")
				throw new Exception(Messages.OsDriverNotAvailable);

			Shell(driverPath, "/S");

			System.Threading.Thread.Sleep(3000);
		}

		public override void UnInstallDriver()
		{
			string uninstallPath = GetDriverUninstallPath();
			if (uninstallPath == "")
				return;

			Shell(uninstallPath, "/S");

			System.Threading.Thread.Sleep(3000);
		}

		
        // Specific
		public bool IsWindows8()
		{
			if ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 2))
				return true;

			return false;
		}

        public void HackWindowsInterfaceUp()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in interfaces)
            {
                if (adapter.Description.StartsWith("TAP-Win"))
                {
                    Engine.Instance.Log(Engine.LogType.Verbose, Messages.Format(Messages.HackInterfaceUpDone, adapter.Name));
                    ShellCmd("netsh interface set interface \"" + adapter.Name + "\" ENABLED");
                }
            }            
        }

		/*
		private void WaitUntilEnabled(ManagementObject objMOC)
		{		
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in interfaces)
			{
				if (objMOC["SettingID"] as string == adapter.Id)
				{
					OperationalStatus status = adapter.OperationalStatus;
					if (status == OperationalStatus.Up)
					{
						// TODO: It's always up...
					}
				}
			}
		}
		*/

		private bool SwitchToStaticDo()
		{
			try
			{
				ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection objMOC = objMC.GetInstances();

				foreach (ManagementObject objMO in objMOC)
				{
					if (!((bool)objMO["IPEnabled"]))
						continue;
					if (!((bool)objMO["DHCPEnabled"]))
						continue;

					NetworkManagerDhcpEntry entry = new NetworkManagerDhcpEntry();

					entry.Guid = objMO["SettingID"] as string;
					entry.Description = objMO["Description"] as string;
					entry.IpAddress = (objMO["IPAddress"] as string[]);
					entry.SubnetMask = (objMO["IPSubnet"] as string[]);
					entry.Dns = objMO["DNSServerSearchOrder"] as string[];
					entry.Gateway = objMO["DefaultIPGateway"] as string[];
					
					entry.AutoDns = ((Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + entry.Guid, "NameServer", "") as string) == "");

					Engine.Instance.Log(Engine.LogType.Info, Messages.Format(Messages.NetworkAdapterDhcpDone, entry.Description));
					
					ManagementBaseObject objEnableStatic = objMO.GetMethodParameters("EnableStatic");
					//objNewIP["IPAddress"] = new string[] { ipAddress };
					//objNewIP["SubnetMask"] = new string[] { subnetMask };
					objEnableStatic["IPAddress"] = new string[] { entry.IpAddress[0] };
					objEnableStatic["SubnetMask"] = new string[] { entry.SubnetMask[0] };
					ManagementBaseObject objEnableStaticMethod = objMO.InvokeMethod("EnableStatic", objEnableStatic, null);

					ManagementBaseObject objSetDNSServerSearchOrder = objMO.GetMethodParameters("SetDNSServerSearchOrder");
					objSetDNSServerSearchOrder["DNSServerSearchOrder"] = entry.Dns;
					ManagementBaseObject objSetDNSServerSearchOrderMethod = objMO.InvokeMethod("SetDNSServerSearchOrder", objSetDNSServerSearchOrder, null);

					ManagementBaseObject objSetGateways = objMO.GetMethodParameters("SetGateways");
					objSetGateways["DefaultIPGateway"] = new string[] { entry.Gateway[0] };
					ManagementBaseObject objSetGatewaysMethod = objMO.InvokeMethod("SetGateways", objSetGateways, null);
					
					ListOldDhcp.Add(entry);
					
					// TOOPTIMIZE: Need to wait until the interface return UP...
					//WaitUntilEnabled(objMO); // Checking the OperationalStatus changes are not effective.
					System.Threading.Thread.Sleep(3000);
				}

				return true;
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
				return false;
			}
		}

		private void SwitchToStaticRestore()
		{
			try
			{
				ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection objMOC = objMC.GetInstances();

				foreach (ManagementObject objMO in objMOC)
				{
					string guid = objMO["SettingID"] as string;
					foreach (NetworkManagerDhcpEntry entry in ListOldDhcp)
					{
						if (entry.Guid == guid)
						{
							ManagementBaseObject objEnableDHCP = objMO.GetMethodParameters("EnableDHCP");
							ManagementBaseObject objEnableDHCPMethod = objMO.InvokeMethod("EnableDHCP", objEnableDHCP, null);

							ManagementBaseObject objSetDNSServerSearchOrder = objMO.GetMethodParameters("SetDNSServerSearchOrder");
							if (entry.AutoDns == false)
							{
								objSetDNSServerSearchOrder["DNSServerSearchOrder"] = entry.Dns;
							}
							ManagementBaseObject objSetDNSServerSearchOrderMethod = objMO.InvokeMethod("SetDNSServerSearchOrder", objSetDNSServerSearchOrder, null);

							Engine.Instance.Log(Engine.LogType.Info, Messages.Format(Messages.NetworkAdapterDhcpRestored, entry.Description));
						}
					}
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
			}

			ListOldDhcp.Clear();
		}

		private bool DnsForceDo()
		{
			try
			{
				ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection objMOC = objMC.GetInstances();

				foreach (ManagementObject objMO in objMOC)
				{
					if (!((bool)objMO["IPEnabled"]))
						continue;

					NetworkManagerDnsEntry entry = new NetworkManagerDnsEntry();

					entry.Guid = objMO["SettingID"] as string;
					entry.Description = objMO["Description"] as string;
					entry.Dns = objMO["DNSServerSearchOrder"] as string[];
										
					entry.AutoDns = ((Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + entry.Guid, "NameServer", "") as string) == "");

					if (entry.Dns == null)
						continue;

					Engine.Instance.Log(Engine.LogType.Info, Messages.Format(Messages.NetworkAdapterDnsDone, entry.Description));
					
					ManagementBaseObject objSetDNSServerSearchOrder = objMO.GetMethodParameters("SetDNSServerSearchOrder");
					//objSetDNSServerSearchOrder["DNSServerSearchOrder"] = null;
					objSetDNSServerSearchOrder["DNSServerSearchOrder"] = new string[] { Constants.DnsVpn };
					ManagementBaseObject objSetDNSServerSearchOrderMethod = objMO.InvokeMethod("SetDNSServerSearchOrder", objSetDNSServerSearchOrder, null);
										
					ListOldDns.Add(entry);
				}

				return true;
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
				return false;
			}
		}

		private void DnsForceRestore()
		{
			try
			{
				ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection objMOC = objMC.GetInstances();

				foreach (ManagementObject objMO in objMOC)
				{
					string guid = objMO["SettingID"] as string;
					foreach (NetworkManagerDnsEntry entry in ListOldDns)
					{
						if (entry.Guid == guid)
						{
							ManagementBaseObject objSetDNSServerSearchOrder = objMO.GetMethodParameters("SetDNSServerSearchOrder");
							if (entry.AutoDns == false)
							{
								objSetDNSServerSearchOrder["DNSServerSearchOrder"] = entry.Dns;
							}
							ManagementBaseObject objSetDNSServerSearchOrderMethod = objMO.InvokeMethod("SetDNSServerSearchOrder", objSetDNSServerSearchOrder, null);

							Engine.Instance.Log(Engine.LogType.Info, Messages.Format(Messages.NetworkAdapterDnsRestored, entry.Description));
						}
					}
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
			}

			ListOldDns.Clear();
		}
    }

	public class NetworkManagerDhcpEntry
	{
		//public ManagementObject Obj;
		public string Description;
		public string[] IpAddress;
		public string[] SubnetMask;
		public string[] Dns;
		public string[] Gateway;
		public string Guid;
		public bool AutoDns;

		public void ReadXML(XmlElement node)
		{
			Description = Utils.XmlGetAttributeString(node, "description", "");
			IpAddress = Utils.XmlGetAttributeStringArray(node, "ip_address");
			SubnetMask = Utils.XmlGetAttributeStringArray(node, "subnet_mask");
			Dns = Utils.XmlGetAttributeStringArray(node, "dns");
			Gateway = Utils.XmlGetAttributeStringArray(node, "gateway");
			Guid = Utils.XmlGetAttributeString(node, "guid", "");
			AutoDns = Utils.XmlGetAttributeBool(node, "auto_def", false);
		}

		public void WriteXML(XmlElement node)
		{
			Utils.XmlSetAttributeString(node, "description", Description);
			Utils.XmlSetAttributeStringArray(node, "ip_address", IpAddress);
			Utils.XmlSetAttributeStringArray(node, "subnet_mask", SubnetMask);
			Utils.XmlSetAttributeStringArray(node, "dns", Dns);
			Utils.XmlSetAttributeStringArray(node, "gateway", Gateway);
			Utils.XmlSetAttributeString(node, "guid", Guid);
			Utils.XmlSetAttributeBool(node, "auto_dns", AutoDns);			
		}
	}

	public class NetworkManagerDnsEntry
	{
		//public ManagementObject Obj;
		public string Description;
		public string[] Dns;
		public string Guid;
		public bool AutoDns;

		public void ReadXML(XmlElement node)
		{
			Description = Utils.XmlGetAttributeString(node, "description", "");
			Dns = Utils.XmlGetAttributeStringArray(node, "dns");
			Guid = Utils.XmlGetAttributeString(node, "guid", "");
			AutoDns = Utils.XmlGetAttributeBool(node, "auto_def", false);
		}

		public void WriteXML(XmlElement node)
		{
			Utils.XmlSetAttributeString(node, "description", Description);
			Utils.XmlSetAttributeStringArray(node, "dns", Dns);
			Utils.XmlSetAttributeString(node, "guid", Guid);
			Utils.XmlSetAttributeBool(node, "auto_dns", AutoDns);			
		}
	}
}
