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

			pnlGeneralWindowsOnly.Visible = Platform.Instance.IsWindowsSystem();
			pnlAdvancedGeneralWindowsOnly.Visible = Platform.Instance.IsWindowsSystem();

			lblGeneralTheme.Visible = Engine.Instance.DevelopmentEnvironment;
			cboGeneralTheme.Visible = Engine.Instance.DevelopmentEnvironment;

            cboRoutesOtherwise.Items.Add(Settings.RouteDirectionToDescription("in"));
            cboRoutesOtherwise.Items.Add(Settings.RouteDirectionToDescription("out"));

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
                if (routeEntries.Length != 3)
                    continue;

                ListViewItem item = new ListViewItem();
                item.Text = routeEntries[0];
                item.SubItems.Add(routeEntries[1]);
                item.SubItems.Add(RouteDirectionToDescription(routeEntries[2]));
                item.ImageKey = routeEntries[2];
                lstRoutes.Items.Add(item);
            }

            // Advanced
            chkExpert.Checked = s.GetBool("advanced.expert");

            
                        
            chkAdvancedCheckDns.Checked = s.GetBool("advanced.check.dns");
            chkAdvancedCheckRoute.Checked = s.GetBool("advanced.check.route");
			//chkDnsSwitch.Checked = s.GetBool("advanced.dnsswitch"); // TOCLEAN
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
			

            chkAdvancedOpenVpnDirectivesDefaultSkip.Checked = s.GetBool("openvpn.skip_defaults");
			txtExePath.Text = s.Get("executables.openvpn");
            txtAdvancedOpenVpnDirectivesCustom.Text = s.Get("openvpn.custom");
            String openVpnDirectivesDefault = "# Common:\n" + s.GetManifestKeyValue("openvpn_directives_common","") + "\n# UDP only:\n" + s.GetManifestKeyValue("openvpn_directives_udp","") + "\n# TCP Only:\n" + s.GetManifestKeyValue("openvpn_directives_tcp","");
			openVpnDirectivesDefault = Platform.Instance.NormalizeString(openVpnDirectivesDefault);
            openVpnDirectivesDefault = openVpnDirectivesDefault.Replace("\t", "");
            txtAdvancedOpenVpnDirectivesDefault.Text = openVpnDirectivesDefault;

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
                routes += item.Text + "," + item.SubItems[1].Text + "," + RouteDescriptionToDirection(item.SubItems[2].Text);
            }
            s.Set("routes.custom", routes);

            // Advanced
            s.SetBool("advanced.expert", chkExpert.Checked);            
            s.SetBool("advanced.check.dns", chkAdvancedCheckDns.Checked);
            s.SetBool("advanced.check.route", chkAdvancedCheckRoute.Checked);
			//s.SetBool("advanced.dnsswitch", chkDnsSwitch.Checked); // TOCLEAN
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
            s.Set("openvpn.custom", txtAdvancedOpenVpnDirectivesCustom.Text);
            s.SetBool("openvpn.skip_defaults", chkAdvancedOpenVpnDirectivesDefaultSkip.Checked);

            SaveOptionsEvent("app.start", 0);
            SaveOptionsEvent("app.stop", 1);
			SaveOptionsEvent("session.start", 2);
			SaveOptionsEvent("session.stop", 3);
            SaveOptionsEvent("vpn.pre", 2);
            SaveOptionsEvent("vpn.up", 3);
            SaveOptionsEvent("vpn.down", 4);

			Platform.Instance.SetAutoStart(chkSystemStart.Checked);			
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

        private void lnkModeMore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
			Core.UI.Actions.OpenUrlDocsProtocols();
        }


		private void lnkAdvancedDocs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Core.UI.Actions.OpenUrlDocsAdvanced();
		}

        private void cmdRouteAdd_Click(object sender, EventArgs e)
        {
            SettingsRoute Dlg = new SettingsRoute();
            Dlg.NetMask = "255.255.255.255";
            Dlg.Action = "out";
            if (Dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ListViewItem item = new ListViewItem();
                item.Text = Dlg.Host;
                item.SubItems.Add(Dlg.NetMask);
                item.SubItems.Add(RouteDirectionToDescription(Dlg.Action));
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

            Dlg.Host = item.Text;
            Dlg.NetMask = item.SubItems[1].Text;
            Dlg.Action = RouteDescriptionToDirection(item.SubItems[2].Text);

            if (Dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                item.Text = Dlg.Host;
                item.SubItems[1].Text = Dlg.NetMask;
                item.SubItems[2].Text = RouteDirectionToDescription(Dlg.Action);
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


        

        
        
        
    }
}