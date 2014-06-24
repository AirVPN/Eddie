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

			TableRoutingController = new TableRoutingController (this.TableRoutes);
			TableAdvancedEventsController = new TableAdvancedEventsController (this.TableAdvancedEvents);

			CmdSave.Activated += (object sender, EventArgs e) => {
				SaveOptions ();
				Close ();
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

			CmdRouteAdd.Activated += (object sender, EventArgs e) => {
				RouteAdd();
			};

			CmdRouteRemove.Activated += (object sender, EventArgs e) => {
				RouteRemove();
			};

			CmdRouteEdit.Activated += (object sender, EventArgs e) => {
				RouteEdit();
			};

			// Advanced
			CmdAdvancedHelp.Activated += (object sender, EventArgs e) => {
				Core.UI.Actions.OpenUrlDocsAdvanced();
			};

			CmdAdvancedOpenVpnPath.Activated += (object sender, EventArgs e) => {
				TxtAdvancedOpenVpnPath.StringValue = "todo";
			};

			TableAdvancedEvents.DoubleClick += (object sender, EventArgs e) => {
				AdvancedEventEdit();
			};

			CmdAdvancedEventsEdit.Activated += (object sender, EventArgs e) => {
				AdvancedEventEdit();
			};

			CmdAdvancedEventsClear.Activated += (object sender, EventArgs e) => {
				AdvancedEventClear();

			};


			ChkAdvancedNetworkLocking.Hidden = (Engine.Instance.DevelopmentEnvironment == false);

			ReadOptions ();

			EnableIde ();

		}

		string RouteDirectionToDescription(string v)
		{
			if (v == "in")
				return "Inside the VPN tunnel";
			else if (v == "out")
				return "Outside the VPN tunnel";
			else
				return "";
		}

		string RouteDescriptionToDirection(string v)
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
			item.NetMask = "255.255.255.255";
			item.Icon = "out";
			item.Action = RouteDirectionToDescription("out");

			WindowPreferencesRouteController dlg = new WindowPreferencesRouteController ();
			dlg.Window.ReleasedWhenClosed = true;
			dlg.Item = item;
			NSApplication.SharedApplication.RunModalForWindow (dlg.Window);
			dlg.Window.Close ();

			if (dlg.Item != null) {
				TableRoutingController.Items.Add(dlg.Item);
			}

			this.EnableIde();
		}

		void RouteEdit()
		{
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


		void AdvancedEventEdit()
		{
			int index = TableAdvancedEvents.SelectedRow;

			WindowPreferencesEventController dlg = new WindowPreferencesEventController ();
			dlg.Window.ReleasedWhenClosed = true;
			dlg.Filename = TableAdvancedEventsController.Items [index].Filename;
			dlg.Arguments = TableAdvancedEventsController.Items [index].Arguments;
			dlg.WaitEnd = TableAdvancedEventsController.Items [index].WaitEnd;

			NSApplication.SharedApplication.RunModalForWindow (dlg.Window);
			dlg.Window.Close ();

			if (dlg.Accepted) {
				TableAdvancedEventsController.Items [index].Filename = dlg.Filename;
				TableAdvancedEventsController.Items [index].Arguments = dlg.Arguments;
				TableAdvancedEventsController.Items [index].WaitEnd = dlg.WaitEnd;
			}
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
		}

		void ChangeMode()
		{
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

			GuiUtils.SetCheck (ChkAutoStart, s.GetBool ("connect")); 
			GuiUtils.SetCheck (ChkGeneralStartLast, s.GetBool("servers.startlast"));
			GuiUtils.SetCheck (ChkGeneralOsxNotifications, s.GetBool ("gui.osx.notifications"));

			// Mode
			m_mode_protocol = s.Get ("mode.protocol").ToUpperInvariant ();
			m_mode_port = s.GetInt ("mode.port");
			m_mode_alternate = s.GetInt ("mode.alt");
			ChangeMode ();

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
				if (routeEntries.Length != 3)
					continue;

				TableRoutingControllerItem item = new TableRoutingControllerItem ();
				item.Ip = routeEntries [0];
				item.NetMask = routeEntries [1];
				item.Action = RouteDirectionToDescription (routeEntries [2]);
				item.Icon = routeEntries [2];
				TableRoutingController.Items.Add (item);
			}

			TableRoutingController.RefreshUI();

			// Advanced
			GuiUtils.SetCheck (ChkAdvancedExpertMode, s.GetBool ("advanced.expert"));
			GuiUtils.SetCheck (ChkAdvancedCheckDns, s.GetBool ("advanced.check.dns"));
			GuiUtils.SetCheck (ChkAdvancedCheckRoute, s.GetBool ("advanced.check.route"));

			string dnsMode = s.Get ("advanced.dns.mode");
			if (dnsMode == "none")
				GuiUtils.SetSelected (CboAdvancedDnsSwitchMode, "Disabled");
			else
				GuiUtils.SetSelected (CboAdvancedDnsSwitchMode, "Automatic");

			GuiUtils.SetCheck (ChkAdvancedPingerEnabled, s.GetBool ("advanced.pinger.enabled"));
			GuiUtils.SetCheck (ChkAdvancedPingerAlways, s.GetBool ("advanced.pinger.always"));

			GuiUtils.SetCheck (ChkAdvancedNetworkLocking, s.GetBool ("advanced.netlock.enabled"));

			GuiUtils.SetCheck (ChkAdvancedOpenVpnDirectivesDefaultSkip, s.GetBool ("openvpn.skip_defaults"));

			TxtAdvancedOpenVpnPath.StringValue = s.Get ("executables.openvpn");
			TxtAdvancedOpenVpnDirectivesCustom.StringValue = s.Get ("openvpn.custom");
			TxtAdvancedOpenVpnDirectivesDefault.StringValue = s.GetDefaultDirectives ().Replace("\t","");

			ReadOptionsEvent ("app.start", 0);
			ReadOptionsEvent ("app.stop", 1);
			ReadOptionsEvent ("session.start", 2);
			ReadOptionsEvent ("session.stop", 3);
			ReadOptionsEvent ("vpn.pre", 4);
			ReadOptionsEvent ("vpn.up", 5);
			ReadOptionsEvent ("vpn.down", 6);

			TableAdvancedEventsController.RefreshUI ();
		}

		void SaveOptions()
		{
			Storage s = Engine.Instance.Storage;

			// General

			s.SetBool ("connect", GuiUtils.GetCheck (ChkAutoStart));
			s.SetBool ("servers.startlast", GuiUtils.GetCheck (ChkGeneralStartLast));
			s.SetBool ("gui.osx.notifications", GuiUtils.GetCheck (ChkGeneralOsxNotifications));

			// Mode

			s.Set ("mode.protocol", m_mode_protocol);
			s.SetInt ("mode.port", m_mode_port);
			s.SetInt ("mode.alt", m_mode_alternate);

			// Proxy

			s.Set ("proxy.mode", GuiUtils.GetSelected (CboProxyType));
			s.Set ("proxy.host", TxtProxyHost.StringValue);
			s.Set ("proxy.port", TxtProxyPort.StringValue);
			s.Set ("proxy.auth", GuiUtils.GetSelected (CboProxyAuthentication));
			s.Set ("proxy.login", TxtProxyLogin.StringValue);
			s.Set ("proxy.password", TxtProxyPassword.StringValue);

			// Routes
			s.Set ("routes.default", RouteDescriptionToDirection (GuiUtils.GetSelected (CboRoutesOtherwise)));

			string routes = "";
			foreach (TableRoutingControllerItem item in TableRoutingController.Items) {
				if (routes != "")
					routes += ";";
				routes += item.Ip + "," + item.NetMask + "," + item.Icon;
			}
			s.Set("routes.custom", routes);

			// Advanced
			s.SetBool ("advanced.expert", GuiUtils.GetCheck (ChkAdvancedExpertMode));
			s.SetBool ("advanced.check.dns", GuiUtils.GetCheck (ChkAdvancedCheckDns));
			s.SetBool ("advanced.check.route", GuiUtils.GetCheck (ChkAdvancedCheckRoute));

			string dnsMode = GuiUtils.GetSelected (CboAdvancedDnsSwitchMode);
			if (dnsMode == "Disabled")
				s.Set ("advanced.dns.mode", "none");
			else
				s.Set ("advanced.dns.mode", "auto");

			s.SetBool ("advanced.pinger.enabled", GuiUtils.GetCheck (ChkAdvancedPingerEnabled));
			s.SetBool ("advanced.pinger.always", GuiUtils.GetCheck (ChkAdvancedPingerAlways));

			s.SetBool ("advanced.netlock.enabled", GuiUtils.GetCheck (ChkAdvancedNetworkLocking));

			s.SetBool ("openvpn.skip_defaults", GuiUtils.GetCheck (ChkAdvancedOpenVpnDirectivesDefaultSkip));

			s.Set ("executables.openvpn", TxtAdvancedOpenVpnPath.StringValue);
			s.Set ("openvpn.custom", TxtAdvancedOpenVpnDirectivesCustom.StringValue);

			SaveOptionsEvent ("app.start", 0);
			SaveOptionsEvent ("app.stop", 1);
			SaveOptionsEvent ("session.start", 2);
			SaveOptionsEvent ("session.stop", 3);
			SaveOptionsEvent ("vpn.pre", 4);
			SaveOptionsEvent ("vpn.up", 5);
			SaveOptionsEvent ("vpn.down", 6);
		}

		void EnableIde()
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
		
			ChkModeUdp2018.Enabled = (proxy == false);
			ChkModeUdp2018Alt.Enabled = (proxy == false);
			ChkModeUdp443.Enabled = (proxy == false);
			ChkModeUdp443Alt.Enabled = (proxy == false);
			ChkModeUdp53.Enabled = (proxy == false);
			ChkModeUdp53Alt.Enabled = (proxy == false);
			ChkModeUdp80.Enabled = (proxy == false);
			ChkModeUdp80Alt.Enabled = (proxy == false);

			CmdAdvancedEventsClear.Enabled = (TableAdvancedEvents.SelectedRowCount == 1);
			CmdAdvancedEventsEdit.Enabled = (TableAdvancedEvents.SelectedRowCount == 1);
		}
	}
}

