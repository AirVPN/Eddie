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

			cmdAdvancedUninstallDriver.Visible = Platform.Instance.CanUnInstallDriver();
			cmdAdvancedUninstallDriver.Enabled = (Platform.Instance.GetDriverAvailable() != "");            
						
			ReadOptions();

			RefreshLogPreview();

            EnableIde();
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
            chkAutoStart.Checked = s.GetBool("connect");
            chkMinimizeTray.Checked = s.GetBool("gui.windows.tray");
            chkGeneralStartLast.Checked = s.GetBool("servers.startlast");
			chkExitConfirm.Checked = s.GetBool("gui.exit_confirm");

            // Modes
            String protocol = s.Get("mode.protocol").ToUpperInvariant();
            int port = s.GetInt("mode.port");
            int alternate = s.GetInt("mode.alt");

            if( (protocol == "UDP") && (port == 443) && (alternate == 0) )
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
            else
            {
                optModeUDP443.Checked = true;
            }

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

            
                        
            chkAdvancedCheckDns.Checked = s.GetBool("advanced.check.dns");
            chkAdvancedCheckRoute.Checked = s.GetBool("advanced.check.route");
			cboDnsSwitchMode.Text = s.Get("advanced.dns.mode");

			string dnsMode = s.Get("advanced.dns.mode");
			if(dnsMode == "none")
				cboDnsSwitchMode.Text = "Disabled";
			else if(dnsMode == "auto")
				cboDnsSwitchMode.Text = "Automatic";
			else if(dnsMode == "resolvconf")
				cboDnsSwitchMode.Text = "Resolvconf (Linux only)";
			else if(dnsMode == "rename")
				cboDnsSwitchMode.Text = "Renaming (Linux only)";			
			else
				cboDnsSwitchMode.Text = "Automatic";

			chkAdvancedPingerEnabled.Checked = s.GetBool("advanced.pinger.enabled");
			chkAdvancedPingerAlways.Checked = s.GetBool("advanced.pinger.always");
			
			chkAdvancedWindowsTapUp.Checked = s.GetBool("advanced.windows.tap_up");
			chkAdvancedWindowsForceDns.Checked = s.GetBool("advanced.windows.dns_force");
			chkAdvancedWindowsDhcpSwitch.Checked = s.GetBool("advanced.windows.dhcp_disable");

			txtExePath.Text = s.Get("executables.openvpn");

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
			txtLockAllowedIPS.Text = s.Get("netlock.allowed_ips");

			// Advanced - Logging
			chkLoggingEnabled.Checked = s.GetBool("log.file.enabled");
			TxtLoggingPath.Text = s.Get("log.file.path");

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

        public void SaveOptions()
        {
            Storage s = Engine.Instance.Storage;


            // General
            s.Set("gui.skin", cboGeneralTheme.Text);
            s.SetBool("connect", chkAutoStart.Checked);
            s.SetBool("gui.windows.tray", chkMinimizeTray.Checked);
            s.SetBool("servers.startlast", chkGeneralStartLast.Checked);
			s.SetBool("gui.exit_confirm", chkExitConfirm.Checked);

            // Modes
            String protocol;
            int port = 0;
            int alternate = 0;

            if (optModeUDP443.Checked)
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
            else
            {
                // Unexpected...
                protocol = "UDP";
                port = 443;
                alternate = 0;
            }

            s.Set("mode.protocol", protocol.ToUpperInvariant());
            s.SetInt("mode.port", port);
            s.SetInt("mode.alt", alternate);

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
            s.SetBool("advanced.check.dns", chkAdvancedCheckDns.Checked);
            s.SetBool("advanced.check.route", chkAdvancedCheckRoute.Checked);
			s.Set("advanced.dns.mode", cboDnsSwitchMode.Text);

			string dnsMode = cboDnsSwitchMode.Text;
			if (dnsMode == "Disabled")
				s.Set("advanced.dns.mode", "none");
			else if (dnsMode == "Automatic")
				s.Set("advanced.dns.mode", "auto");
			else if (dnsMode == "Resolvconf (Linux only)")
				s.Set("advanced.dns.mode", "resolvconf");
			else if (dnsMode == "Renaming (Linux only)")
				s.Set("advanced.dns.mode", "rename");
			else
				s.Set("advanced.dns.mode", "auto");

			s.SetBool("advanced.pinger.enabled", chkAdvancedPingerEnabled.Checked);
			s.SetBool("advanced.pinger.always", chkAdvancedPingerAlways.Checked);
						
			s.SetBool("advanced.windows.tap_up", chkAdvancedWindowsTapUp.Checked);
			s.SetBool("advanced.windows.dns_force", chkAdvancedWindowsForceDns.Checked);
			s.SetBool("advanced.windows.dhcp_disable", chkAdvancedWindowsDhcpSwitch.Checked);

			s.Set("executables.openvpn", txtExePath.Text);

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
			s.Set("netlock.allowed_ips", txtLockAllowedIPS.Text);

			// Advanced - Logging
			s.SetBool("log.file.enabled", chkLoggingEnabled.Checked);
			s.Set("log.file.path", TxtLoggingPath.Text);

			// Advanced - OVPN Directives
            s.Set("openvpn.custom", txtAdvancedOpenVpnDirectivesCustom.Text);
            s.SetBool("openvpn.skip_defaults", chkAdvancedOpenVpnDirectivesDefaultSkip.Checked);

			// Advanced - Events
            SaveOptionsEvent("app.start", 0);
            SaveOptionsEvent("app.stop", 1);
			SaveOptionsEvent("session.start", 2);
			SaveOptionsEvent("session.stop", 3);
            SaveOptionsEvent("vpn.pre", 2);
            SaveOptionsEvent("vpn.up", 3);
            SaveOptionsEvent("vpn.down", 4);

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

            optModeUDP2018.Enabled = (proxy == false);
            optModeUDP2018Alt.Enabled = (proxy == false);
            optModeUDP443.Enabled = (proxy == false);
            optModeUDP443Alt.Enabled = (proxy == false);
            optModeUDP53.Enabled = (proxy == false);
            optModeUDP53Alt.Enabled = (proxy == false);
            optModeUDP80.Enabled = (proxy == false);
            optModeUDP80Alt.Enabled = (proxy == false);

            cmdRouteAdd.Enabled = true;
            mnuRoutesAdd.Enabled = cmdRouteAdd.Enabled;
            cmdRouteRemove.Enabled = (lstRoutes.SelectedItems.Count > 0);
            mnuRoutesRemove.Enabled = cmdRouteRemove.Enabled;
            cmdRouteEdit.Enabled = (lstRoutes.SelectedItems.Count == 1);
            mnuRoutesEdit.Enabled = cmdRouteEdit.Enabled;

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
            if (openExeDialog.ShowDialog() == DialogResult.OK)
            {
                txtExePath.Text = openExeDialog.FileName;
            }
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            SaveOptions();
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

		
		

		

        

        
        
        
    }
}