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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Eddie.Core;

namespace Eddie.Gui.Forms
{
    public partial class Settings : Eddie.Gui.Form
    {
        private Controls.TabNavigator m_tabMain;

        public bool m_onLoadCompleted = false;
        public bool m_modeSshEnabled = true;
        public bool m_modeSslEnabled = true;

        public Settings()
        {
            OnPreInitializeComponent();
            InitializeComponent();
            OnInitializeComponent();
        }

        public override void OnInitializeComponent()
        {
            base.OnInitializeComponent();
        }

        public override void OnApplySkin()
        {
            base.OnApplySkin();

            mnuRoutes.Font = Skin.FontNormal;

            GuiUtils.FixHeightVs(cboProxyMode, lblProxyType);
            GuiUtils.FixHeightVs(txtProxyHost, lblProxyHost);
            GuiUtils.FixHeightVs(txtProxyPort, lblProxyPort);
            GuiUtils.FixHeightVs(cboProxyAuthentication, lblProxyAuthentication);
            GuiUtils.FixHeightVs(txtProxyLogin, lblProxyLogin);
            GuiUtils.FixHeightVs(txtProxyPassword, lblProxyPassword);
            GuiUtils.FixHeightVs(txtProxyTorControlPort, lblProxyTorControlPort);
            GuiUtils.FixHeightVs(txtProxyTorControlPassword, lblProxyTorControlPassword);
            GuiUtils.FixHeightVs(cboRoutesOtherwise, lblRoutesOtherwise);            
            GuiUtils.FixHeightVs(cboDnsSwitchMode, lblDnsSwitchMode);
            GuiUtils.FixHeightVs(chkDnsCheck, lblDnsCheck);

            GuiUtils.FixHeightVs(cboLockMode, lblLockMode);
            GuiUtils.FixHeightVs(chkLockAllowPrivate, lblLockAllowPrivate);
            GuiUtils.FixHeightVs(chkLockAllowPing, lblLockAllowPing);

            GuiUtils.FixHeightVs(chkExpert, lblExpert);
            GuiUtils.FixHeightVs(chkAdvancedCheckRoute, lblAdvancedCheckRoute);
            GuiUtils.FixHeightVs(cboIpV6, lblIpV6);
            GuiUtils.FixHeightVs(cboAdvancedManifestRefresh, lblAdvancedManifestRefresh);
            GuiUtils.FixHeightVs(chkAdvancedPingerEnabled, lblAdvancedPingerEnabled);
            GuiUtils.FixHeightVs(chkRouteRemoveDefault, lblRouteRemoveDefault);
            GuiUtils.FixHeightVs(cboOpenVpnRcvbuf, lblOpenVpnRcvbuf);
            GuiUtils.FixHeightVs(cboOpenVpnSndbuf, lblOpenVpnSndbuf);
            GuiUtils.FixHeightVs(txtExePath, lblExePath);
            GuiUtils.FixHeightVs(txtExePath, cmdExeBrowse);

            GuiUtils.FixHeightVs(txtLogPath, lblLogPath);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CommonInit("Settings");
            
            BuildTreeTabs();

            lblLoggingHelp.Text = Messages.WindowsSettingsLoggingHelp;

			pnlGeneralWindowsOnly.Visible = Platform.Instance.IsWindowsSystem();
			pnlAdvancedGeneralWindowsOnly.Visible = Platform.Instance.IsWindowsSystem();
            chkWindowsWfp.Visible = Platform.Instance.IsWindowsSystem();

            if( (Engine.Instance.AirVPN != null) && (Engine.Instance.AirVPN.Manifest != null) )
            {
                XmlNodeList xmlModes = Engine.Instance.AirVPN.Manifest.SelectNodes("//modes/mode");
                foreach (XmlElement xmlMode in xmlModes)
                {
                    Controls.ListViewItemProtocol itemMode = new Controls.ListViewItemProtocol();
                    itemMode.Protocol = xmlMode.GetAttribute("protocol").ToUpperInvariant();
                    itemMode.Port = Conversions.ToInt32(xmlMode.GetAttribute("port"), 443);
                    itemMode.Entry = Conversions.ToInt32(xmlMode.GetAttribute("entry_index"),0);
                    while (itemMode.SubItems.Count < 5)
                        itemMode.SubItems.Add("");

                    itemMode.SubItems[0].Text = itemMode.Protocol;
                    itemMode.SubItems[1].Text = itemMode.Port.ToString();
                    if (itemMode.Entry == 0)
                        itemMode.SubItems[2].Text = "Primary";
                    else if (itemMode.Entry == 1)
                        itemMode.SubItems[2].Text = "Alternative";
                    else
                        itemMode.SubItems[2].Text = "Alternative " + xmlMode.GetAttribute("entry_index");
                    itemMode.SubItems[3].Text = xmlMode.GetAttribute("title");
                    lstProtocols.Items.Add(itemMode);
                }
                lstProtocols.ResizeColumnsAuto();
            }

            // UI
            cboUiUnit.Items.Clear();
            cboUiUnit.Items.Add(Messages.WindowsSettingsUiUnit0);
            cboUiUnit.Items.Add(Messages.WindowsSettingsUiUnit1);
            cboUiUnit.Items.Add(Messages.WindowsSettingsUiUnit2);

            // Proxy
            cboProxyMode.Items.Clear();
            cboProxyMode.Items.Add("None");
            cboProxyMode.Items.Add("Detect");
            cboProxyMode.Items.Add("Http");
            cboProxyMode.Items.Add("Socks");
            cboProxyMode.Items.Add("Tor");

            // Routes
            lstRoutes.ResizeColumnString(0, "255.255.255.255/255.255.255.255");
            lstRoutes.ResizeColumnString(1, "Outside the VPN tunnel");
            lstRoutes.ResizeColumnMax(2);
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
            lstAdvancedEvents.ResizeColumnsAuto();
            
			lblOpenVpnRcvbuf.Text = Messages.WindowsSettingsOpenVpnRcvBuf + ":";
			lblOpenVpnSndbuf.Text = Messages.WindowsSettingsOpenVpnSndBuf + ":";
			cboOpenVpnRcvbuf.Items.Clear();
            cboOpenVpnRcvbuf.Items.Add(Messages.Automatic);
            cboOpenVpnRcvbuf.Items.Add(Messages.WindowsSettingsOpenVpnDefault);
			cboOpenVpnRcvbuf.Items.Add("8 KB");
			cboOpenVpnRcvbuf.Items.Add("16 KB");
			cboOpenVpnRcvbuf.Items.Add("32 KB");
			cboOpenVpnRcvbuf.Items.Add("64 KB");
			cboOpenVpnRcvbuf.Items.Add("128 KB");
			cboOpenVpnRcvbuf.Items.Add("256 KB");
			cboOpenVpnRcvbuf.Items.Add("512 KB");
			cboOpenVpnSndbuf.Items.Clear();
            cboOpenVpnSndbuf.Items.Add(Messages.Automatic);
            cboOpenVpnSndbuf.Items.Add(Messages.WindowsSettingsOpenVpnDefault);
			cboOpenVpnSndbuf.Items.Add("8 KB");
			cboOpenVpnSndbuf.Items.Add("16 KB");
			cboOpenVpnSndbuf.Items.Add("32 KB");
			cboOpenVpnSndbuf.Items.Add("64 KB");
			cboOpenVpnSndbuf.Items.Add("128 KB");
			cboOpenVpnSndbuf.Items.Add("256 KB");
			cboOpenVpnSndbuf.Items.Add("512 KB");

			cmdAdvancedUninstallDriver.Visible = Platform.Instance.CanUnInstallDriver();
			cmdAdvancedUninstallDriver.Enabled = (Platform.Instance.GetDriverAvailable() != "");

            // OVPN directives

            cboOpenVpnDirectivesDefaultSkip.Items.Clear();
            cboOpenVpnDirectivesDefaultSkip.Items.Add(Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip1);
            cboOpenVpnDirectivesDefaultSkip.Items.Add(Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip2);
            
            ReadOptions();

			RefreshLogPreview();

            EnableIde();

			m_onLoadCompleted = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Engine.FormMain.ResetSkinCache();
            Engine.FormMain.ApplySkin();
        }

