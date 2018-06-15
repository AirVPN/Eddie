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
//using Foundation;
//using AppKit;
using Foundation;
using AppKit;
using Eddie.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class WindowPreferencesController : AppKit.NSWindowController
	{
		private TableTabsController TableTabsController;
		private TableProtocolsController TableProtocolsController;
		private TableRoutingController TableRoutingController;
		private TableDnsServersController TableDnsServersController;
		private TableAdvancedEventsController TableAdvancedEventsController;

		private Dictionary<string, string> m_mapNetworkEntryIFace = new Dictionary<string, string>();

		#region Constructors
		// Called when created from unmanaged code
		public WindowPreferencesController(IntPtr handle) : base(handle)
		{
			Initialize();
		}
		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public WindowPreferencesController(NSCoder coder) : base(coder)
		{
			Initialize();
		}
		// Call to load from the XIB/NIB file
		public WindowPreferencesController() : base("WindowPreferences")
		{
			Initialize();
		}
		// Shared initialization code
		void Initialize()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowPreferences Window
		{
			get
			{
				return (WindowPreferences)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Title = Constants.Name + " - " + Messages.WindowsSettingsTitle;

			TableTabsController = new TableTabsController(TableTabs, TabMain);

			ChkNetLock.Activated += (object sender, EventArgs e) =>
			{
				if (GuiUtils.GetCheck(ChkNetLock))
				{
					if ((Engine.Instance as UI.Cocoa.Osx.Engine).MainWindow.NetworkLockKnowledge() == false)
						GuiUtils.SetCheck(ChkNetLock, false);
				}
			};

			TableRoutes.Delegate = new TableRoutingDelegate(this);

			LblDnsServers.StringValue = Messages.WindowsSettingsDnsServers;
			TableDnsServers.Delegate = new TableDnsServersDelegate(this);

			TableAdvancedEvents.Delegate = new TableAdvancedEventsDelegate(this);

			LblLoggingHelp.StringValue = Messages.WindowsSettingsLoggingHelp;

			TableRoutingController = new TableRoutingController(this.TableRoutes);
			TableDnsServersController = new TableDnsServersController(this.TableDnsServers);
			TableAdvancedEventsController = new TableAdvancedEventsController(this.TableAdvancedEvents);

			CmdSave.Activated += (object sender, EventArgs e) =>
			{
				try
				{
					if (Check())
					{
						SaveOptions();
						Close();
					}

				}
				catch (Exception ex)
				{
					Core.Engine.Instance.Logs.Log(LogType.Fatal, ex);
				}
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
				Close();
			};

			// General

			CmdGeneralTos.Activated += (object sender, EventArgs e) =>
			{
				WindowTosController tos = new WindowTosController();
				tos.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow(tos.Window);
				tos.Window.Close();
			};

			CmdResetToDefault.Activated += (object sender, EventArgs e) =>
			{
                if (GuiUtils.MessageYesNo(Messages.ResetSettingsConfirm))
				{
					Engine.Instance.Storage.ResetAll(false);
					ReadOptions();
					GuiUtils.MessageBoxInfo(Messages.ResetSettingsDone);
				}
			};

			// UI

			CboUiUnit.RemoveAllItems();
			CboUiUnit.AddItem(Messages.WindowsSettingsUiUnit0);
			CboUiUnit.AddItem(Messages.WindowsSettingsUiUnit1);
			CboUiUnit.AddItem(Messages.WindowsSettingsUiUnit2);

			// Protocols

			CmdProtocolsHelp1.Activated += (object sender, EventArgs e) =>
			{
				Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["help"]["protocols"].Value as string);
			};

			CmdProtocolsHelp2.Activated += (object sender, EventArgs e) =>
			{
				Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["help"]["udp_vs_tcp"].Value as string);
			};

			ChkProtocolsAutomatic.Activated += (object sender, EventArgs e) =>
			{
				EnableIde();
			};

			TableProtocols.Delegate = new TableProtocolsDelegate(this);
			TableProtocolsController = new TableProtocolsController(this.TableProtocols);

			// Proxy
			CboProxyType.RemoveAllItems();
			CboProxyType.AddItem("None");
			CboProxyType.AddItem("Http");
			CboProxyType.AddItem("Socks");
			CboProxyType.AddItem("Tor");
			CboProxyWhen.RemoveAllItems();
			CboProxyWhen.AddItem(Messages.WindowsSettingsProxyWhenAlways);
			CboProxyWhen.AddItem(Messages.WindowsSettingsProxyWhenWeb);
			CboProxyWhen.AddItem(Messages.WindowsSettingsProxyWhenOpenVPN);
			CboProxyWhen.AddItem(Messages.WindowsSettingsProxyWhenNone);

			CmdProxyTorHelp.Activated += (object sender, EventArgs e) =>
			{
				Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["help"]["tor"].Value as string);
			};
			CboProxyType.Activated += (object sender, EventArgs e) =>
			{
				EnableIde();

				if (GuiUtils.GetSelected(CboProxyType) == "Tor")
					TxtProxyPort.StringValue = "9150";
				else
					TxtProxyPort.StringValue = "8080";
			};
			CboProxyAuthentication.Activated += (object sender, EventArgs e) =>
			{
				EnableIde();
			};
			CmdProxyTorTest.Activated += (object sender, EventArgs e) =>
			{
				string result = TorControl.Test(TxtProxyHost.StringValue, Conversions.ToInt32(TxtProxyTorControlPort.StringValue), TxtProxyTorControlPassword.StringValue);
				GuiUtils.MessageBoxInfo(result);
			};

			// Routes
			CboRoutesOtherwise.RemoveAllItems();
			CboRoutesOtherwise.AddItem(RouteDirectionToDescription("in"));
			CboRoutesOtherwise.AddItem(RouteDirectionToDescription("out"));
			CboRoutesOtherwise.Activated += (object sender, EventArgs e) =>
			{
				EnableIde();
			};

			TableRoutes.DoubleClick += (object sender, EventArgs e) =>
			{
				RouteEdit();
			};

			CmdRouteAdd.Activated += (object sender, EventArgs e) =>
			{
				RouteAdd();
			};

			CmdRouteRemove.Activated += (object sender, EventArgs e) =>
			{
				RouteRemove();
			};

			CmdRouteEdit.Activated += (object sender, EventArgs e) =>
			{
				RouteEdit();
			};

			// Dns

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

			// Networking

			CboNetworkIPv4Mode.RemoveAllItems();
			CboNetworkIPv4Mode.AddItem(Messages.WindowsSettingsNetworkIpModeInAlways);
			CboNetworkIPv4Mode.AddItem(Messages.WindowsSettingsNetworkIpModeInOrOut);
			CboNetworkIPv4Mode.AddItem(Messages.WindowsSettingsNetworkIpModeInOrBlock);
			CboNetworkIPv4Mode.AddItem(Messages.WindowsSettingsNetworkIpModeOut);
			CboNetworkIPv4Mode.AddItem(Messages.WindowsSettingsNetworkIpModeBlock);

			CboNetworkIPv6Mode.RemoveAllItems();
			CboNetworkIPv6Mode.AddItem(Messages.WindowsSettingsNetworkIpModeInAlways);
			CboNetworkIPv6Mode.AddItem(Messages.WindowsSettingsNetworkIpModeInOrOut);
			CboNetworkIPv6Mode.AddItem(Messages.WindowsSettingsNetworkIpModeInOrBlock);
			CboNetworkIPv6Mode.AddItem(Messages.WindowsSettingsNetworkIpModeOut);
			CboNetworkIPv6Mode.AddItem(Messages.WindowsSettingsNetworkIpModeBlock);

			CboProtocolIPEntry.RemoveAllItems();
			CboProtocolIPEntry.AddItem("IPv6, IPv4");
			CboProtocolIPEntry.AddItem("IPv4, IPv6");
			CboProtocolIPEntry.AddItem("IPv6 only");
			CboProtocolIPEntry.AddItem("IPv4 only");

			CboNetworkEntryInterface.RemoveAllItems();
			m_mapNetworkEntryIFace[""] = "Automatic";
			CboNetworkEntryInterface.AddItem("Automatic");

			Json jNetworkInfo = Engine.Instance.Manifest["network_info"].Value as Json;
			foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
			{
				if ((bool)jNetworkInterface["bind"].Value)
				{
					foreach (string ip in jNetworkInterface["ips"].Json.GetArray())
					{
						string desc = jNetworkInterface["friendly"].Value as string + " - " + ip;
						m_mapNetworkEntryIFace[ip] = desc;
						CboNetworkEntryInterface.AddItem(desc);
					}
				}
			}

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

			// Network Lock

			CmdLockHelp.Activated += (object sender, EventArgs e) =>
			{
				Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["help"]["netlock"].Value as string);
			};
			CboLockMode.RemoveAllItems();
			CboLockMode.AddItem("None");
			CboLockMode.AddItem("Automatic");
			foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
			{
				CboLockMode.AddItem(lockPlugin.GetName());
			}
			CboLockIncoming.RemoveAllItems();
			CboLockIncoming.AddItem("Block");
			CboLockIncoming.AddItem("Allow");
			CboLockOutgoing.RemoveAllItems();
			CboLockOutgoing.AddItem("Block");
			CboLockOutgoing.AddItem("Allow");

			LblRoutesNetworkLockWarning.StringValue = Messages.WindowsSettingsRouteLockHelp;
			LblLockRoutingOutWarning.StringValue = Messages.NetworkLockNotAvailableWithRouteOut;

			// Advanced

			CmdAdvancedHelp.Activated += (object sender, EventArgs e) =>
			{
				Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["help"]["advanced"].Value as string);
			};

			CboIpV6.RemoveAllItems();
			CboIpV6.AddItem("None");
			CboIpV6.AddItem("Disable");

			CboAdvancedManifestRefresh.RemoveAllItems();
			CboAdvancedManifestRefresh.AddItem("Automatic");
			CboAdvancedManifestRefresh.AddItem("Never");
			CboAdvancedManifestRefresh.AddItem("Every minute");
			CboAdvancedManifestRefresh.AddItem("Every ten minute");
			CboAdvancedManifestRefresh.AddItem("Every one hour");

			CmdAdvancedOpenVpnPath.Activated += (object sender, EventArgs e) =>
			{
				GuiUtils.SelectFile(this.Window, TxtAdvancedOpenVpnPath);
			};

			// Logging

			TxtLoggingPath.Changed += (object sender, EventArgs e) =>
			{
				RefreshLogPreview();
			};

			CmdLoggingOpen.Activated += (object sender, EventArgs e) =>
			{
				List<string> paths = Engine.Instance.Logs.ParseLogFilePath(TxtLoggingPath.StringValue);
				foreach (string path in paths)
				{
					if (Core.Platform.Instance.OpenDirectoryInFileManager(path) == false)
						GuiUtils.MessageBoxError(MessagesFormatter.Format(Messages.WindowsSettingsLogsCannotOpenDirectory, path));
				}
			};

			// Directives
			CboOpenVpnDirectivesSkipDefault.RemoveAllItems();
			CboOpenVpnDirectivesSkipDefault.AddItem(Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip1);
			CboOpenVpnDirectivesSkipDefault.AddItem(Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip2);
			CmdOpenVpnDirectivesHelp.Activated += (object sender, EventArgs e) =>
			{
				Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["help"]["directives"].Value as string);
			};
			CmdOpenVpnDirectivesCustomPathBrowse.Activated += (object sender, EventArgs e) =>
			{
				GuiUtils.SelectFile(this.Window, TxtOpenVpnDirectivesCustomPath);
			};

			// Events

			TableAdvancedEvents.DoubleClick += (object sender, EventArgs e) =>
			{
				AdvancedEventEdit();
			};

			CmdAdvancedEventsEdit.Activated += (object sender, EventArgs e) =>
			{
				AdvancedEventEdit();
			};

			CmdAdvancedEventsClear.Activated += (object sender, EventArgs e) =>
			{
				AdvancedEventClear();

			};

			if (Constants.FeatureIPv6ControlOptions)
			{
				LblNetworkIPv4Mode.Hidden = false;
				LblNetworkIPv6Mode.Hidden = false;
				CboNetworkIPv4Mode.Hidden = false;
				CboNetworkIPv6Mode.Hidden = false;
				LblIPv6.Hidden = true;
				CboIpV6.Hidden = true;
				LblRoutesOtherwise.Hidden = true;
				CboRoutesOtherwise.Hidden = true;
			}
			else
			{
				LblNetworkIPv4Mode.Hidden = true;
				LblNetworkIPv6Mode.Hidden = true;
				CboNetworkIPv4Mode.Hidden = true;
				CboNetworkIPv6Mode.Hidden = true;
				LblIPv6.Hidden = false;
				CboIpV6.Hidden = false;
				LblRoutesOtherwise.Hidden = false;
				CboRoutesOtherwise.Hidden = false;
			}

			ReadOptions();

			EnableIde();

			RefreshLogPreview();

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

		void RouteAdd()
		{
			TableRoutingControllerItem item = new TableRoutingControllerItem();
			item.Ip = "";
			item.Icon = "out";
			item.Action = "out";
			item.Notes = "";

			WindowPreferencesRouteController.Item = item;
			WindowPreferencesRouteController dlg = new WindowPreferencesRouteController();
			dlg.Window.ReleasedWhenClosed = true;
			NSApplication.SharedApplication.RunModalForWindow(dlg.Window);
			dlg.Window.Close();

			if (dlg.Accepted)
			{
				TableRoutingController.Items.Add(item);
				TableRoutingController.RefreshUI();
			}

			this.EnableIde();
		}

		void RouteEdit()
		{
			nint i = TableRoutes.SelectedRow;
			if (i != -1)
			{
				TableRoutingControllerItem item = TableRoutingController.Items[(int)i];

				WindowPreferencesRouteController.Item = item;
				WindowPreferencesRouteController dlg = new WindowPreferencesRouteController();
				dlg.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow(dlg.Window);
				dlg.Window.Close();

				TableRoutingController.RefreshUI();
				this.EnableIde();
			}
		}

		void RouteRemove()
		{
			nint i = TableRoutes.SelectedRow;
			if (i != -1)
			{
				TableRoutingController.Items.RemoveAt((int)i);
				TableRoutingController.RefreshUI();
				this.EnableIde();
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
			nint i = TableDnsServers.SelectedRow;
			if (i != -1)
			{
				TableDnsServersController.RemoveAt((int)i);
				TableDnsServersController.RefreshUI();
				this.EnableIde();
			}
		}

		void DnsServersEdit()
		{
			nint i = TableDnsServers.SelectedRow;
			if (i != -1)
			{
				string dns = TableDnsServersController.Get((int)i);

				WindowPreferencesIpController.Ip = dns;
				WindowPreferencesIpController dlg = new WindowPreferencesIpController();
				dlg.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow(dlg.Window);
				dlg.Window.Close();

				if (dlg.Accepted)
				{
					TableDnsServersController.Set((int)i, WindowPreferencesIpController.Ip);
					TableDnsServersController.RefreshUI();
				}

				this.EnableIde();
			}
		}

		void AdvancedEventEdit()
		{
			nint index = TableAdvancedEvents.SelectedRow;

			WindowPreferencesEventController.Item = TableAdvancedEventsController.Items[(int)index];
			WindowPreferencesEventController dlg = new WindowPreferencesEventController();
			dlg.Window.ReleasedWhenClosed = true;

			NSApplication.SharedApplication.RunModalForWindow(dlg.Window);
			dlg.Window.Close();

			TableAdvancedEventsController.RefreshUI();
			this.EnableIde();
		}

		void AdvancedEventClear()
		{
			nint index = TableAdvancedEvents.SelectedRow;
			if (index != -1)
			{
				TableAdvancedEventsController.Items[(int)index].Filename = "";
				TableAdvancedEventsController.Items[(int)index].Arguments = "";
				TableAdvancedEventsController.Items[(int)index].WaitEnd = true;
				TableAdvancedEventsController.RefreshUI();
			}
			TableAdvancedEventsController.RefreshUI();
			this.EnableIde();
		}

		void RefreshLogPreview()
		{
			TxtLoggingComputedPath.StringValue = Engine.Instance.Logs.GetParseLogFilePaths(TxtLoggingPath.StringValue);
		}

		void ReadOptionsEvent(string name, int index)
		{
			Storage s = Engine.Instance.Storage;

			string filename = s.Get("event." + name + ".filename");
			if (filename != "")
			{
				TableAdvancedEventsController.Items[index].Filename = filename;
				TableAdvancedEventsController.Items[index].Arguments = s.Get("event." + name + ".arguments");
				TableAdvancedEventsController.Items[index].WaitEnd = s.GetBool("event." + name + ".waitend");
				TableAdvancedEventsController.RefreshUI();
			}
		}

		void SaveOptionsEvent(string name, int index)
		{
			Storage s = Engine.Instance.Storage;

			TableAdvancedEventsControllerItem i = TableAdvancedEventsController.Items[index];
			s.Set("event." + name + ".filename", i.Filename);
			s.Set("event." + name + ".arguments", i.Arguments);
			s.SetBool("event." + name + ".waitend", i.WaitEnd);
		}

		void ReadOptions()
		{
			Storage s = Engine.Instance.Storage;

			// General

			GuiUtils.SetCheck(ChkConnect, s.GetBool("connect"));
			GuiUtils.SetCheck(ChkNetLock, s.GetBool("netlock"));

			GuiUtils.SetCheck(ChkGeneralStartLast, s.GetBool("servers.startlast"));
			GuiUtils.SetCheck(ChkGeneralOsxVisible, s.GetBool("gui.osx.visible"));
			// GuiUtils.SetCheck (ChkGeneralOsxDock, s.GetBool ("gui.osx.dock")); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			GuiUtils.SetCheck(ChkUiSystemBarShowInfo, s.GetBool("gui.osx.sysbar.show_info"));
			GuiUtils.SetCheck(ChkUiSystemBarShowSpeed, s.GetBool("gui.osx.sysbar.show_speed"));
			GuiUtils.SetCheck(ChkUiSystemBarShowServer, s.GetBool("gui.osx.sysbar.show_server"));

			GuiUtils.SetCheck(ChkExitConfirm, s.GetBool("gui.exit_confirm"));

			// UI
			GuiUtils.SetCheck(ChkGeneralOsxNotifications, s.GetBool("gui.notifications"));
			string uiUnit = s.Get("ui.unit");
			if (uiUnit == "bytes")
				GuiUtils.SetSelected(CboUiUnit, Messages.WindowsSettingsUiUnit1);
			else if (uiUnit == "bits")
				GuiUtils.SetSelected(CboUiUnit, Messages.WindowsSettingsUiUnit2);
			else
				GuiUtils.SetSelected(CboUiUnit, Messages.WindowsSettingsUiUnit0);
			GuiUtils.SetCheck(ChkUiIEC, s.GetBool("ui.iec"));
            GuiUtils.SetCheck(ChkUiSkipProviderManifestFailed, s.GetBool("ui.skip.provider.manifest.failed"));

			/*
			string interfaceMode = GuiUtils.InterfaceColorMode ();
			if (interfaceMode == "Dark")
				GuiUtils.SetSelected (CboGeneralOsxInterfaceStyle,"Dark");
			else
				GuiUtils.SetSelected (CboGeneralOsxInterfaceStyle,"Default");
			*/

			// Protocols
			String protocol = s.Get("mode.protocol").ToUpperInvariant();
			int port = s.GetInt("mode.port");
			int entryIP = s.GetInt("mode.alt");
			if (protocol == "AUTO")
			{
				GuiUtils.SetCheck(ChkProtocolsAutomatic, true);
			}
			else
			{
				bool found = false;

				int iRow = 0;
				foreach (TableProtocolsControllerItem itemProtocol in TableProtocolsController.Items)
				{
					if ((itemProtocol.Protocol == protocol) &&
					   (itemProtocol.Port == port) &&
					   (itemProtocol.IP == entryIP))
					{
						found = true;
						TableProtocols.SelectRow(iRow, false);
						TableProtocols.ScrollRowToVisible(iRow);
						break;
					}
					iRow++;
				}

				if (found == false)
					GuiUtils.SetCheck(ChkProtocolsAutomatic, true);
				else
					GuiUtils.SetCheck(ChkProtocolsAutomatic, false);
			}

			// Proxy

			GuiUtils.SetSelected(CboProxyType, s.Get("proxy.mode"));
			if (s.Get("proxy.when") == "always")
				GuiUtils.SetSelected(CboProxyWhen, Messages.WindowsSettingsProxyWhenAlways);
			else if (s.Get("proxy.when") == "web")
				GuiUtils.SetSelected(CboProxyWhen, Messages.WindowsSettingsProxyWhenWeb);
			else if (s.Get("proxy.when") == "openvpn")
				GuiUtils.SetSelected(CboProxyWhen, Messages.WindowsSettingsProxyWhenOpenVPN);
			else if (s.Get("proxy.when") == "none")
				GuiUtils.SetSelected(CboProxyWhen, Messages.WindowsSettingsProxyWhenNone);
			else
				GuiUtils.SetSelected(CboProxyWhen, Messages.WindowsSettingsProxyWhenAlways);
			TxtProxyHost.StringValue = s.Get("proxy.host");
			TxtProxyPort.StringValue = s.Get("proxy.port");
			GuiUtils.SetSelected(CboProxyAuthentication, s.Get("proxy.auth"));
			TxtProxyLogin.StringValue = s.Get("proxy.login");
			TxtProxyPassword.StringValue = s.Get("proxy.password");
			TxtProxyTorControlPort.StringValue = s.Get("proxy.tor.control.port");
			TxtProxyTorControlPassword.StringValue = s.Get("proxy.tor.control.password");

			// Routes
			GuiUtils.SetSelected(CboRoutesOtherwise, RouteDirectionToDescription(s.Get("routes.default")));

			string routes = s.Get("routes.custom");
			string[] routes2 = routes.Split(';');
			foreach (string route in routes2)
			{
				string[] routeEntries = route.Split(',');
				if (routeEntries.Length < 2)
					continue;

				TableRoutingControllerItem item = new TableRoutingControllerItem();
				item.Ip = routeEntries[0];
				item.Action = routeEntries[1];
				item.Icon = routeEntries[1];
				if (routeEntries.Length == 3)
					item.Notes = routeEntries[2];
				TableRoutingController.Items.Add(item);
			}

			TableRoutingController.RefreshUI();

			// DNS

			string dnsMode = s.Get("dns.mode");
			if (dnsMode == "none")
				GuiUtils.SetSelected(CboDnsSwitchMode, "Disabled");
			else
				GuiUtils.SetSelected(CboDnsSwitchMode, "Automatic");

			GuiUtils.SetCheck(ChkDnsCheck, s.GetBool("dns.check"));

			TableDnsServersController.Clear();
			string[] dnsServers = s.Get("dns.servers").Split(',');
			foreach (string dnsServer in dnsServers)
			{
				if (IpAddress.IsIP(dnsServer))
					TableDnsServersController.Add(dnsServer);
			}

			// Networking

			string networkIPv4Mode = s.Get("network.ipv4.mode");
			if (networkIPv4Mode == "in")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, Messages.WindowsSettingsNetworkIpModeInAlways);
			else if (networkIPv4Mode == "in-out")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, Messages.WindowsSettingsNetworkIpModeInOrOut);
			else if (networkIPv4Mode == "in-block")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, Messages.WindowsSettingsNetworkIpModeInOrBlock);
			else if (networkIPv4Mode == "out")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, Messages.WindowsSettingsNetworkIpModeOut);
			else if (networkIPv4Mode == "block")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, Messages.WindowsSettingsNetworkIpModeBlock);
			else
				GuiUtils.SetSelected(CboNetworkIPv4Mode, Messages.WindowsSettingsNetworkIpModeInOrBlock);

			string networkIPv6Mode = s.Get("network.ipv6.mode");
			if (networkIPv6Mode == "in")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, Messages.WindowsSettingsNetworkIpModeInAlways);
			else if (networkIPv6Mode == "in-out")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, Messages.WindowsSettingsNetworkIpModeInOrOut);
			else if (networkIPv6Mode == "in-block")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, Messages.WindowsSettingsNetworkIpModeInOrBlock);
			else if (networkIPv6Mode == "out")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, Messages.WindowsSettingsNetworkIpModeOut);
			else if (networkIPv6Mode == "block")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, Messages.WindowsSettingsNetworkIpModeBlock);
			else
				GuiUtils.SetSelected(CboNetworkIPv6Mode, Messages.WindowsSettingsNetworkIpModeInOrBlock);

			string networkEntryIpLayer = s.Get("network.entry.iplayer");
			if (networkEntryIpLayer == "ipv6-ipv4")
				GuiUtils.SetSelected(CboProtocolIPEntry, "IPv6, IPv4");
			else if (networkEntryIpLayer == "ipv4-ipv6")
				GuiUtils.SetSelected(CboProtocolIPEntry, "IPv4, IPv6");
			else if (networkEntryIpLayer == "ipv6-only")
				GuiUtils.SetSelected(CboProtocolIPEntry, "IPv6 only");
			else if (networkEntryIpLayer == "ipv4-only")
				GuiUtils.SetSelected(CboProtocolIPEntry, "IPv4 only");
			else
				GuiUtils.SetSelected(CboProtocolIPEntry, "IPv4, IPv6");

			string sNetworkEntryIFace = s.Get("network.entry.iface");
			foreach (KeyValuePair<string, string> kp in m_mapNetworkEntryIFace)
			{
				if (sNetworkEntryIFace == kp.Key)
				{
					GuiUtils.SetSelected(CboNetworkEntryInterface, kp.Value);
				}
			}

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

			GuiUtils.SetCheck(ChkRouteRemoveDefaultGateway, s.GetBool("routes.remove_default"));

			// Network Lock

			string lockMode = s.Get("netlock.mode");
			GuiUtils.SetSelected(CboLockMode, "None");
			if (lockMode == "auto")
				GuiUtils.SetSelected(CboLockMode, "Automatic");
			else
			{
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				{
					if (lockPlugin.GetCode() == lockMode)
					{
						GuiUtils.SetSelected(CboLockMode, lockPlugin.GetName());
					}
				}
			}
			string lockIncoming = s.Get("netlock.incoming");
			if (lockIncoming == "allow")
				GuiUtils.SetSelected(CboLockIncoming, "Allow");
			else
				GuiUtils.SetSelected(CboLockIncoming, "Block");
			string lockOutgoing = s.Get("netlock.outgoing");
			if (lockOutgoing == "allow")
				GuiUtils.SetSelected(CboLockOutgoing, "Allow");
			else
				GuiUtils.SetSelected(CboLockOutgoing, "Block");
			GuiUtils.SetCheck(ChkLockAllowPrivate, s.GetBool("netlock.allow_private"));
			GuiUtils.SetCheck(ChkLockAllowPing, s.GetBool("netlock.allow_ping"));
			GuiUtils.SetCheck(ChkLockAllowDNS, s.GetBool("netlock.allow_dns"));
			TxtLockAllowedIPS.StringValue = s.Get("netlock.allowed_ips");

			// Advanced

			GuiUtils.SetCheck(ChkAdvancedExpertMode, s.GetBool("advanced.expert"));
			GuiUtils.SetCheck(ChkAdvancedCheckRoute, s.GetBool("advanced.check.route"));
			string ipV6Mode = s.Get("ipv6.mode");
			if (ipV6Mode == "none")
				GuiUtils.SetSelected(CboIpV6, "None");
			else if (ipV6Mode == "disable")
				GuiUtils.SetSelected(CboIpV6, "Disable");
			else
				GuiUtils.SetSelected(CboIpV6, "None");

			GuiUtils.SetCheck(ChkAdvancedPingerEnabled, s.GetBool("pinger.enabled"));


			TxtAdvancedOpenVpnPath.StringValue = s.Get("tools.openvpn.path");
			GuiUtils.SetCheck(ChkAdvancedSkipAlreadyRun, s.GetBool("advanced.skip_alreadyrun"));
			GuiUtils.SetCheck(ChkAdvancedProviders, s.GetBool("advanced.providers"));

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

			// Logging
			GuiUtils.SetCheck(ChkLoggingEnabled, s.GetBool("log.file.enabled"));
			GuiUtils.SetCheck(ChkLogLevelDebug, s.GetBool("log.level.debug"));
			TxtLoggingPath.StringValue = s.Get("log.file.path");

			// OVPN Directives
			GuiUtils.SetSelected(CboOpenVpnDirectivesSkipDefault, (s.GetBool("openvpn.skip_defaults") ? Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip2 : Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip1));
			TxtAdvancedOpenVpnDirectivesDefault.StringValue = s.Get("openvpn.directives");
			TxtAdvancedOpenVpnDirectivesCustom.StringValue = s.Get("openvpn.custom");
			TxtOpenVpnDirectivesCustomPath.StringValue = s.Get("openvpn.directives.path");

			// Events
			ReadOptionsEvent("app.start", 0);
			ReadOptionsEvent("app.stop", 1);
			ReadOptionsEvent("session.start", 2);
			ReadOptionsEvent("session.stop", 3);
			ReadOptionsEvent("vpn.pre", 4);
			ReadOptionsEvent("vpn.up", 5);
			ReadOptionsEvent("vpn.down", 6);

			TableAdvancedEventsController.RefreshUI();
		}

		bool Check()
		{
			if ((RouteDescriptionToDirection(GuiUtils.GetSelected(CboRoutesOtherwise)) == "out") && (TableRoutingController.Items.Count == 0))
			{
				if (GuiUtils.MessageYesNo(Messages.WindowsSettingsRouteOutEmptyList) == false)
					return false;
			}

			if (GuiUtils.GetCheck(ChkLockAllowDNS) == false)
			{
				bool hostNameUsed = false;
				foreach (TableRoutingControllerItem item in TableRoutingController.Items)
				{
					if (IpAddress.IsIP(item.Ip) == false)
					{
						hostNameUsed = true;
						break;
					}
				}

				if (hostNameUsed)
                    if (GuiUtils.MessageYesNo(Messages.WindowsSettingsRouteWithHostname) == false)
						return false;
			}

			return true;
		}

		void SaveOptions()
		{
			Storage s = Engine.Instance.Storage;

			// General

			s.SetBool("connect", GuiUtils.GetCheck(ChkConnect));
			s.SetBool("netlock", GuiUtils.GetCheck(ChkNetLock));


			s.SetBool("servers.startlast", GuiUtils.GetCheck(ChkGeneralStartLast));
			s.SetBool("gui.osx.visible", GuiUtils.GetCheck(ChkGeneralOsxVisible));
			// s.SetBool ("gui.osx.dock", GuiUtils.GetCheck (ChkGeneralOsxDock)); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			s.SetBool("gui.osx.sysbar.show_info", GuiUtils.GetCheck(ChkUiSystemBarShowInfo));
			s.SetBool("gui.osx.sysbar.show_speed", GuiUtils.GetCheck(ChkUiSystemBarShowSpeed));
			s.SetBool("gui.osx.sysbar.show_server", GuiUtils.GetCheck(ChkUiSystemBarShowServer));
			s.SetBool("gui.exit_confirm", GuiUtils.GetCheck(ChkExitConfirm));

			// UI

			s.SetBool("gui.notifications", GuiUtils.GetCheck(ChkGeneralOsxNotifications));
			string uiUnit = "";
			if (GuiUtils.GetSelected(CboUiUnit) == Messages.WindowsSettingsUiUnit1)
				uiUnit = "bytes";
			else if (GuiUtils.GetSelected(CboUiUnit) == Messages.WindowsSettingsUiUnit2)
				uiUnit = "bits";
			s.Set("ui.unit", uiUnit);
			s.SetBool("ui.iec", GuiUtils.GetCheck(ChkUiIEC));
            s.SetBool("ui.skip.provider.manifest.failed", GuiUtils.GetCheck(ChkUiSkipProviderManifestFailed));

			// Protocols

			if (GuiUtils.GetCheck(ChkProtocolsAutomatic))
			{
				s.Set("mode.protocol", "AUTO");
				s.SetInt("mode.port", 443);
				s.SetInt("mode.alt", 0);
			}
			else if (TableProtocols.SelectedRowCount == 1)
			{
				TableProtocolsControllerItem itemProtocol = TableProtocolsController.Items[(int)TableProtocols.SelectedRow];
				s.Set("mode.protocol", itemProtocol.Protocol);
				s.SetInt("mode.port", itemProtocol.Port);
				s.SetInt("mode.alt", itemProtocol.IP);
			}
			else
			{
				s.Set("mode.protocol", "AUTO");
				s.SetInt("mode.port", 443);
				s.SetInt("mode.alt", 0);
			}

			// Proxy

			s.Set("proxy.mode", GuiUtils.GetSelected(CboProxyType));

			if (GuiUtils.GetSelected(CboProxyWhen) == Messages.WindowsSettingsProxyWhenAlways)
				s.Set("proxy.when", "always");
			else if (GuiUtils.GetSelected(CboProxyWhen) == Messages.WindowsSettingsProxyWhenWeb)
				s.Set("proxy.when", "web");
			else if (GuiUtils.GetSelected(CboProxyWhen) == Messages.WindowsSettingsProxyWhenOpenVPN)
				s.Set("proxy.when", "openvpn");
			else if (GuiUtils.GetSelected(CboProxyWhen) == Messages.WindowsSettingsProxyWhenNone)
				s.Set("proxy.when", "none");
			else
				s.Set("proxy.when", "always");

			s.Set("proxy.host", TxtProxyHost.StringValue);
			s.SetInt("proxy.port", Conversions.ToInt32(TxtProxyPort.StringValue));
			s.Set("proxy.auth", GuiUtils.GetSelected(CboProxyAuthentication));
			s.Set("proxy.login", TxtProxyLogin.StringValue);
			s.Set("proxy.password", TxtProxyPassword.StringValue);
			s.SetInt("proxy.tor.control.port", Conversions.ToInt32(TxtProxyTorControlPort.StringValue));
			s.Set("proxy.tor.control.password", TxtProxyTorControlPassword.StringValue);

			// Routes
			s.Set("routes.default", RouteDescriptionToDirection(GuiUtils.GetSelected(CboRoutesOtherwise)));

			string routes = "";
			foreach (TableRoutingControllerItem item in TableRoutingController.Items)
			{
				if (routes != "")
					routes += ";";
				routes += item.Ip + "," + item.Action + "," + item.Notes;
			}
			s.Set("routes.custom", routes);

			// DNS

			string dnsMode = GuiUtils.GetSelected(CboDnsSwitchMode);
			if (dnsMode == "Disabled")
				s.Set("dns.mode", "none");
			else
				s.Set("dns.mode", "auto");
			s.SetBool("dns.check", GuiUtils.GetCheck(ChkDnsCheck));

			string dnsServers = "";
			for (int i = 0; i < TableDnsServersController.GetCount(); i++)
			{
				if (dnsServers != "")
					dnsServers += ",";
				dnsServers += TableDnsServersController.Get(i);
			}
			s.Set("dns.servers", dnsServers);

			// Networking

			string networkIPv4Mode = GuiUtils.GetSelected(CboNetworkIPv4Mode);
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

			string networkIPv6Mode = GuiUtils.GetSelected(CboNetworkIPv6Mode);
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

			string networkEntryIpLayer = GuiUtils.GetSelected(CboProtocolIPEntry);
			if (networkEntryIpLayer == "IPv6, IPv4")
				s.Set("network.entry.iplayer", "ipv6-ipv4");
			else if (networkEntryIpLayer == "IPv4, IPv6")
				s.Set("network.entry.iplayer", "ipv4-ipv6");
			else if (networkEntryIpLayer == "IPv6 only")
				s.Set("network.entry.iplayer", "ipv6-only");
			else if (networkEntryIpLayer == "IPv4 only")
				s.Set("network.entry.iplayer", "ipv4-only");
			else
				s.Set("network.entry.iplayer", "ipv4-ipv6");

			string tNetworkEntryIFace = GuiUtils.GetSelected(CboNetworkEntryInterface);
			foreach (KeyValuePair<string, string> kp in m_mapNetworkEntryIFace)
			{
				if (kp.Value == tNetworkEntryIFace)
				{
					s.Set("network.entry.iface", kp.Key);
					break;
				}
			}

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

			s.SetBool("routes.remove_default", GuiUtils.GetCheck(ChkRouteRemoveDefaultGateway));

			// Network Lock

			string lockMode = GuiUtils.GetSelected(CboLockMode);
			s.Set("netlock.mode", "none");
			if (lockMode == "Automatic")
			{
				s.Set("netlock.mode", "auto");
			}
			else
			{
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				{
					if (lockPlugin.GetName() == lockMode)
					{
						s.Set("netlock.mode", lockPlugin.GetCode());
					}
				}
			}
			string lockIncoming = GuiUtils.GetSelected(CboLockIncoming);
			if (lockIncoming == "Allow")
				s.Set("netlock.incoming", "allow");
			else
				s.Set("netlock.incoming", "block");
			string lockOutgoing = GuiUtils.GetSelected(CboLockOutgoing);
			if (lockOutgoing == "Allow")
				s.Set("netlock.outgoing", "allow");
			else
				s.Set("netlock.outgoing", "block");
			s.SetBool("netlock.allow_private", GuiUtils.GetCheck(ChkLockAllowPrivate));
			s.SetBool("netlock.allow_ping", GuiUtils.GetCheck(ChkLockAllowPing));
			s.SetBool("netlock.allow_dns", GuiUtils.GetCheck(ChkLockAllowDNS));
			s.Set("netlock.allowed_ips", TxtLockAllowedIPS.StringValue);

			// Advanced - General
			s.SetBool("advanced.expert", GuiUtils.GetCheck(ChkAdvancedExpertMode));

			s.SetBool("advanced.check.route", GuiUtils.GetCheck(ChkAdvancedCheckRoute));
			string ipV6Mode = GuiUtils.GetSelected(CboIpV6);
			if (ipV6Mode == "None")
				s.Set("ipv6.mode", "none");
			else if (ipV6Mode == "Disable")
				s.Set("ipv6.mode", "disable");
			else
				s.Set("ipv6.mode", "disable");
			s.SetBool("pinger.enabled", GuiUtils.GetCheck(ChkAdvancedPingerEnabled));


			s.Set("tools.openvpn.path", TxtAdvancedOpenVpnPath.StringValue);
			s.SetBool("advanced.skip_alreadyrun", GuiUtils.GetCheck(ChkAdvancedSkipAlreadyRun));
			s.SetBool("advanced.providers", GuiUtils.GetCheck(ChkAdvancedProviders));

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

			// Logging
			s.SetBool("log.file.enabled", GuiUtils.GetCheck(ChkLoggingEnabled));
			s.SetBool("log.level.debug", GuiUtils.GetCheck(ChkLogLevelDebug));
			s.Set("log.file.path", TxtLoggingPath.StringValue);

			// OVPN Directives
			s.SetBool("openvpn.skip_defaults", GuiUtils.GetSelected(CboOpenVpnDirectivesSkipDefault) == Messages.WindowsSettingsOpenVpnDirectivesDefaultSkip2);
			s.Set("openvpn.directives", TxtAdvancedOpenVpnDirectivesDefault.StringValue);
			s.Set("openvpn.custom", TxtAdvancedOpenVpnDirectivesCustom.StringValue);
			s.Set("openvpn.directives.path", TxtOpenVpnDirectivesCustomPath.StringValue);

			// Events
			SaveOptionsEvent("app.start", 0);
			SaveOptionsEvent("app.stop", 1);
			SaveOptionsEvent("session.start", 2);
			SaveOptionsEvent("session.stop", 3);
			SaveOptionsEvent("vpn.pre", 4);
			SaveOptionsEvent("vpn.up", 5);
			SaveOptionsEvent("vpn.down", 6);

			Engine.Instance.OnSettingsChanged();
		}

		public void EnableIde()
		{
			// Protocols
			TableProtocols.Enabled = (GuiUtils.GetCheck(ChkProtocolsAutomatic) == false);

			// Proxy
			bool proxy = (GuiUtils.GetSelected(CboProxyType) != "None");
			bool tor = (GuiUtils.GetSelected(CboProxyType) == "Tor");

			TxtProxyHost.Enabled = proxy;
			TxtProxyPort.Enabled = proxy;
			CboProxyWhen.Enabled = proxy;
			CboProxyAuthentication.Enabled = (proxy && !tor);
			TxtProxyLogin.Enabled = ((proxy) && (!tor) && (GuiUtils.GetSelected(CboProxyAuthentication) != "None"));
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
			LblLockRoutingOutWarning.Hidden = (GuiUtils.GetSelected(CboRoutesOtherwise) == RouteDirectionToDescription("in"));

			// Events
			CmdAdvancedEventsClear.Enabled = (TableAdvancedEvents.SelectedRowCount == 1);
			CmdAdvancedEventsEdit.Enabled = (TableAdvancedEvents.SelectedRowCount == 1);
		}
	}
}

