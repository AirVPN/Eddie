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

namespace Eddie.Core
{
	public class NetworkLockManager
	{
		public List<NetworkLockPlugin> Modes = new List<NetworkLockPlugin>();

		private NetworkLockPlugin m_current = null;

		public NetworkLockManager()
		{			
		}

		public void Init()
		{
			Platform.Instance.OnNetworkLockManagerInit();
			//Modes.Add(new NetworkLocks.RoutingTable());
		}

		public void AddPlugin(NetworkLockPlugin plugin)
		{
			//Engine.Instance.Storage.SetDefaultBool("advanced.netlock." + plugin.GetCode() + ".enabled", true, "");
			plugin.Init();
			Modes.Add(plugin);
		}

		public bool IsActive()
		{
			return m_current != null;
		}

		public bool CanEnabled()
		{
			if (IsActive()) // This because if is active, the button need anyway to be showed to deactivated it.
				return true;
 
			if (Modes.Count == 0) // 2.10.1
				return false;

			if (Engine.Instance.Storage.Get("netlock.mode") == "none")
				return false;

			if (Engine.Instance.Storage.Get("routes.default") == "out")
				return false;

			return true;
		}

		public void Activation()
		{
			try
			{
				if (m_current != null)
					throw new Exception(Messages.NetworkLockUnexpectedAlreadyActive);

				NetworkLockPlugin nextCurrent = null;
                
                string requestedMode = Engine.Instance.Storage.Get("netlock.mode");
                if (requestedMode == "auto")
                    requestedMode = Platform.Instance.OnNetworkLockRecommendedMode();

				if (requestedMode != "none")
				{   
					foreach (NetworkLockPlugin plugin in Modes)
					{
                        if (requestedMode == plugin.GetCode())
                        {
							nextCurrent = plugin;
							break;
						}
					}
				}

				if (nextCurrent == null)
				{
					Engine.Instance.Logs.Log(LogType.Fatal, Messages.NetworkLockNoMode);
				}
				else
				{
					string message = Messages.NetworkLockActivation + " - " + nextCurrent.GetName();
					Engine.Instance.WaitMessageSet(message, false);
					Engine.Instance.Logs.Log(LogType.InfoImportant, message);

					nextCurrent.Activation();

					m_current = nextCurrent;
				}                
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(LogType.Fatal, e);
			}

			Recovery.Save();
		}

		public void Deactivation(bool onExit)
		{
			if (m_current != null)
			{
				if (onExit == false)
				{
					Engine.Instance.WaitMessageSet(Messages.NetworkLockDeactivation, false);
					Engine.Instance.Logs.Log(LogType.InfoImportant, Messages.NetworkLockDeactivation);
				}
				else
					Engine.Instance.Logs.Log(LogType.Verbose, Messages.NetworkLockDeactivation);

				try
				{
					m_current.Deactivation();
				}
				catch (Exception e)
				{
					Engine.Instance.Logs.Log(e);
				}

				m_current = null;
			}

			Recovery.Save();
		}

		public bool IsDnsResolutionAvailable(string host)
		{
			if (m_current == null)
				return true;
			else
				return m_current.IsDnsResolutionAvailable(host);
		}

		public void OnUpdateIps()
		{
			if (m_current != null)
			{
				try
				{
					m_current.OnUpdateIps();
					Recovery.Save();
				}
				catch (Exception e)
				{
					Engine.Instance.Logs.Log(e);
				}
			}
		}

		public virtual void OnVpnEstablished()
		{
			if (m_current != null)
			{
				try
				{
					m_current.OnVpnEstablished();
					Recovery.Save();
				}
				catch (Exception e)
				{
					Engine.Instance.Logs.Log(e);
				}
			}
		}

		public virtual void OnVpnDisconnected()
		{
			if (m_current != null)
			{
				try
				{
					m_current.OnVpnDisconnected();
					Recovery.Save();
				}
				catch (Exception e)
				{
					Engine.Instance.Logs.Log(e);
				}
			}
		}

		public void OnRecoveryLoad(XmlElement root)
		{
			try
			{
				if (m_current != null)
					throw new Exception(Messages.NetworkLockRecoveryWhenActive);

				XmlElement node = Utils.XmlGetFirstElementByTagName(root, "netlock");
				if (node != null)
				{
					string code = node.GetAttribute("mode");

					foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
					{
						if (lockPlugin.GetCode() == code)
						{
							m_current = lockPlugin;
							break;
						}
					}

					if (m_current != null)
						m_current.OnRecoveryLoad(node);
					else
						Engine.Instance.Logs.Log(LogType.Warning, Messages.NetworkLockRecoveryUnknownMode);

					Deactivation(false);
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(e);
			}
		}

		public void OnRecoverySave(XmlElement root)
		{
			try
			{
				if (m_current != null)
				{
					XmlElement node = (XmlElement) root.AppendChild(root.OwnerDocument.CreateElement("netlock"));

					node.SetAttribute("mode", m_current.GetCode());

					m_current.OnRecoverySave(node);
				}				
			}
			catch (Exception e)
			{
				Engine.Instance.Logs.Log(e);
			}
		}

		public void AllowIP(IpAddress ip)
		{
			if (m_current != null)
				m_current.AllowIP(ip);
		}

		public void DeallowIP(IpAddress ip)
		{
			if (m_current != null)
				m_current.DeallowIP(ip);
		}

        public virtual void AllowProgram(string path, string name, string guid)
        {
            if (m_current != null)
                m_current.AllowProgram(path, name, guid);
        }

        public virtual void DeallowProgram(string path, string name, string guid)
        {
            if (m_current != null)
                m_current.DeallowProgram(path, name, guid);
        }

        public virtual void AllowInterface(string id)
        {
            if (m_current != null)
                m_current.AllowInterface(id);
        }

        public virtual void DeallowInterface(string id)
        {
            if (m_current != null)
                m_current.DeallowInterface(id);
        }
    }
}
