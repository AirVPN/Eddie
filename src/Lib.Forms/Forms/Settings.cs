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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AirVPN.Core;

namespace AirVPN.Gui.Forms
{
    public partial class Settings : AirVPN.Gui.Form
    {
		public bool m_onLoadCompleted = false;
        public bool m_modeSshEnabled = true;
        public bool m_modeSslEnabled = true;

        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CommonInit("Settings");

			chkConnect.Text = Messages.WindowsSettingsConnect;
			chkNetLock.Text = Messages.WindowsSettingsNetLock;

			lblLoggingHelp.Text = Messages.WindowsSettingsLoggingHelp;

			pnlGeneralWindowsOnly.Visible = Platform.Instance.IsWindowsSystem();
			pnlAdvancedGeneralWindowsOnly.Visible = Platform.Instance.IsWindowsSystem();

			lblGeneralTheme.Visible = Engine.Instance.DevelopmentEnvironment;
			cboGeneralTheme.Visible = Engine.Instance.DevelopmentEnvironment;

            cboRoutesOtherwise.Items.Add(Settings.RouteDirectionToDescription("in"));
            cboRoutesOtherwise.Items.Add(Settings.RouteDirectionToDescription("out"));

			cboLockMode.Items.Clear();
			cboLockMode.Items.Add("None");
			cboLockMode.Items.Add("Automatic");
			foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				cboLockMode.Items.Add(lockPlugin.GetName());
			lblRoutesNetworkLockWarning.Text = Messages.WindowsSettingsRouteLockHelp;
			lblLockRoutingOutWarning.Text = Messages.NetworkLockNotAvailableWithRouteOut;

            lstAdvancedEvents.Items.Add(new ListViewItem("App Start"));
            lstAdvancedEvents.Items.Add(new ListViewItem("App End"));
			lstAdvancedEvents.Items.Add(new ListViewItem("Session Start"));
			lstAdvancedEvents.Items.Add(new ListViewItem("Session End"));
            lstAdvancedEvents.Items.Add(new ListViewItem("VPN Pre"));
            lstAdvancedEvents.Items.Add(new ListViewItem("VPN Up"));
            lstAdvancedEvents.Items.Add(new ListViewItem("VPN Down"));

            String sshStatus = (Software.SshVersion != "" ? "" : "Not available");
            lblModeSSH.Text = "SSH Tunnel";
            if(sshStatus != "")
            {
                lblModeSSH.Text += " - " + sshStatus;
                m_modeSshEnabled = false;
            }

			String sslStatus = (Software.SslVersion != "" ? "" : "Not available");
            lblModeSSL.Text = "SSL Tunnel";
            if (sslStatus != "")
            {
                lblModeSSL.Text += " - " + sslStatus;
                m_modeSslEnabled = false;                
            }

			lblOpenVpnRcvbuf.Text = Messages.WindowsSettingsOpenVpnRcvBuf + ":";
			lblOpenVpnSndbuf.Text = Messages.WindowsSettingsOpenVpnSndBuf + ":";
			cboOpenVpnRcvbuf.Items.Clear();
			cboOpenVpnRcvbuf.Items.Add(Messages.WindowsSettingsOpenVpnDefault);
			cboOpenVpnRcvbuf.Items.Add("8k");
			cboOpenVpnRcvbuf.Items.Add("16k");
			cboOpenVpnRcvbuf.Items.Add("32k");
			cboOpenVpnRcvbuf.Items.Add("64k");
			cboOpenVpnRcvbuf.Items.Add("128k");
			cboOpenVpnRcvbuf.Items.Add("256k");
			cboOpenVpnRcvbuf.Items.Add("512k");
			cboOpenVpnSndbuf.Items.Clear();
			cboOpenVpnSndbuf.Items.Add(Messages.WindowsSettingsOpenVpnDefault);
			cboOpenVpnSndbuf.Items.Add("8k");
			cboOpenVpnSndbuf.Items.Add("16k");
			cboOpenVpnSndbuf.Items.Add("32k");
			cboOpenVpnSndbuf.Items.Add("64k");
			cboOpenVpnSndbuf.Items.Add("128k");
			cboOpenVpnSndbuf.Items.Add("256k");
			cboOpenVpnSndbuf.Items.Add("512k");

			cmdAdvancedUninstallDriver.Visible = Platform.Instance.CanUnInstallDriver();
			cmdAdvancedUninstallDriver.Enabled = (Platform.Instance.GetDriverAvailable() != "");

			if (Platform.IsUnix())
			{
				lblModeGroup1.Visible = false;
				lblModeGroup2.Visible = false;
				lblModeGroup3.Visible = false;
				lblModeGroup4.Visible = false;
				lblModeGroup5.Visible = false;
			}

			// DNS

			chkDnsCheck.Text = Messages.WindowsSettingsDnsCheck;
			lblDnsServers.Text = Messages.WindowsSettingsDnsServers;
			
			// 
						
			ReadOptions();

			RefreshLogPreview();

            EnableIde();

