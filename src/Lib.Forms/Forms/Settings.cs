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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Eddie.Core;

namespace Eddie.Forms.Forms
{
	public partial class Settings : Eddie.Forms.Form
	{
		private Controls.TabNavigator m_tabMain;

		public bool m_onLoadCompleted = false;
		public bool m_modeSshEnabled = true;
		public bool m_modeSslEnabled = true;

		private Dictionary<string, string> m_mapNetworkEntryIFace = new Dictionary<string, string>();

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

			Skin.Apply(mnuRoutes);

			mnuRoutes.Font = Skin.FontNormal;

			GuiUtils.FixHeightVs(cboStorageMode, lblStorageMode);
			GuiUtils.FixHeightVs(txtStoragePassword, lblStoragePassword);
			GuiUtils.FixHeightVs(txtStoragePasswordConfirm, lblStoragePasswordConfirm);

			GuiUtils.FixHeightVs(cboProxyMode, lblProxyType);
			GuiUtils.FixHeightVs(cboProxyWhen, lblProxyWhen);
			GuiUtils.FixHeightVs(txtProxyHost, lblProxyHost);
			GuiUtils.FixHeightVs(txtProxyPort, lblProxyPort);
			GuiUtils.FixHeightVs(cboProxyAuthentication, lblProxyAuthentication);
			GuiUtils.FixHeightVs(txtProxyLogin, lblProxyLogin);
			GuiUtils.FixHeightVs(txtProxyPassword, lblProxyPassword);
			GuiUtils.FixHeightVs(txtProxyTorControlPort, lblProxyTorControlPort);
			GuiUtils.FixHeightVs(txtProxyTorControlPassword, lblProxyTorControlPassword);
			GuiUtils.FixHeightVs(cboDnsSwitchMode, lblDnsSwitchMode);
			GuiUtils.FixHeightVs(chkDnsCheck, lblDnsCheck);
						
			GuiUtils.FixHeightVs(cboNetworkIPv4Mode, lblNetworkIPv4Mode);
			GuiUtils.FixHeightVs(cboNetworkIPv6Mode, lblNetworkIPv6Mode);
			GuiUtils.FixHeightVs(cboNetworkEntryIpLayer, lblNetworkEntryIpLayer);
			GuiUtils.FixHeightVs(cboNetworkEntryInterface, lblNetworkEntryInterface);
			GuiUtils.FixHeightVs(cboOpenVpnRcvbuf, lblOpenVpnRcvbuf);
			GuiUtils.FixHeightVs(cboOpenVpnSndbuf, lblOpenVpnSndbuf);
			
			GuiUtils.FixHeightVs(cboLockMode, lblLockMode);
			GuiUtils.FixHeightVs(cboLockIncoming, lblLockIncoming);
			GuiUtils.FixHeightVs(cboLockOutgoing, lblLockOutgoing);

			GuiUtils.FixHeightVs(chkLockAllowPrivate, lblLockAllowPrivate);
			GuiUtils.FixHeightVs(chkLockAllowDHCP, lblLockAllowDHCP);
			GuiUtils.FixHeightVs(chkLockAllowPing, lblLockAllowPing);
			GuiUtils.FixHeightVs(chkLockAllowDNS, lblLockAllowDNS);

			GuiUtils.FixHeightVs(chkExpert, lblExpert);
			GuiUtils.FixHeightVs(chkAdvancedCheckRoute, lblAdvancedCheckRoute);
			
			GuiUtils.FixHeightVs(cboAdvancedManifestRefresh, lblAdvancedManifestRefresh);
			GuiUtils.FixHeightVs(cboAdvancedUpdaterChannel, lblAdvancedUpdaterChannel);
			GuiUtils.FixHeightVs(chkAdvancedPingerEnabled, lblAdvancedPingerEnabled);

			GuiUtils.FixHeightVs(lblAdvancedSkipAlreadyRun, chkAdvancedSkipAlreadyRun);
			GuiUtils.FixHeightVs(lblAdvancedProviders, chkAdvancedProviders);
			GuiUtils.FixHeightVs(lblHummingbirdPrefer, chkHummingbirdPrefer);

			GuiUtils.FixHeightVs(txtExePath, lblExePath);
			GuiUtils.FixHeightVs(txtExePath, cmdExeBrowse);

			GuiUtils.FixHeightVs(txtHummingbirdPath, lblHummingbirdPrefer);
			GuiUtils.FixHeightVs(txtHummingbirdPath, cmdHummingbirdPathBrowse);

			GuiUtils.FixHeightVs(txtLogPath, lblLogPath);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			CommonInit("Settings");

			BuildTreeTabs();

			lblLoggingHelp.Text = LanguageManager.GetText("WindowsSettingsLoggingHelp");

			// General

			chkSystemStart.Visible = GuiUtils.IsWindows();
			chkSystemService.Visible = Platform.Instance.AllowService();
			lblSystemService.Text = Platform.Instance.AllowServiceUserDescription();
			pnlAdvancedGeneralWindowsOnly.Visible = GuiUtils.IsWindows();
			pnlDnsWindowsOnly.Visible = GuiUtils.IsWindows();
			chkWindowsWintun.Visible = GuiUtils.IsWindows();
			chkWindowsDebugWorkaround.Visible = GuiUtils.IsWindows();
			lblHummingbirdPrefer.Visible = (GuiUtils.IsWindows() == false);
			chkHummingbirdPrefer.Visible = (GuiUtils.IsWindows() == false);
			txtHummingbirdPath.Visible = (GuiUtils.IsWindows() == false);
			cmdHummingbirdPathBrowse.Visible = (GuiUtils.IsWindows() == false);

			cboStorageMode.Items.Add(LanguageManager.GetText("WindowsSettingsStorageModeNone"));			
			cboStorageMode.Items.Add(LanguageManager.GetText("WindowsSettingsStorageModePassword"));
			if (Platform.Instance.OsCredentialSystemName() != "")
				cboStorageMode.Items.Add(LanguageManager.GetText("WindowsSettingsStorageModeOs", Platform.Instance.OsCredentialSystemName()));

			// UI
			cboUiUnit.Items.Clear();
			cboUiUnit.Items.Add(LanguageManager.GetText("WindowsSettingsUiUnit0"));
			cboUiUnit.Items.Add(LanguageManager.GetText("WindowsSettingsUiUnit1"));
			cboUiUnit.Items.Add(LanguageManager.GetText("WindowsSettingsUiUnit2"));

