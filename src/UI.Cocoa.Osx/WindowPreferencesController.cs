// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
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
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Core;

namespace Eddie.UI.Osx
{
	public partial class WindowPreferencesController : MonoMac.AppKit.NSWindowController
	{
		private TableTabsController TableTabsController;
		private TableProtocolsController TableProtocolsController;
		private TableRoutingController TableRoutingController;
		private TableDnsServersController TableDnsServersController;
		private TableAdvancedEventsController TableAdvancedEventsController;

		#region Constructors
		// Called when created from unmanaged code
		public WindowPreferencesController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowPreferencesController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public WindowPreferencesController () : base ("WindowPreferences")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowPreferences Window {
			get {
				return (WindowPreferences)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib ();

			Window.Title = Constants.Name + " - " + Messages.WindowsSettingsTitle;

			TableTabsController = new TableTabsController (TableTabs, TabMain);

			ChkNetLock.Activated += (object sender, EventArgs e) => {
				if (GuiUtils.GetCheck (ChkNetLock)) {
					if ((Engine.Instance as UI.Osx.Engine).MainWindow.NetworkLockKnowledge () == false)
						GuiUtils.SetCheck (ChkNetLock, false);
				}
			};

			TableRoutes.Delegate = new TableRoutingDelegate (this);

			LblDnsServers.StringValue = Messages.WindowsSettingsDnsServers;
			TableDnsServers.Delegate = new TableDnsServersDelegate (this);

			TableAdvancedEvents.Delegate = new TableAdvancedEventsDelegate (this);

			LblLoggingHelp.StringValue = Messages.WindowsSettingsLoggingHelp;

			TableRoutingController = new TableRoutingController (this.TableRoutes);
			TableDnsServersController = new TableDnsServersController (this.TableDnsServers);
			TableAdvancedEventsController = new TableAdvancedEventsController (this.TableAdvancedEvents);

			CmdSave.Activated += (object sender, EventArgs e) => {
				if(Check())
				{
					SaveOptions ();
					Close ();
				}
			};

			CmdCancel.Activated += (object sender, EventArgs e) => {
				Close ();
			};

			// General

			CmdGeneralTos.Activated += (object sender, EventArgs e) => {
				WindowTosController tos = new WindowTosController ();
				tos.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow (tos.Window);
				tos.Window.Close ();
			};

			CmdResetToDefault.Activated += (object sender, EventArgs e) => {
				if(Engine.Instance.OnAskYesNo(Messages.ResetSettingsConfirm))
				{
					Engine.Instance.Storage.ResetAll(false);
					ReadOptions();
					Engine.Instance.OnMessageInfo(Messages.ResetSettingsDone);
				}
			};

			// UI

			CboUiUnit.RemoveAllItems ();
			CboUiUnit.AddItem (Messages.WindowsSettingsUiUnit0);
			CboUiUnit.AddItem (Messages.WindowsSettingsUiUnit1);
			CboUiUnit.AddItem (Messages.WindowsSettingsUiUnit2);

			// Protocols

			CmdProtocolsHelp1.Activated += (object sender, EventArgs e) => {
				Engine.Instance.Command ("ui.show.docs.protocols");
			};

			CmdProtocolsHelp2.Activated += (object sender, EventArgs e) => {
				Engine.Instance.Command ("ui.show.docs.udp_vs_tcp");
			};

			ChkProtocolsAutomatic.Activated += (object sender, EventArgs e) => {
				EnableIde();
			};

			TableProtocols.Delegate = new TableProtocolsDelegate (this);
			TableProtocolsController = new TableProtocolsController (this.TableProtocols);

			// Proxy
			CboProxyType.RemoveAllItems ();
			CboProxyType.AddItem ("None");
			CboProxyType.AddItem ("Detect");
			CboProxyType.AddItem ("Http");
			CboProxyType.AddItem ("Socks");
			CboProxyType.AddItem ("Tor");

			CmdProxyTorHelp.Activated += (object sender, EventArgs e) => {
				Engine.Instance.Command ("ui.show.docs.tor");
			};
			CboProxyType.Activated += (object sender, EventArgs e) => {
				EnableIde();

				if(GuiUtils.GetSelected(CboProxyType) == "Tor")
					TxtProxyPort.StringValue = "9150";
				else
					TxtProxyPort.StringValue = "8080";
			};
			CboProxyAuthentication.Activated += (object sender, EventArgs e) => {
				EnableIde();
			};
			CmdProxyTorTest.Activated += (object sender, EventArgs e) => {
				string result = TorControl.Test(TxtProxyHost.StringValue, Conversions.ToInt32(TxtProxyTorControlPort.StringValue), TxtProxyTorControlPassword.StringValue);
				GuiUtils.MessageBox(result);
			};

			// Routes
			CboRoutesOtherwise.RemoveAllItems();
			CboRoutesOtherwise.AddItem(RouteDirectionToDescription("in"));
			CboRoutesOtherwise.AddItem(RouteDirectionToDescription("out"));
			CboRoutesOtherwise.Activated += (object sender, EventArgs e) => {
				EnableIde ();
			};

			TableRoutes.DoubleClick += (object sender, EventArgs e) => {
				RouteEdit();
			};

			CmdRouteAdd.Activated += (object sender, EventArgs e) => {
				RouteAdd();
			};

			CmdRouteRemove.Activated += (object sender, EventArgs e) => {
				RouteRemove();
			};

			CmdRouteEdit.Activated += (object sender, EventArgs e) => {
				RouteEdit();
			};

			// Advanced - General

			CmdAdvancedHelp.Activated += (object sender, EventArgs e) => {
				Engine.Instance.Command ("ui.show.docs.advanced");
			};

			CboIpV6.RemoveAllItems ();
			CboIpV6.AddItem ("None");
			CboIpV6.AddItem ("Disable");

			CboAdvancedManifestRefresh.RemoveAllItems ();
			CboAdvancedManifestRefresh.AddItem ("Automatic");
			CboAdvancedManifestRefresh.AddItem ("Never");
			CboAdvancedManifestRefresh.AddItem ("Every minute");
			CboAdvancedManifestRefresh.AddItem ("Every ten minute");
			CboAdvancedManifestRefresh.AddItem ("Every one hour");

			LblOpenVpnRcvBuf.StringValue = Messages.WindowsSettingsOpenVpnRcvBuf + ":";
			LblOpenVpnSndBuf.StringValue = Messages.WindowsSettingsOpenVpnSndBuf + ":";
			CboOpenVpnRcvBuf.RemoveAllItems();
			CboOpenVpnRcvBuf.AddItem(Messages.Automatic);
			CboOpenVpnRcvBuf.AddItem(Messages.WindowsSettingsOpenVpnDefault);
			CboOpenVpnRcvBuf.AddItem("8 KB");
			CboOpenVpnRcvBuf.AddItem("16 KB");
			CboOpenVpnRcvBuf.AddItem("32 KB");
			CboOpenVpnRcvBuf.AddItem("64 KB");
			CboOpenVpnRcvBuf.AddItem("128 KB");
			CboOpenVpnRcvBuf.AddItem("256 KB");
			CboOpenVpnRcvBuf.AddItem("512 KB");
			CboOpenVpnSndBuf.RemoveAllItems();
			CboOpenVpnSndBuf.AddItem(Messages.Automatic);
			CboOpenVpnSndBuf.AddItem(Messages.WindowsSettingsOpenVpnDefault);
			CboOpenVpnSndBuf.AddItem("8 KB");
			CboOpenVpnSndBuf.AddItem("16 KB");
			CboOpenVpnSndBuf.AddItem("32 KB");
			CboOpenVpnSndBuf.AddItem("64 KB");
			CboOpenVpnSndBuf.AddItem("128 KB");
			CboOpenVpnSndBuf.AddItem("256 KB");
			CboOpenVpnSndBuf.AddItem("512 KB");

			CmdAdvancedOpenVpnPath.Activated += (object sender, EventArgs e) => {
				GuiUtils.SelectFile(this.Window, TxtAdvancedOpenVpnPath);
			};



			// Advanced - DNS
			TableDnsServers.DoubleClick += (object sender, EventArgs e) =>
			{
				DnsServersEdit();
			};

			CmdDnsAdd.Activated += (object sender, EventArgs e) =>
			{
				DnsServersAdd();
			};

			CmdDnsRemove.Activated += (object sender, EventArgs e) =>
			{
				DnsServersRemove();
			};

			CmdDnsEdit.Activated += (object sender, EventArgs e) =>
			{
				DnsServersEdit();
			};

			// Advanced - Net Lock
			CmdLockHelp.Activated += (object sender, EventArgs e) => {
				Engine.Instance.Command ("ui.show.docs.lock");
			};
			CboLockMode.RemoveAllItems ();
			CboLockMode.AddItem ("None");
			CboLockMode.AddItem ("Automatic");
			foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes) {
				CboLockMode.AddItem (lockPlugin.GetName ());
			}

			LblRoutesNetworkLockWarning.StringValue = Messages.WindowsSettingsRouteLockHelp;
			LblLockRoutingOutWarning.StringValue = Messages.NetworkLockNotAvailableWithRouteOut;

			// Advanced - Logging

			TxtLoggingPath.Changed += (object sender, EventArgs e) => {
				RefreshLogPreview();
			};

			// Directives
			CboOpenVpnDirectivesSkipDefault.RemoveAllItems ();
			CboOpenVpnDirectivesSkipDefault.AddItem (Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip1);
			CboOpenVpnDirectivesSkipDefault.AddItem (Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip2);
			CmdOpenVpnDirectivesHelp.Activated += (object sender, EventArgs e) => {
				Engine.Instance.Command ("ui.show.docs.directives");
			};

			// Advanced - Events

			TableAdvancedEvents.DoubleClick += (object sender, EventArgs e) => {
				AdvancedEventEdit();
			};

			CmdAdvancedEventsEdit.Activated += (object sender, EventArgs e) => {
				AdvancedEventEdit();
			};

			CmdAdvancedEventsClear.Activated += (object sender, EventArgs e) => {
				AdvancedEventClear();

			};


			ReadOptions ();

			EnableIde ();

			RefreshLogPreview ();

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
			if (v == RouteDirectionToDescription ("in"))
				return "in";
			else if (v == RouteDirectionToDescription ("out"))
				return "out";
			else
				return "";
		}