        public void BuildTreeTabs()
        {     
            m_tabMain = new Gui.Controls.TabNavigator();
            m_tabMain.Font = Skin.FontNormal;
            m_tabMain.Top = 0;
            m_tabMain.Left = 0;
            m_tabMain.Height = tabSettings.Height;
            m_tabMain.Width = ClientSize.Width;
            m_tabMain.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            m_tabMain.ImportTabControl(tabSettings);
            Controls.Add(m_tabMain);            
        }
        
        public void ReadOptions()
        {
            Storage s = Engine.Instance.Storage;
			
            // General            
            chkConnect.Checked = s.GetBool("connect");
			chkNetLock.Checked = s.GetBool("netlock");
            chkUiMinimizeStart.Checked = s.GetBool("gui.windows.start_minimized");
            chkUiMinimizeTray.Checked = s.GetBool("gui.windows.tray");
			chkSystemNotifications.Checked = s.GetBool("gui.windows.notifications");
            chkGeneralStartLast.Checked = s.GetBool("servers.startlast");
			chkExitConfirm.Checked = s.GetBool("gui.exit_confirm");
            chkOsSingleInstance.Checked = s.GetBool("os.single_instance");

            // Ui
            string uiUnit = s.Get("ui.unit");
            if(uiUnit == "bytes")
                cboUiUnit.SelectedIndex = 1;
            else if (uiUnit == "bits")
                cboUiUnit.SelectedIndex = 2;
            else 
                cboUiUnit.SelectedIndex = 0;

            if (s.Get("gui.font.normal.name") != "")
            {
                chkUiFontGeneral.Checked = true;
                lblUiFontGeneral.Text = s.Get("gui.font.normal.name") + ", " + s.GetFloat("gui.font.normal.size").ToString();
            }
            else
            {
                chkUiFontGeneral.Checked = false;
            }                
            UpdateUiFontGeneral();
            
            // Protocol
            String protocol = s.Get("mode.protocol").ToUpperInvariant();
            int port = s.GetInt("mode.port");
            int alternate = s.GetInt("mode.alt");			
			if (protocol == "AUTO")
            {
                chkProtocolsAutomatic.Checked = true;
            }
            else
            {
                bool found = false;

                foreach(Controls.ListViewItemProtocol itemProtocol in lstProtocols.Items)
                {
                    if( (itemProtocol.Protocol == protocol) &&
                        (itemProtocol.Port == port) &&
                        (itemProtocol.Entry == alternate) )
                    {
                        found = true;
                        itemProtocol.Selected = true;
                        lstProtocols.EnsureVisible(itemProtocol.Index);
                        break;
                    }
                }

                if(found == false)
                    chkProtocolsAutomatic.Checked = true;
                else
                    chkProtocolsAutomatic.Checked = false;
            }

            // Proxy
            cboProxyMode.Text = s.Get("proxy.mode");
            txtProxyHost.Text = s.Get("proxy.host");
            txtProxyPort.Text = s.Get("proxy.port");
            cboProxyAuthentication.Text = s.Get("proxy.auth");
            txtProxyLogin.Text = s.Get("proxy.login");
            txtProxyPassword.Text = s.Get("proxy.password");
            txtProxyTorControlPort.Text = s.Get("proxy.tor.control.port");
            txtProxyTorControlPassword.Text = s.Get("proxy.tor.control.password");


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

			
			chkAdvancedPingerEnabled.Checked = s.GetBool("pinger.enabled");
			chkRouteRemoveDefault.Checked = s.GetBool("routes.remove_default");
			
			chkWindowsTapUp.Checked = s.GetBool("windows.tap_up");
			chkWindowsDhcpSwitch.Checked = s.GetBool("windows.dhcp_disable");
			chkWindowsDisableDriverUpgrade.Checked = s.GetBool("windows.disable_driver_upgrade");
            chkWindowsIPv6DisableAtOs.Checked = s.GetBool("windows.ipv6.os_disable");
            chkWindowsDnsForceAllInterfaces.Checked = s.GetBool("windows.dns.force_all_interfaces");
            chkWindowsDnsLock.Checked = s.GetBool("windows.dns.lock");            
            chkWindowsWfp.Checked = s.GetBool("windows.wfp");

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
            if (openVpnSndBuf == -2)
                cboOpenVpnSndbuf.SelectedIndex = 0;
            else if (openVpnSndBuf == -1)
				cboOpenVpnSndbuf.SelectedIndex = 1;
			else if (openVpnSndBuf == 1024 * 8)
				cboOpenVpnSndbuf.SelectedIndex = 2;
			else if (openVpnSndBuf == 1024 * 16)
				cboOpenVpnSndbuf.SelectedIndex = 3;
			else if (openVpnSndBuf == 1024 * 32)
				cboOpenVpnSndbuf.SelectedIndex = 4;
			else if (openVpnSndBuf == 1024 * 64)
				cboOpenVpnSndbuf.SelectedIndex = 5;
			else if (openVpnSndBuf == 1024 * 128)
				cboOpenVpnSndbuf.SelectedIndex = 6;
			else if (openVpnSndBuf == 1024 * 256)
				cboOpenVpnSndbuf.SelectedIndex = 7;
			else if (openVpnSndBuf == 1024 * 512)
				cboOpenVpnSndbuf.SelectedIndex = 8;

			int openVpnRcvBuf = s.GetInt("openvpn.rcvbuf");
            if (openVpnRcvBuf == -2)
                cboOpenVpnRcvbuf.SelectedIndex = 0;
            else if (openVpnRcvBuf == -1)
				cboOpenVpnRcvbuf.SelectedIndex = 1;
			else if (openVpnRcvBuf == 1024*8)
				cboOpenVpnRcvbuf.SelectedIndex = 2;
			else if (openVpnRcvBuf == 1024 * 16)
				cboOpenVpnRcvbuf.SelectedIndex = 3;
			else if (openVpnRcvBuf == 1024 * 32)
				cboOpenVpnRcvbuf.SelectedIndex = 4;
			else if (openVpnRcvBuf == 1024 * 64)
				cboOpenVpnRcvbuf.SelectedIndex = 5;
			else if (openVpnRcvBuf == 1024 * 128)
				cboOpenVpnRcvbuf.SelectedIndex = 6;
			else if (openVpnRcvBuf == 1024 * 256)
				cboOpenVpnRcvbuf.SelectedIndex = 7;
			else if (openVpnRcvBuf == 1024 * 512)
				cboOpenVpnRcvbuf.SelectedIndex = 8;

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
				if(IpAddress.IsIP(dnsServer))
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
			txtLogPath.Text = s.Get("log.file.path");
			chkLogLevelDebug.Checked = s.GetBool("log.level.debug");

            // Advanced - OVPN Directives
            cboOpenVpnDirectivesDefaultSkip.SelectedIndex = (s.GetBool("openvpn.skip_defaults") ? 1:0);
            txtOpenVpnDirectivesBase.Text = s.Get("openvpn.directives");
            txtOpenVpnDirectivesCustom.Text = s.Get("openvpn.custom");
			
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
                if(Engine.Instance.OnAskYesNo(Messages.WindowsSettingsRouteOutEmptyList) == false)
					return false;  
			}