			m_onLoadCompleted = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Restore previous skin.
            if(Form.ChangeSkin(Engine.Storage.Get("gui.skin")))
                Engine.FormMain.ApplySkin();            
        }

        
        public void ReadOptions()
        {
            Storage s = Engine.Instance.Storage;
			
            // General
            cboGeneralTheme.Text = s.Get("gui.skin");
            chkConnect.Checked = s.GetBool("connect");
			chkNetLock.Checked = s.GetBool("netlock");
            chkMinimizeTray.Checked = s.GetBool("gui.windows.tray");
            chkGeneralStartLast.Checked = s.GetBool("servers.startlast");
			chkExitConfirm.Checked = s.GetBool("gui.exit_confirm");

            // Modes
            String protocol = s.Get("mode.protocol").ToUpperInvariant();
            int port = s.GetInt("mode.port");
            int alternate = s.GetInt("mode.alt");

			if(protocol == "AUTO")
			{
				optModeAutomatic.Checked = true;
			}
            else if( (protocol == "UDP") && (port == 443) && (alternate == 0) )
			{
				optModeUDP443.Checked = true;
            }
            else if( (protocol == "TCP") && (port == 443) && (alternate == 0) )
            {
            	optModeTCP443.Checked = true;
            }
            else if( (protocol == "UDP") && (port == 80) && (alternate == 0) )
            {
            	optModeUDP80.Checked = true;
            }
            else if( (protocol == "TCP") && (port == 80) && (alternate == 0) )
            {
            	optModeTCP80.Checked = true;
            }
            else if( (protocol == "UDP") && (port == 53) && (alternate == 0) )
            {
            	optModeUDP53.Checked = true;
            }
            else if( (protocol == "TCP") && (port == 53) && (alternate == 0) )
            {
            	optModeTCP53.Checked = true;
            }
            else if( (protocol == "UDP") && (port == 2018) && (alternate == 0) )
            {
            	optModeUDP2018.Checked = true;
            }
            else if( (protocol == "TCP") && (port == 2018) && (alternate == 0) )
            {
            	optModeTCP2018.Checked = true;
            }
            else if( (protocol == "UDP") && (port == 443) && (alternate == 1) )
            {
            	optModeUDP443Alt.Checked = true;
            }
            else if( (protocol == "UDP") && (port == 80) && (alternate == 1) )
            {
            	optModeUDP80Alt.Checked = true;
            }
            else if( (protocol == "UDP") && (port == 53) && (alternate == 1) )
            {
            	optModeUDP53Alt.Checked = true;
            }
            else if( (protocol == "UDP") && (port == 2018) && (alternate == 1) )
            {
            	optModeUDP2018Alt.Checked = true;
            }
            else if( (protocol == "TCP") && (port == 2018) && (alternate == 1) )
            {
            	optModeTCP2018Alt.Checked = true;
            }
            else if( (protocol == "SSH") && (port == 22) && (alternate == 0) )
            {
            	optModeSSH22.Checked = true;
            }
            else if( (protocol == "SSH") && (port == 22) && (alternate == 1) )
            {
            	optModeSSH22Alt.Checked = true;
            }
            else if( (protocol == "SSH") && (port == 80) && (alternate == 1) )
            {
            	optModeSSH80.Checked = true;
            }
            else if( (protocol == "SSH") && (port == 53) && (alternate == 1) )
            {
            	optModeSSH53.Checked = true;
            }
            else if( (protocol == "SSL") && (port == 443) && (alternate == 1) )
            {
            	optModeSSL443.Checked = true;
            }
            else if(protocol == "TOR")
			{
				optModeTOR.Checked = true;
			}
			else
            {
                optModeAutomatic.Checked = true;
            }

			txtModeTorHost.Text = s.Get("mode.tor.host");
			txtModeTorPort.Text = s.Get("mode.tor.port");
			txtModeTorControlPort.Text = s.Get("mode.tor.control.port");
			txtModeTorControlPassword.Text = s.Get("mode.tor.control.password");

            // Proxy
            cboProxyMode.Text = s.Get("proxy.mode");
            txtProxyHost.Text = s.Get("proxy.host");
            txtProxyPort.Text = s.Get("proxy.port");
            cboProxyAuthentication.Text = s.Get("proxy.auth");
            txtProxyLogin.Text = s.Get("proxy.login");
            txtProxyPassword.Text = s.Get("proxy.password");
            
                        
            // Routes
            cboRoutesOtherwise.Text = RouteDirectionToDescription(s.Get("routes.default"));

            string routes = s.Get("routes.custom");
            String[] routes2 = routes.Split(';');
            foreach(String route in routes2)
            {
                String[] routeEntries = route.Split(',');
                if (routeEntries.Length < 2)
                    continue;

				string ip = routeEntries[0];
				string action = routeEntries[1];
				string notes = "";
				if (routeEntries.Length == 3)
					notes = routeEntries[2];

                ListViewItem item = new ListViewItem();
                item.Text = ip;                
                item.SubItems.Add(RouteDirectionToDescription(action));
				item.SubItems.Add(notes);
                item.ImageKey = action;
                lstRoutes.Items.Add(item);
            }

            // Advanced - General
            chkExpert.Checked = s.GetBool("advanced.expert");
			chkAdvancedCheckRoute.Checked = s.GetBool("advanced.check.route");

			string ipV6 = s.Get("ipv6.mode");
			if (ipV6 == "none")
				cboIpV6.Text = "None";
			else if (ipV6 == "disable")
				cboIpV6.Text = "Disable";
			else
				cboIpV6.Text = "None";

			
			chkAdvancedPingerEnabled.Checked = s.GetBool("advanced.pinger.enabled");
			chkRouteRemoveDefault.Checked = s.GetBool("routes.remove_default");
			
			chkAdvancedWindowsTapUp.Checked = s.GetBool("advanced.windows.tap_up");
			chkAdvancedWindowsDhcpSwitch.Checked = s.GetBool("advanced.windows.dhcp_disable");
			chkWindowsDisableDriverUpgrade.Checked = s.GetBool("windows.disable_driver_upgrade");

			txtExePath.Text = s.Get("executables.openvpn");

			int manifestRefresh = s.GetInt("advanced.manifest.refresh");
			if (manifestRefresh == 60)
				cboAdvancedManifestRefresh.SelectedIndex = 4;
			else if (manifestRefresh == 10)
					cboAdvancedManifestRefresh.SelectedIndex = 3;
			else if (manifestRefresh == 1)
				cboAdvancedManifestRefresh.SelectedIndex = 2;
			else if (manifestRefresh == 0)
				cboAdvancedManifestRefresh.SelectedIndex = 1;
			else
				cboAdvancedManifestRefresh.SelectedIndex = 0;

			int openVpnSndBuf = s.GetInt("openvpn.sndbuf");
			if (openVpnSndBuf == -1)
				cboOpenVpnSndbuf.SelectedIndex = 0;
			else if (openVpnSndBuf == 1024 * 8)
				cboOpenVpnSndbuf.SelectedIndex = 1;
			else if (openVpnSndBuf == 1024 * 16)
				cboOpenVpnSndbuf.SelectedIndex = 2;
			else if (openVpnSndBuf == 1024 * 32)
				cboOpenVpnSndbuf.SelectedIndex = 3;
			else if (openVpnSndBuf == 1024 * 64)
				cboOpenVpnSndbuf.SelectedIndex = 4;
			else if (openVpnSndBuf == 1024 * 128)
				cboOpenVpnSndbuf.SelectedIndex = 5;
			else if (openVpnSndBuf == 1024 * 256)
				cboOpenVpnSndbuf.SelectedIndex = 6;
			else if (openVpnSndBuf == 1024 * 512)
				cboOpenVpnSndbuf.SelectedIndex = 7;

			int openVpnRcvBuf = s.GetInt("openvpn.rcvbuf");
			if (openVpnRcvBuf == -1)
				cboOpenVpnRcvbuf.SelectedIndex = 0;
			else if (openVpnRcvBuf == 1024*8)
				cboOpenVpnRcvbuf.SelectedIndex = 1;
			else if (openVpnRcvBuf == 1024 * 16)
				cboOpenVpnRcvbuf.SelectedIndex = 2;
			else if (openVpnRcvBuf == 1024 * 32)
				cboOpenVpnRcvbuf.SelectedIndex = 3;
			else if (openVpnRcvBuf == 1024 * 64)
				cboOpenVpnRcvbuf.SelectedIndex = 4;
			else if (openVpnRcvBuf == 1024 * 128)
				cboOpenVpnRcvbuf.SelectedIndex = 5;
			else if (openVpnRcvBuf == 1024 * 256)
				cboOpenVpnRcvbuf.SelectedIndex = 6;
			else if (openVpnRcvBuf == 1024 * 512)
				cboOpenVpnRcvbuf.SelectedIndex = 7;

			// Advanced - DNS
			cboDnsSwitchMode.Text = s.Get("dns.mode");
			string dnsMode = s.Get("dns.mode");
			if (dnsMode == "none")
				cboDnsSwitchMode.Text = "Disabled";
			else if (dnsMode == "auto")
				cboDnsSwitchMode.Text = "Automatic";
			else if (dnsMode == "resolvconf")
				cboDnsSwitchMode.Text = "Resolvconf (Linux only)";
			else if (dnsMode == "rename")
				cboDnsSwitchMode.Text = "Renaming (Linux only)";
			else
				cboDnsSwitchMode.Text = "Automatic";

			chkDnsCheck.Checked = s.GetBool("dns.check");

			lstDnsServers.Items.Clear();
			string[] dnsServers = s.Get("dns.servers").Split(',');
			foreach (string dnsServer in dnsServers)
			{
				if(Utils.IsIP(dnsServer))
					lstDnsServers.Items.Add(new ListViewItem(dnsServer));
			}

			// Advanced - Lock
			string lockMode = s.Get("netlock.mode");
			cboLockMode.Text = "None";
			if (lockMode == "auto")
				cboLockMode.Text = "Automatic";
			else
			{
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				{
					if (lockPlugin.GetCode() == lockMode)
						cboLockMode.Text = lockPlugin.GetName();
				}
			}
			chkLockAllowPrivate.Checked = s.GetBool("netlock.allow_private");
			chkLockAllowPing.Checked = s.GetBool("netlock.allow_ping");
			txtLockAllowedIPS.Text = s.Get("netlock.allowed_ips");

			// Advanced - Logging
			chkLoggingEnabled.Checked = s.GetBool("log.file.enabled");
			TxtLoggingPath.Text = s.Get("log.file.path");
			chkLogLevelDebug.Checked = s.GetBool("log.level.debug");

			// Advanced - OVPN Directives
            chkAdvancedOpenVpnDirectivesDefaultSkip.Checked = s.GetBool("openvpn.skip_defaults");			
            txtAdvancedOpenVpnDirectivesCustom.Text = s.Get("openvpn.custom");
			String openVpnDirectivesDefault = s.GetDefaultDirectives();				
			openVpnDirectivesDefault = openVpnDirectivesDefault.Replace("\t", "");
            txtAdvancedOpenVpnDirectivesDefault.Text = openVpnDirectivesDefault;

			// Advanced - Events
            ReadOptionsEvent("app.start", 0);
            ReadOptionsEvent("app.stop", 1);
			ReadOptionsEvent("session.start", 2);
			ReadOptionsEvent("session.stop", 3);
            ReadOptionsEvent("vpn.pre", 4);
            ReadOptionsEvent("vpn.up", 5);
            ReadOptionsEvent("vpn.down", 6);

			chkSystemStart.Checked = Platform.Instance.GetAutoStart();			
        }

