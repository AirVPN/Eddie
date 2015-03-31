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
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public partial class WindowPreferencesController : MonoMac.AppKit.NSWindowController
	{
		private string m_mode_protocol;
		private int m_mode_port;
		private int m_mode_alternate;

		private bool m_modeSshEnabled = true;
		private bool m_modeSslEnabled = true;


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

			LblConnect.StringValue = Messages.WindowsSettingsConnect;
			LblNetLock.StringValue = Messages.WindowsSettingsNetLock;

			TableRoutes.Delegate = new TableRoutingDelegate (this);
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

			CmdGeneralTos.Activated += (object sender, EventArgs e) => {
				WindowTosController tos = new WindowTosController ();
				tos.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow (tos.Window);
				tos.Window.Close ();
			};

			// Modes
			string sshStatus = (Software.SshVersion != "" ? "" : "Not available");
			if (sshStatus != "") {
				m_modeSshEnabled = false;
			}
			// TODO: disable ssh & ssl
			string sslStatus = (Software.SslVersion != "" ? "" : "Not available");
			if (sslStatus != "") {
				m_modeSslEnabled = false;
			}

			CmdModeHelp.Activated += (object sender, EventArgs e) => {
				Core.UI.Actions.OpenUrlDocsProtocols();
			};

			ChkModeAutomatic.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "AUTO";
				m_mode_port = 443;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeUdp443.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "UDP";
				m_mode_port = 443;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeTcp443.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "TCP";
				m_mode_port = 443;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeUdp80.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 80;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeTcp80.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "TCP";
				m_mode_port = 80;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeUdp53.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 53;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeTcp53.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "TCP";
				m_mode_port = 53;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeUdp2018.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 2018;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeTcp2018.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "TCP";
				m_mode_port = 2018;
				m_mode_alternate = 0;
				ChangeMode();
			};



			ChkModeUdp443Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 443;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeUdp80Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 80;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeUdp53Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 53;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeUdp2018Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 2018;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeTcp2018Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "TCP";
				m_mode_port = 2018;
				m_mode_alternate = 1;
				ChangeMode();
			};


			ChkModeSsh22.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "SSH";
				m_mode_port = 22;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeSsh22Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "SSH";
				m_mode_port = 22;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeSsh80.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "SSH";
				m_mode_port = 80;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeSsh53.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "SSH";
				m_mode_port = 53;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeSsl443.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "SSL";
				m_mode_port = 443;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeTor.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "TOR";
				m_mode_port = 2018;
				m_mode_alternate = 0;
				ChangeMode();
			};

			CmdModeTorTest.Activated += (object sender, EventArgs e) => {
				string result = TorControl.Test(TxtModeTorHost.StringValue, Conversions.ToInt32(TxtModeTorControlPort.StringValue), TxtModeTorControlPassword.StringValue);
				GuiUtils.MessageBox(result);
			};

			// Proxy
			CboProxyType.Activated += (object sender, EventArgs e) => {
				EnableIde();
			};
			CboProxyAuthentication.Activated += (object sender, EventArgs e) => {
				EnableIde();
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

			CboIpV6.RemoveAllItems ();
			CboIpV6.AddItem ("None");
			CboIpV6.AddItem ("Disable");

			CboAdvancedManifestRefresh.RemoveAllItems ();
			CboAdvancedManifestRefresh.AddItem ("Automatic");
			CboAdvancedManifestRefresh.AddItem ("Never");
			CboAdvancedManifestRefresh.AddItem ("Every minute");
			CboAdvancedManifestRefresh.AddItem ("Every ten minute");
			CboAdvancedManifestRefresh.AddItem ("Every one hour");

			CmdAdvancedOpenVpnPath.Activated += (object sender, EventArgs e) => {
				GuiUtils.SelectFile(this.Window, TxtAdvancedOpenVpnPath);
			};

			CmdAdvancedHelp.Activated += (object sender, EventArgs e) => {
				Core.UI.Actions.OpenUrlDocsAdvanced();
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
				Core.UI.Actions.OpenUrlDocsLock();
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


			ChkAdvancedPingerAlways.Hidden = true; // TOCLEAN

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
			WindowPreferencesIpController.Dns = "";
			WindowPreferencesIpController dlg = new WindowPreferencesIpController();
			dlg.Window.ReleasedWhenClosed = true;
			NSApplication.SharedApplication.RunModalForWindow(dlg.Window);
			dlg.Window.Close();

			if (dlg.Accepted)
			{
				TableDnsServersController.Items.Add(WindowPreferencesIpController.Dns);
				TableDnsServersController.RefreshUI();
			}

			this.EnableIde();
		}

		void DnsServersRemove()
		{
			int i = TableDnsServers.SelectedRow;
			if (i != -1)
			{
				TableDnsServersController.Items.RemoveAt(i);
				TableDnsServersController.RefreshUI();
				this.EnableIde();
			}
		}

		void DnsServersEdit()
		{
			int i = TableDnsServers.SelectedRow;
			if (i != -1)
			{
				string dns = TableDnsServersController.Items[i];

				WindowPreferencesIpController.Dns = dns;
				WindowPreferencesIpController dlg = new WindowPreferencesIpController();
				dlg.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow(dlg.Window);
				dlg.Window.Close();

				if (dlg.Accepted)
				{
					TableDnsServersController.Items[i] = dns;
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

		void ChangeMode()
		{
			GuiUtils.SetCheck (ChkModeAutomatic, ((m_mode_protocol == "AUTO") && (m_mode_port == 443) && (m_mode_alternate == 0)));

			GuiUtils.SetCheck (ChkModeUdp443, ((m_mode_protocol == "UDP") && (m_mode_port == 443) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeTcp443, ((m_mode_protocol == "TCP") && (m_mode_port == 443) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeUdp80, ((m_mode_protocol == "UDP") && (m_mode_port == 80) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeTcp80, ((m_mode_protocol == "TCP") && (m_mode_port == 80) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeUdp53, ((m_mode_protocol == "UDP") && (m_mode_port == 53) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeTcp53, ((m_mode_protocol == "TCP") && (m_mode_port == 53) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeUdp2018, ((m_mode_protocol == "UDP") && (m_mode_port == 2018) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeTcp2018, ((m_mode_protocol == "TCP") && (m_mode_port == 2018) && (m_mode_alternate == 0)));

			GuiUtils.SetCheck(ChkModeUdp443Alt, ((m_mode_protocol == "UDP") && (m_mode_port == 443) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck(ChkModeUdp80Alt, ((m_mode_protocol == "UDP") && (m_mode_port == 80) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck(ChkModeUdp53Alt, ((m_mode_protocol == "UDP") && (m_mode_port == 53) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck(ChkModeUdp2018Alt, ((m_mode_protocol == "UDP") && (m_mode_port == 2018) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck(ChkModeTcp2018Alt, ((m_mode_protocol == "TCP") && (m_mode_port == 2018) && (m_mode_alternate == 1)));
			
			GuiUtils.SetCheck (ChkModeSsh22, ((m_mode_protocol == "SSH") && (m_mode_port == 22) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeSsh22Alt, ((m_mode_protocol == "SSH") && (m_mode_port == 22) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck (ChkModeSsh80, ((m_mode_protocol == "SSH") && (m_mode_port == 80) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck (ChkModeSsh53, ((m_mode_protocol == "SSH") && (m_mode_port == 53) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck (ChkModeSsl443, ((m_mode_protocol == "SSL") && (m_mode_port == 443) && (m_mode_alternate == 1)));

			GuiUtils.SetCheck (ChkModeTor, ((m_mode_protocol == "TOR") && (m_mode_port == 2018) && (m_mode_alternate == 0)));

			EnableIde ();
		}

		void RefreshLogPreview()
		{
			TxtLoggingComputedPath.StringValue = Engine.Instance.GetParseLogFilePaths (TxtLoggingPath.StringValue);
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
			GuiUtils.SetCheck (ChkExitConfirm, s.GetBool("gui.exit_confirm"));

			/*
			string interfaceMode = GuiUtils.InterfaceColorMode ();
			if (interfaceMode == "Dark")
				GuiUtils.SetSelected (CboGeneralOsxInterfaceStyle,"Dark");
			else
				GuiUtils.SetSelected (CboGeneralOsxInterfaceStyle,"Default");
			*/

			// Mode
			m_mode_protocol = s.Get ("mode.protocol").ToUpperInvariant ();
			m_mode_port = s.GetInt ("mode.port");
			m_mode_alternate = s.GetInt ("mode.alt");
			ChangeMode ();
			TxtModeTorHost.StringValue = s.Get ("mode.tor.host");
			TxtModeTorPort.StringValue = s.Get ("mode.tor.port");
			TxtModeTorControlPort.StringValue = s.Get ("mode.tor.control.port");
			TxtModeTorControlPassword.StringValue = s.Get ("mode.tor.control.password");

			// Proxy

			GuiUtils.SetSelected (CboProxyType, s.Get("proxy.mode"));
			TxtProxyHost.StringValue = s.Get ("proxy.host");
			TxtProxyPort.StringValue = s.Get ("proxy.port");
			GuiUtils.SetSelected (CboProxyAuthentication, s.Get ("proxy.auth"));
			TxtProxyLogin.StringValue = s.Get ("proxy.login");
			TxtProxyPassword.StringValue = s.Get ("proxy.password");

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

			GuiUtils.SetCheck (ChkAdvancedPingerEnabled, s.GetBool ("advanced.pinger.enabled"));
			GuiUtils.SetCheck (ChkAdvancedPingerAlways, s.GetBool ("advanced.pinger.always"));

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

			// Advanced - DNS

			string dnsMode = s.Get ("dns.mode");
			if (dnsMode == "none")
				GuiUtils.SetSelected (CboAdvancedDnsSwitchMode, "Disabled");
			else
				GuiUtils.SetSelected (CboAdvancedDnsSwitchMode, "Automatic");

			GuiUtils.SetCheck (ChkAdvancedCheckDns, s.GetBool ("dns.check"));

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
			GuiUtils.SetCheck(ChkLockAllowIpV6, s.GetBool("netlock.allow_ipv6"));
			TxtLockAllowedIPS.StringValue = s.Get("netlock.allowed_ips");

			// Advanced - Logging
			GuiUtils.SetCheck (ChkLoggingEnabled, s.GetBool ("log.file.enabled"));
			TxtLoggingPath.StringValue = s.Get ("log.file.path");

			// Advanced - OVPN Directives
			GuiUtils.SetCheck (ChkAdvancedOpenVpnDirectivesDefaultSkip, s.GetBool ("openvpn.skip_defaults"));

			TxtAdvancedOpenVpnDirectivesCustom.StringValue = s.Get ("openvpn.custom");
			TxtAdvancedOpenVpnDirectivesDefault.StringValue = s.GetDefaultDirectives ().Replace("\t","");

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
			s.SetBool ("gui.exit_confirm", GuiUtils.GetCheck (ChkExitConfirm));

			/*
			string interfaceStyle = GuiUtils.GetSelected (CboGeneralOsxInterfaceStyle);
			//string currentInterfaceStyle = GuiUtils.InterfaceColorMode ();
			if(interfaceStyle == "Dark")
				Platform.Instance.ShellCmd ("defaults write -g AppleInterfaceStyle Dark");
			else 
				Platform.Instance.ShellCmd ("defaults remove -g AppleInterfaceStyle");
			*/

			// Mode

			s.Set ("mode.protocol", m_mode_protocol);
			s.SetInt ("mode.port", m_mode_port);
			s.SetInt ("mode.alt", m_mode_alternate);
			s.Set ("mode.tor.host", TxtModeTorHost.StringValue);
			s.SetInt ("mode.tor.port", Conversions.ToInt32(TxtModeTorPort.StringValue));
			s.SetInt ("mode.tor.control.port", Conversions.ToInt32(TxtModeTorControlPort.StringValue));
			s.Set ("mode.tor.control.password", TxtModeTorControlPassword.StringValue);

			// Proxy

			s.Set ("proxy.mode", GuiUtils.GetSelected (CboProxyType));
			s.Set ("proxy.host", TxtProxyHost.StringValue);
			s.SetInt ("proxy.port", Conversions.ToInt32(TxtProxyPort.StringValue));
			s.Set ("proxy.auth", GuiUtils.GetSelected (CboProxyAuthentication));
			s.Set ("proxy.login", TxtProxyLogin.StringValue);
			s.Set ("proxy.password", TxtProxyPassword.StringValue);

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
			s.SetBool ("advanced.pinger.enabled", GuiUtils.GetCheck (ChkAdvancedPingerEnabled));
			s.SetBool ("advanced.pinger.always", GuiUtils.GetCheck (ChkAdvancedPingerAlways));

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

			// Advanced - DNS
			string dnsMode = GuiUtils.GetSelected (CboAdvancedDnsSwitchMode);
			if (dnsMode == "Disabled")
				s.Set ("dns.mode", "none");
			else
				s.Set ("dns.mode", "auto");
			s.SetBool ("dns.check", GuiUtils.GetCheck (ChkAdvancedCheckDns));

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
			s.SetBool ("netlock.allow_ipv6", GuiUtils.GetCheck (ChkLockAllowIpV6));
			s.Set ("netlock.allowed_ips", TxtLockAllowedIPS.StringValue);

			// Advanced - Logging
			s.SetBool ("log.file.enabled", GuiUtils.GetCheck (ChkLoggingEnabled));
			s.Set ("log.file.path", TxtLoggingPath.StringValue);

			// Advanced - OVPN Directives
			s.SetBool ("openvpn.skip_defaults", GuiUtils.GetCheck (ChkAdvancedOpenVpnDirectivesDefaultSkip));

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
			bool proxy = (GuiUtils.GetSelected (CboProxyType) != "None");
			TxtProxyHost.Enabled = proxy;
			TxtProxyPort.Enabled = proxy;
			CboProxyAuthentication.Enabled = proxy;
			TxtProxyLogin.Enabled = ((proxy) && (GuiUtils.GetSelected (CboProxyAuthentication) != "None"));
			TxtProxyPassword.Enabled = TxtProxyLogin.Enabled;

			ChkModeSsh22.Enabled = ((proxy == false) && (m_modeSshEnabled));
			ChkModeSsh22Alt.Enabled = ((proxy == false) && (m_modeSshEnabled));
			ChkModeSsh53.Enabled = ((proxy == false) && (m_modeSshEnabled));
			ChkModeSsh80.Enabled = ((proxy == false) && (m_modeSshEnabled));
			ChkModeSsl443.Enabled = ((proxy == false) && (m_modeSslEnabled));
			ChkModeTor.Enabled = (proxy == false);

			ChkModeUdp2018.Enabled = (proxy == false);
			ChkModeUdp2018Alt.Enabled = (proxy == false);
			ChkModeUdp443.Enabled = (proxy == false);
			ChkModeUdp443Alt.Enabled = (proxy == false);
			ChkModeUdp53.Enabled = (proxy == false);
			ChkModeUdp53Alt.Enabled = (proxy == false);
			ChkModeUdp80.Enabled = (proxy == false);
			ChkModeUdp80Alt.Enabled = (proxy == false);

			TxtModeTorHost.Enabled = GuiUtils.GetCheck (ChkModeTor);
			TxtModeTorPort.Enabled = GuiUtils.GetCheck (ChkModeTor);
			TxtModeTorControlPort.Enabled = GuiUtils.GetCheck (ChkModeTor);
			TxtModeTorControlPassword.Enabled = GuiUtils.GetCheck (ChkModeTor);
			CmdModeTorTest.Enabled = GuiUtils.GetCheck (ChkModeTor);

			// Routing
			CmdRouteAdd.Enabled = true;
			CmdRouteRemove.Enabled = (TableRoutes.SelectedRowCount > 0);
			CmdRouteEdit.Enabled = (TableRoutes.SelectedRowCount == 1);

			// Lock
			LblLockRoutingOutWarning.Hidden = (GuiUtils.GetSelected (CboRoutesOtherwise) == RouteDirectionToDescription ("in"));

			CmdAdvancedEventsClear.Enabled = (TableAdvancedEvents.SelectedRowCount == 1);
			CmdAdvancedEventsEdit.Enabled = (TableAdvancedEvents.SelectedRowCount == 1);
		}
	}
}