			return true;
		}

        public void SaveOptions()
        {
            Storage s = Engine.Instance.Storage;


            // General            
            s.SetBool("connect", chkConnect.Checked);
			s.SetBool("netlock", chkNetLock.Checked);
            s.SetBool("gui.windows.start_minimized", chkUiMinimizeStart.Checked);
            s.SetBool("gui.windows.tray", chkUiMinimizeTray.Checked);
			s.SetBool("gui.windows.notifications", chkSystemNotifications.Checked);			 
            s.SetBool("servers.startlast", chkGeneralStartLast.Checked);
			s.SetBool("gui.exit_confirm", chkExitConfirm.Checked);
            s.SetBool("os.single_instance", chkOsSingleInstance.Checked);

            // Ui
            string uiUnit = "";
            if (cboUiUnit.SelectedIndex == 1)
                uiUnit = "bytes";
            else if (cboUiUnit.SelectedIndex == 2)
                uiUnit = "bits";
            s.Set("ui.unit", uiUnit);

            if (chkUiFontGeneral.Checked)
            {
                int posComma = lblUiFontGeneral.Text.IndexOf(",");
                s.Set("gui.font.normal.name", lblUiFontGeneral.Text.Substring(0, posComma));
                s.Set("gui.font.normal.size", lblUiFontGeneral.Text.Substring(posComma+1));
            }
            else
            {
                s.Set("gui.font.normal.name", "");
                s.SetFloat("gui.font.normal.size", 0);
            }

            // Protocols
            if (lstProtocols.Items.Count == 0) // Occur if AirVPN provider is disabled
                chkProtocolsAutomatic.Checked = true;

            if (chkProtocolsAutomatic.Checked)
            {
                s.Set("mode.protocol", "AUTO");
                s.SetInt("mode.port", 443);
                s.SetInt("mode.alt", 0);
            }
            else if(lstProtocols.SelectedItems.Count == 1)
            {
                Controls.ListViewItemProtocol item = lstProtocols.SelectedItems[0] as Controls.ListViewItemProtocol;

                s.Set("mode.protocol", item.Protocol);
                s.SetInt("mode.port", item.Port);
                s.SetInt("mode.alt", item.Entry);
            }
            else
            {
                s.Set("mode.protocol", "AUTO");
                s.SetInt("mode.port", 443);
                s.SetInt("mode.alt", 0);
            }
            
			// Proxy
            s.Set("proxy.mode", cboProxyMode.Text);
            s.Set("proxy.host", txtProxyHost.Text);
            s.Set("proxy.port", txtProxyPort.Text);
            s.Set("proxy.auth", cboProxyAuthentication.Text);
            s.Set("proxy.login", txtProxyLogin.Text);
            s.Set("proxy.password", txtProxyPassword.Text);
            s.SetInt("proxy.tor.control.port", Conversions.ToInt32(txtProxyTorControlPort.Text));
            s.Set("proxy.tor.control.password", txtProxyTorControlPassword.Text);

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
			
			s.SetBool("pinger.enabled", chkAdvancedPingerEnabled.Checked);
			s.SetBool("routes.remove_default", chkRouteRemoveDefault.Checked);
						
			s.SetBool("windows.tap_up", chkWindowsTapUp.Checked);
			s.SetBool("windows.dhcp_disable", chkWindowsDhcpSwitch.Checked);
			s.SetBool("windows.disable_driver_upgrade", chkWindowsDisableDriverUpgrade.Checked);
            s.SetBool("windows.ipv6.os_disable", chkWindowsIPv6DisableAtOs.Checked);
            s.SetBool("windows.dns.force_all_interfaces", chkWindowsDnsForceAllInterfaces.Checked);
            s.SetBool("windows.dns.lock", chkWindowsDnsLock.Checked);
            s.SetBool("windows.wfp", chkWindowsWfp.Checked);

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
				s.SetInt("openvpn.sndbuf", -2);
            else if (openVpnSndBufIndex == 1)
                s.SetInt("openvpn.sndbuf", -1);
            else if (openVpnSndBufIndex == 2)
				s.SetInt("openvpn.sndbuf", 1024 * 8);
			else if (openVpnSndBufIndex == 3)
				s.SetInt("openvpn.sndbuf", 1024 * 16);
			else if (openVpnSndBufIndex == 4)
				s.SetInt("openvpn.sndbuf", 1024 * 32);
			else if (openVpnSndBufIndex == 5)
				s.SetInt("openvpn.sndbuf", 1024 * 64);
			else if (openVpnSndBufIndex == 6)
				s.SetInt("openvpn.sndbuf", 1024 * 128);
			else if (openVpnSndBufIndex == 7)
				s.SetInt("openvpn.sndbuf", 1024 * 256);
			else if (openVpnSndBufIndex == 8)
				s.SetInt("openvpn.sndbuf", 1024 * 512);
			
			int openVpnRcvBufIndex = cboOpenVpnRcvbuf.SelectedIndex;
			if(openVpnRcvBufIndex == 0)
				s.SetInt("openvpn.rcvbuf",-2);
            else if (openVpnRcvBufIndex == 1)
                s.SetInt("openvpn.rcvbuf", -1);
            else if (openVpnRcvBufIndex == 2)
				s.SetInt("openvpn.rcvbuf", 1024*8);
			else if (openVpnRcvBufIndex == 3)
				s.SetInt("openvpn.rcvbuf", 1024 * 16);
			else if (openVpnRcvBufIndex == 4)
				s.SetInt("openvpn.rcvbuf", 1024 * 32);
			else if (openVpnRcvBufIndex == 5)
				s.SetInt("openvpn.rcvbuf", 1024 * 64);
			else if (openVpnRcvBufIndex == 6)
				s.SetInt("openvpn.rcvbuf", 1024 * 128);
			else if (openVpnRcvBufIndex == 7)
				s.SetInt("openvpn.rcvbuf", 1024 * 256);
			else if (openVpnRcvBufIndex == 8)
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
			s.Set("log.file.path", txtLogPath.Text);
			s.SetBool("log.level.debug", chkLogLevelDebug.Checked);

            // Advanced - OVPN Directives
            s.Set("openvpn.directives", txtOpenVpnDirectivesBase.Text);
            s.Set("openvpn.custom", txtOpenVpnDirectivesCustom.Text);
            s.SetBool("openvpn.skip_defaults", (cboOpenVpnDirectivesDefaultSkip.SelectedIndex == 1));

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
			TxtLoggingPathComputed.Text = Engine.Logs.GetParseLogFilePaths(txtLogPath.Text);
		}