			// Protocols
			int nNotAvailable = 0;
			if ((Engine.Instance.AirVPN != null) && (Engine.Instance.AirVPN.Manifest != null))
			{
				foreach (ConnectionMode mode in Engine.Instance.AirVPN.Modes)
				{
					if (mode.Available == false)
					{
						nNotAvailable++;
						continue;
					}
					Controls.ListViewItemProtocol itemMode = new Controls.ListViewItemProtocol();
					itemMode.Mode = mode;
					while (itemMode.SubItems.Count < 6)
						itemMode.SubItems.Add("");
					itemMode.SubItems[0].Text = mode.Protocol;
					itemMode.SubItems[1].Text = Conversions.ToString(mode.Port);
					itemMode.SubItems[2].Text = Conversions.ToString(mode.EntryIndex + 1);
					itemMode.SubItems[3].Text = mode.Title;
					itemMode.SubItems[4].Text = mode.Specs;
					lstProtocols.Items.Add(itemMode);
				}
				lstProtocols.ResizeColumnsAuto();
			}

			if (nNotAvailable != 0)
			{
				lblProtocolsAvailable.Visible = true;
				lblProtocolsAvailable.Text = LanguageManager.GetText("WindowsSettingsSomeProtocolsUnavailable", nNotAvailable.ToString());
			}
			else
			{
				lblProtocolsAvailable.Visible = false;
				lblProtocolsAvailable.Text = "";
			}

			// Proxy
			cboProxyMode.Items.Clear();
			cboProxyMode.Items.Add("None");
			cboProxyMode.Items.Add("Http");
			cboProxyMode.Items.Add("Socks");
			cboProxyMode.Items.Add("Tor");
			cboProxyWhen.Items.Clear();
			cboProxyWhen.Items.Add(LanguageManager.GetText("WindowsSettingsProxyWhenAlways"));
			cboProxyWhen.Items.Add(LanguageManager.GetText("WindowsSettingsProxyWhenWeb"));
			cboProxyWhen.Items.Add(LanguageManager.GetText("WindowsSettingsProxyWhenOpenVPN"));
			cboProxyWhen.Items.Add(LanguageManager.GetText("WindowsSettingsProxyWhenNone"));

			// Routes
			lstRoutes.ImageIconResourcePrefix = "routes_";
			lstRoutes.ResizeColumnString(0, "255.255.255.255/255.255.255.255");
			lstRoutes.ResizeColumnString(1, "Outside the VPN tunnel");
			lstRoutes.ResizeColumnMax(2);
			
			cboLockMode.Items.Clear();
			cboLockMode.Items.Add("None");
			cboLockMode.Items.Add("Automatic");
			foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				cboLockMode.Items.Add(lockPlugin.GetTitleForList());
			
			lstAdvancedEvents.Items.Add(new ListViewItem("App Start"));
			lstAdvancedEvents.Items.Add(new ListViewItem("App End"));
			lstAdvancedEvents.Items.Add(new ListViewItem("Session Start"));
			lstAdvancedEvents.Items.Add(new ListViewItem("Session End"));
			lstAdvancedEvents.Items.Add(new ListViewItem("VPN Pre"));
			lstAdvancedEvents.Items.Add(new ListViewItem("VPN Up"));
			lstAdvancedEvents.Items.Add(new ListViewItem("VPN Down"));
			lstAdvancedEvents.ResizeColumnsAuto();

			lblOpenVpnRcvbuf.Text = LanguageManager.GetText("WindowsSettingsOpenVpnRcvBuf") + ":";
			lblOpenVpnSndbuf.Text = LanguageManager.GetText("WindowsSettingsOpenVpnSndBuf") + ":";
			cboOpenVpnRcvbuf.Items.Clear();
			cboOpenVpnRcvbuf.Items.Add(LanguageManager.GetText("Automatic"));
			cboOpenVpnRcvbuf.Items.Add(LanguageManager.GetText("WindowsSettingsOpenVpnDefault"));
			cboOpenVpnRcvbuf.Items.Add("8 KB");
			cboOpenVpnRcvbuf.Items.Add("16 KB");
			cboOpenVpnRcvbuf.Items.Add("32 KB");
			cboOpenVpnRcvbuf.Items.Add("64 KB");
			cboOpenVpnRcvbuf.Items.Add("128 KB");
			cboOpenVpnRcvbuf.Items.Add("256 KB");
			cboOpenVpnRcvbuf.Items.Add("512 KB");
			cboOpenVpnSndbuf.Items.Clear();
			cboOpenVpnSndbuf.Items.Add(LanguageManager.GetText("Automatic"));
			cboOpenVpnSndbuf.Items.Add(LanguageManager.GetText("WindowsSettingsOpenVpnDefault"));
			cboOpenVpnSndbuf.Items.Add("8 KB");
			cboOpenVpnSndbuf.Items.Add("16 KB");
			cboOpenVpnSndbuf.Items.Add("32 KB");
			cboOpenVpnSndbuf.Items.Add("64 KB");
			cboOpenVpnSndbuf.Items.Add("128 KB");
			cboOpenVpnSndbuf.Items.Add("256 KB");
			cboOpenVpnSndbuf.Items.Add("512 KB");

			cboNetworkEntryIpLayer.Items.Clear();
			cboNetworkEntryIpLayer.Items.Add("IPv6, IPv4");
			cboNetworkEntryIpLayer.Items.Add("IPv4, IPv6");
			cboNetworkEntryIpLayer.Items.Add("IPv6 only");
			cboNetworkEntryIpLayer.Items.Add("IPv4 only");

			cboNetworkIPv4Mode.Items.Clear();
			cboNetworkIPv4Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeInAlways"));
			cboNetworkIPv4Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrOut"));
			cboNetworkIPv4Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrBlock"));
			cboNetworkIPv4Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeOut"));
			cboNetworkIPv4Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeBlock"));

			cboNetworkIPv6Mode.Items.Clear();
			cboNetworkIPv6Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeInAlways"));
			cboNetworkIPv6Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrOut"));
			cboNetworkIPv6Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrBlock"));
			cboNetworkIPv6Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeOut"));
			cboNetworkIPv6Mode.Items.Add(LanguageManager.GetText("WindowsSettingsNetworkIpModeBlock"));

			cboNetworkEntryInterface.Items.Clear();
			m_mapNetworkEntryIFace[""] = "Automatic";
			cboNetworkEntryInterface.Items.Add("Automatic");
			
			Json jNetworkInfo = Engine.Instance.Manifest["network_info"].Value as Json;
			foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
			{
				if ((bool)jNetworkInterface["bind"].Value)
				{
					foreach (string ip in jNetworkInterface["ips"].Json.GetArray())
					{
						string desc = jNetworkInterface["friendly"].Value as string + " - " + ip;
						m_mapNetworkEntryIFace[ip] = desc;
						cboNetworkEntryInterface.Items.Add(desc);
					}
				}
			}

			cboAdvancedUpdaterChannel.Items.Clear();
			cboAdvancedUpdaterChannel.Items.Add("Stable");
			cboAdvancedUpdaterChannel.Items.Add("Beta");
			cboAdvancedUpdaterChannel.Items.Add("None");
						
