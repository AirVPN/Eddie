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
using System.Text;
using System.Xml;

namespace AirVPN.Core
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
			if (IsActive()) // This because if is active, the button need aynway to be showed to deactivated it.
				return true;

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

				if (requestedMode != "none")
				{
					foreach (NetworkLockPlugin plugin in Modes)
					{
						if( (requestedMode == "auto") || (requestedMode == plugin.GetCode()) )
						{
							nextCurrent = plugin;
							break;
						}
					}
				}

				if (nextCurrent == null)
				{
					Engine.Instance.Log(Engine.LogType.Fatal, Messages.NetworkLockNoMode);
				}
				else
				{
					Engine.Instance.WaitMessageSet(Messages.NetworkLockActivation + " - " + nextCurrent.GetName(), false);

					nextCurrent.Activation();

					m_current = nextCurrent;
				}

				Engine.Instance.Storage.SetBool("netlock.active", true);
			}
			catch (Exception e)
			{
				Engine.Instance.Log(Engine.LogType.Fatal, e);

				Engine.Instance.Storage.SetBool("netlock.active", false);
			}

			Recovery.Save();
		}

		public void Deactivation(bool onExit)
		{
			if (m_current != null)
			{
				if (onExit == false)
					Engine.Instance.WaitMessageSet(Messages.NetworkLockDeactivation, false);
				else
					Engine.Instance.Log(Engine.LogType.Verbose, Messages.NetworkLockDeactivation);

				try
				{
					m_current.Deactivation();
				}
				catch (Exception e)
				{
					Engine.Instance.Log(e);
				}

				m_current = null;
			}

			Engine.Instance.Storage.SetBool("netlock.active", false);

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
					Engine.Instance.Log(e);
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
					Engine.Instance.Log(e);
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
					Engine.Instance.Log(e);
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
						Engine.Instance.Log(Engine.LogType.Warning, Messages.NetworkLockRecoveryUnknownMode);

					Deactivation(false);
				}
			}
			catch (Exception e)
			{
				Engine.Instance.Log(e);
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
				Engine.Instance.Log(e);
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
	}
}
