// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org )
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

			{
				// Deprecated, controls in xcode interface can be removed
				GuiUtils.SetHidden(LblIPv6, true);
				GuiUtils.SetHidden(CboIpV6, true);
				GuiUtils.SetHidden(LblRoutesOtherwise, true);
				GuiUtils.SetHidden(CboRoutesOtherwise, true);
				GuiUtils.SetHidden(LblLockRoutingOutWarning, true);
				GuiUtils.SetHidden(CmdGeneralTos, true);
			}

			Window.Title = Constants.Name + " - " + LanguageManager.GetText(LanguageItems.WindowsSettingsTitle);

			GuiUtils.SetButtonCancel(Window, CmdCancel);
            GuiUtils.SetButtonDefault(Window, CmdSave);

            TableTabsController = new TableTabsController(TableTabs, TabMain);

			ChkNetLock.Activated += (object sender, EventArgs e) =>
			{
				if (GuiUtils.GetCheck(ChkNetLock))
				{
                    if (UiClient.Instance.MainWindow.NetworkLockKnowledge() == false)
						GuiUtils.SetCheck(ChkNetLock, false);
				}
			};

			TableRoutes.Delegate = new TableRoutingDelegate(this);

			LblDnsServers.StringValue = LanguageManager.GetText(LanguageItems.WindowsSettingsDnsServers);
			TableDnsServers.Delegate = new TableDnsServersDelegate(this);

			TableAdvancedEvents.Delegate = new TableAdvancedEventsDelegate(this);

			LblLoggingHelp.StringValue = LanguageManager.GetText(LanguageItems.WindowsSettingsLoggingHelp);

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
						
			CmdResetToDefault.Activated += (object sender, EventArgs e) =>
			{
                if (GuiUtils.MessageYesNo(LanguageManager.GetText(LanguageItems.ResetSettingsConfirm)))
				{
					Engine.Instance.ProfileOptions.ResetAll(false);
					ReadOptions();
					GuiUtils.MessageBoxInfo(LanguageManager.GetText(LanguageItems.ResetSettingsDone));
				}
			};

            // General
            LblSystemService.StringValue = Core.Platform.Instance.AllowServiceUserDescription();
            CboStorageMode.RemoveAllItems();
            CboStorageMode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModeNone));
			CboStorageMode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModePassword));
			if (Core.Platform.Instance.OsCredentialSystemName() != "")
                CboStorageMode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModeOs, Core.Platform.Instance.OsCredentialSystemName()));
            CboStorageMode.Activated += (object sender, EventArgs e) =>
            {
                EnableIde();
            };

            // UI
            CboUiUnit.RemoveAllItems();
			CboUiUnit.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsUiUnit0));
			CboUiUnit.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsUiUnit1));
			CboUiUnit.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsUiUnit2));

			// Protocols
			CmdProtocolsHelp1.Activated += (object sender, EventArgs e) =>
			{
                GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["protocols"].Value as string);
			};

			CmdProtocolsHelp2.Activated += (object sender, EventArgs e) =>
			{
                GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["udp_vs_tcp"].Value as string);
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
			CboProxyWhen.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenAlways));
			CboProxyWhen.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenWeb));
			CboProxyWhen.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenOpenVPN));
			CboProxyWhen.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenNone));

			CmdProxyTorHelp.Activated += (object sender, EventArgs e) =>
			{
                GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["tor"].Value as string);
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
			CboNetworkIPv4Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInAlways));
			CboNetworkIPv4Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrOut));
			CboNetworkIPv4Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrBlock));
			CboNetworkIPv4Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeOut));
			CboNetworkIPv4Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeBlock));

			CboNetworkIPv6Mode.RemoveAllItems();
			CboNetworkIPv6Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInAlways));
			CboNetworkIPv6Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrOut));
			CboNetworkIPv6Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrBlock));
			CboNetworkIPv6Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeOut));
			CboNetworkIPv6Mode.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeBlock));

			CboProtocolIPEntry.RemoveAllItems();
			CboProtocolIPEntry.AddItem("IPv6, IPv4");
			CboProtocolIPEntry.AddItem("IPv4, IPv6");
			CboProtocolIPEntry.AddItem("IPv6 only");
			CboProtocolIPEntry.AddItem("IPv4 only");

			CboNetworkEntryInterface.RemoveAllItems();
			m_mapNetworkEntryIFace[""] = "Automatic";
			CboNetworkEntryInterface.AddItem("Automatic");

			Json jNetworkInfo = Engine.Instance.NetworkInfoGet();
			foreach (Json jNetworkInterface in jNetworkInfo["interfaces"].Json.GetArray())
			{
				if ((bool)jNetworkInterface["bind"].Value)
				{
					// Interface
					string ifaceId = jNetworkInterface["id"].ValueString;
					string desc = jNetworkInterface["friendly"].ValueString;
					m_mapNetworkEntryIFace[ifaceId] = desc;
					CboNetworkEntryInterface.AddItem(desc);
					
					// Specific IP
					foreach (string ip in jNetworkInterface["ips"].Json.GetArray())
					{
						string desc2 = desc + " - " + ip;
						m_mapNetworkEntryIFace[ip] = desc2;
						CboNetworkEntryInterface.AddItem(desc2);
					}
				}
			}

			LblOpenVpnRcvBuf.StringValue = LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnRcvBuf) + ":";
			LblOpenVpnSndBuf.StringValue = LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnSndBuf) + ":";
			CboOpenVpnRcvBuf.RemoveAllItems();
			CboOpenVpnRcvBuf.AddItem(LanguageManager.GetText(LanguageItems.Automatic));
			CboOpenVpnRcvBuf.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDefault));
			CboOpenVpnRcvBuf.AddItem("8 KB");
			CboOpenVpnRcvBuf.AddItem("16 KB");
			CboOpenVpnRcvBuf.AddItem("32 KB");
			CboOpenVpnRcvBuf.AddItem("64 KB");
			CboOpenVpnRcvBuf.AddItem("128 KB");
			CboOpenVpnRcvBuf.AddItem("256 KB");
			CboOpenVpnRcvBuf.AddItem("512 KB");
			CboOpenVpnSndBuf.RemoveAllItems();
			CboOpenVpnSndBuf.AddItem(LanguageManager.GetText(LanguageItems.Automatic));
			CboOpenVpnSndBuf.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDefault));
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
                GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["netlock"].Value as string);
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

			LblRoutesNetworkLockWarning.StringValue = LanguageManager.GetText(LanguageItems.WindowsSettingsRouteLockHelp);
			
			// Advanced

			CmdAdvancedHelp.Activated += (object sender, EventArgs e) =>
			{
                GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["advanced"].Value as string);
			};

			CboAdvancedManifestRefresh.RemoveAllItems();
			CboAdvancedManifestRefresh.AddItem("Automatic");
			CboAdvancedManifestRefresh.AddItem("Never");
			CboAdvancedManifestRefresh.AddItem("Every minute");
			CboAdvancedManifestRefresh.AddItem("Every ten minute");
			CboAdvancedManifestRefresh.AddItem("Every one hour");

            CboAdvancedUpdaterChannel.RemoveAllItems();
            CboAdvancedUpdaterChannel.AddItem("Stable");
            CboAdvancedUpdaterChannel.AddItem("Beta");
            CboAdvancedUpdaterChannel.AddItem("None");

			CmdAdvancedOpenVpnPath.Activated += (object sender, EventArgs e) =>
			{
				GuiUtils.SelectFile(this.Window, TxtAdvancedOpenVpnPath);
			};

			CmdHummingbirdPathBrowse.Activated += (object sender, EventArgs e) =>
			{
				GuiUtils.SelectFile(this.Window, TxtHummingbirdPath);
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
						GuiUtils.MessageBoxError(LanguageManager.GetText(LanguageItems.WindowsSettingsLogsCannotOpenDirectory, path));
				}
			};

			// OpenVPN Directives
			CboOpenVpnDirectivesSkipDefault.RemoveAllItems();
			CboOpenVpnDirectivesSkipDefault.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDirectivesDefaultSkip1));
			CboOpenVpnDirectivesSkipDefault.AddItem(LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDirectivesDefaultSkip2));
			CmdOpenVpnDirectivesHelp.Activated += (object sender, EventArgs e) =>
			{
                GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["directives"].Value as string);
			};
			CmdOpenVpnDirectivesCustomPathBrowse.Activated += (object sender, EventArgs e) =>
			{
				GuiUtils.SelectFile(this.Window, TxtOpenVpnDirectivesCustomPath);
			};

			// WireGuard

            CboWireGuardMTU.RemoveAllItems();
            CboWireGuardMTU.AddItem("Recommended (1320)");
            CboWireGuardMTU.AddItem("Omit (WG automatic)");
            CboWireGuardMTU.AddItem("1400");
			CboWireGuardMTU.AddItem("1392");
            CboWireGuardMTU.AddItem("1320");
            CboWireGuardMTU.AddItem("1280");

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

            CmdShellExternalView.Activated += (object sender, EventArgs e) => 
            {
                Json rules = Engine.Instance.ProfileOptions.GetJson("external.rules");
                Engine.Instance.OnShowText("Rules", rules.ToJsonPretty());
            };

            CmdShellExternalClear.Activated += (object sender, EventArgs e) => 
            {
                Engine.Instance.ProfileOptions.Set("external.rules", Engine.Instance.ProfileOptions.Dict["external.rules"].Default);
                GuiUtils.MessageBoxInfo("Done.");
            };

			// Disabled in this version
			GuiUtils.SetHidden(LblSystemStart, true);
			GuiUtils.SetHidden(ChkSystemStart, true);
			GuiUtils.SetHidden(LblShellExternal, true);
			GuiUtils.SetHidden(ChkShellExternalRecommended, true);
			GuiUtils.SetHidden(CmdShellExternalClear, true);
			GuiUtils.SetHidden(CmdShellExternalView, true);
			GuiUtils.SetHidden(ChkOpenVpnDirectivesAllowScriptSecurity, true);

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
			//dlg.Window.ReleaseWhenClosed(true); // Throw TrackReleasedWhenClosed exception. Xamarin issue, anyway deprecated.
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
                //dlg.Window.ReleaseWhenClosed(true); // Throw TrackReleasedWhenClosed exception. Xamarin issue, anyway deprecated.
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
            //dlg.Window.ReleaseWhenClosed(true); // Throw TrackReleasedWhenClosed exception. Xamarin issue, anyway deprecated.
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
                //dlg.Window.ReleaseWhenClosed(true); // Throw TrackReleasedWhenClosed exception. Xamarin issue, anyway deprecated.
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
            //dlg.Window.ReleaseWhenClosed(true); // Throw TrackReleasedWhenClosed exception. Xamarin issue, anyway deprecated.

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
			ProfileOptions o = Engine.Instance.ProfileOptions;

			string filename = o.Get("event." + name + ".filename");
			if (filename != "")
			{
				TableAdvancedEventsController.Items[index].Filename = filename;
				TableAdvancedEventsController.Items[index].Arguments = o.Get("event." + name + ".arguments");
				TableAdvancedEventsController.Items[index].WaitEnd = o.GetBool("event." + name + ".waitend");
				TableAdvancedEventsController.RefreshUI();
			}
		}

		void SaveOptionsEvent(string name, int index)
		{
			ProfileOptions o = Engine.Instance.ProfileOptions;

			TableAdvancedEventsControllerItem i = TableAdvancedEventsController.Items[index];
			o.Set("event." + name + ".filename", i.Filename);
			o.Set("event." + name + ".arguments", i.Arguments);
			o.SetBool("event." + name + ".waitend", i.WaitEnd);
		}

		void ReadOptions()
		{
			Storage s = Engine.Instance.Storage;
			ProfileOptions o = Engine.Instance.ProfileOptions;

            // General
            GuiUtils.SetCheck(ChkSystemStart, Core.Platform.Instance.GetAutoStart());
            GuiUtils.SetCheck(ChkSystemService, Core.Platform.Instance.GetService());
			GuiUtils.SetCheck(ChkConnect, o.GetBool("connect"));
			GuiUtils.SetCheck(ChkNetLock, o.GetBool("netlock"));

			GuiUtils.SetCheck(ChkGeneralStartLast, o.GetBool("servers.startlast"));
			GuiUtils.SetCheck(ChkGeneralOsxVisible, o.GetBool("gui.osx.visible"));
			// GuiUtils.SetCheck (ChkGeneralOsxDock, o.GetBool ("gui.osx.dock")); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			GuiUtils.SetCheck(ChkCliShortcut, Core.Platform.Instance.FileExists("/usr/local/bin/eddie-cli"));
			GuiUtils.SetCheck(ChkUiSystemBarShowInfo, o.GetBool("gui.osx.sysbar.show_info"));
			GuiUtils.SetCheck(ChkUiSystemBarShowSpeed, o.GetBool("gui.osx.sysbar.show_speed"));
			GuiUtils.SetCheck(ChkUiSystemBarShowServer, o.GetBool("gui.osx.sysbar.show_server"));

			GuiUtils.SetCheck(ChkExitConfirm, o.GetBool("gui.exit_confirm"));

            if (s.SaveFormat == "v2n")
                GuiUtils.SetSelected(CboStorageMode, LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModeNone));
            else if (s.SaveFormat == "v2p")
                GuiUtils.SetSelected(CboStorageMode, LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModePassword));
            else if ((s.SaveFormat == "v2s") && (Core.Platform.Instance.OsCredentialSystemName() != ""))
                GuiUtils.SetSelected(CboStorageMode, LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModeOs, Core.Platform.Instance.OsCredentialSystemName()));
            else
				GuiUtils.SetSelected(CboStorageMode, LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModeNone));

            if (s.SaveFormat == "v2p")
            {
                TxtStoragePassword.StringValue = s.SavePassword;
                TxtStoragePasswordConfirm.StringValue = s.SavePassword;
            }

            // UI
            GuiUtils.SetCheck(ChkGeneralOsxNotifications, o.GetBool("gui.notifications"));
			string uiUnit = o.Get("ui.unit");
			if (uiUnit == "bytes")
				GuiUtils.SetSelected(CboUiUnit, LanguageManager.GetText(LanguageItems.WindowsSettingsUiUnit1));
			else if (uiUnit == "bits")
				GuiUtils.SetSelected(CboUiUnit, LanguageManager.GetText(LanguageItems.WindowsSettingsUiUnit2));
			else
				GuiUtils.SetSelected(CboUiUnit, LanguageManager.GetText(LanguageItems.WindowsSettingsUiUnit0));
			GuiUtils.SetCheck(ChkUiIEC, o.GetBool("ui.iec"));
            GuiUtils.SetCheck(ChkUiSkipProviderManifestFailed, o.GetBool("ui.skip.provider.manifest.failed"));
            GuiUtils.SetCheck(ChkUiSkipPromotional, o.GetBool("ui.skip.promotional"));
			GuiUtils.SetCheck(ChkUiSkipNetlockConfirm, o.GetBool("ui.skip.netlock.confirm"));

			/*
			string interfaceMode = GuiUtils.InterfaceColorMode ();
			if (interfaceMode == "Dark")
				GuiUtils.SetSelected (CboGeneralOsxInterfaceStyle,"Dark");
			else
				GuiUtils.SetSelected (CboGeneralOsxInterfaceStyle,"Default");
			*/

			// Protocols
			string modeType = o.Get("mode.type").ToLowerInvariant();
			string protocol = o.Get("mode.protocol").ToUpperInvariant();
			int port = o.GetInt("mode.port");
			int entryIP = o.GetInt("mode.alt");
			if ((modeType != "auto") && (Engine.Instance.AirVPN != null))
			{
				ConnectionMode mode = Engine.Instance.AirVPN.GetMode();
				modeType = mode.Type;
				protocol = mode.Protocol;
				port = mode.Port;
				entryIP = mode.EntryIndex;
			}

			if (modeType == "auto")
			{
				GuiUtils.SetCheck(ChkProtocolsAutomatic, true);
			}
			else
			{
				bool found = false;

				int iRow = 0;
				foreach (TableProtocolsControllerItem itemProtocol in TableProtocolsController.Items)
				{
					if (
						(itemProtocol.Type == modeType) &&
						(itemProtocol.Protocol == protocol) &&
						(itemProtocol.Port == port) &&
						(itemProtocol.IP == entryIP)
						)
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

			GuiUtils.SetSelected(CboProxyType, o.Get("proxy.mode"));
			if (o.Get("proxy.when") == "always")
				GuiUtils.SetSelected(CboProxyWhen, LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenAlways));
			else if (o.Get("proxy.when") == "web")
				GuiUtils.SetSelected(CboProxyWhen, LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenWeb));
			else if (o.Get("proxy.when") == "openvpn")
				GuiUtils.SetSelected(CboProxyWhen, LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenOpenVPN));
			else if (o.Get("proxy.when") == "none")
				GuiUtils.SetSelected(CboProxyWhen, LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenNone));
			else
				GuiUtils.SetSelected(CboProxyWhen, LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenAlways));
			TxtProxyHost.StringValue = o.Get("proxy.host");
			TxtProxyPort.StringValue = o.Get("proxy.port");
			GuiUtils.SetSelected(CboProxyAuthentication, o.Get("proxy.auth"));
			TxtProxyLogin.StringValue = o.Get("proxy.login");
			TxtProxyPassword.StringValue = o.Get("proxy.password");
			TxtProxyTorControlPort.StringValue = o.Get("proxy.tor.control.port");
			TxtProxyTorControlPassword.StringValue = o.Get("proxy.tor.control.password");

			// Routes
			string routes = o.Get("routes.custom");
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

			string dnsMode = o.Get("dns.mode");
			if (dnsMode == "none")
				GuiUtils.SetSelected(CboDnsSwitchMode, "Disabled");
			else
				GuiUtils.SetSelected(CboDnsSwitchMode, "Automatic");

			GuiUtils.SetCheck(ChkDnsCheck, o.GetBool("dns.check"));

			TableDnsServersController.Clear();
			string[] dnsServers = o.Get("dns.servers").Split(',');
			foreach (string dnsServer in dnsServers)
			{
				if (IpAddress.IsIP(dnsServer))
					TableDnsServersController.Add(dnsServer);
			}

			// Networking

			string networkIPv4Mode = o.Get("network.ipv4.mode");
			if (networkIPv4Mode == "in")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInAlways));
			else if (networkIPv4Mode == "in-out")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrOut));
			else if (networkIPv4Mode == "in-block")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrBlock));
			else if (networkIPv4Mode == "out")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeOut));
			else if (networkIPv4Mode == "block")
				GuiUtils.SetSelected(CboNetworkIPv4Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeBlock));
			else
				GuiUtils.SetSelected(CboNetworkIPv4Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrBlock));
			GuiUtils.SetCheck(ChkNetworkIPv4AutoSwitch, o.GetBool("network.ipv4.autoswitch"));

			string networkIPv6Mode = o.Get("network.ipv6.mode");
			if (networkIPv6Mode == "in")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInAlways));
			else if (networkIPv6Mode == "in-out")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrOut));
			else if (networkIPv6Mode == "in-block")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrBlock));
			else if (networkIPv6Mode == "out")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeOut));
			else if (networkIPv6Mode == "block")
				GuiUtils.SetSelected(CboNetworkIPv6Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeBlock));
			else
				GuiUtils.SetSelected(CboNetworkIPv6Mode, LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrBlock));
			GuiUtils.SetCheck(ChkNetworkIPv6AutoSwitch, o.GetBool("network.ipv6.autoswitch"));

			string networkEntryIpLayer = o.Get("network.entry.iplayer");
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

			string sNetworkEntryIFace = o.Get("network.entry.iface");
			foreach (KeyValuePair<string, string> kp in m_mapNetworkEntryIFace)
			{
				if (sNetworkEntryIFace == kp.Key)
				{
					GuiUtils.SetSelected(CboNetworkEntryInterface, kp.Value);
				}
			}

			int openVpnSndBuf = o.GetInt("openvpn.sndbuf");
			if (openVpnSndBuf == -2)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, LanguageManager.GetText(LanguageItems.Automatic));
			else if (openVpnSndBuf == -1)
				GuiUtils.SetSelected(CboOpenVpnSndBuf, LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDefault));
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

			int openVpnRcvBuf = o.GetInt("openvpn.rcvbuf");
			if (openVpnRcvBuf == -2)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, LanguageManager.GetText(LanguageItems.Automatic));
			else if (openVpnRcvBuf == -1)
				GuiUtils.SetSelected(CboOpenVpnRcvBuf, LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDefault));
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

            GuiUtils.SetCheck(ChkNetworkingDnsLookupIPv6, o.GetBool("macos.ipv6.dnslookup"));

			// Network Lock
			GuiUtils.SetCheck(ChkNetLockConnection, o.GetBool("netlock.connection"));
            string lockMode = o.Get("netlock.mode");
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
			string lockIncoming = o.Get("netlock.incoming");
			if (lockIncoming == "allow")
				GuiUtils.SetSelected(CboLockIncoming, "Allow");
			else
				GuiUtils.SetSelected(CboLockIncoming, "Block");
			string lockOutgoing = o.Get("netlock.outgoing");
			if (lockOutgoing == "allow")
				GuiUtils.SetSelected(CboLockOutgoing, "Allow");
			else
				GuiUtils.SetSelected(CboLockOutgoing, "Block");
			GuiUtils.SetCheck(ChkLockAllowPrivate, o.GetBool("netlock.allow_private"));
			GuiUtils.SetCheck(ChkLockAllowPing, o.GetBool("netlock.allow_ping"));
			GuiUtils.SetCheck(ChkLockAllowDNS, o.GetBool("netlock.allow_dns"));
			TxtLockAllowlistIncomingIPs.StringValue = o.Get("netlock.allowlist.incoming.ips");
            TxtLockAllowlistOutgoingIPs.StringValue = o.Get("netlock.allowlist.outgoing.ips");

            // Advanced

            GuiUtils.SetCheck(ChkAdvancedExpertMode, o.GetBool("advanced.expert"));
			GuiUtils.SetCheck(ChkAdvancedCheckRoute, o.GetBool("advanced.check.route"));
			
			GuiUtils.SetCheck(ChkAdvancedPingerEnabled, o.GetBool("pinger.enabled"));


			TxtAdvancedOpenVpnPath.StringValue = o.Get("tools.openvpn.path");
			GuiUtils.SetCheck(ChkAdvancedSkipAlreadyRun, o.GetBool("advanced.skip_alreadyrun"));
			GuiUtils.SetCheck(ChkAdvancedProviders, o.GetBool("advanced.providers"));
			GuiUtils.SetCheck(ChkHummingbirdPrefer, o.GetBool("tools.hummingbird.preferred"));
			TxtHummingbirdPath.StringValue = o.Get("tools.hummingbird.path");

			if (Core.Platform.Instance.GetVersion().VersionUnder("10.14")) // Hummingbird require Mojave
			{
				GuiUtils.SetEnabled(ChkHummingbirdPrefer, false);
				GuiUtils.SetCheck(ChkHummingbirdPrefer, false);
			}

			int manifestRefresh = o.GetInt("advanced.manifest.refresh");
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

            string updaterChannel = o.Get("updater.channel");
            if (updaterChannel == "stable")
                GuiUtils.SetSelected(CboAdvancedUpdaterChannel, "Stable");
            else if(updaterChannel == "beta")
                GuiUtils.SetSelected(CboAdvancedUpdaterChannel, "Beta");
            else if(updaterChannel == "none")
                GuiUtils.SetSelected(CboAdvancedUpdaterChannel, "None");
            else
                GuiUtils.SetSelected(CboAdvancedUpdaterChannel, "Stable");

            // Logging
            GuiUtils.SetCheck(ChkLoggingEnabled, o.GetBool("log.file.enabled"));
			GuiUtils.SetCheck(ChkLogLevelDebug, o.GetBool("log.level.debug"));
			TxtLoggingPath.StringValue = o.Get("log.file.path");

			// OpenVPN Directives
			GuiUtils.SetSelected(CboOpenVpnDirectivesSkipDefault, (o.GetBool("openvpn.skip_defaults") ? LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDirectivesDefaultSkip2) : LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDirectivesDefaultSkip1)));
			TxtAdvancedOpenVpnDirectivesDefault.StringValue = o.Get("openvpn.directives");
			TxtAdvancedOpenVpnDirectivesCustom.StringValue = o.Get("openvpn.custom");
			TxtOpenVpnDirectivesCustomPath.StringValue = o.Get("openvpn.directives.path");
			//GuiUtils.SetCheck(ChkOpenVpnDirectivesAllowScriptSecurity, o.GetBool("openvpn.allow.script-security"));
			GuiUtils.SetCheck(ChkOpenVpnDirectivesChaCha, o.GetBool("openvpn.directives.chacha20"));

            // WireGuard

            string wireguardInterfaceMTU = o.Get("wireguard.interface.mtu");
			if (wireguardInterfaceMTU == "-1")
				GuiUtils.SetSelected(CboWireGuardMTU, "Recommended (1320)");
			else if (wireguardInterfaceMTU == "0")
				GuiUtils.SetSelected(CboWireGuardMTU, "Omit (WG automatic)");
			else if (wireguardInterfaceMTU == "1400")
				GuiUtils.SetSelected(CboWireGuardMTU, "1400");
			else if (wireguardInterfaceMTU == "1392")
				GuiUtils.SetSelected(CboWireGuardMTU, "1392");
			else if (wireguardInterfaceMTU == "1320")
				GuiUtils.SetSelected(CboWireGuardMTU, "1320");
			else if (wireguardInterfaceMTU == "1280")
				GuiUtils.SetSelected(CboWireGuardMTU, "1280");
			else
				GuiUtils.SetSelected(CboWireGuardMTU, "Recommended (1320)");

            // Events
            ReadOptionsEvent("app.start", 0);
			ReadOptionsEvent("app.stop", 1);
			ReadOptionsEvent("session.start", 2);
			ReadOptionsEvent("session.stop", 3);
			ReadOptionsEvent("vpn.pre", 4);
			ReadOptionsEvent("vpn.up", 5);
			ReadOptionsEvent("vpn.down", 6);
            GuiUtils.SetCheck(ChkShellExternalRecommended, o.GetBool("external.rules.recommended"));

			TableAdvancedEventsController.RefreshUI();
		}

		bool Check()
		{
            if (GuiUtils.GetSelected(CboStorageMode) == LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModePassword))
            {
                if( (TxtStoragePassword.StringValue == "") || (TxtStoragePassword.StringValue != TxtStoragePasswordConfirm.StringValue) )
                {
                    GuiUtils.MessageBoxError(LanguageManager.GetText(LanguageItems.WindowsSettingsStoragePasswordMismatch));
                    return false;
                }
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
                    if (GuiUtils.MessageYesNo(LanguageManager.GetText(LanguageItems.WindowsSettingsRouteWithHostname)) == false)
						return false;
			}

			return true;
		}

		void SaveOptions()
		{
			Storage s = Engine.Instance.Storage;
			ProfileOptions o = Engine.Instance.ProfileOptions;

            // General
            Core.Platform.Instance.SetAutoStart(GuiUtils.GetCheck(ChkSystemStart));
            Core.Platform.Instance.SetService(GuiUtils.GetCheck(ChkSystemService), false);
            o.SetBool("connect", GuiUtils.GetCheck(ChkConnect));
			o.SetBool("netlock", GuiUtils.GetCheck(ChkNetLock));


			o.SetBool("servers.startlast", GuiUtils.GetCheck(ChkGeneralStartLast));
			o.SetBool("gui.osx.visible", GuiUtils.GetCheck(ChkGeneralOsxVisible));
			// o.SetBool ("gui.osx.dock", GuiUtils.GetCheck (ChkGeneralOsxDock)); // See this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/

			string pathCLI = Core.Platform.Instance.GetExecutablePath();
			pathCLI = pathCLI.Substring(0, pathCLI.Length - 2) + "CLI";
			Engine.Instance.Elevated.DoCommandSync("shortcut-cli", "action", (GuiUtils.GetCheck(ChkCliShortcut) ? "set" : "del"), "path", pathCLI);

			o.SetBool("gui.osx.sysbar.show_info", GuiUtils.GetCheck(ChkUiSystemBarShowInfo));
			o.SetBool("gui.osx.sysbar.show_speed", GuiUtils.GetCheck(ChkUiSystemBarShowSpeed));
			o.SetBool("gui.osx.sysbar.show_server", GuiUtils.GetCheck(ChkUiSystemBarShowServer));
			o.SetBool("gui.exit_confirm", GuiUtils.GetCheck(ChkExitConfirm));



            if(GuiUtils.GetSelected(CboStorageMode) == LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModeNone))
			{
                s.SaveFormat = "v2n";
            }
            else if (GuiUtils.GetSelected(CboStorageMode) == LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModePassword))
			{
                s.SaveFormat = "v2p";
                s.SavePassword = TxtStoragePassword.StringValue;
            }
            else if (GuiUtils.GetSelected(CboStorageMode) == LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModeOs, Core.Platform.Instance.OsCredentialSystemName()))
			{
                s.SaveFormat = "v2s";
                s.SavePassword = s.LoadPassword;
			}

            // UI

            o.SetBool("gui.notifications", GuiUtils.GetCheck(ChkGeneralOsxNotifications));
			string uiUnit = "";
			if (GuiUtils.GetSelected(CboUiUnit) == LanguageManager.GetText(LanguageItems.WindowsSettingsUiUnit1))
				uiUnit = "bytes";
			else if (GuiUtils.GetSelected(CboUiUnit) == LanguageManager.GetText(LanguageItems.WindowsSettingsUiUnit2))
				uiUnit = "bits";
			o.Set("ui.unit", uiUnit);
			o.SetBool("ui.iec", GuiUtils.GetCheck(ChkUiIEC));
            o.SetBool("ui.skip.provider.manifest.failed", GuiUtils.GetCheck(ChkUiSkipProviderManifestFailed));
            o.SetBool("ui.skip.promotional", GuiUtils.GetCheck(ChkUiSkipPromotional));
			o.SetBool("ui.skip.netlock.confirm", GuiUtils.GetCheck(ChkUiSkipNetlockConfirm));

			// Protocols

			if (GuiUtils.GetCheck(ChkProtocolsAutomatic))
			{
				o.Set("mode.type", "auto");
				o.Set("mode.protocol", "udp");
				o.SetInt("mode.port", 443);
				o.SetInt("mode.alt", 0);
			}
			else if (TableProtocols.SelectedRowCount == 1)
			{
				TableProtocolsControllerItem itemProtocol = TableProtocolsController.Items[(int)TableProtocols.SelectedRow];
				o.Set("mode.type", itemProtocol.Type);
				o.Set("mode.protocol", itemProtocol.Protocol);
				o.SetInt("mode.port", itemProtocol.Port);
				o.SetInt("mode.alt", itemProtocol.IP);
			}
			else
			{
				o.Set("mode.type", "auto");
				o.Set("mode.protocol", "udp");
				o.SetInt("mode.port", 443);
				o.SetInt("mode.alt", 0);
			}

			// Proxy

			o.Set("proxy.mode", GuiUtils.GetSelected(CboProxyType));

			if (GuiUtils.GetSelected(CboProxyWhen) == LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenAlways))
				o.Set("proxy.when", "always");
			else if (GuiUtils.GetSelected(CboProxyWhen) == LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenWeb))
				o.Set("proxy.when", "web");
			else if (GuiUtils.GetSelected(CboProxyWhen) == LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenOpenVPN))
				o.Set("proxy.when", "openvpn");
			else if (GuiUtils.GetSelected(CboProxyWhen) == LanguageManager.GetText(LanguageItems.WindowsSettingsProxyWhenNone))
				o.Set("proxy.when", "none");
			else
				o.Set("proxy.when", "always");

			o.Set("proxy.host", TxtProxyHost.StringValue);
			o.SetInt("proxy.port", Conversions.ToInt32(TxtProxyPort.StringValue));
			o.Set("proxy.auth", GuiUtils.GetSelected(CboProxyAuthentication));
			o.Set("proxy.login", TxtProxyLogin.StringValue);
			o.Set("proxy.password", TxtProxyPassword.StringValue);
			o.SetInt("proxy.tor.control.port", Conversions.ToInt32(TxtProxyTorControlPort.StringValue));
			o.Set("proxy.tor.control.password", TxtProxyTorControlPassword.StringValue);

			// Routes			
			string routes = "";
			foreach (TableRoutingControllerItem item in TableRoutingController.Items)
			{
				if (routes != "")
					routes += ";";
				routes += item.Ip + "," + item.Action + "," + item.Notes;
			}
			o.Set("routes.custom", routes);

			// DNS

			string dnsMode = GuiUtils.GetSelected(CboDnsSwitchMode);
			if (dnsMode == "Disabled")
				o.Set("dns.mode", "none");
			else
				o.Set("dns.mode", "auto");
			o.SetBool("dns.check", GuiUtils.GetCheck(ChkDnsCheck));

			string dnsServers = "";
			for (int i = 0; i < TableDnsServersController.GetCount(); i++)
			{
				if (dnsServers != "")
					dnsServers += ",";
				dnsServers += TableDnsServersController.Get(i);
			}
			o.Set("dns.servers", dnsServers);

			// Networking

			string networkIPv4Mode = GuiUtils.GetSelected(CboNetworkIPv4Mode);
			if (networkIPv4Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInAlways))
				o.Set("network.ipv4.mode", "in");
			else if (networkIPv4Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrOut))
				o.Set("network.ipv4.mode", "in-out");
			else if (networkIPv4Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrBlock))
				o.Set("network.ipv4.mode", "in-block");
			else if (networkIPv4Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeOut))
				o.Set("network.ipv4.mode", "out");
			else if (networkIPv4Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeBlock))
				o.Set("network.ipv4.mode", "block");
			else
				o.Set("network.ipv4.mode", "in");
            o.SetBool("network.ipv4.autoswitch", GuiUtils.GetCheck(ChkNetworkIPv4AutoSwitch));

			string networkIPv6Mode = GuiUtils.GetSelected(CboNetworkIPv6Mode);
			if (networkIPv6Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInAlways))
				o.Set("network.ipv6.mode", "in");
			else if (networkIPv6Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrOut))
				o.Set("network.ipv6.mode", "in-out");
			else if (networkIPv6Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeInOrBlock))
				o.Set("network.ipv6.mode", "in-block");
			else if (networkIPv6Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeOut))
				o.Set("network.ipv6.mode", "out");
			else if (networkIPv6Mode == LanguageManager.GetText(LanguageItems.WindowsSettingsNetworkIpModeBlock))
				o.Set("network.ipv6.mode", "block");
			else
				o.Set("network.ipv6.mode", "in-block");
            o.SetBool("network.ipv6.autoswitch", GuiUtils.GetCheck(ChkNetworkIPv6AutoSwitch));

			string networkEntryIpLayer = GuiUtils.GetSelected(CboProtocolIPEntry);
			if (networkEntryIpLayer == "IPv6, IPv4")
				o.Set("network.entry.iplayer", "ipv6-ipv4");
			else if (networkEntryIpLayer == "IPv4, IPv6")
				o.Set("network.entry.iplayer", "ipv4-ipv6");
			else if (networkEntryIpLayer == "IPv6 only")
				o.Set("network.entry.iplayer", "ipv6-only");
			else if (networkEntryIpLayer == "IPv4 only")
				o.Set("network.entry.iplayer", "ipv4-only");
			else
				o.Set("network.entry.iplayer", "ipv4-ipv6");

			string tNetworkEntryIFace = GuiUtils.GetSelected(CboNetworkEntryInterface);
			foreach (KeyValuePair<string, string> kp in m_mapNetworkEntryIFace)
			{
				if (kp.Value == tNetworkEntryIFace)
				{
					o.Set("network.entry.iface", kp.Key);
					break;
				}
			}

			string openVpnSndBuf = GuiUtils.GetSelected(CboOpenVpnSndBuf);
			if (openVpnSndBuf == LanguageManager.GetText(LanguageItems.Automatic))
				o.SetInt("openvpn.sndbuf", -2);
			else if (openVpnSndBuf == LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDefault))
				o.SetInt("openvpn.sndbuf", -1);
			else if (openVpnSndBuf == "8 KB")
				o.SetInt("openvpn.sndbuf", 1024 * 8);
			else if (openVpnSndBuf == "16 KB")
				o.SetInt("openvpn.sndbuf", 1024 * 16);
			else if (openVpnSndBuf == "32 KB")
				o.SetInt("openvpn.sndbuf", 1024 * 32);
			else if (openVpnSndBuf == "64 KB")
				o.SetInt("openvpn.sndbuf", 1024 * 64);
			else if (openVpnSndBuf == "128 KB")
				o.SetInt("openvpn.sndbuf", 1024 * 128);
			else if (openVpnSndBuf == "256 KB")
				o.SetInt("openvpn.sndbuf", 1024 * 256);
			else if (openVpnSndBuf == "512 KB")
				o.SetInt("openvpn.sndbuf", 1024 * 512);

			string openVpnRcvBuf = GuiUtils.GetSelected(CboOpenVpnRcvBuf);
			if (openVpnRcvBuf == LanguageManager.GetText(LanguageItems.Automatic))
				o.SetInt("openvpn.rcvbuf", -2);
			else if (openVpnRcvBuf == LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDefault))
				o.SetInt("openvpn.rcvbuf", -1);
			else if (openVpnRcvBuf == "8 KB")
				o.SetInt("openvpn.rcvbuf", 1024 * 8);
			else if (openVpnRcvBuf == "16 KB")
				o.SetInt("openvpn.rcvbuf", 1024 * 16);
			else if (openVpnRcvBuf == "32 KB")
				o.SetInt("openvpn.rcvbuf", 1024 * 32);
			else if (openVpnRcvBuf == "64 KB")
				o.SetInt("openvpn.rcvbuf", 1024 * 64);
			else if (openVpnRcvBuf == "128 KB")
				o.SetInt("openvpn.rcvbuf", 1024 * 128);
			else if (openVpnRcvBuf == "256 KB")
				o.SetInt("openvpn.rcvbuf", 1024 * 256);
			else if (openVpnRcvBuf == "512 KB")
				o.SetInt("openvpn.rcvbuf", 1024 * 512);

            o.SetBool("macos.ipv6.dnslookup", GuiUtils.GetCheck(ChkNetworkingDnsLookupIPv6));

            // Network Lock
			            
			o.Set("netlock.connection", GuiUtils.GetCheck(ChkNetLockConnection));
            string lockMode = GuiUtils.GetSelected(CboLockMode);
			o.Set("netlock.mode", "none");
			if (lockMode == "Automatic")
			{
				o.Set("netlock.mode", "auto");
			}
			else
			{
				foreach (NetworkLockPlugin lockPlugin in Engine.Instance.NetworkLockManager.Modes)
				{
					if (lockPlugin.GetName() == lockMode)
					{
						o.Set("netlock.mode", lockPlugin.GetCode());
					}
				}
			}
			string lockIncoming = GuiUtils.GetSelected(CboLockIncoming);
			if (lockIncoming == "Allow")
				o.Set("netlock.incoming", "allow");
			else
				o.Set("netlock.incoming", "block");
			string lockOutgoing = GuiUtils.GetSelected(CboLockOutgoing);
			if (lockOutgoing == "Allow")
				o.Set("netlock.outgoing", "allow");
			else
				o.Set("netlock.outgoing", "block");
			o.SetBool("netlock.allow_private", GuiUtils.GetCheck(ChkLockAllowPrivate));
			o.SetBool("netlock.allow_ping", GuiUtils.GetCheck(ChkLockAllowPing));
			o.SetBool("netlock.allow_dns", GuiUtils.GetCheck(ChkLockAllowDNS));
			o.Set("netlock.allowlist.incoming.ips", TxtLockAllowlistIncomingIPs.StringValue);
            o.Set("netlock.allowlist.outgoing.ips", TxtLockAllowlistOutgoingIPs.StringValue);

            // Advanced - General
            o.SetBool("advanced.expert", GuiUtils.GetCheck(ChkAdvancedExpertMode));

			o.SetBool("advanced.check.route", GuiUtils.GetCheck(ChkAdvancedCheckRoute));
			
			o.SetBool("pinger.enabled", GuiUtils.GetCheck(ChkAdvancedPingerEnabled));


			o.Set("tools.openvpn.path", TxtAdvancedOpenVpnPath.StringValue);
			o.SetBool("advanced.skip_alreadyrun", GuiUtils.GetCheck(ChkAdvancedSkipAlreadyRun));
			o.SetBool("advanced.providers", GuiUtils.GetCheck(ChkAdvancedProviders));
			o.SetBool("tools.hummingbird.preferred", GuiUtils.GetCheck(ChkHummingbirdPrefer));
			o.Set("tools.hummingbird.path", TxtHummingbirdPath.StringValue);

			string manifestRefresh = GuiUtils.GetSelected(CboAdvancedManifestRefresh);
			if (manifestRefresh == "Automatic") // Auto
				o.SetInt("advanced.manifest.refresh", -1);
			else if (manifestRefresh == "Never") // Never
				o.SetInt("advanced.manifest.refresh", 0);
			else if (manifestRefresh == "Every minute") // One minute
				o.SetInt("advanced.manifest.refresh", 1);
			else if (manifestRefresh == "Every ten minute") // Ten minute
				o.SetInt("advanced.manifest.refresh", 10);
			else if (manifestRefresh == "Every one hour") // One hour
				o.SetInt("advanced.manifest.refresh", 60);

            string updaterChannel = GuiUtils.GetSelected(CboAdvancedUpdaterChannel);
            if (updaterChannel == "Stable")
                o.Set("updater.channel", "stable");
            else if(updaterChannel == "Beta")
                o.Set("updater.channel", "beta");
            else if(updaterChannel == "None")
                o.Set("updater.channel", "none");
            else
                o.Set("updater.channel", "stable");

            // Logging
            o.SetBool("log.file.enabled", GuiUtils.GetCheck(ChkLoggingEnabled));
			o.SetBool("log.level.debug", GuiUtils.GetCheck(ChkLogLevelDebug));
			o.Set("log.file.path", TxtLoggingPath.StringValue);

			// OpenVPN Directives
			o.SetBool("openvpn.skip_defaults", GuiUtils.GetSelected(CboOpenVpnDirectivesSkipDefault) == LanguageManager.GetText(LanguageItems.WindowsSettingsOpenVpnDirectivesDefaultSkip2));
			o.Set("openvpn.directives", TxtAdvancedOpenVpnDirectivesDefault.StringValue);
			o.Set("openvpn.custom", TxtAdvancedOpenVpnDirectivesCustom.StringValue);
			o.Set("openvpn.directives.path", TxtOpenVpnDirectivesCustomPath.StringValue);
			//s.Set("openvpn.allow.script-security", GuiUtils.GetCheck(ChkOpenVpnDirectivesAllowScriptSecurity));
			o.SetBool("openvpn.directives.chacha20", GuiUtils.GetCheck(ChkOpenVpnDirectivesChaCha));

			// WireGuard
			string wireguardInterfaceMTU = GuiUtils.GetSelected(CboWireGuardMTU);
            if (wireguardInterfaceMTU == "Recommended (1320)")
                o.Set("wireguard.interface.mtu", "-1");
            else if (wireguardInterfaceMTU == "Omit (WG automatic)")
                o.Set("wireguard.interface.mtu", "0");
            else if (wireguardInterfaceMTU == "1400")
                o.Set("wireguard.interface.mtu", "1400");
			else if (wireguardInterfaceMTU == "1392")
                o.Set("wireguard.interface.mtu", "1392");
            else if (wireguardInterfaceMTU == "1320")
                o.Set("wireguard.interface.mtu", "1320");
            else if (wireguardInterfaceMTU == "1280")
                o.Set("wireguard.interface.mtu", "1280");
            else
                o.Set("wireguard.interface.mtu", "-1");

            // Events
            SaveOptionsEvent("app.start", 0);
			SaveOptionsEvent("app.stop", 1);
			SaveOptionsEvent("session.start", 2);
			SaveOptionsEvent("session.stop", 3);
			SaveOptionsEvent("vpn.pre", 4);
			SaveOptionsEvent("vpn.up", 5);
			SaveOptionsEvent("vpn.down", 6);
            o.SetBool("external.rules.recommended", GuiUtils.GetCheck(ChkShellExternalRecommended));

			Engine.Instance.OnSettingsChanged();
		}

		public void EnableIde()
		{
			// General
			GuiUtils.SetEnabled(TxtStoragePassword, (GuiUtils.GetSelected(CboStorageMode) == LanguageManager.GetText(LanguageItems.WindowsSettingsStorageModePassword)));
			GuiUtils.SetEnabled(TxtStoragePasswordConfirm, TxtStoragePassword.Enabled);

			// Protocols
			GuiUtils.SetEnabled(TableProtocols, (GuiUtils.GetCheck(ChkProtocolsAutomatic) == false));

			// Proxy
			bool proxy = (GuiUtils.GetSelected(CboProxyType) != "None");
			bool tor = (GuiUtils.GetSelected(CboProxyType) == "Tor");

			GuiUtils.SetEnabled(TxtProxyHost, proxy);
			GuiUtils.SetEnabled(TxtProxyPort, proxy);
			GuiUtils.SetEnabled(CboProxyWhen, proxy);
			GuiUtils.SetEnabled(CboProxyAuthentication, (proxy && !tor));
			GuiUtils.SetEnabled(TxtProxyLogin, ((proxy) && (!tor) && (GuiUtils.GetSelected(CboProxyAuthentication) != "None")));
			GuiUtils.SetEnabled(TxtProxyPassword, TxtProxyLogin.Enabled);
			GuiUtils.SetEnabled(TxtProxyTorControlPort, tor);
			GuiUtils.SetEnabled(TxtProxyTorControlPassword, tor);
			GuiUtils.SetEnabled(CmdProxyTorTest, tor);

			// Routing
			GuiUtils.SetEnabled(CmdRouteAdd, true);
			GuiUtils.SetEnabled(CmdRouteRemove, (TableRoutes.SelectedRowCount > 0));
			GuiUtils.SetEnabled(CmdRouteEdit, (TableRoutes.SelectedRowCount == 1));

			// DNS
			GuiUtils.SetEnabled(CmdDnsAdd, true);
			GuiUtils.SetEnabled(CmdDnsRemove, (TableDnsServers.SelectedRowCount > 0));
			GuiUtils.SetEnabled(CmdDnsEdit, (TableDnsServers.SelectedRowCount == 1));

			// Events
			GuiUtils.SetEnabled(CmdAdvancedEventsClear, (TableAdvancedEvents.SelectedRowCount == 1));
			GuiUtils.SetEnabled(CmdAdvancedEventsEdit, (TableAdvancedEvents.SelectedRowCount == 1));
		}
	}
}