			if(Platform.IsWindows())
			{
				cmdAdvancedUninstallDriverTap.Visible = true;
				cmdAdvancedUninstallDriverTap.Enabled = (Platform.Instance.GetDriverVersion("0901") != "");
				cmdAdvancedUninstallDriverWintun.Visible = true;
				cmdAdvancedUninstallDriverWintun.Enabled = (Platform.Instance.GetDriverVersion("wintun") != "");
			}
			else
			{
				cmdAdvancedUninstallDriverTap.Visible = false;
				cmdAdvancedUninstallDriverWintun.Visible = false;
			}
			

			// OVPN directives
			cboOpenVpnDirectivesDefaultSkip.Items.Clear();
			cboOpenVpnDirectivesDefaultSkip.Items.Add(LanguageManager.GetText("WindowsSettingsOpenVpnDirectivesDefaultSkip1"));
			cboOpenVpnDirectivesDefaultSkip.Items.Add(LanguageManager.GetText("WindowsSettingsOpenVpnDirectivesDefaultSkip2"));

			// Disabled in this version
			lblShellExternal.Visible = false;
			chkShellExternalRecommended.Visible = false;
			cmdShellExternalClear.Visible = false;
			cmdShellExternalView.Visible = false;
			chkOpenVpnDirectivesAllowScriptSecurity.Visible = false;

			// Init

			ReadOptions();

			RefreshLogPreview();

			EnableIde();

			if (Platform.IsUnix())
				MinimumSize = new Size(1020, 530);

			m_onLoadCompleted = true;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			UiClient.Instance.MainWindow.ResetSkinCache();
			UiClient.Instance.MainWindow.ApplySkin();
		}

		public void BuildTreeTabs()
		{
			m_tabMain = new Eddie.Forms.Controls.TabNavigator();
			m_tabMain.Font = Skin.FontNormal;
			m_tabMain.Top = 0;
			m_tabMain.Left = 0;
			m_tabMain.Height = tabSettings.Height;
			m_tabMain.Width = ClientSize.Width;
			m_tabMain.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
			m_tabMain.ImportTabControl(tabSettings);
			Controls.Add(m_tabMain);

			m_tabMain.SetPageVisible(12, Constants.AlphaFeatures);
		}