        public void ReadOptionsEvent(string name, int index)
        {
            Storage s = Engine.Instance.Storage;

			String filename = s.Get("event." + name + ".filename");
            if (filename != "")
            {
                lstAdvancedEvents.Items[index].SubItems.Add(filename);
				lstAdvancedEvents.Items[index].SubItems.Add(s.Get("event." + name + ".arguments"));
				lstAdvancedEvents.Items[index].SubItems.Add(s.GetBool("event." + name + ".waitend") ? "Yes" : "No");
            }
        }

		public bool Check()
		{
			if( (RouteDescriptionToDirection(cboRoutesOtherwise.Text) == "out") && (lstRoutes.Items.Count == 0) )			
			{
				if (MessageBox.Show(this, Messages.WindowsSettingsRouteOutEmptyList, Constants.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
					return false;  
			}

			return true;
		}

        public void SaveOptions()
        {
            Storage s = Engine.Instance.Storage;


            // General
            s.Set("gui.skin", cboGeneralTheme.Text);
            s.SetBool("connect", chkConnect.Checked);
			s.SetBool("netlock", chkNetLock.Checked);
            s.SetBool("gui.windows.tray", chkMinimizeTray.Checked);
            s.SetBool("servers.startlast", chkGeneralStartLast.Checked);
			s.SetBool("gui.exit_confirm", chkExitConfirm.Checked);

            // Modes
            String protocol;
            int port = 0;
            int alternate = 0;

			if (optModeAutomatic.Checked)
			{
				protocol = "AUTO";
				port = 443;
				alternate = 0;
			}
            else if (optModeUDP443.Checked)
            {
                protocol = "UDP";
                port = 443;
                alternate = 0;
            }
            else if (optModeTCP443.Checked)
            {
                protocol = "TCP";
                port = 443;
                alternate = 0;
            }
            else if (optModeUDP80.Checked)
            {
                protocol = "UDP";
                port = 80;
                alternate = 0;
            }
            else if (optModeTCP80.Checked)
            {
                protocol = "TCP";
                port = 80;
                alternate = 0;
            }
            else if (optModeUDP53.Checked)
            {
                protocol = "UDP";
                port = 53;
                alternate = 0;
            }
            else if (optModeTCP53.Checked)
            {
                protocol = "TCP";
                port = 53;
                alternate = 0;
            }
            else if (optModeUDP2018.Checked)
            {
                protocol = "UDP";
                port = 2018;
                alternate = 0;
            }
            else if (optModeTCP2018.Checked)
            {
                protocol = "TCP";
                port = 2018;
                alternate = 0;
            }
            else if (optModeUDP443Alt.Checked)
            {
                protocol = "UDP";
                port = 443;
                alternate = 1;
            }
            else if (optModeUDP80Alt.Checked)
            {
                protocol = "UDP";
                port = 80;
                alternate = 1;
            }
            else if (optModeUDP53Alt.Checked)
            {
                protocol = "UDP";
                port = 53;
                alternate = 1;
            }
            else if (optModeUDP2018Alt.Checked)
            {
                protocol = "UDP";
                port = 2018;
                alternate = 1;
            }
            else if (optModeTCP2018Alt.Checked)
            {
                protocol = "TCP";
                port = 2018;
                alternate = 1;
            }
            else if (optModeSSH22.Checked)
            {
                protocol = "SSH";
                port = 22;
                alternate = 0;
            }
            else if (optModeSSH22Alt.Checked)
            {
                protocol = "SSH";
                port = 22;
                alternate = 1;
            }
            else if (optModeSSH80.Checked)
            {
                protocol = "SSH";
                port = 80;
                alternate = 1;
            }
            else if (optModeSSH53.Checked)
            {
                protocol = "SSH";
                port = 53;
                alternate = 1;
            }
            else if (optModeSSL443.Checked)
            {
                protocol = "SSL";
                port = 443;
                alternate = 1;
            }
			else if (optModeTOR.Checked)
			{
				protocol = "TOR";
				port = 2018;
				alternate = 0;
			}
			else
			{
				// Unexpected...
				protocol = "AUTO";
				port = 443;
				alternate = 0;
			}

            s.Set("mode.protocol", protocol.ToUpperInvariant());
            s.SetInt("mode.port", port);
            s.SetInt("mode.alt", alternate);

			s.Set("mode.tor.host", txtModeTorHost.Text);
			s.SetInt("mode.tor.port", Conversions.ToInt32(txtModeTorPort.Text));
			s.SetInt("mode.tor.control.port", Conversions.ToInt32(txtModeTorControlPort.Text));
			s.Set("mode.tor.control.password", txtModeTorControlPassword.Text);

            // Proxy
            s.Set("proxy.mode", cboProxyMode.Text);
            s.Set("proxy.host", txtProxyHost.Text);
            s.Set("proxy.port", txtProxyPort.Text);
            s.Set("proxy.auth", cboProxyAuthentication.Text);
            s.Set("proxy.login", txtProxyLogin.Text);
            s.Set("proxy.password", txtProxyPassword.Text);
                        
            // Routes
            s.Set("routes.default", RouteDescriptionToDirection(cboRoutesOtherwise.Text));

            String routes = "";
            foreach (ListViewItem item in lstRoutes.Items)
            {
                if (routes != "")
                    routes += ";";
				routes += item.Text + "," + RouteDescriptionToDirection(item.SubItems[1].Text) + "," + item.SubItems[2].Text;
            }
            s.Set("routes.custom", routes);

            // Advanced - General
            s.SetBool("advanced.expert", chkExpert.Checked);                        
            s.SetBool("advanced.check.route", chkAdvancedCheckRoute.Checked);

			string ipV6 = cboIpV6.Text;
			if (ipV6 == "None")
				s.Set("ipv6.mode", "none");
			else if(ipV6 == "Disable")
				s.Set("ipv6.mode", "disable");
			else
				s.Set("ipv6.mode", "none");
			
			s.SetBool("advanced.pinger.enabled", chkAdvancedPingerEnabled.Checked);
			s.SetBool("routes.remove_default", chkRouteRemoveDefault.Checked);
						
			s.SetBool("advanced.windows.tap_up", chkAdvancedWindowsTapUp.Checked);
			s.SetBool("advanced.windows.dhcp_disable", chkAdvancedWindowsDhcpSwitch.Checked);
			s.SetBool("windows.disable_driver_upgrade", chkWindowsDisableDriverUpgrade.Checked);

			s.Set("executables.openvpn", txtExePath.Text);

			int manifestRefreshIndex = cboAdvancedManifestRefresh.SelectedIndex;
			if (manifestRefreshIndex == 0) // Auto
				s.SetInt("advanced.manifest.refresh", -1);
			else if (manifestRefreshIndex == 1) // Never
				s.SetInt("advanced.manifest.refresh", 0);
			else if (manifestRefreshIndex == 2) // One minute
				s.SetInt("advanced.manifest.refresh", 1);
			else if (manifestRefreshIndex == 3) // Ten minute
				s.SetInt("advanced.manifest.refresh", 10);
			else if (manifestRefreshIndex == 4) // One hour
				s.SetInt("advanced.manifest.refresh", 60);

			int openVpnSndBufIndex = cboOpenVpnSndbuf.SelectedIndex;
			if (openVpnSndBufIndex == 0)
				s.SetInt("openvpn.sndbuf", -1);
			else if (openVpnSndBufIndex == 1)
				s.SetInt("openvpn.sndbuf", 1024 * 8);
			else if (openVpnSndBufIndex == 2)
				s.SetInt("openvpn.sndbuf", 1024 * 16);
			else if (openVpnSndBufIndex == 3)
				s.SetInt("openvpn.sndbuf", 1024 * 32);
			else if (openVpnSndBufIndex == 4)
				s.SetInt("openvpn.sndbuf", 1024 * 64);
			else if (openVpnSndBufIndex == 5)
				s.SetInt("openvpn.sndbuf", 1024 * 128);
			else if (openVpnSndBufIndex == 6)
				s.SetInt("openvpn.sndbuf", 1024 * 256);
			else if (openVpnSndBufIndex == 7)
				s.SetInt("openvpn.sndbuf", 1024 * 512);
			
			int openVpnRcvBufIndex = cboOpenVpnRcvbuf.SelectedIndex;
			if(openVpnRcvBufIndex == 0)
				s.SetInt("openvpn.rcvbuf",-1);
			else if (openVpnRcvBufIndex == 1)
				s.SetInt("openvpn.rcvbuf", 1024*8);
			else if (openVpnRcvBufIndex == 2)
				s.SetInt("openvpn.rcvbuf", 1024 * 16);
			else if (openVpnRcvBufIndex == 3)
				s.SetInt("openvpn.rcvbuf", 1024 * 32);
			else if (openVpnRcvBufIndex == 4)
				s.SetInt("openvpn.rcvbuf", 1024 * 64);
			else if (openVpnRcvBufIndex == 5)
				s.SetInt("openvpn.rcvbuf", 1024 * 128);
			else if (openVpnRcvBufIndex == 6)
				s.SetInt("openvpn.rcvbuf", 1024 * 256);
			else if (openVpnRcvBufIndex == 7)
				s.SetInt("openvpn.rcvbuf", 1024 * 512);

			// Advanced - DNS

			s.Set("dns.mode", cboDnsSwitchMode.Text);

			string dnsMode = cboDnsSwitchMode.Text;
			if (dnsMode == "Disabled")
				s.Set("dns.mode", "none");
			else if (dnsMode == "Automatic")
				s.Set("dns.mode", "auto");
			else if (dnsMode == "Resolvconf (Linux only)")
				s.Set("dns.mode", "resolvconf");
			else if (dnsMode == "Renaming (Linux only)")
				s.Set("dns.mode", "rename");
			else
				s.Set("dns.mode", "auto");

			s.SetBool("dns.check", chkDnsCheck.Checked);

			string dnsServers = "";
			foreach (ListViewItem dnsServerItem in lstDnsServers.Items)
			{
				if (dnsServers != "")
					dnsServers += ",";
				dnsServers += dnsServerItem.Text;
			}
			s.Set("dns.servers", dnsServers);

			// Advanced - Lock
			string lockMode = cboLockMode.Text;
			s.Set("netlock.mode", "none");
			if (lockMode == "Automatic")
				s.Set("netlock.mode", "auto");
			else
			{
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				{
					if (lockPlugin.GetName() == lockMode)
						s.Set("netlock.mode", lockPlugin.GetCode());
				}
			}
			s.SetBool("netlock.allow_private", chkLockAllowPrivate.Checked);
			s.SetBool("netlock.allow_ping", chkLockAllowPing.Checked);
			s.Set("netlock.allowed_ips", txtLockAllowedIPS.Text);

			// Advanced - Logging
			s.SetBool("log.file.enabled", chkLoggingEnabled.Checked);
			s.Set("log.file.path", TxtLoggingPath.Text);
			s.SetBool("log.level.debug", chkLogLevelDebug.Checked);

			// Advanced - OVPN Directives
            s.Set("openvpn.custom", txtAdvancedOpenVpnDirectivesCustom.Text);
            s.SetBool("openvpn.skip_defaults", chkAdvancedOpenVpnDirectivesDefaultSkip.Checked);

			// Advanced - Events
            SaveOptionsEvent("app.start", 0);
            SaveOptionsEvent("app.stop", 1);
			SaveOptionsEvent("session.start", 2);
			SaveOptionsEvent("session.stop", 3);
            SaveOptionsEvent("vpn.pre", 4);
            SaveOptionsEvent("vpn.up", 5);
            SaveOptionsEvent("vpn.down", 6);

			Platform.Instance.SetAutoStart(chkSystemStart.Checked);

			Engine.OnSettingsChanged();
        }

        public void SaveOptionsEvent(string name, int index)
        {
            Storage s = Engine.Instance.Storage;

            if (lstAdvancedEvents.Items[index].SubItems.Count == 1)
            {
				s.Set("event." + name + ".filename", "");
				s.Set("event." + name + ".arguments", "");
				s.SetBool("event." + name + ".waitend", true);
            }
            else
            {
				s.Set("event." + name + ".filename", lstAdvancedEvents.Items[index].SubItems[1].Text);
				s.Set("event." + name + ".arguments", lstAdvancedEvents.Items[index].SubItems[2].Text);
				s.SetBool("event." + name + ".waitend", (lstAdvancedEvents.Items[index].SubItems[3].Text != "No"));
            }
        }

		public void RefreshLogPreview()
		{			
			TxtLoggingPathComputed.Text = Engine.GetParseLogFilePaths(TxtLoggingPath.Text);
		}

        public void EnableIde()
        {
            bool proxy = (cboProxyMode.Text != "None");
			txtProxyHost.Enabled = proxy;
            txtProxyPort.Enabled = proxy;
            cboProxyAuthentication.Enabled = proxy;
            txtProxyLogin.Enabled = ((proxy) && (cboProxyAuthentication.Text != "None"));
            txtProxyPassword.Enabled = txtProxyLogin.Enabled;


            // Modes
            optModeSSH22.Enabled = ((proxy == false) && (m_modeSshEnabled));
            optModeSSH22Alt.Enabled = ( (proxy == false) && (m_modeSshEnabled) );
            optModeSSH53.Enabled = ( (proxy == false) && (m_modeSshEnabled) );
            optModeSSH80.Enabled = ( (proxy == false) && (m_modeSshEnabled) );
            optModeSSL443.Enabled = ( (proxy == false) && (m_modeSslEnabled) );
			optModeTOR.Enabled = (proxy == false);

            optModeUDP2018.Enabled = (proxy == false);
            optModeUDP2018Alt.Enabled = (proxy == false);
            optModeUDP443.Enabled = (proxy == false);
            optModeUDP443Alt.Enabled = (proxy == false);
            optModeUDP53.Enabled = (proxy == false);
            optModeUDP53Alt.Enabled = (proxy == false);
            optModeUDP80.Enabled = (proxy == false);
            optModeUDP80Alt.Enabled = (proxy == false);

			txtModeTorHost.Enabled = optModeTOR.Checked;
			txtModeTorPort.Enabled = optModeTOR.Checked;
			txtModeTorControlPort.Enabled = optModeTOR.Checked;
			txtModeTorControlPassword.Enabled = optModeTOR.Checked;
			cmdModeTorTest.Enabled = optModeTOR.Checked;

            cmdRouteAdd.Enabled = true;
            mnuRoutesAdd.Enabled = cmdRouteAdd.Enabled;
            cmdRouteRemove.Enabled = (lstRoutes.SelectedItems.Count > 0);
            mnuRoutesRemove.Enabled = cmdRouteRemove.Enabled;
            cmdRouteEdit.Enabled = (lstRoutes.SelectedItems.Count == 1);
            mnuRoutesEdit.Enabled = cmdRouteEdit.Enabled;

			// Dns
			cmdDnsAdd.Enabled = true;
			cmdDnsRemove.Enabled = (lstDnsServers.SelectedItems.Count > 0);
			cmdDnsEdit.Enabled = (lstDnsServers.SelectedItems.Count == 1);

			// Lock
			lblLockRoutingOutWarning.Visible = (cboRoutesOtherwise.Text == Settings.RouteDirectionToDescription("out"));

            cmdAdvancedEventsClear.Enabled = (lstAdvancedEvents.SelectedItems.Count == 1);
            cmdAdvancedEventsEdit.Enabled = (lstAdvancedEvents.SelectedItems.Count == 1);
        }

        public static string RouteDirectionToDescription(string v)
        {
            if (v == "in")
                return "Inside the VPN tunnel";
            else if (v == "out")
                return "Outside the VPN tunnel";
            else
                return "";
        }

        public static string RouteDescriptionToDirection(string v)
        {
            if (v == RouteDirectionToDescription("in"))
                return "in";
            else if (v == RouteDirectionToDescription("out"))
                return "out";
            else
                return "";
        }

        private void cmdExeBrowse_Click(object sender, EventArgs e)
        {
			string result = GuiUtils.FilePicker();
			if (result != "")
				txtExePath.Text = result;
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
			if (Check())
			{
				SaveOptions();
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}			
        }

        private void cmdTos_Click(object sender, EventArgs e)
        {
            Engine.FormMain.TermsOfServiceCheck(true);
        }

        private void cboProxyMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableIde();
        }

        private void cboProxyAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableIde();
        }