		void RouteAdd()
		{
			TableRoutingControllerItem item = new TableRoutingControllerItem();
			item.Ip = "";
			item.Icon = "out";
			item.Action = "out";
			item.Notes = "";

			WindowPreferencesRouteController.Item = item;
			WindowPreferencesRouteController dlg = new WindowPreferencesRouteController ();
			dlg.Window.ReleasedWhenClosed = true;
			NSApplication.SharedApplication.RunModalForWindow (dlg.Window);
			dlg.Window.Close ();

			if(dlg.Accepted) {
				TableRoutingController.Items.Add(item);
				TableRoutingController.RefreshUI ();
			}

			this.EnableIde();
		}

		void RouteEdit()
		{
			int i = TableRoutes.SelectedRow;
			if (i != -1) {
				TableRoutingControllerItem item = TableRoutingController.Items [i];
			
				WindowPreferencesRouteController.Item = item;
				WindowPreferencesRouteController dlg = new WindowPreferencesRouteController ();
				dlg.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow (dlg.Window);
				dlg.Window.Close ();

				TableRoutingController.RefreshUI ();
				this.EnableIde ();
			}
		}

		void RouteRemove()
		{
			int i = TableRoutes.SelectedRow;
			if(i != -1)
			{
				TableRoutingController.Items.RemoveAt (i);
				TableRoutingController.RefreshUI ();
				this.EnableIde ();
			}
		}