        public void EnableIde()
        {
            // Ui
            cmdUiFontGeneral.Enabled = chkUiFontGeneral.Checked;
            
            // Protocols
            lstProtocols.Enabled = (chkProtocolsAutomatic.Checked == false);
            
            cmdRouteAdd.Enabled = true;
            mnuRoutesAdd.Enabled = cmdRouteAdd.Enabled;
            cmdRouteRemove.Enabled = (lstRoutes.SelectedItems.Count > 0);
            mnuRoutesRemove.Enabled = cmdRouteRemove.Enabled;
            cmdRouteEdit.Enabled = (lstRoutes.SelectedItems.Count == 1);
            mnuRoutesEdit.Enabled = cmdRouteEdit.Enabled;

            // Proxy
            bool proxy = (cboProxyMode.Text != "None");
            bool tor = (cboProxyMode.Text == "Tor");            
            txtProxyHost.Enabled = proxy;
            lblProxyHost.Enabled = txtProxyHost.Enabled;            
            txtProxyPort.Enabled = proxy;
            lblProxyPort.Enabled = txtProxyPort.Enabled;
            cboProxyAuthentication.Enabled = (proxy && !tor);
            lblProxyAuthentication.Enabled = cboProxyAuthentication.Enabled;
            txtProxyLogin.Enabled = ((proxy) && (!tor) && (cboProxyAuthentication.Text != "None"));
            lblProxyLogin.Enabled = txtProxyLogin.Enabled;
            txtProxyPassword.Enabled = txtProxyLogin.Enabled;
            lblProxyPassword.Enabled = txtProxyPassword.Enabled;
            txtProxyTorControlPort.Enabled = tor;
            lblProxyTorControlPort.Enabled = txtProxyTorControlPort.Enabled;
            txtProxyTorControlPassword.Enabled = tor;
            lblProxyTorControlPassword.Enabled = txtProxyTorControlPassword.Enabled;
            cmdProxyTorTest.Enabled = tor;

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
            if (cboProxyMode.Text == "Tor")
                txtProxyPort.Text = "9150";
            else
                txtProxyPort.Text = "8080";
            EnableIde();
        }