        private void cmdModeDocs_Click(object sender, EventArgs e)
		{
			Core.UI.Actions.OpenUrlDocsProtocols();
        }

		
		private void cmdAdvancedGeneralDocs_Click(object sender, EventArgs e)
		{
			Core.UI.Actions.OpenUrlDocsAdvanced();
		}

		private void cmdLockHelp_Click(object sender, EventArgs e)
		{
			Core.UI.Actions.OpenUrlDocsLock();
		}

        private void cmdRouteAdd_Click(object sender, EventArgs e)
        {
            SettingsRoute Dlg = new SettingsRoute();            
            Dlg.Action = "out";
			Dlg.Notes = "";
            if (Dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ListViewItem item = new ListViewItem();
                item.Text = Dlg.Ip;                
                item.SubItems.Add(RouteDirectionToDescription(Dlg.Action));
				item.SubItems.Add(Dlg.Notes);
                item.ImageKey = Dlg.Action;

                lstRoutes.Items.Add(item);
                lstRoutes.SelectedItems.Clear();
                item.Selected = true;
                item.EnsureVisible();

                EnableIde();
            }
        }

        private void cmdRouteRemove_Click(object sender, EventArgs e)
        {
            if (lstRoutes.SelectedItems.Count > 0)
                lstRoutes.Items.Remove(lstRoutes.SelectedItems[0]);

            EnableIde();
        }