		void DnsServersAdd()
		{
			WindowPreferencesIpController.Ip = "";
			WindowPreferencesIpController dlg = new WindowPreferencesIpController();
			dlg.Window.ReleasedWhenClosed = true;
			NSApplication.SharedApplication.RunModalForWindow(dlg.Window);
			dlg.Window.Close();

			if (dlg.Accepted)
			{
				TableDnsServersController.Add(WindowPreferencesIpController.Ip);
				TableDnsServersController.RefreshUI();
			}

			this.EnableIde();
		}

		void DnsServersRemove()
		{
			int i = TableDnsServers.SelectedRow;
			if (i != -1)
			{
				TableDnsServersController.RemoveAt(i);
				TableDnsServersController.RefreshUI();
				this.EnableIde();
			}
		}

		void DnsServersEdit()
		{
			int i = TableDnsServers.SelectedRow;
			if (i != -1)
			{
				string dns = TableDnsServersController.Get(i);

				WindowPreferencesIpController.Ip = dns;
				WindowPreferencesIpController dlg = new WindowPreferencesIpController();
				dlg.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow(dlg.Window);
				dlg.Window.Close();

				if (dlg.Accepted)
				{
					TableDnsServersController.Set (i, WindowPreferencesIpController.Ip);
					TableDnsServersController.RefreshUI();
				}

				this.EnableIde();
			}
		}

		void AdvancedEventEdit()
		{
			int index = TableAdvancedEvents.SelectedRow;

			WindowPreferencesEventController.Item = TableAdvancedEventsController.Items [index];
			WindowPreferencesEventController dlg = new WindowPreferencesEventController ();
			dlg.Window.ReleasedWhenClosed = true;

			NSApplication.SharedApplication.RunModalForWindow (dlg.Window);
			dlg.Window.Close ();

			TableAdvancedEventsController.RefreshUI ();
			this.EnableIde ();
		}

		void AdvancedEventClear()
		{
			int index = TableAdvancedEvents.SelectedRow;
			if(index != -1)
			{
				TableAdvancedEventsController.Items[index].Filename = "";
				TableAdvancedEventsController.Items[index].Arguments = "";
				TableAdvancedEventsController.Items[index].WaitEnd = true;
				TableAdvancedEventsController.RefreshUI();
			}
			TableAdvancedEventsController.RefreshUI ();
			this.EnableIde ();
		}

		void RefreshLogPreview()
		{
			TxtLoggingComputedPath.StringValue = Engine.Instance.Logs.GetParseLogFilePaths (TxtLoggingPath.StringValue);
		}