		public void ReadOptions()
		{
			Storage s = Engine.Instance.Storage;
			Options o = Engine.Instance.Options;

			// General            
			chkSystemStart.Checked = Platform.Instance.GetAutoStart();
			chkSystemService.Checked = Platform.Instance.GetService();
			chkConnect.Checked = o.GetBool("connect");
			chkNetLock.Checked = o.GetBool("netlock");
			chkGeneralStartLast.Checked = o.GetBool("servers.startlast");
			chkUiExitConfirm.Checked = o.GetBool("gui.exit_confirm");
			chkOsSingleInstance.Checked = o.GetBool("os.single_instance");

			if (s.SaveFormat == "v2n")
				cboStorageMode.SelectedIndex = 0;
			else if (s.SaveFormat == "v2p")
				cboStorageMode.SelectedIndex = 1;
			else if( (s.SaveFormat == "v2s") && (Platform.Instance.OsCredentialSystemName() != "") )
				cboStorageMode.SelectedIndex = 2;
			else
				cboStorageMode.SelectedIndex = 0;
			if (s.SaveFormat == "v2p")
			{
				txtStoragePassword.Text = s.SavePassword;
				txtStoragePasswordConfirm.Text = s.SavePassword;
			}

			// Ui
			chkUiSystemNotifications.Checked = o.GetBool("gui.notifications");
			string uiUnit = o.Get("ui.unit");
			if (uiUnit == "bytes")
				cboUiUnit.SelectedIndex = 1;
			else if (uiUnit == "bits")
				cboUiUnit.SelectedIndex = 2;
			else
				cboUiUnit.SelectedIndex = 0;
			chkUiIEC.Checked = o.GetBool("ui.iec");

			if (o.Get("gui.font.normal.name") != "")
			{
				chkUiFontGeneral.Checked = true;
				lblUiFontGeneral.Text = o.Get("gui.font.normal.name") + ", " + o.GetFloat("gui.font.normal.size").ToString();
			}
			else
			{
				chkUiFontGeneral.Checked = false;
			}
			UpdateUiFontGeneral();
			chkUiStartMinimized.Checked = o.GetBool("gui.start_minimized");
			chkUiTrayShow.Checked = o.GetBool("gui.tray_show");
			chkUiTrayMinimized.Checked = o.GetBool("gui.tray_minimized");

			chkUiSkipProviderManifestFailed.Checked = o.GetBool("ui.skip.provider.manifest.failed");
			chkUiSkipPromotional.Checked = o.GetBool("ui.skip.promotional");

			// Protocol
			String protocol = o.Get("mode.protocol").ToUpperInvariant();
			int port = o.GetInt("mode.port");
			int entryIP = o.GetInt("mode.alt");
			if (protocol == "AUTO")
			{
				chkProtocolsAutomatic.Checked = true;
			}
			else
			{
				bool found = false;

				foreach (Controls.ListViewItemProtocol itemProtocol in lstProtocols.Items)
				{
					if ((itemProtocol.Mode.Protocol == protocol) &&
						(itemProtocol.Mode.Port == port) &&
						(itemProtocol.Mode.EntryIndex == entryIP))
					{
						found = true;
						itemProtocol.Selected = true;
						lstProtocols.EnsureVisible(itemProtocol.Index);
						break;
					}
				}

				if (found == false)
					chkProtocolsAutomatic.Checked = true;
				else
					chkProtocolsAutomatic.Checked = false;
			}

			// Proxy
			cboProxyMode.Text = o.Get("proxy.mode");
			if (o.Get("proxy.when") == "always")
				cboProxyWhen.Text = LanguageManager.GetText("WindowsSettingsProxyWhenAlways");
			else if (o.Get("proxy.when") == "web")
				cboProxyWhen.Text = LanguageManager.GetText("WindowsSettingsProxyWhenWeb");
			else if (o.Get("proxy.when") == "openvpn")
				cboProxyWhen.Text = LanguageManager.GetText("WindowsSettingsProxyWhenOpenVPN");
			else if (o.Get("proxy.when") == "none")
				cboProxyWhen.Text = LanguageManager.GetText("WindowsSettingsProxyWhenNone");
			else
				cboProxyWhen.Text = LanguageManager.GetText("WindowsSettingsProxyWhenAlways");
			txtProxyHost.Text = o.Get("proxy.host");
			txtProxyPort.Text = o.Get("proxy.port");
			cboProxyAuthentication.Text = o.Get("proxy.auth");
			txtProxyLogin.Text = o.Get("proxy.login");
			txtProxyPassword.Text = o.Get("proxy.password");
			txtProxyTorControlPort.Text = o.Get("proxy.tor.control.port");
			txtProxyTorControlPassword.Text = o.Get("proxy.tor.control.password");


			// Routes
			string routes = o.Get("routes.custom");
			String[] routes2 = routes.Split(';');
			foreach (String route in routes2)
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

			// DNS
			cboDnsSwitchMode.Text = o.Get("dns.mode");
			string dnsMode = o.Get("dns.mode");
			if (dnsMode == "none")
				cboDnsSwitchMode.Text = "Disabled";
			else if (dnsMode == "auto")
				cboDnsSwitchMode.Text = "Automatic";
            /* TOCLEAN
			else if (dnsMode == "resolvconf")
				cboDnsSwitchMode.Text = "Resolvconf (Linux only)";
			else if (dnsMode == "rename")
				cboDnsSwitchMode.Text = "Renaming (Linux only)";
			*/               
			else
				cboDnsSwitchMode.Text = "Automatic";

			chkDnsCheck.Checked = o.GetBool("dns.check");

			lstDnsServers.Items.Clear();
			string[] dnsServers = o.Get("dns.servers").Split(',');
			foreach (string dnsServer in dnsServers)
			{
				if (IpAddress.IsIP(dnsServer))
					lstDnsServers.Items.Add(new ListViewItem(dnsServer));
			}

			chkDnsForceAllInterfaces.Checked = o.GetBool("windows.dns.force_all_interfaces");
			chkDnsEnsureLock.Checked = o.GetBool("windows.dns.lock");
			chkDnsIgnoreDNS6.Checked = o.GetBool("windows.ipv6.bypass_dns");

			// Networking

			string networkIPv4Mode = o.Get("network.ipv4.mode");
			if (networkIPv4Mode == "in")
				cboNetworkIPv4Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeInAlways");
			else if (networkIPv4Mode == "in-out")
				cboNetworkIPv4Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrOut");
			else if (networkIPv4Mode == "in-block")
				cboNetworkIPv4Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrBlock");
			else if (networkIPv4Mode == "out")
				cboNetworkIPv4Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeOut");
			else if (networkIPv4Mode == "block")
				cboNetworkIPv4Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeBlock");
			else
				cboNetworkIPv4Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrBlock");
			chkNetworkIPv4AutoSwitch.Checked = o.GetBool("network.ipv4.autoswitch");			

			string networkIPv6Mode = o.Get("network.ipv6.mode");
			if (networkIPv6Mode == "in")
				cboNetworkIPv6Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeInAlways");
			else if (networkIPv6Mode == "in-out")
				cboNetworkIPv6Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrOut");
			else if (networkIPv6Mode == "in-block")
				cboNetworkIPv6Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrBlock");
			else if (networkIPv6Mode == "out")
				cboNetworkIPv6Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeOut");
			else if (networkIPv6Mode == "block")
				cboNetworkIPv6Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeBlock");
			else
				cboNetworkIPv6Mode.Text = LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrBlock");
			chkNetworkIPv6AutoSwitch.Checked = o.GetBool("network.ipv6.autoswitch");

			string networkEntryIpLayer = o.Get("network.entry.iplayer");
			if (networkEntryIpLayer == "ipv6-ipv4")
				cboNetworkEntryIpLayer.Text = "IPv6, IPv4";
			else if (networkEntryIpLayer == "ipv4-ipv6")
				cboNetworkEntryIpLayer.Text = "IPv4, IPv6";
			else if (networkEntryIpLayer == "ipv6-only")
				cboNetworkEntryIpLayer.Text = "IPv6 only";
			else if (networkEntryIpLayer == "ipv4-only")
				cboNetworkEntryIpLayer.Text = "IPv4 only";
			else
				cboNetworkEntryIpLayer.Text = "IPv6, IPv4";

			int iNetworkEntryIFace = 0;
			string sNetworkEntryIFace = o.Get("network.entry.iface");
			foreach (KeyValuePair<string, string> kp in m_mapNetworkEntryIFace)
			{
				if (sNetworkEntryIFace == kp.Key)
				{
					cboNetworkEntryInterface.SelectedIndex = iNetworkEntryIFace;
					break;
				}
				iNetworkEntryIFace++;
			}

			int openVpnSndBuf = o.GetInt("openvpn.sndbuf");
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

			int openVpnRcvBuf = o.GetInt("openvpn.rcvbuf");
			if (openVpnRcvBuf == -2)
				cboOpenVpnRcvbuf.SelectedIndex = 0;
			else if (openVpnRcvBuf == -1)
				cboOpenVpnRcvbuf.SelectedIndex = 1;
			else if (openVpnRcvBuf == 1024 * 8)
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

			// Advanced - General
			chkExpert.Checked = o.GetBool("advanced.expert");
			chkAdvancedCheckRoute.Checked = o.GetBool("advanced.check.route");

			chkAdvancedPingerEnabled.Checked = o.GetBool("pinger.enabled");
			chkAdvancedSkipAlreadyRun.Checked = o.GetBool("advanced.skip_alreadyrun");
			chkAdvancedProviders.Checked = o.GetBool("advanced.providers");
			chkHummingbirdPrefer.Checked = o.GetBool("tools.hummingbird.preferred");

			chkWindowsWintun.Checked = o.GetBool("windows.wintun");
			chkWindowsTapUp.Checked = o.GetBool("windows.tap_up");
			chkWindowsDisableDriverUpgrade.Checked = o.GetBool("windows.disable_driver_upgrade");
			chkWindowsDebugWorkaround.Checked = o.GetBool("windows.workarounds");
			chkWindowsSshPlinkForce.Checked = o.GetBool("windows.ssh.plink.force");

			txtExePath.Text = o.Get("tools.openvpn.path");
			txtHummingbirdPath.Text = o.Get("tools.hummingbird.path");

			int manifestRefresh = o.GetInt("advanced.manifest.refresh");
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

			string updaterChannel = o.Get("updater.channel");
			if (updaterChannel == "stable")
				cboAdvancedUpdaterChannel.Text = "Stable";
			else if (updaterChannel == "beta")
				cboAdvancedUpdaterChannel.Text = "Beta";
			else if (updaterChannel == "none")
				cboAdvancedUpdaterChannel.Text = "None";
			else
				cboAdvancedUpdaterChannel.Text = "Stable";


			// Advanced - Lock
			string lockMode = o.Get("netlock.mode");
			cboLockMode.Text = "None";
			if (lockMode == "auto")
				cboLockMode.Text = "Automatic";
			else
			{
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				{
					if (lockPlugin.GetCode() == lockMode)
						cboLockMode.Text = lockPlugin.GetTitleForList();
				}
			}
			string lockIncoming = o.Get("netlock.incoming");
			cboLockIncoming.Text = "Block";
			if (lockIncoming == "allow")
				cboLockIncoming.Text = "Allow";
			string lockOutgoing = o.Get("netlock.outgoing");
			cboLockOutgoing.Text = "Block";
			if (lockOutgoing == "allow")
				cboLockOutgoing.Text = "Allow";
			chkLockAllowPrivate.Checked = o.GetBool("netlock.allow_private");
			chkLockAllowDHCP.Checked = o.GetBool("netlock.allow_dhcp");
			chkLockAllowPing.Checked = o.GetBool("netlock.allow_ping");
			chkLockAllowDNS.Checked = o.GetBool("netlock.allow_dns");
			txtLockWhiteListIncomingIPs.Text = o.Get("netlock.whitelist.incoming.ips");
			txtLockWhiteListOutgoingIPs.Text = o.Get("netlock.whitelist.outgoing.ips");

			// Advanced - Logging
			chkLoggingEnabled.Checked = o.GetBool("log.file.enabled");
			txtLogPath.Text = o.Get("log.file.path");
			chkLogLevelDebug.Checked = o.GetBool("log.level.debug");

			// Advanced - OVPN Directives
			cboOpenVpnDirectivesDefaultSkip.SelectedIndex = (o.GetBool("openvpn.skip_defaults") ? 1 : 0);
			txtOpenVpnDirectivesBase.Text = o.Get("openvpn.directives");
			txtOpenVpnDirectivesCustom.Text = o.Get("openvpn.custom");
			txtOpenVpnDirectivesCustomPath.Text = o.Get("openvpn.directives.path");
			//chkOpenVpnDirectivesAllowScriptSecurity.Checked = o.GetBool("openvpn.allow.script-security");
			chkOpenVpnDirectivesDataCiphersChaCha.Checked = o.GetBool("openvpn.directives.chacha20");

			// Advanced - Events
			ReadOptionsEvent("app.start", 0);
			ReadOptionsEvent("app.stop", 1);
			ReadOptionsEvent("session.start", 2);
			ReadOptionsEvent("session.stop", 3);
			ReadOptionsEvent("vpn.pre", 4);
			ReadOptionsEvent("vpn.up", 5);
			ReadOptionsEvent("vpn.down", 6);
			chkShellExternalRecommended.Checked = o.GetBool("external.rules.recommended");
		}