        private void cboProxyAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableIde();
        }

        private void lnkProtocolsHelp1_LinkClicked(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.docs.protocols");
        }

        private void lnkProtocolsHelp2_LinkClicked(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.docs.udp_vs_tcp");
        }

        private void lnkProxyTorHelp_LinkClicked(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.docs.tor");
        }

        private void lnkLockHelp_LinkClicked(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.docs.lock");
        }

        private void lnkAdvancedHelp_LinkClicked(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.docs.advanced");
        }

        private void lnkOpenVpnDirectivesHelp_Click(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.docs.directives");
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
                lstAdvancedEvents.ResizeColumnsAuto();
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

		private void cmdAdvancedUninstallDriver_Click(object sender, EventArgs e)
		{
			Platform.Instance.UnInstallDriver();

			cmdAdvancedUninstallDriver.Enabled = (Platform.Instance.GetDriverAvailable() != "");

            Engine.Instance.OnMessageInfo(Messages.OsDriverUninstallDone);
		}

		private void TxtLoggingPath_TextChanged(object sender, EventArgs e)
		{
			RefreshLogPreview();
		}

		private void cboRoutesOtherwise_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void cmdProxyTorTest_Click(object sender, EventArgs e)
		{
            string t = TorControl.Test(txtProxyHost.Text, Conversions.ToInt32(txtProxyTorControlPort.Text), txtProxyTorControlPassword.Text);
            Engine.Instance.OnMessageInfo(t);
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

		private void optModeTor_CheckedChanged(object sender, EventArgs e)
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

        private void UpdateUiFontGeneral()
        {
            if (chkUiFontGeneral.Checked == false)
                lblUiFontGeneral.Text = Platform.Instance.GetSystemFont();                    
        }

        private void chkUiFontGeneral_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUiFontGeneral();
            EnableIde();
        }

        private void cmdUiFontGeneral_Click(object sender, EventArgs e)
        {
            FontDialog dlg = new FontDialog();
            dlg.Font = Form.Skin.FontNormal;
            if(dlg.ShowDialog() == DialogResult.OK)
            {
                lblUiFontGeneral.Text = dlg.Font.Name + ", " + dlg.Font.SizeInPoints.ToString();
            }
        }

        private void chkProtocolsAutomatic_CheckedChanged(object sender, EventArgs e)
        {
            EnableIde();
        }

        private void cmdResetToDefault_Click(object sender, EventArgs e)
        {
            if(Engine.Instance.OnAskYesNo(Messages.ResetSettingsConfirm))
            {
                Engine.Instance.Storage.ResetAll(false);
                ReadOptions();
                Engine.Instance.OnMessageInfo(Messages.ResetSettingsDone);
            }
        }
    }
}