        private void cmdRouteEdit_Click(object sender, EventArgs e)
        {
            if (lstRoutes.SelectedItems.Count != 1)
                return;
            ListViewItem item = lstRoutes.SelectedItems[0];

            SettingsRoute Dlg = new SettingsRoute();

            Dlg.Ip = item.Text;            
            Dlg.Action = RouteDescriptionToDirection(item.SubItems[1].Text);
			Dlg.Notes = item.SubItems[2].Text;

            if (Dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                item.Text = Dlg.Ip;                
                item.SubItems[1].Text = RouteDirectionToDescription(Dlg.Action);
				item.SubItems[2].Text = Dlg.Notes;
                item.ImageKey = Dlg.Action;

				EnableIde();
            }
        }

        private void mnuRoutesAdd_Click(object sender, EventArgs e)
        {
            cmdRouteAdd_Click(sender, e);
        }

        private void mnuRoutesRemove_Click(object sender, EventArgs e)
        {
            cmdRouteRemove_Click(sender, e);
        }

        private void mnuRoutesEdit_Click(object sender, EventArgs e)
        {
            cmdRouteEdit_Click(sender, e);
        }

        private void lstRoutes_DoubleClick(object sender, EventArgs e)
        {
            if(cmdRouteEdit.Enabled)
                cmdRouteEdit_Click(sender, e);
        }