		public void ReadOptionsEvent(string name, int index)
		{
			Options o = Engine.Instance.Options;

			String filename = o.Get("event." + name + ".filename");
			if (filename != "")
			{
				lstAdvancedEvents.Items[index].SubItems.Add(filename);
				lstAdvancedEvents.Items[index].SubItems.Add(o.Get("event." + name + ".arguments"));
				lstAdvancedEvents.Items[index].SubItems.Add(o.GetBool("event." + name + ".waitend") ? "Yes" : "No");
			}
		}

		public bool Check()
		{
			if (cboStorageMode.SelectedIndex == 1)
			{
				if( (txtStoragePassword.Text.Trim() == "") || (txtStoragePassword.Text != txtStoragePasswordConfirm.Text) )
				{
					UiClient.Instance.MainWindow.ShowMessageError(LanguageManager.GetText("WindowsSettingsStoragePasswordMismatch"));
					return false;
				}
			}
			
			if (chkLockAllowDNS.Checked == false)
			{
				bool hostNameUsed = false;
				foreach (ListViewItem item in lstRoutes.Items)
				{
					if (IpAddress.IsIP(item.Text) == false)
					{
						hostNameUsed = true;
						break;
					}
				}

				if (hostNameUsed)
					if (UiClient.Instance.MainWindow.AskYesNo(LanguageManager.GetText("WindowsSettingsRouteWithHostname")) == false)
						return false;
			}

			return true;
		}