		void ReadOptionsEvent(string name, int index)
		{
			Storage s = Engine.Instance.Storage;

			string filename = s.Get("event." + name + ".filename");
			if (filename != "") {
				TableAdvancedEventsController.Items [index].Filename = filename;
				TableAdvancedEventsController.Items [index].Arguments = s.Get("event." + name + ".arguments");
				TableAdvancedEventsController.Items [index].WaitEnd = s.GetBool("event." + name + ".waitend");
				TableAdvancedEventsController.RefreshUI ();
			}
		}

		void SaveOptionsEvent(string name, int index)
		{
			Storage s = Engine.Instance.Storage;

			TableAdvancedEventsControllerItem i = TableAdvancedEventsController.Items [index];
			s.Set ("event." + name + ".filename", i.Filename);
			s.Set ("event." + name + ".arguments", i.Arguments);
			s.SetBool ("event." + name + ".waitend", i.WaitEnd);
		}

		void ReadOptions()
		{
			Storage s = Engine.Instance.Storage;

			// General

			GuiUtils.SetCheck (ChkConnect, s.GetBool ("connect"));
			GuiUtils.SetCheck (ChkNetLock, s.GetBool ("netlock")); 

			GuiUtils.SetCheck (ChkGeneralStartLast, s.GetBool("servers.startlast"));
			GuiUtils.SetCheck (ChkGeneralOsxVisible, s.GetBool ("gui.osx.visible"));
			// GuiUtils.SetCheck (ChkGeneralOsxDock, s.GetBool ("gui.osx.dock")); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			GuiUtils.SetCheck (ChkGeneralOsxNotifications, s.GetBool ("gui.osx.notifications"));
			GuiUtils.SetCheck (ChkUiSystemBarShowInfo, s.GetBool ("gui.osx.sysbar.show_info"));
			GuiUtils.SetCheck (ChkUiSystemBarShowSpeed, s.GetBool ("gui.osx.sysbar.show_speed"));
			GuiUtils.SetCheck (ChkUiSystemBarShowServer, s.GetBool ("gui.osx.sysbar.show_server"));

			GuiUtils.SetCheck (ChkExitConfirm, s.GetBool("gui.exit_confirm"));

			// UI
			string uiUnit = s.Get ("ui.unit");
			if (uiUnit == "bytes")
				GuiUtils.SetSelected (CboUiUnit, Messages.WindowsSettingsUiUnit1);
			else if (uiUnit == "bits")
				GuiUtils.SetSelected (CboUiUnit, Messages.WindowsSettingsUiUnit2);
			else
				GuiUtils.SetSelected (CboUiUnit, Messages.WindowsSettingsUiUnit0);

			/*
			string interfaceMode = GuiUtils.InterfaceColorMode ();
			if (interfaceMode == "Dark")
				GuiUtils.SetSelected (CboGeneralOsxInterfaceStyle,"Dark");
			else
				GuiUtils.SetSelected (CboGeneralOsxInterfaceStyle,"Default");
			*/

			// Protocols
			String protocol = s.Get ("mode.protocol").ToUpperInvariant ();
			int port = s.GetInt ("mode.port");
			int alternate = s.GetInt ("mode.alt");
			if (protocol == "AUTO") {
				GuiUtils.SetCheck (ChkProtocolsAutomatic, true);
			} else {
				bool found = false;

				int iRow = 0;
				foreach (TableProtocolsControllerItem itemProtocol in TableProtocolsController.Items) {
					if ((itemProtocol.Protocol == protocol) &&
					   (itemProtocol.Port == port) &&
					   (itemProtocol.Entry == alternate)) {
						found = true;
						TableProtocols.SelectRow (iRow, false);
						TableProtocols.ScrollRowToVisible (iRow);
						break;
					}
					iRow++;
				}

				if(found == false)
					GuiUtils.SetCheck (ChkProtocolsAutomatic, true);
				else
					GuiUtils.SetCheck (ChkProtocolsAutomatic, false);
			}

			// Proxy

			GuiUtils.SetSelected (CboProxyType, s.Get("proxy.mode"));
			TxtProxyHost.StringValue = s.Get ("proxy.host");
			TxtProxyPort.StringValue = s.Get ("proxy.port");
			GuiUtils.SetSelected (CboProxyAuthentication, s.Get ("proxy.auth"));
			TxtProxyLogin.StringValue = s.Get ("proxy.login");
			TxtProxyPassword.StringValue = s.Get ("proxy.password");
			TxtProxyTorControlPort.StringValue = s.Get ("proxy.tor.control.port");
			TxtProxyTorControlPassword.StringValue = s.Get ("proxy.tor.control.password");

			// Routes
			GuiUtils.SetSelected(CboRoutesOtherwise, RouteDirectionToDescription(s.Get("routes.default")));

			string routes = s.Get ("routes.custom");
			string[] routes2 = routes.Split (';');
			foreach (string route in routes2) {
				string[] routeEntries = route.Split (',');
				if (routeEntries.Length < 2)
					continue;

				TableRoutingControllerItem item = new TableRoutingControllerItem ();
				item.Ip = routeEntries [0];
				item.Action = routeEntries [1];
				item.Icon = routeEntries [1];
				if(routeEntries.Length == 3)
					item.Notes = routeEntries [2];
				TableRoutingController.Items.Add (item);
			}

			TableRoutingController.RefreshUI();

			// Advanced - General

			GuiUtils.SetCheck (ChkAdvancedExpertMode, s.GetBool ("advanced.expert"));
			GuiUtils.SetCheck (ChkAdvancedCheckRoute, s.GetBool ("advanced.check.route"));
			string ipV6Mode = s.Get ("ipv6.mode");
			if (ipV6Mode == "none")
				GuiUtils.SetSelected (CboIpV6, "None");
			else if (ipV6Mode == "disable")
				GuiUtils.SetSelected (CboIpV6, "Disable");
			else
				GuiUtils.SetSelected (CboIpV6, "None");

			GuiUtils.SetCheck (ChkAdvancedPingerEnabled, s.GetBool ("pinger.enabled"));
			GuiUtils.SetCheck (ChkRouteRemoveDefaultGateway, s.GetBool("routes.remove_default"));
			
			TxtAdvancedOpenVpnPath.StringValue = s.Get ("executables.openvpn");

			int manifestRefresh = s.GetInt("advanced.manifest.refresh");
			if (manifestRefresh == 60)
				GuiUtils.SetSelected(CboAdvancedManifestRefresh, "Every one hour");
			else if (manifestRefresh == 10)
				GuiUtils.SetSelected(CboAdvancedManifestRefresh, "Every ten minute");
			else if (manifestRefresh == 1)
				GuiUtils.SetSelected(CboAdvancedManifestRefresh, "Every minute");
			else if (manifestRefresh == 0)
				GuiUtils.SetSelected(CboAdvancedManifestRefresh, "Never");
			else
				GuiUtils.SetSelected(CboAdvancedManifestRefresh, "Automatic");

			int openVpnSndBuf = s.GetInt("openvpn.sndbuf");
			if (openVpnSndBuf == -2)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, Messages.Automatic);
			else if (openVpnSndBuf == -1)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, Messages.WindowsSettingsOpenVpnDefault);
			else if (openVpnSndBuf == 1024 * 8)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, "8 KB");
			else if (openVpnSndBuf == 1024 * 16)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, "16 KB");
			else if (openVpnSndBuf == 1024 * 32)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, "32 KB");
			else if (openVpnSndBuf == 1024 * 64)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, "64 KB");
			else if (openVpnSndBuf == 1024 * 128)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, "128 KB");
			else if (openVpnSndBuf == 1024 * 256)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, "256 KB");
			else if (openVpnSndBuf == 1024 * 512)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, "512 KB");

			int openVpnRcvBuf = s.GetInt("openvpn.rcvbuf");
			if (openVpnRcvBuf == -2)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, Messages.Automatic);
			else if (openVpnRcvBuf == -1)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, Messages.WindowsSettingsOpenVpnDefault);
			else if (openVpnRcvBuf == 1024 * 8)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, "8 KB");
			else if (openVpnRcvBuf == 1024 * 16)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, "16 KB");
			else if (openVpnRcvBuf == 1024 * 32)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, "32 KB");
			else if (openVpnRcvBuf == 1024 * 64)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, "64 KB");
			else if (openVpnRcvBuf == 1024 * 128)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, "128 KB");
			else if (openVpnRcvBuf == 1024 * 256)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, "256 KB");
			else if (openVpnRcvBuf == 1024 * 512)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, "512 KB");

			// Advanced - DNS

			string dnsMode = s.Get ("dns.mode");
			if (dnsMode == "none")
				GuiUtils.SetSelected (CboDnsSwitchMode, "Disabled");
			else
				GuiUtils.SetSelected (CboDnsSwitchMode, "Automatic");

			GuiUtils.SetCheck (ChkDnsCheck, s.GetBool ("dns.check"));

			TableDnsServersController.Clear ();
			string[] dnsServers = s.Get ("dns.servers").Split (',');
			foreach (string dnsServer in dnsServers) {
				if (IpAddress.IsIP (dnsServer))
					TableDnsServersController.Add (dnsServer);
			}

			// Advanced - Lock
			string lockMode = s.Get ("netlock.mode");
			GuiUtils.SetSelected (CboLockMode, "None");
			if (lockMode == "auto")
				GuiUtils.SetSelected (CboLockMode, "Automatic");
			else {
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes) {
					if (lockPlugin.GetCode () == lockMode) {
						GuiUtils.SetSelected(CboLockMode, lockPlugin.GetName());
					}
				}
			}
			GuiUtils.SetCheck(ChkLockAllowPrivate, s.GetBool("netlock.allow_private"));
			GuiUtils.SetCheck(ChkLockAllowPing, s.GetBool("netlock.allow_ping"));			
			TxtLockAllowedIPS.StringValue = s.Get("netlock.allowed_ips");

			// Advanced - Logging
			GuiUtils.SetCheck (ChkLoggingEnabled, s.GetBool ("log.file.enabled"));
			GuiUtils.SetCheck (ChkLogLevelDebug, s.GetBool ("log.level.debug"));
			TxtLoggingPath.StringValue = s.Get ("log.file.path");

			// Advanced - OVPN Directives
			GuiUtils.SetSelected(CboOpenVpnDirectivesSkipDefault, (s.GetBool("openvpn.skip_defaults") ? Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip2 : Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip1));
			TxtAdvancedOpenVpnDirectivesDefault.StringValue = s.Get ("openvpn.directives");
			TxtAdvancedOpenVpnDirectivesCustom.StringValue = s.Get ("openvpn.custom");

			// Advanced - Events
			ReadOptionsEvent ("app.start", 0);
			ReadOptionsEvent ("app.stop", 1);
			ReadOptionsEvent ("session.start", 2);
			ReadOptionsEvent ("session.stop", 3);
			ReadOptionsEvent ("vpn.pre", 4);
			ReadOptionsEvent ("vpn.up", 5);
			ReadOptionsEvent ("vpn.down", 6);

			TableAdvancedEventsController.RefreshUI ();
		}

		bool Check()
		{
			if ((RouteDescriptionToDirection (GuiUtils.GetSelected (CboRoutesOtherwise)) == "out") && (TableRoutingController.Items.Count == 0)) {
				if (GuiUtils.MessageYesNo (Messages.WindowsSettingsRouteOutEmptyList) == false)
					return false;
			}

			return true;
		}

		void SaveOptions()
		{
			Storage s = Engine.Instance.Storage;

			// General

			s.SetBool ("connect", GuiUtils.GetCheck (ChkConnect));
			s.SetBool("netlock", GuiUtils.GetCheck(ChkNetLock));


			s.SetBool ("servers.startlast", GuiUtils.GetCheck (ChkGeneralStartLast));
			s.SetBool ("gui.osx.visible", GuiUtils.GetCheck (ChkGeneralOsxVisible));
			// s.SetBool ("gui.osx.dock", GuiUtils.GetCheck (ChkGeneralOsxDock)); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			s.SetBool ("gui.osx.notifications", GuiUtils.GetCheck (ChkGeneralOsxNotifications));
			s.SetBool ("gui.osx.sysbar.show_info", GuiUtils.GetCheck (ChkUiSystemBarShowInfo));
			s.SetBool ("gui.osx.sysbar.show_speed", GuiUtils.GetCheck (ChkUiSystemBarShowSpeed));
			s.SetBool ("gui.osx.sysbar.show_server", GuiUtils.GetCheck (ChkUiSystemBarShowServer));
			s.SetBool ("gui.exit_confirm", GuiUtils.GetCheck (ChkExitConfirm));

			// UI
			string uiUnit = "";
			if (GuiUtils.GetSelected (CboUiUnit) == Messages.WindowsSettingsUiUnit1)
				uiUnit = "bytes";
			else if (GuiUtils.GetSelected (CboUiUnit) == Messages.WindowsSettingsUiUnit2)
				uiUnit = "bits";
			s.Set ("ui.unit", uiUnit);
		
			/*
			string interfaceStyle = GuiUtils.GetSelected (CboGeneralOsxInterfaceStyle);
			//string currentInterfaceStyle = GuiUtils.InterfaceColorMode ();
			if(interfaceStyle == "Dark")
				Platform.Instance.ShellCmd ("defaults write -g AppleInterfaceStyle Dark");
			else 
				Platform.Instance.ShellCmd ("defaults remove -g AppleInterfaceStyle");
			*/

			// Protocols
			if (GuiUtils.GetCheck (ChkProtocolsAutomatic)) {
				s.Set ("mode.protocol", "AUTO");
				s.SetInt ("mode.port", 443);
				s.SetInt ("mode.alt", 0);
			} else if (TableProtocols.SelectedRowCount == 1) {
				TableProtocolsControllerItem itemProtocol = TableProtocolsController.Items [TableProtocols.SelectedRow];
				s.Set("mode.protocol", itemProtocol.Protocol);
				s.SetInt ("mode.port", itemProtocol.Port);
				s.SetInt ("mode.alt", itemProtocol.Entry);
			} else {
				s.Set ("mode.protocol", "AUTO");
				s.SetInt ("mode.port", 443);
				s.SetInt ("mode.alt", 0);
			}


			// Proxy

			s.Set ("proxy.mode", GuiUtils.GetSelected (CboProxyType));
			s.Set ("proxy.host", TxtProxyHost.StringValue);
			s.SetInt ("proxy.port", Conversions.ToInt32(TxtProxyPort.StringValue));
			s.Set ("proxy.auth", GuiUtils.GetSelected (CboProxyAuthentication));
			s.Set ("proxy.login", TxtProxyLogin.StringValue);
			s.Set ("proxy.password", TxtProxyPassword.StringValue);
			s.SetInt ("proxy.tor.control.port", Conversions.ToInt32 (TxtProxyTorControlPort.StringValue));
			s.Set ("proxy.tor.control.password", TxtProxyTorControlPassword.StringValue);

			// Routes
			s.Set ("routes.default", RouteDescriptionToDirection (GuiUtils.GetSelected (CboRoutesOtherwise)));

			string routes = "";
			foreach (TableRoutingControllerItem item in TableRoutingController.Items) {
				if (routes != "")
					routes += ";";
				routes += item.Ip + "," + item.Action + "," + item.Notes;
			}
			s.Set("routes.custom", routes);

			// Advanced - General
			s.SetBool ("advanced.expert", GuiUtils.GetCheck (ChkAdvancedExpertMode));

			s.SetBool ("advanced.check.route", GuiUtils.GetCheck (ChkAdvancedCheckRoute));
			string ipV6Mode = GuiUtils.GetSelected (CboIpV6);
			if (ipV6Mode == "None")
				s.Set ("ipv6.mode", "none");
			else if (ipV6Mode == "Disable")
				s.Set ("ipv6.mode", "disable");
			else
				s.Set ("ipv6.mode", "disable");
			s.SetBool ("pinger.enabled", GuiUtils.GetCheck (ChkAdvancedPingerEnabled));
			s.SetBool ("routes.remove_default", GuiUtils.GetCheck(ChkRouteRemoveDefaultGateway));
			
			s.Set ("executables.openvpn", TxtAdvancedOpenVpnPath.StringValue);

			string manifestRefresh = GuiUtils.GetSelected(CboAdvancedManifestRefresh);
			if (manifestRefresh == "Automatic") // Auto
				s.SetInt("advanced.manifest.refresh", -1);
			else if (manifestRefresh == "Never") // Never
				s.SetInt("advanced.manifest.refresh", 0);
			else if (manifestRefresh == "Every minute") // One minute
				s.SetInt("advanced.manifest.refresh", 1);
			else if (manifestRefresh == "Every ten minute") // Ten minute
				s.SetInt("advanced.manifest.refresh", 10);
			else if (manifestRefresh == "Every one hour") // One hour
				s.SetInt("advanced.manifest.refresh", 60);

			string openVpnSndBuf = GuiUtils.GetSelected(CboOpenVpnSndBuf);
			if (openVpnSndBuf == Messages.Automatic)
				s.SetInt("openvpn.sndbuf", -2);
			else if (openVpnSndBuf == Messages.WindowsSettingsOpenVpnDefault)
				s.SetInt("openvpn.sndbuf", -1);
			else if (openVpnSndBuf == "8 KB")
				s.SetInt("openvpn.sndbuf", 1024 * 8);
			else if (openVpnSndBuf == "16 KB")
				s.SetInt("openvpn.sndbuf", 1024 * 16);
			else if (openVpnSndBuf == "32 KB")
				s.SetInt("openvpn.sndbuf", 1024 * 32);
			else if (openVpnSndBuf == "64 KB")
				s.SetInt("openvpn.sndbuf", 1024 * 64);
			else if (openVpnSndBuf == "128 KB")
				s.SetInt("openvpn.sndbuf", 1024 * 128);
			else if (openVpnSndBuf == "256 KB")
				s.SetInt("openvpn.sndbuf", 1024 * 256);
			else if (openVpnSndBuf == "512 KB")
				s.SetInt("openvpn.sndbuf", 1024 * 512);

			string openVpnRcvBuf = GuiUtils.GetSelected(CboOpenVpnRcvBuf);
			if (openVpnRcvBuf == Messages.Automatic)
				s.SetInt("openvpn.rcvbuf", -2);
			else if (openVpnRcvBuf == Messages.WindowsSettingsOpenVpnDefault)
				s.SetInt("openvpn.rcvbuf", -1);
			else if (openVpnRcvBuf == "8 KB")
				s.SetInt("openvpn.rcvbuf", 1024 * 8);
			else if (openVpnRcvBuf == "16 KB")
				s.SetInt("openvpn.rcvbuf", 1024 * 16);
			else if (openVpnRcvBuf == "32 KB")
				s.SetInt("openvpn.rcvbuf", 1024 * 32);
			else if (openVpnRcvBuf == "64 KB")
				s.SetInt("openvpn.rcvbuf", 1024 * 64);
			else if (openVpnRcvBuf == "128 KB")
				s.SetInt("openvpn.rcvbuf", 1024 * 128);
			else if (openVpnRcvBuf == "256 KB")
				s.SetInt("openvpn.rcvbuf", 1024 * 256);
			else if (openVpnRcvBuf == "512 KB")
				s.SetInt("openvpn.rcvbuf", 1024 * 512);

			// Advanced - DNS
			string dnsMode = GuiUtils.GetSelected (CboDnsSwitchMode);
			if (dnsMode == "Disabled")
				s.Set ("dns.mode", "none");
			else
				s.Set ("dns.mode", "auto");
			s.SetBool ("dns.check", GuiUtils.GetCheck (ChkDnsCheck));

			string dnsServers = "";
			for (int i = 0; i < TableDnsServersController.GetCount (); i++) {
				if (dnsServers != "")
					dnsServers += ",";
				dnsServers += TableDnsServersController.Get (i);
			}
			s.Set ("dns.servers", dnsServers);

			// Advanced - Lock
			string lockMode = GuiUtils.GetSelected (CboLockMode);
			s.Set ("netlock.mode", "none");
			if (lockMode == "Automatic") {
				s.Set ("netlock.mode", "auto");
			} else {
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes) {
					if (lockPlugin.GetName () == lockMode) {
						s.Set ("netlock.mode", lockPlugin.GetCode ());
					}
				}
			}
			s.SetBool ("netlock.allow_private", GuiUtils.GetCheck (ChkLockAllowPrivate));
			s.SetBool ("netlock.allow_ping", GuiUtils.GetCheck (ChkLockAllowPing));			
			s.Set ("netlock.allowed_ips", TxtLockAllowedIPS.StringValue);

			// Advanced - Logging
			s.SetBool ("log.file.enabled", GuiUtils.GetCheck (ChkLoggingEnabled));
			s.SetBool ("log.level.debug", GuiUtils.GetCheck (ChkLogLevelDebug));
			s.Set ("log.file.path", TxtLoggingPath.StringValue);

			// Advanced - OVPN Directives
			s.SetBool ("openvpn.skip_defaults", GuiUtils.GetSelected (CboOpenVpnDirectivesSkipDefault) == Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip2);
			s.Set ("openvpn.directives", TxtAdvancedOpenVpnDirectivesDefault.StringValue);
			s.Set ("openvpn.custom", TxtAdvancedOpenVpnDirectivesCustom.StringValue);

			// Advanced - Events
			SaveOptionsEvent ("app.start", 0);
			SaveOptionsEvent ("app.stop", 1);
			SaveOptionsEvent ("session.start", 2);
			SaveOptionsEvent ("session.stop", 3);
			SaveOptionsEvent ("vpn.pre", 4);
			SaveOptionsEvent ("vpn.up", 5);
			SaveOptionsEvent ("vpn.down", 6);

			Engine.Instance.OnSettingsChanged ();
		}

		public void EnableIde()
		{
			// Protocols
			TableProtocols.Enabled = (GuiUtils.GetCheck (ChkProtocolsAutomatic) == false);

			// Proxy
			bool proxy = (GuiUtils.GetSelected (CboProxyType) != "None");
			bool tor = (GuiUtils.GetSelected (CboProxyType) == "Tor");

			TxtProxyHost.Enabled = proxy;
			TxtProxyPort.Enabled = proxy;
			CboProxyAuthentication.Enabled = (proxy && !tor);
			TxtProxyLogin.Enabled = ((proxy) && (!tor) && (GuiUtils.GetSelected (CboProxyAuthentication) != "None"));
			TxtProxyPassword.Enabled = TxtProxyLogin.Enabled;
			TxtProxyTorControlPort.Enabled = tor;
			TxtProxyTorControlPassword.Enabled = tor;
			CmdProxyTorTest.Enabled = tor;
		
			// Routing
			CmdRouteAdd.Enabled = true;
			CmdRouteRemove.Enabled = (TableRoutes.SelectedRowCount > 0);
			CmdRouteEdit.Enabled = (TableRoutes.SelectedRowCount == 1);

			// DNS
			CmdDnsAdd.Enabled = true;
			CmdDnsRemove.Enabled = (TableDnsServers.SelectedRowCount > 0);
			CmdDnsEdit.Enabled = (TableDnsServers.SelectedRowCount == 1);

			// Lock
			LblLockRoutingOutWarning.Hidden = (GuiUtils.GetSelected (CboRoutesOtherwise) == RouteDirectionToDescription ("in"));

			// Events
			CmdAdvancedEventsClear.Enabled = (TableAdvancedEvents.SelectedRowCount == 1);
			CmdAdvancedEventsEdit.Enabled = (TableAdvancedEvents.SelectedRowCount == 1);
		}
	}
}