        private void lstRoutes_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableIde();
        }

        private void cmdAdvancedEventsClear_Click(object sender, EventArgs e)
        {
            if (lstAdvancedEvents.SelectedItems.Count != 1)
                return;

            while(lstAdvancedEvents.SelectedItems[0].SubItems.Count > 1)
                lstAdvancedEvents.SelectedItems[0].SubItems.RemoveAt(1);
        }

        private void cmdAdvancedEventsEdit_Click(object sender, EventArgs e)
        {
            if (lstAdvancedEvents.SelectedItems.Count != 1)
                return;

            SettingsEvent dlg = new SettingsEvent();
            if (lstAdvancedEvents.SelectedItems[0].SubItems.Count > 1)
            {
                dlg.FileName = lstAdvancedEvents.SelectedItems[0].SubItems[1].Text;
                dlg.Arguments = lstAdvancedEvents.SelectedItems[0].SubItems[2].Text;
                dlg.WaitEnd = (lstAdvancedEvents.SelectedItems[0].SubItems[3].Text != "No");
            }
            else
                dlg.WaitEnd = true;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cmdAdvancedEventsClear_Click(sender, e);

                lstAdvancedEvents.SelectedItems[0].SubItems.Add(dlg.FileName);
                lstAdvancedEvents.SelectedItems[0].SubItems.Add(dlg.Arguments);
                lstAdvancedEvents.SelectedItems[0].SubItems.Add(dlg.WaitEnd ? "Yes" : "No");
            }
        }

        private void lstAdvancedEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableIde();
        }

        private void lstAdvancedEvents_DoubleClick(object sender, EventArgs e)
        {
            cmdAdvancedEventsEdit_Click(sender, e);
        }

        private void cboGeneralTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            Form.ChangeSkin(cboGeneralTheme.Text);

            ApplySkin();
            Engine.FormMain.ApplySkin();            
        }

		private void cmdAdvancedUninstallDriver_Click(object sender, EventArgs e)
		{
			Platform.Instance.UnInstallDriver();

			cmdAdvancedUninstallDriver.Enabled = (Platform.Instance.GetDriverAvailable() != "");

			MessageBox.Show(Messages.OsDriverUninstallDone, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void TxtLoggingPath_TextChanged(object sender, EventArgs e)
		{
			RefreshLogPreview();
		}

		private void cboRoutesOtherwise_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void cmdModeTorTest_Click(object sender, EventArgs e)
		{
			MessageBox.Show(TorControl.Test(txtModeTorHost.Text, Conversions.ToInt32(txtModeTorControlPort.Text), txtModeTorControlPassword.Text));
		}

		private void optModeAutomatic_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeUDP80_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeTCP80_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeUDP53_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeTCP53_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeUDP2018_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeTCP2018_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeUDP443Alt_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeUDP80Alt_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeUDP53Alt_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeUDP2018Alt_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeTCP2018Alt_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeSSH22_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeSSH22Alt_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeSSH80_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeSSH53_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeSSL443_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void optModeTOR_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void cmdDnsAdd_Click(object sender, EventArgs e)
		{
			SettingsIp Dlg = new SettingsIp();
			if (Dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				ListViewItem item = new ListViewItem();
				item.Text = Dlg.Ip;
				
				lstDnsServers.Items.Add(item);
				lstDnsServers.SelectedItems.Clear();
				item.Selected = true;
				item.EnsureVisible();

				EnableIde();
			}
		}

		private void cmdDnsRemove_Click(object sender, EventArgs e)
		{
			if (lstDnsServers.SelectedItems.Count > 0)
				lstDnsServers.Items.Remove(lstDnsServers.SelectedItems[0]);

			EnableIde();			
		}

		private void cmdDnsEdit_Click(object sender, EventArgs e)
		{
			if (lstDnsServers.SelectedItems.Count == 1)
			{
				ListViewItem item = lstDnsServers.SelectedItems[0];

				SettingsIp Dlg = new SettingsIp();

				Dlg.Ip = item.Text;
				
				if (Dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					item.Text = Dlg.Ip;

					EnableIde();
				}
			}
		}

		private void lstDnsServers_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void lstDnsServers_DoubleClick(object sender, EventArgs e)
		{
			cmdDnsEdit_Click(sender, e);
		}

		private void chkNetLock_CheckedChanged(object sender, EventArgs e)
		{
			if (m_onLoadCompleted)
			{
				if (chkNetLock.Checked)
				{
					if (Engine.FormMain.NetworkLockKnowledge() == false)
						chkNetLock.Checked = false;
				}
			}
		}

		
		

		

        

        
        
        
    }
}