		public void SaveOptions()
		{
			Storage s = Engine.Instance.Storage;
			Options o = Engine.Instance.Options;

			// General            
			Platform.Instance.SetAutoStart(chkSystemStart.Checked);
			Platform.Instance.SetService(chkSystemService.Checked, false);
			o.SetBool("connect", chkConnect.Checked);
			o.SetBool("netlock", chkNetLock.Checked);
			o.SetBool("servers.startlast", chkGeneralStartLast.Checked);
			o.SetBool("gui.exit_confirm", chkUiExitConfirm.Checked);
			o.SetBool("os.single_instance", chkOsSingleInstance.Checked);

			if(cboStorageMode.SelectedIndex == 0)
			{
				s.SaveFormat = "v2n";
			}
			else if (cboStorageMode.SelectedIndex == 1)
			{
				s.SaveFormat = "v2p";
				s.SavePassword = txtStoragePassword.Text;
			}
			else if (cboStorageMode.SelectedIndex == 2)
			{
				s.SaveFormat = "v2s";
				s.SavePassword = s.LoadPassword;
			}

			// Ui
			o.SetBool("gui.notifications", chkUiSystemNotifications.Checked);
			string uiUnit = "";
			if (cboUiUnit.SelectedIndex == 1)
				uiUnit = "bytes";
			else if (cboUiUnit.SelectedIndex == 2)
				uiUnit = "bits";
			o.Set("ui.unit", uiUnit);
			o.SetBool("ui.iec", chkUiIEC.Checked);

			if (chkUiFontGeneral.Checked)
			{
				int posComma = lblUiFontGeneral.Text.IndexOfInv(",");
				o.Set("gui.font.normal.name", lblUiFontGeneral.Text.Substring(0, posComma));
				o.Set("gui.font.normal.size", lblUiFontGeneral.Text.Substring(posComma + 1));
			}
			else
			{
				o.Set("gui.font.normal.name", "");
				o.SetFloat("gui.font.normal.size", 0);
			}
			o.SetBool("gui.start_minimized", chkUiStartMinimized.Checked);
			o.SetBool("gui.tray_show", chkUiTrayShow.Checked);
			o.SetBool("gui.tray_minimized", chkUiTrayShow.Checked && chkUiTrayMinimized.Checked);

			o.SetBool("ui.skip.provider.manifest.failed", chkUiSkipProviderManifestFailed.Checked);
			o.SetBool("ui.skip.promotional", chkUiSkipPromotional.Checked);

			// Protocols
			if (lstProtocols.Items.Count == 0) // Occur if AirVPN provider is disabled
				chkProtocolsAutomatic.Checked = true;

			if (chkProtocolsAutomatic.Checked)
			{
				o.Set("mode.protocol", "AUTO");
				o.SetInt("mode.port", 443);
				o.SetInt("mode.alt", 0);
			}
			else if (lstProtocols.SelectedItems.Count == 1)
			{
				Controls.ListViewItemProtocol item = lstProtocols.SelectedItems[0] as Controls.ListViewItemProtocol;

				o.Set("mode.protocol", item.Mode.Protocol);
				o.SetInt("mode.port", item.Mode.Port);
				o.SetInt("mode.alt", item.Mode.EntryIndex);
			}
			else
			{
				o.Set("mode.protocol", "AUTO");
				o.SetInt("mode.port", 443);
				o.SetInt("mode.alt", 0);
			}

			// Proxy
			o.Set("proxy.mode", cboProxyMode.Text);
			if (cboProxyWhen.Text == LanguageManager.GetText("WindowsSettingsProxyWhenAlways"))
				o.Set("proxy.when", "always");
			else if (cboProxyWhen.Text == LanguageManager.GetText("WindowsSettingsProxyWhenWeb"))
				o.Set("proxy.when", "web");
			else if (cboProxyWhen.Text == LanguageManager.GetText("WindowsSettingsProxyWhenOpenVPN"))
				o.Set("proxy.when", "openvpn");
			else if (cboProxyWhen.Text == LanguageManager.GetText("WindowsSettingsProxyWhenNone"))
				o.Set("proxy.when", "none");
			else
				o.Set("proxy.when", "always");
			o.Set("proxy.host", txtProxyHost.Text);
			o.Set("proxy.port", txtProxyPort.Text);
			o.Set("proxy.auth", cboProxyAuthentication.Text);
			o.Set("proxy.login", txtProxyLogin.Text);
			o.Set("proxy.password", txtProxyPassword.Text);
			o.SetInt("proxy.tor.control.port", Conversions.ToInt32(txtProxyTorControlPort.Text));
			o.Set("proxy.tor.control.password", txtProxyTorControlPassword.Text);

			// Routes
			String routes = "";
			foreach (ListViewItem item in lstRoutes.Items)
			{
				if (routes != "")
					routes += ";";
				routes += item.Text + "," + RouteDescriptionToDirection(item.SubItems[1].Text) + "," + item.SubItems[2].Text;
			}
			o.Set("routes.custom", routes);

			// DNS
			o.Set("dns.mode", cboDnsSwitchMode.Text);

			string dnsMode = cboDnsSwitchMode.Text;
			if (dnsMode == "Disabled")
				o.Set("dns.mode", "none");
			else if (dnsMode == "Automatic")
				o.Set("dns.mode", "auto");
			/* TOCLEAN
			else if (dnsMode == "Resolvconf (Linux only)")
				o.Set("dns.mode", "resolvconf");
			else if (dnsMode == "Renaming (Linux only)")
				o.Set("dns.mode", "rename");
			*/
			else
				o.Set("dns.mode", "auto");

			o.SetBool("dns.check", chkDnsCheck.Checked);

			string dnsServers = "";
			foreach (ListViewItem dnsServerItem in lstDnsServers.Items)
			{
				if (dnsServers != "")
					dnsServers += ",";
				dnsServers += dnsServerItem.Text;
			}
			o.Set("dns.servers", dnsServers);

			o.SetBool("windows.dns.force_all_interfaces", chkDnsForceAllInterfaces.Checked);
			o.SetBool("windows.dns.lock", chkDnsEnsureLock.Checked);
			o.SetBool("windows.ipv6.bypass_dns", chkDnsIgnoreDNS6.Checked);

			// Networking
			string networkIPv4Mode = cboNetworkIPv4Mode.Text;
			if (networkIPv4Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeInAlways"))
				o.Set("network.ipv4.mode", "in");
			else if (networkIPv4Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrOut"))
				o.Set("network.ipv4.mode", "in-out");
			else if (networkIPv4Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrBlock"))
				o.Set("network.ipv4.mode", "in-block");
			else if (networkIPv4Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeOut"))
				o.Set("network.ipv4.mode", "out");
			else if (networkIPv4Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeBlock"))
				o.Set("network.ipv4.mode", "block");
			else
				o.Set("network.ipv4.mode", "in");
			o.SetBool("network.ipv4.autoswitch", chkNetworkIPv4AutoSwitch.Checked);

			string networkIPv6Mode = cboNetworkIPv6Mode.Text;
			if (networkIPv6Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeInAlways"))
				o.Set("network.ipv6.mode", "in");
			else if (networkIPv6Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrOut"))
				o.Set("network.ipv6.mode", "in-out");
			else if (networkIPv6Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeInOrBlock"))
				o.Set("network.ipv6.mode", "in-block");
			else if (networkIPv6Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeOut"))
				o.Set("network.ipv6.mode", "out");
			else if (networkIPv6Mode == LanguageManager.GetText("WindowsSettingsNetworkIpModeBlock"))
				o.Set("network.ipv6.mode", "block");
			else
				o.Set("network.ipv6.mode", "in-block");
			o.SetBool("network.ipv6.autoswitch", chkNetworkIPv6AutoSwitch.Checked);

			string networkEntryIpLayer = cboNetworkEntryIpLayer.Text;
			if (networkEntryIpLayer == "IPv6, IPv4")
				o.Set("network.entry.iplayer", "ipv6-ipv4");
			else if (networkEntryIpLayer == "IPv4, IPv6")
				o.Set("network.entry.iplayer", "ipv4-ipv6");
			else if (networkEntryIpLayer == "IPv6 only")
				o.Set("network.entry.iplayer", "ipv6-only");
			else if (networkEntryIpLayer == "IPv4 only")
				o.Set("network.entry.iplayer", "ipv4-only");
			else
				o.Set("network.entry.iplayer", "ipv6-ipv4");

			string networkEntryIFace = cboNetworkEntryInterface.Text;
			foreach (KeyValuePair<string, string> kp in m_mapNetworkEntryIFace)
			{
				if (kp.Value == networkEntryIFace)
				{
					o.Set("network.entry.iface", kp.Key);
					break;
				}
			}

			int openVpnSndBufIndex = cboOpenVpnSndbuf.SelectedIndex;
			if (openVpnSndBufIndex == 0)
				o.SetInt("openvpn.sndbuf", -2);
			else if (openVpnSndBufIndex == 1)
				o.SetInt("openvpn.sndbuf", -1);
			else if (openVpnSndBufIndex == 2)
				o.SetInt("openvpn.sndbuf", 1024 * 8);
			else if (openVpnSndBufIndex == 3)
				o.SetInt("openvpn.sndbuf", 1024 * 16);
			else if (openVpnSndBufIndex == 4)
				o.SetInt("openvpn.sndbuf", 1024 * 32);
			else if (openVpnSndBufIndex == 5)
				o.SetInt("openvpn.sndbuf", 1024 * 64);
			else if (openVpnSndBufIndex == 6)
				o.SetInt("openvpn.sndbuf", 1024 * 128);
			else if (openVpnSndBufIndex == 7)
				o.SetInt("openvpn.sndbuf", 1024 * 256);
			else if (openVpnSndBufIndex == 8)
				o.SetInt("openvpn.sndbuf", 1024 * 512);

			int openVpnRcvBufIndex = cboOpenVpnRcvbuf.SelectedIndex;
			if (openVpnRcvBufIndex == 0)
				o.SetInt("openvpn.rcvbuf", -2);
			else if (openVpnRcvBufIndex == 1)
				o.SetInt("openvpn.rcvbuf", -1);
			else if (openVpnRcvBufIndex == 2)
				o.SetInt("openvpn.rcvbuf", 1024 * 8);
			else if (openVpnRcvBufIndex == 3)
				o.SetInt("openvpn.rcvbuf", 1024 * 16);
			else if (openVpnRcvBufIndex == 4)
				o.SetInt("openvpn.rcvbuf", 1024 * 32);
			else if (openVpnRcvBufIndex == 5)
				o.SetInt("openvpn.rcvbuf", 1024 * 64);
			else if (openVpnRcvBufIndex == 6)
				o.SetInt("openvpn.rcvbuf", 1024 * 128);
			else if (openVpnRcvBufIndex == 7)
				o.SetInt("openvpn.rcvbuf", 1024 * 256);
			else if (openVpnRcvBufIndex == 8)
				o.SetInt("openvpn.rcvbuf", 1024 * 512);

			// Advanced - General
			o.SetBool("advanced.expert", chkExpert.Checked);
			o.SetBool("advanced.check.route", chkAdvancedCheckRoute.Checked);

			o.SetBool("pinger.enabled", chkAdvancedPingerEnabled.Checked);
			o.SetBool("advanced.skip_alreadyrun", chkAdvancedSkipAlreadyRun.Checked);
			o.SetBool("advanced.providers", chkAdvancedProviders.Checked);
			o.SetBool("tools.hummingbird.preferred", chkHummingbirdPrefer.Checked);

			o.SetBool("windows.wintun", chkWindowsWintun.Checked);
			o.SetBool("windows.tap_up", chkWindowsTapUp.Checked);
			o.SetBool("windows.disable_driver_upgrade", chkWindowsDisableDriverUpgrade.Checked);
			o.SetBool("windows.workarounds", chkWindowsDebugWorkaround.Checked);
			o.SetBool("windows.ssh.plink.force", chkWindowsSshPlinkForce.Checked);

			SetOption("tools.openvpn.path", txtExePath.Text);
			SetOption("tools.hummingbird.path", txtHummingbirdPath.Text);
			

			int manifestRefreshIndex = cboAdvancedManifestRefresh.SelectedIndex;
			if (manifestRefreshIndex == 0) // Auto
				o.SetInt("advanced.manifest.refresh", -1);
			else if (manifestRefreshIndex == 1) // Never
				o.SetInt("advanced.manifest.refresh", 0);
			else if (manifestRefreshIndex == 2) // One minute
				o.SetInt("advanced.manifest.refresh", 1);
			else if (manifestRefreshIndex == 3) // Ten minute
				o.SetInt("advanced.manifest.refresh", 10);
			else if (manifestRefreshIndex == 4) // One hour
				o.SetInt("advanced.manifest.refresh", 60);

			string updaterChannel = cboAdvancedUpdaterChannel.Text;
			if (updaterChannel == "Stable")
				o.Set("updater.channel", "stable");
			else if (updaterChannel == "Beta")
				o.Set("updater.channel", "beta");
			else if (updaterChannel == "None")
				o.Set("updater.channel", "none");
			else
				o.Set("updater.channel", "stable");

			// Advanced - Lock
			string lockMode = cboLockMode.Text;
			o.Set("netlock.mode", "none");
			if (lockMode == "Automatic")
				o.Set("netlock.mode", "auto");
			else
			{
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				{
					if (lockPlugin.GetTitleForList() == lockMode)
						o.Set("netlock.mode", lockPlugin.GetCode());
				}
			}
			string lockIncoming = cboLockIncoming.Text;
			if (lockIncoming == "Allow")
				o.Set("netlock.incoming", "allow");
			else
				o.Set("netlock.incoming", "block");
			string lockOutgoing = cboLockOutgoing.Text;
			if (lockOutgoing == "Allow")
				o.Set("netlock.outgoing", "allow");
			else
				o.Set("netlock.outgoing", "block");
			o.SetBool("netlock.allow_private", chkLockAllowPrivate.Checked);
			o.SetBool("netlock.allow_dhcp", chkLockAllowDHCP.Checked);
			o.SetBool("netlock.allow_ping", chkLockAllowPing.Checked);
			o.SetBool("netlock.allow_dns", chkLockAllowDNS.Checked);
			o.Set("netlock.whitelist.incoming.ips", txtLockWhiteListIncomingIPs.Text);
			o.Set("netlock.whitelist.outgoing.ips", txtLockWhiteListOutgoingIPs.Text);

			// Advanced - Networking

			// Advanced - Logging
			o.SetBool("log.file.enabled", chkLoggingEnabled.Checked);
			o.Set("log.file.path", txtLogPath.Text);
			o.SetBool("log.level.debug", chkLogLevelDebug.Checked);

			// Advanced - OVPN Directives
			o.Set("openvpn.directives", txtOpenVpnDirectivesBase.Text);
			o.Set("openvpn.custom", txtOpenVpnDirectivesCustom.Text);
			o.Set("openvpn.directives.path", txtOpenVpnDirectivesCustomPath.Text);
			o.SetBool("openvpn.skip_defaults", (cboOpenVpnDirectivesDefaultSkip.SelectedIndex == 1));
			//o.SetBool("openvpn.allow.script-security", chkOpenVpnDirectivesAllowScriptSecurity.Checked);

			o.SetBool("openvpn.directives.chacha20", chkOpenVpnDirectivesDataCiphersChaCha.Checked);

			// Advanced - Events
			SaveOptionsEvent("app.start", 0);
			SaveOptionsEvent("app.stop", 1);
			SaveOptionsEvent("session.start", 2);
			SaveOptionsEvent("session.stop", 3);
			SaveOptionsEvent("vpn.pre", 4);
			SaveOptionsEvent("vpn.up", 5);
			SaveOptionsEvent("vpn.down", 6);
			o.SetBool("external.rules.recommended", chkShellExternalRecommended.Checked);
			
			Engine.OnSettingsChanged();
		}

		public void SaveOptionsEvent(string name, int index)
		{
			Storage s = Engine.Instance.Storage;
			Options o = Engine.Instance.Options;

			if (lstAdvancedEvents.Items[index].SubItems.Count == 1)
			{
				o.Set("event." + name + ".filename", "");
				o.Set("event." + name + ".arguments", "");
				o.SetBool("event." + name + ".waitend", true);
			}
			else
			{
				o.Set("event." + name + ".filename", lstAdvancedEvents.Items[index].SubItems[1].Text);
				o.Set("event." + name + ".arguments", lstAdvancedEvents.Items[index].SubItems[2].Text);
				o.SetBool("event." + name + ".waitend", (lstAdvancedEvents.Items[index].SubItems[3].Text != "No"));
			}
		}
				
		public void SetOption(string name, object value)
		{
			Json jCommand = new Json();
			jCommand["command"].Value = "options.set";
			jCommand["name"].Value = name;
			jCommand["value"].Value = value;
			UiClient.Instance.Command(jCommand);
		}		

		public void RefreshLogPreview()
		{
			TxtLoggingPathComputed.Text = Engine.Logs.GetParseLogFilePaths(txtLogPath.Text);
		}

		public void EnableIde()
		{
			// General
			txtStoragePassword.Enabled = (cboStorageMode.SelectedIndex == 1);
			txtStoragePasswordConfirm.Enabled = (cboStorageMode.SelectedIndex == 1);
			txtStoragePassword.ReadOnly = (txtStoragePassword.Enabled == false);
			txtStoragePasswordConfirm.ReadOnly = (txtStoragePasswordConfirm.Enabled == false);

			// Ui
			cmdUiFontGeneral.Enabled = chkUiFontGeneral.Checked;
			chkUiTrayMinimized.Enabled = chkUiTrayShow.Checked;

			// Protocols
			lstProtocols.Enabled = (chkProtocolsAutomatic.Checked == false);

			// Routes            
			cmdRouteAdd.Enabled = true;
			mnuRoutesAdd.Enabled = cmdRouteAdd.Enabled;
			cmdRouteRemove.Enabled = (lstRoutes.SelectedItems.Count > 0);
			mnuRoutesRemove.Enabled = cmdRouteRemove.Enabled;
			cmdRouteEdit.Enabled = (lstRoutes.SelectedItems.Count == 1);
			mnuRoutesEdit.Enabled = cmdRouteEdit.Enabled;

			// Proxy
			bool proxy = (cboProxyMode.Text != "None");
			bool tor = (cboProxyMode.Text == "Tor");
			lblProxyWhen.Enabled = proxy;
			cboProxyWhen.Enabled = proxy;
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

			cmdAdvancedEventsClear.Enabled = (lstAdvancedEvents.SelectedItems.Count == 1);
			cmdAdvancedEventsEdit.Enabled = (lstAdvancedEvents.SelectedItems.Count == 1);
		}

		public static string RouteDirectionToDescription(string v)
		{
			if (v == "none")
				return "None";
			else if (v == "in")
				return "Inside the VPN tunnel";
			else if (v == "out")
				return "Outside the VPN tunnel";
			else
				return "";
		}

		public static string RouteDescriptionToDirection(string v)
		{
			if (v == RouteDirectionToDescription("none"))
				return "none";
			else if (v == RouteDirectionToDescription("in"))
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

		private void cmdHummingbirdPathBrowse_Click(object sender, EventArgs e)
		{
			string result = GuiUtils.FilePicker();
			if (result != "")
				txtHummingbirdPath.Text = result;
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
			GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["protocols"].Value as string);
		}

		private void lnkProtocolsHelp2_LinkClicked(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["udp_vs_tcp"].Value as string);
		}

		private void lnkProxyTorHelp_LinkClicked(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["tor"].Value as string);
		}

		private void lnkLockHelp_LinkClicked(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["netlock"].Value as string);
		}

		private void lnkAdvancedHelp_LinkClicked(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["advanced"].Value as string);
		}

		private void lnkOpenVpnDirectivesHelp_Click(object sender, EventArgs e)
		{
			GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["directives"].Value as string);
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
			if (cmdRouteEdit.Enabled)
				cmdRouteEdit_Click(sender, e);
		}

		private void lstRoutes_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void cmdOpenVpnDirectivesCustomPathBrowse_Click(object sender, EventArgs e)
		{
			string path = GuiUtils.FilePicker();
			if (path != "")
				txtOpenVpnDirectivesCustomPath.Text = path;
		}

		private void cmdAdvancedEventsClear_Click(object sender, EventArgs e)
		{
			if (lstAdvancedEvents.SelectedItems.Count != 1)
				return;

			while (lstAdvancedEvents.SelectedItems[0].SubItems.Count > 1)
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
			if (Platform.Instance.UninstallDriver("0901"))
			{
				ShowMessageInfo(LanguageManager.GetText("OsDriverUninstallDone"));
				cmdAdvancedUninstallDriverTap.Enabled = false;
			}
		}

		private void cmdAdvancedUninstallDriverWintun_Click(object sender, EventArgs e)
		{
			if (Platform.Instance.UninstallDriver("wintun"))
			{
				ShowMessageInfo(LanguageManager.GetText("OsDriverUninstallDone"));
				cmdAdvancedUninstallDriverWintun.Enabled = false;
			}
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
			ShowMessageInfo(t);
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
					if (UiClient.Instance.MainWindow.NetworkLockKnowledge() == false)
						chkNetLock.Checked = false;
				}
			}
		}

		private void UpdateUiFontGeneral()
		{
			if (chkUiFontGeneral.Checked == false)
				lblUiFontGeneral.Text = GuiUtils.GetSystemFont();
		}

		private void chkUiFontGeneral_CheckedChanged(object sender, EventArgs e)
		{
			UpdateUiFontGeneral();
			EnableIde();
		}

		private void cmdUiFontGeneral_Click(object sender, EventArgs e)
		{
			using (FontDialog dlg = new FontDialog())
			{
				dlg.Font = Form.Skin.FontNormal;
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					lblUiFontGeneral.Text = dlg.Font.Name + ", " + dlg.Font.SizeInPoints.ToString();
				}
			}
		}

		private void chkProtocolsAutomatic_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void cmdResetToDefault_Click(object sender, EventArgs e)
		{
			if (UiClient.Instance.MainWindow.AskYesNo(LanguageManager.GetText("ResetSettingsConfirm")))
			{
				Engine.Instance.Options.ResetAll(false);
				ReadOptions();
				ShowMessageInfo(LanguageManager.GetText("ResetSettingsDone"));
			}
		}

		private void cmdLoggingOpen_Click(object sender, EventArgs e)
		{
			List<string> paths = Engine.Instance.Logs.ParseLogFilePath(txtLogPath.Text);
			foreach (string path in paths)
			{
				if (Platform.Instance.OpenDirectoryInFileManager(path) == false)
					ShowMessageError(LanguageManager.GetText("WindowsSettingsLogsCannotOpenDirectory", path));
			}
		}

		private void chkUiTrayShow_CheckedChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void chkUiFontGeneral_CheckedChanged_1(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void cboStorageMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnableIde();
		}

		private void cmdShellExternalView_Click(object sender, EventArgs e)
		{
			Json rules = Engine.Instance.Options.GetJson("external.rules");
			Engine.Instance.OnShowText("Rules", rules.ToJsonPretty());
		}

		private void cmdShellExternalClear_Click(object sender, EventArgs e)
		{
			Engine.Instance.Options.Set("external.rules", Engine.Instance.Options.Dict["external.rules"].Default);
			ShowMessageInfo("Done.");
		}

		
	}
}
