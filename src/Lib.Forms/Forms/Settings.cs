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
using Eddie.Common;
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

			GuiUtils.FixHeightVs(cboProxyMode, lblProxyType);
			GuiUtils.FixHeightVs(cboProxyWhen, lblProxyWhen);
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
						
			GuiUtils.FixHeightVs(cboNetworkIPv4Mode, lblNetworkIPv4Mode);
			GuiUtils.FixHeightVs(cboNetworkIPv6Mode, lblNetworkIPv6Mode);
			GuiUtils.FixHeightVs(cboNetworkEntryIpLayer, lblNetworkEntryIpLayer);
			GuiUtils.FixHeightVs(cboNetworkEntryInterface, lblNetworkEntryInterface);
			GuiUtils.FixHeightVs(cboOpenVpnRcvbuf, lblOpenVpnRcvbuf);
			GuiUtils.FixHeightVs(cboOpenVpnSndbuf, lblOpenVpnSndbuf);
			GuiUtils.FixHeightVs(chkRouteRemoveDefault, lblRouteRemoveDefault);

			GuiUtils.FixHeightVs(cboLockMode, lblLockMode);
			GuiUtils.FixHeightVs(cboLockIncoming, lblLockIncoming);
			GuiUtils.FixHeightVs(cboLockOutgoing, lblLockOutgoing);

			GuiUtils.FixHeightVs(chkLockAllowPrivate, lblLockAllowPrivate);
			GuiUtils.FixHeightVs(chkLockAllowDHCP, lblLockAllowDHCP);
			GuiUtils.FixHeightVs(chkLockAllowPing, lblLockAllowPing);
			GuiUtils.FixHeightVs(chkLockAllowDNS, lblLockAllowDNS);

			GuiUtils.FixHeightVs(chkExpert, lblExpert);
			GuiUtils.FixHeightVs(chkAdvancedCheckRoute, lblAdvancedCheckRoute);
			GuiUtils.FixHeightVs(cboIpV6, lblIpV6);

			GuiUtils.FixHeightVs(cboAdvancedManifestRefresh, lblAdvancedManifestRefresh);
			GuiUtils.FixHeightVs(cboAdvancedUpdaterChannel, lblAdvancedUpdaterChannel);
			GuiUtils.FixHeightVs(chkAdvancedPingerEnabled, lblAdvancedPingerEnabled);			
			
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

			chkSystemStart.Visible = GuiUtils.IsWindows();
			pnlAdvancedGeneralWindowsOnly.Visible = GuiUtils.IsWindows();
			pnlDnsWindowsOnly.Visible = GuiUtils.IsWindows();
			chkWindowsDebugWorkaround.Visible = GuiUtils.IsWindows();

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
				lblProtocolsAvailable.Text = MessagesFormatter.Format(Messages.WindowsSettingsSomeProtocolsUnavailable, nNotAvailable.ToString());
			}
			else
			{
				lblProtocolsAvailable.Visible = false;
				lblProtocolsAvailable.Text = "";
			}
			
			// UI
			cboUiUnit.Items.Clear();
			cboUiUnit.Items.Add(Messages.WindowsSettingsUiUnit0);
			cboUiUnit.Items.Add(Messages.WindowsSettingsUiUnit1);
			cboUiUnit.Items.Add(Messages.WindowsSettingsUiUnit2);

			// Proxy
			cboProxyMode.Items.Clear();
			cboProxyMode.Items.Add("None");
			cboProxyMode.Items.Add("Http");
			cboProxyMode.Items.Add("Socks");
			cboProxyMode.Items.Add("Tor");
			cboProxyWhen.Items.Clear();
			cboProxyWhen.Items.Add(Messages.WindowsSettingsProxyWhenAlways);
			cboProxyWhen.Items.Add(Messages.WindowsSettingsProxyWhenWeb);
			cboProxyWhen.Items.Add(Messages.WindowsSettingsProxyWhenOpenVPN);
			cboProxyWhen.Items.Add(Messages.WindowsSettingsProxyWhenNone);

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
				cboLockMode.Items.Add(lockPlugin.GetTitleForList());
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

			cboNetworkEntryIpLayer.Items.Clear();
			cboNetworkEntryIpLayer.Items.Add("IPv6, IPv4");
			cboNetworkEntryIpLayer.Items.Add("IPv4, IPv6");
			cboNetworkEntryIpLayer.Items.Add("IPv6 only");
			cboNetworkEntryIpLayer.Items.Add("IPv4 only");

			cboNetworkIPv4Mode.Items.Clear();
			cboNetworkIPv4Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeInAlways);
			cboNetworkIPv4Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeInOrOut);
			cboNetworkIPv4Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeInOrBlock);
			cboNetworkIPv4Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeOut);
			cboNetworkIPv4Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeBlock);

			cboNetworkIPv6Mode.Items.Clear();
			cboNetworkIPv6Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeInAlways);
			cboNetworkIPv6Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeInOrOut);
			cboNetworkIPv6Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeInOrBlock);
			cboNetworkIPv6Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeOut);
			cboNetworkIPv6Mode.Items.Add(Messages.WindowsSettingsNetworkIpModeBlock);

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

			cmdAdvancedUninstallDriver.Visible = Platform.Instance.CanUnInstallDriver();
			cmdAdvancedUninstallDriver.Enabled = (Platform.Instance.GetDriverAvailable() != "");

			// OVPN directives
			cboOpenVpnDirectivesDefaultSkip.Items.Clear();
			cboOpenVpnDirectivesDefaultSkip.Items.Add(Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip1);
			cboOpenVpnDirectivesDefaultSkip.Items.Add(Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip2);

			if(Constants.FeatureIPv6ControlOptions)
			{
				lblNetworkIPv4Mode.Visible = true;
				lblNetworkIPv6Mode.Visible = true;
				cboNetworkIPv4Mode.Visible = true;
				cboNetworkIPv6Mode.Visible = true;
				lblIpV6.Visible = false;
				cboIpV6.Visible = false;
				cboRoutesOtherwise.Visible = false;
				lblRoutesOtherwise.Visible = false;
			}
			else
			{
				lblNetworkIPv4Mode.Visible = false;
				lblNetworkIPv6Mode.Visible = false;
				cboNetworkIPv4Mode.Visible = false;
				cboNetworkIPv6Mode.Visible = false;
				lblIpV6.Visible = true;
				cboIpV6.Visible = true;
				cboRoutesOtherwise.Visible = true;
				lblRoutesOtherwise.Visible = true;
			}

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

			Engine.FormMain.ResetSkinCache();
			Engine.FormMain.ApplySkin();
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

			// General            
			chkConnect.Checked = s.GetBool("connect");
			chkNetLock.Checked = s.GetBool("netlock");
			chkGeneralStartLast.Checked = s.GetBool("servers.startlast");
			chkUiExitConfirm.Checked = s.GetBool("gui.exit_confirm");
			chkOsSingleInstance.Checked = s.GetBool("os.single_instance");

			// Ui
			chkUiSystemNotifications.Checked = s.GetBool("gui.notifications");
			string uiUnit = s.Get("ui.unit");
			if (uiUnit == "bytes")
				cboUiUnit.SelectedIndex = 1;
			else if (uiUnit == "bits")
				cboUiUnit.SelectedIndex = 2;
			else
				cboUiUnit.SelectedIndex = 0;
			chkUiIEC.Checked = s.GetBool("ui.iec");

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
			chkUiStartMinimized.Checked = s.GetBool("gui.start_minimized");
			chkUiTrayShow.Checked = s.GetBool("gui.tray_show");
			chkUiTrayMinimized.Checked = s.GetBool("gui.tray_minimized");

			chkUiSkipProviderManifestFailed.Checked = s.GetBool("ui.skip.provider.manifest.failed");

			// Protocol
			String protocol = s.Get("mode.protocol").ToUpperInvariant();
			int port = s.GetInt("mode.port");
			int entryIP = s.GetInt("mode.alt");
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
			cboProxyMode.Text = s.Get("proxy.mode");
			if (s.Get("proxy.when") == "always")
				cboProxyWhen.Text = Messages.WindowsSettingsProxyWhenAlways;
			else if (s.Get("proxy.when") == "web")
				cboProxyWhen.Text = Messages.WindowsSettingsProxyWhenWeb;
			else if (s.Get("proxy.when") == "openvpn")
				cboProxyWhen.Text = Messages.WindowsSettingsProxyWhenOpenVPN;
			else if (s.Get("proxy.when") == "none")
				cboProxyWhen.Text = Messages.WindowsSettingsProxyWhenNone;
			else
				cboProxyWhen.Text = Messages.WindowsSettingsProxyWhenAlways;
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
				if (IpAddress.IsIP(dnsServer))
					lstDnsServers.Items.Add(new ListViewItem(dnsServer));
			}

			chkDnsForceAllInterfaces.Checked = s.GetBool("windows.dns.force_all_interfaces");
			chkDnsEnsureLock.Checked = s.GetBool("windows.dns.lock");
			chkDnsIgnoreDNS6.Checked = s.GetBool("windows.ipv6.bypass_dns");

			// Networking

			string networkIPv4Mode = s.Get("network.ipv4.mode");
			if (networkIPv4Mode == "in")
				cboNetworkIPv4Mode.Text = Messages.WindowsSettingsNetworkIpModeInAlways;
			else if (networkIPv4Mode == "in-out")
				cboNetworkIPv4Mode.Text = Messages.WindowsSettingsNetworkIpModeInOrOut;
			else if (networkIPv4Mode == "in-block")
				cboNetworkIPv4Mode.Text = Messages.WindowsSettingsNetworkIpModeInOrBlock;
			else if (networkIPv4Mode == "out")
				cboNetworkIPv4Mode.Text = Messages.WindowsSettingsNetworkIpModeOut;
			else if (networkIPv4Mode == "block")
				cboNetworkIPv4Mode.Text = Messages.WindowsSettingsNetworkIpModeBlock;
			else
				cboNetworkIPv4Mode.Text = Messages.WindowsSettingsNetworkIpModeInOrBlock;
			chkNetworkIPv4AutoSwitch.Checked = s.GetBool("network.ipv4.autoswitch");			

			string networkIPv6Mode = s.Get("network.ipv6.mode");
			if (networkIPv6Mode == "in")
				cboNetworkIPv6Mode.Text = Messages.WindowsSettingsNetworkIpModeInAlways;
			else if (networkIPv6Mode == "in-out")
				cboNetworkIPv6Mode.Text = Messages.WindowsSettingsNetworkIpModeInOrOut;
			else if (networkIPv6Mode == "in-block")
				cboNetworkIPv6Mode.Text = Messages.WindowsSettingsNetworkIpModeInOrBlock;
			else if (networkIPv6Mode == "out")
				cboNetworkIPv6Mode.Text = Messages.WindowsSettingsNetworkIpModeOut;
			else if (networkIPv6Mode == "block")
				cboNetworkIPv6Mode.Text = Messages.WindowsSettingsNetworkIpModeBlock;
			else
				cboNetworkIPv6Mode.Text = Messages.WindowsSettingsNetworkIpModeInOrBlock;
			chkNetworkIPv6AutoSwitch.Checked = s.GetBool("network.ipv6.autoswitch");

			string networkEntryIpLayer = s.Get("network.entry.iplayer");
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
			string sNetworkEntryIFace = s.Get("network.entry.iface");
			foreach (KeyValuePair<string, string> kp in m_mapNetworkEntryIFace)
			{
				if (sNetworkEntryIFace == kp.Key)
				{
					cboNetworkEntryInterface.SelectedIndex = iNetworkEntryIFace;
					break;
				}
				iNetworkEntryIFace++;
			}

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

			chkRouteRemoveDefault.Checked = s.GetBool("routes.remove_default");

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
			chkAdvancedSkipAlreadyRun.Checked = s.GetBool("advanced.skip_alreadyrun");
			chkAdvancedProviders.Checked = s.GetBool("advanced.providers");

			chkWindowsTapUp.Checked = s.GetBool("windows.tap_up");
			chkWindowsDhcpSwitch.Checked = s.GetBool("windows.dhcp_disable");
			chkWindowsDisableDriverUpgrade.Checked = s.GetBool("windows.disable_driver_upgrade");
			// chkWindowsIPv6DisableAtOs.Checked = s.GetBool("windows.ipv6.os_disable"); // Removed in 2.14, in W10 require reboot
			chkWindowsDebugWorkaround.Checked = s.GetBool("windows.workarounds");
			chkWindowsSshPlinkForce.Checked = s.GetBool("windows.ssh.plink.force");

			txtExePath.Text = s.Get("tools.openvpn.path");

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

			string updaterChannel = s.Get("updater.channel");
			if (updaterChannel == "stable")
				cboAdvancedUpdaterChannel.Text = "Stable";
			else if (updaterChannel == "beta")
				cboAdvancedUpdaterChannel.Text = "Beta";
			else if (updaterChannel == "none")
				cboAdvancedUpdaterChannel.Text = "None";
			else
				cboAdvancedUpdaterChannel.Text = "Stable";


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
						cboLockMode.Text = lockPlugin.GetTitleForList();
				}
			}
			string lockIncoming = s.Get("netlock.incoming");
			cboLockIncoming.Text = "Block";
			if (lockIncoming == "allow")
				cboLockIncoming.Text = "Allow";
			string lockOutgoing = s.Get("netlock.outgoing");
			cboLockOutgoing.Text = "Block";
			if (lockOutgoing == "allow")
				cboLockOutgoing.Text = "Allow";
			chkLockAllowPrivate.Checked = s.GetBool("netlock.allow_private");
			chkLockAllowDHCP.Checked = s.GetBool("netlock.allow_dhcp");
			chkLockAllowPing.Checked = s.GetBool("netlock.allow_ping");
			chkLockAllowDNS.Checked = s.GetBool("netlock.allow_dns");
			txtLockAllowedIPS.Text = s.Get("netlock.allowed_ips");

			// Advanced - Logging
			chkLoggingEnabled.Checked = s.GetBool("log.file.enabled");
			txtLogPath.Text = s.Get("log.file.path");
			chkLogLevelDebug.Checked = s.GetBool("log.level.debug");

			// Advanced - OVPN Directives
			cboOpenVpnDirectivesDefaultSkip.SelectedIndex = (s.GetBool("openvpn.skip_defaults") ? 1 : 0);
			txtOpenVpnDirectivesBase.Text = s.Get("openvpn.directives");
			txtOpenVpnDirectivesCustom.Text = s.Get("openvpn.custom");
			txtOpenVpnDirectivesCustomPath.Text = s.Get("openvpn.directives.path");

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
			if ((RouteDescriptionToDirection(cboRoutesOtherwise.Text) == "out") && (lstRoutes.Items.Count == 0))
			{
				if (Engine.FormMain.AskYesNo(Messages.WindowsSettingsRouteOutEmptyList) == false)
					return false;
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
					if (Engine.FormMain.AskYesNo(Messages.WindowsSettingsRouteWithHostname) == false)
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
			s.SetBool("servers.startlast", chkGeneralStartLast.Checked);
			s.SetBool("gui.exit_confirm", chkUiExitConfirm.Checked);
			s.SetBool("os.single_instance", chkOsSingleInstance.Checked);

			// Ui
			s.SetBool("gui.notifications", chkUiSystemNotifications.Checked);
			string uiUnit = "";
			if (cboUiUnit.SelectedIndex == 1)
				uiUnit = "bytes";
			else if (cboUiUnit.SelectedIndex == 2)
				uiUnit = "bits";
			s.Set("ui.unit", uiUnit);
			s.SetBool("ui.iec", chkUiIEC.Checked);

			if (chkUiFontGeneral.Checked)
			{
				int posComma = lblUiFontGeneral.Text.IndexOf(",");
				s.Set("gui.font.normal.name", lblUiFontGeneral.Text.Substring(0, posComma));
				s.Set("gui.font.normal.size", lblUiFontGeneral.Text.Substring(posComma + 1));
			}
			else
			{
				s.Set("gui.font.normal.name", "");
				s.SetFloat("gui.font.normal.size", 0);
			}
			s.SetBool("gui.start_minimized", chkUiStartMinimized.Checked);
			s.SetBool("gui.tray_show", chkUiTrayShow.Checked);
			s.SetBool("gui.tray_minimized", chkUiTrayShow.Checked && chkUiTrayMinimized.Checked);

			s.SetBool("ui.skip.provider.manifest.failed", chkUiSkipProviderManifestFailed.Checked);

			// Protocols
			if (lstProtocols.Items.Count == 0) // Occur if AirVPN provider is disabled
				chkProtocolsAutomatic.Checked = true;

			if (chkProtocolsAutomatic.Checked)
			{
				s.Set("mode.protocol", "AUTO");
				s.SetInt("mode.port", 443);
				s.SetInt("mode.alt", 0);
			}
			else if (lstProtocols.SelectedItems.Count == 1)
			{
				Controls.ListViewItemProtocol item = lstProtocols.SelectedItems[0] as Controls.ListViewItemProtocol;

				s.Set("mode.protocol", item.Mode.Protocol);
				s.SetInt("mode.port", item.Mode.Port);
				s.SetInt("mode.alt", item.Mode.EntryIndex);
			}
			else
			{
				s.Set("mode.protocol", "AUTO");
				s.SetInt("mode.port", 443);
				s.SetInt("mode.alt", 0);
			}

			// Proxy
			s.Set("proxy.mode", cboProxyMode.Text);
			if (cboProxyWhen.Text == Messages.WindowsSettingsProxyWhenAlways)
				s.Set("proxy.when", "always");
			else if (cboProxyWhen.Text == Messages.WindowsSettingsProxyWhenWeb)
				s.Set("proxy.when", "web");
			else if (cboProxyWhen.Text == Messages.WindowsSettingsProxyWhenOpenVPN)
				s.Set("proxy.when", "openvpn");
			else if (cboProxyWhen.Text == Messages.WindowsSettingsProxyWhenNone)
				s.Set("proxy.when", "none");
			else
				s.Set("proxy.when", "always");
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

			// DNS

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

			s.SetBool("windows.dns.force_all_interfaces", chkDnsForceAllInterfaces.Checked);
			s.SetBool("windows.dns.lock", chkDnsEnsureLock.Checked);
			s.SetBool("windows.ipv6.bypass_dns", chkDnsIgnoreDNS6.Checked);

			// Networking
			string networkIPv4Mode = cboNetworkIPv4Mode.Text;
			if (networkIPv4Mode == Messages.WindowsSettingsNetworkIpModeInAlways)
				s.Set("network.ipv4.mode", "in");
			else if (networkIPv4Mode == Messages.WindowsSettingsNetworkIpModeInOrOut)
				s.Set("network.ipv4.mode", "in-out");
			else if (networkIPv4Mode == Messages.WindowsSettingsNetworkIpModeInOrBlock)
				s.Set("network.ipv4.mode", "in-block");
			else if (networkIPv4Mode == Messages.WindowsSettingsNetworkIpModeOut)
				s.Set("network.ipv4.mode", "out");
			else if (networkIPv4Mode == Messages.WindowsSettingsNetworkIpModeBlock)
				s.Set("network.ipv4.mode", "block");
			else
				s.Set("network.ipv4.mode", "in");
			s.SetBool("network.ipv4.autoswitch", chkNetworkIPv4AutoSwitch.Checked);

			string networkIPv6Mode = cboNetworkIPv6Mode.Text;
			if (networkIPv6Mode == Messages.WindowsSettingsNetworkIpModeInAlways)
				s.Set("network.ipv6.mode", "in");
			else if (networkIPv6Mode == Messages.WindowsSettingsNetworkIpModeInOrOut)
				s.Set("network.ipv6.mode", "in-out");
			else if (networkIPv6Mode == Messages.WindowsSettingsNetworkIpModeInOrBlock)
				s.Set("network.ipv6.mode", "in-block");
			else if (networkIPv6Mode == Messages.WindowsSettingsNetworkIpModeOut)
				s.Set("network.ipv6.mode", "out");
			else if (networkIPv6Mode == Messages.WindowsSettingsNetworkIpModeBlock)
				s.Set("network.ipv6.mode", "block");
			else
				s.Set("network.ipv6.mode", "in-block");
			s.SetBool("network.ipv6.autoswitch", chkNetworkIPv6AutoSwitch.Checked);

			string networkEntryIpLayer = cboNetworkEntryIpLayer.Text;
			if (networkEntryIpLayer == "IPv6, IPv4")
				s.Set("network.entry.iplayer", "ipv6-ipv4");
			else if (networkEntryIpLayer == "IPv4, IPv6")
				s.Set("network.entry.iplayer", "ipv4-ipv6");
			else if (networkEntryIpLayer == "IPv6 only")
				s.Set("network.entry.iplayer", "ipv6-only");
			else if (networkEntryIpLayer == "IPv4 only")
				s.Set("network.entry.iplayer", "ipv4-only");
			else
				s.Set("network.entry.iplayer", "ipv6-ipv4");

			string networkEntryIFace = cboNetworkEntryInterface.Text;
			foreach (KeyValuePair<string, string> kp in m_mapNetworkEntryIFace)
			{
				if (kp.Value == networkEntryIFace)
				{
					s.Set("network.entry.iface", kp.Key);
					break;
				}
			}

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
			if (openVpnRcvBufIndex == 0)
				s.SetInt("openvpn.rcvbuf", -2);
			else if (openVpnRcvBufIndex == 1)
				s.SetInt("openvpn.rcvbuf", -1);
			else if (openVpnRcvBufIndex == 2)
				s.SetInt("openvpn.rcvbuf", 1024 * 8);
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

			s.SetBool("routes.remove_default", chkRouteRemoveDefault.Checked);

			// Advanced - General
			s.SetBool("advanced.expert", chkExpert.Checked);
			s.SetBool("advanced.check.route", chkAdvancedCheckRoute.Checked);

			string ipV6 = cboIpV6.Text;
			if (ipV6 == "None")
				s.Set("ipv6.mode", "none");
			else if (ipV6 == "Disable")
				s.Set("ipv6.mode", "disable");
			else
				s.Set("ipv6.mode", "none");

			s.SetBool("pinger.enabled", chkAdvancedPingerEnabled.Checked);
			s.SetBool("advanced.skip_alreadyrun", chkAdvancedSkipAlreadyRun.Checked);
			s.SetBool("advanced.providers", chkAdvancedProviders.Checked);
			
			s.SetBool("windows.tap_up", chkWindowsTapUp.Checked);
			s.SetBool("windows.dhcp_disable", chkWindowsDhcpSwitch.Checked);
			s.SetBool("windows.disable_driver_upgrade", chkWindowsDisableDriverUpgrade.Checked);
			//s.SetBool("windows.ipv6.os_disable", chkWindowsIPv6DisableAtOs.Checked); // Removed in 2.14, in W10 require reboot
			s.SetBool("windows.workarounds", chkWindowsDebugWorkaround.Checked);
			s.SetBool("windows.ssh.plink.force", chkWindowsSshPlinkForce.Checked);

			SetOption("tools.openvpn.path", txtExePath.Text);

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

			string updaterChannel = cboAdvancedUpdaterChannel.Text;
			if (updaterChannel == "Stable")
				s.Set("updater.channel", "stable");
			else if (updaterChannel == "Beta")
				s.Set("updater.channel", "beta");
			else if (updaterChannel == "None")
				s.Set("updater.channel", "none");
			else
				s.Set("updater.channel", "stable");

			// Advanced - Lock
			string lockMode = cboLockMode.Text;
			s.Set("netlock.mode", "none");
			if (lockMode == "Automatic")
				s.Set("netlock.mode", "auto");
			else
			{
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				{
					if (lockPlugin.GetTitleForList() == lockMode)
						s.Set("netlock.mode", lockPlugin.GetCode());
				}
			}
			string lockIncoming = cboLockIncoming.Text;
			if (lockIncoming == "Allow")
				s.Set("netlock.incoming", "allow");
			else
				s.Set("netlock.incoming", "block");
			string lockOutgoing = cboLockOutgoing.Text;
			if (lockOutgoing == "Allow")
				s.Set("netlock.outgoing", "allow");
			else
				s.Set("netlock.outgoing", "block");
			s.SetBool("netlock.allow_private", chkLockAllowPrivate.Checked);
			s.SetBool("netlock.allow_dhcp", chkLockAllowDHCP.Checked);
			s.SetBool("netlock.allow_ping", chkLockAllowPing.Checked);
			s.SetBool("netlock.allow_dns", chkLockAllowDNS.Checked);
			s.Set("netlock.allowed_ips", txtLockAllowedIPS.Text);

			// Advanced - Networking

			// Advanced - Logging
			s.SetBool("log.file.enabled", chkLoggingEnabled.Checked);
			s.Set("log.file.path", txtLogPath.Text);
			s.SetBool("log.level.debug", chkLogLevelDebug.Checked);

			// Advanced - OVPN Directives
			s.Set("openvpn.directives", txtOpenVpnDirectivesBase.Text);
			s.Set("openvpn.custom", txtOpenVpnDirectivesCustom.Text);
			s.Set("openvpn.directives.path", txtOpenVpnDirectivesCustomPath.Text);
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
				
		public void SetOption(string name, object value)
		{
			Json jCommand = new Json();
			jCommand["command"].Value = "set_option";
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

			// Lock
			lblLockRoutingOutWarning.Visible = (cboRoutesOtherwise.Text == Settings.RouteDirectionToDescription("out"));

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
			Platform.Instance.UnInstallDriver();

			cmdAdvancedUninstallDriver.Enabled = (Platform.Instance.GetDriverAvailable() != "");

			ShowMessageInfo(Messages.OsDriverUninstallDone);
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
					if (Engine.FormMain.NetworkLockKnowledge() == false)
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
			if (Engine.FormMain.AskYesNo(Messages.ResetSettingsConfirm))
			{
				Engine.Instance.Storage.ResetAll(false);
				ReadOptions();
				ShowMessageInfo(Messages.ResetSettingsDone);
			}
		}

		private void cmdLoggingOpen_Click(object sender, EventArgs e)
		{
			List<string> paths = Engine.Instance.Logs.ParseLogFilePath(txtLogPath.Text);
			foreach (string path in paths)
			{
				if (Platform.Instance.OpenDirectoryInFileManager(path) == false)
					ShowMessageError(MessagesFormatter.Format(Messages.WindowsSettingsLogsCannotOpenDirectory, path));
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
	}
}
