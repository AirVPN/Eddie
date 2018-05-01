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
using System.Globalization;
using System.Linq;
using System.Xml;
using Foundation;
using AppKit;
using Eddie.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public partial class MainWindowController : AppKit.NSWindowController
	{
		public TableProvidersController TableProvidersController;
		public TableServersController TableServersController;
		public TableAreasController TableAreasController;
		public TableLogsController TableLogsController;
		public TableStatsController TableStatsController;

		public bool ShutdownConfirmed = false;

		public NSStatusItem StatusItem;

		private WindowAboutController windowAbout;
		private WindowPreferencesController windowPreferences;
		private NSTabViewItem TabProviders;

		#region Constructors
		// Called when created from unmanaged code
		public MainWindowController(IntPtr handle) : base(handle)
		{
			Initialize();
		}
		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder) : base(coder)
		{
			Initialize();
		}
		// Call to load from the XIB/NIB file
		public MainWindowController() : base("MainWindow")
		{
			Initialize();
		}
		// Shared initialization code
		void Initialize()
		{
		}
		#endregion
		//strongly typed window accessor
		public new MainWindow Window
		{
			get
			{
				return (MainWindow)base.Window;
			}
		}


		public UI.Cocoa.Osx.Engine Engine
		{
			get
			{
				return Core.Engine.Instance as UI.Cocoa.Osx.Engine;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.Delegate = new MainWindowDelegate(this);

			Window.AcceptsMouseMovedEvents = true;

			TabProviders = TabMain.Items[1];

			CreateMenuBarIcon();

			LblVersion.StringValue = Constants.VersionDesc;

			ChkRemember.State = Engine.Storage.GetBool("remember") ? NSCellStateValue.On : NSCellStateValue.Off;
			ChkServersShowAll.State = NSCellStateValue.Off;
			GuiUtils.SetCheck(ChkServersLockCurrent, Engine.Storage.GetBool("servers.locklast"));
			GuiUtils.SetSelected(CboServersScoringRule, Engine.Storage.Get("servers.scoretype"));

			CboSpeedResolutions.RemoveAllItems();
			CboSpeedResolutions.AddItem(Messages.WindowsMainSpeedResolution1);
			CboSpeedResolutions.AddItem(Messages.WindowsMainSpeedResolution2);
			CboSpeedResolutions.AddItem(Messages.WindowsMainSpeedResolution3);
			CboSpeedResolutions.AddItem(Messages.WindowsMainSpeedResolution4);
			CboSpeedResolutions.AddItem(Messages.WindowsMainSpeedResolution5);
			CboSpeedResolutions.SelectItem(0);

			CmdConnect.Title = Messages.CommandConnect;
			LblConnect.StringValue = Messages.CommandConnectSubtitle;
			CmdDisconnect.Title = Messages.CommandDisconnect;
			CmdCancel.Title = Messages.CommandCancel;

			CboServersScoringRule.ToolTip = Messages.TooltipServersScoreType;
			ChkServersLockCurrent.ToolTip = Messages.TooltipServersLockCurrent;
			ChkServersShowAll.ToolTip = Messages.TooltipServersShowAll;
			CmdServersConnect.ToolTip = Messages.TooltipServersConnect;
			CmdServersUndefined.ToolTip = Messages.TooltipServersUndefined;
			CmdServersBlackList.ToolTip = Messages.TooltipServersBlackList;
			CmdServersWhiteList.ToolTip = Messages.TooltipServersWhiteList;
			CmdServersRename.ToolTip = Messages.TooltipServersRename;
			CmdServersMore.ToolTip = Messages.TooltipServersMore;
			CmdServersRefresh.ToolTip = Messages.TooltipServersRefresh;
			CmdAreasUndefined.ToolTip = Messages.TooltipAreasUndefined;
			CmdAreasBlackList.ToolTip = Messages.TooltipAreasBlackList;
			CmdAreasWhiteList.ToolTip = Messages.TooltipAreasWhiteList;
			CmdLogsCommand.ToolTip = Messages.TooltipLogsCommand;
			CmdLogsClean.ToolTip = Messages.TooltipLogsClean;
			CmdLogsCopy.ToolTip = Messages.TooltipLogsCopy;
			CmdLogsSave.ToolTip = Messages.TooltipLogsSave;
			CmdLogsSupport.ToolTip = Messages.TooltipLogsSupport;

			if (Engine.Storage.GetBool("remember"))
			{
				ChkRemember.State = NSCellStateValue.On;
				TxtLogin.StringValue = Engine.Storage.Get("login");
				TxtPassword.StringValue = Engine.Storage.Get("password");

			}

            CmdMainMenu.Activated += (object sender, EventArgs e) =>
            {
                CoreGraphics.CGPoint p = new CoreGraphics.CGPoint(this.Window.Frame.Left + 10, this.Window.Frame.Bottom - 40);
                //CoreGraphics.CGPoint p = new CoreGraphics.CGPoint(CmdMainMenu.Frame.Left + 10, CmdMainMenu.Frame.Top + 10);
                MnuTray.PopUpMenu(MnuTrayStatus, p, null);
            };

			ChkRemember.Activated += (object sender, EventArgs e) =>
			{
				Engine.Storage.SetBool("remember", ChkRemember.State == NSCellStateValue.On);
			};

			CmdLogin.Activated += (object sender, EventArgs e) =>
			{
				if (Engine.IsLogged() == false)
					Login();
				else
					Logout();
			};

			TxtLogin.Activated += (object sender, EventArgs e) =>
			{
				EnabledUI();
			};

			TxtPassword.Activated += (object sender, EventArgs e) =>
			{
				EnabledUI();
			};

			CboKey.Activated += (object sender, EventArgs e) =>
			{
				Engine.Instance.Storage.Set("key", CboKey.SelectedItem.Title);
			};

			CmdConnect.Activated += (object sender, EventArgs e) =>
			{
				Connect();
			};

			CmdDisconnect.Activated += (object sender, EventArgs e) =>
			{
				Disconnect();
			};

			CmdCancel.Activated += (object sender, EventArgs e) =>
			{
				Disconnect();
			};

			CmdNetworkLock.Activated += (object sender, EventArgs e) =>
			{
				if (Engine.Instance.NetworkLockManager.IsActive())
				{
					NetworkLockDeactivation();
				}
				else
				{
					NetworkLockActivation();
				}
			};

			TableProvidersController = new TableProvidersController(this.TableProviders);
			this.TableProviders.Delegate = new TableProvidersDelegate(this);

			CmdProviderAdd.Activated += (object sender, EventArgs e) =>
			{
				ProviderAdd();
			};

			CmdProviderRemove.Activated += (object sender, EventArgs e) =>
			{
				ProviderRemove();
			};

			CmdProviderEdit.Activated += (object sender, EventArgs e) =>
			{
				ProviderEdit();
			};

			TableProviders.DoubleClick += (object sender, EventArgs e) =>
			{
				ProviderEdit();
			};

			TableServers.DoubleClick += (object sender, EventArgs e) =>
			{
				ConnectManual();
			};

			CmdServersConnect.Activated += (object sender, EventArgs e) =>
			{
				ConnectManual();
			};

			CmdServersWhiteList.Activated += (object sender, EventArgs e) =>
			{
				ServersWhiteList();
			};

			CmdServersBlackList.Activated += (object sender, EventArgs e) =>
			{
				ServersBlackList();
			};

			CmdServersUndefined.Activated += (object sender, EventArgs e) =>
			{
				ServersUndefinedList();
			};

			CmdServersRename.Activated += (object sender, EventArgs e) =>
			{
				ServersRename();
			};

			CmdServersMore.Activated += (object sender, EventArgs e) =>
			{
				ServersMore();
			};

			CmdServersRefresh.Activated += (object sender, EventArgs e) =>
			{
				ServersRefresh();
			};

			MnuServersConnect.Activated += (object sender, EventArgs e) =>
			{
				ConnectManual();
			};

			MnuServersWhitelist.Activated += (object sender, EventArgs e) =>
			{
				ServersWhiteList();
			};

			MnuServersBlacklist.Activated += (object sender, EventArgs e) =>
			{
				ServersBlackList();
			};

			MnuServersUndefined.Activated += (object sender, EventArgs e) =>
			{
				ServersUndefinedList();
			};

			MnuServersRename.Activated += (object sender, EventArgs e) =>
			{
				ServersRename();
			};

			MnuServersMore.Activated += (object sender, EventArgs e) =>
			{
				ServersMore();
			};

			MnuServersRefresh.Activated += (object sender, EventArgs e) =>
			{
				ServersRefresh();
			};

			CmdAreasWhiteList.Activated += (object sender, EventArgs e) =>
			{
				AreasWhiteList();
			};

			CmdAreasBlackList.Activated += (object sender, EventArgs e) =>
			{
				AreasBlackList();
			};

			CmdAreasUndefined.Activated += (object sender, EventArgs e) =>
			{
				AreasUndefinedList();
			};

			MnuAreasWhitelist.Activated += (object sender, EventArgs e) =>
			{
				AreasWhiteList();
			};

			MnuAreasBlacklist.Activated += (object sender, EventArgs e) =>
			{
				AreasBlackList();
			};

			MnuAreasUndefined.Activated += (object sender, EventArgs e) =>
			{
				AreasUndefinedList();
			};

			ChkServersShowAll.Activated += (object sender, EventArgs e) =>
			{
				TableServersController.ShowAll = (ChkServersShowAll.State == NSCellStateValue.On);
				TableServersController.RefreshUI();
			};

			/*
			 * Xamarin Bug: https://bugzilla.xamarin.com/show_bug.cgi?id=12467
			 * Resolved with delegate
			 * 
			TableServers.SelectionDidChange += (object sender, EventArgs e) => {
				EnabledUI();
			};

			TableAreas.SelectionDidChange += (object sender, EventArgs e) => {
				EnabledUI();
			};
			*/

			ChkServersLockCurrent.Activated += (object sender, EventArgs e) =>
			{
				Engine.Storage.SetBool("servers.locklast", ChkServersLockCurrent.State == NSCellStateValue.On);
			};

			CboServersScoringRule.Activated += (object sender, EventArgs e) =>
			{
				Engine.Storage.Set("servers.scoretype", GuiUtils.GetSelected(CboServersScoringRule));

				RefreshUi(Engine.RefreshUiMode.Full);
			};

			CboSpeedResolutions.Activated += (object sender, EventArgs e) =>
			{
				(PnlChart as ChartView).Switch((int)CboSpeedResolutions.IndexOfItem(CboSpeedResolutions.SelectedItem));
			};


			CmdLogsClean.Activated += (object sender, EventArgs e) =>
			{
				TableLogsController.Clear();
			};

			CmdLogsSave.Activated += (object sender, EventArgs e) =>
			{
				LogsDoSave(false);
			};

			CmdLogsCopy.Activated += (object sender, EventArgs e) =>
			{
				LogsDoCopy(false);
			};

			CmdLogsSupport.Activated += (object sender, EventArgs e) =>
			{
				SupportReport();
			};

			MnuLogsCopyAll.Activated += (object sender, EventArgs e) =>
			{
				LogsDoCopy(false);
			};

			MnuLogsSaveAll.Activated += (object sender, EventArgs e) =>
			{
				LogsDoSave(false);
			};
			MnuLogsCopySelected.Activated += (object sender, EventArgs e) =>
			{
				LogsDoCopy(true);
			};
			MnuLogsSaveSelected.Activated += (object sender, EventArgs e) =>
			{
				LogsDoSave(true);
			};

			CmdLogsCommand.Activated += (object sender, EventArgs e) =>
			{
				WindowCommandController w = new WindowCommandController();
				NSApplication.SharedApplication.RunModalForWindow(w.Window);
				if (w.Command != "")
					Core.UI.App.RunCommandString(w.Command);
			};

			TableServersController = new TableServersController(this.TableServers);
			this.TableServers.Delegate = new TableServersDelegate(this);

			TableAreasController = new TableAreasController(this.TableAreas);
			this.TableAreas.Delegate = new TableAreasDelegate(this);

			TableLogsController = new TableLogsController(this.TableLogs);
			TableStatsController = new TableStatsController(this.TableStats);

			TableStats.DoubleClick += (object sender, EventArgs e) =>
			{
				TableStatsController.DoubleClickItem();
			};

			// Topbar Menu

			MnuTrayStatus.Activated += (object sender, EventArgs e) =>
			{
				Restore(sender);
			};

			MnuTrayConnect.Activated += (object sender, EventArgs e) =>
			{
				if (Engine.IsWaiting())
				{
					Disconnect();
				}
				else if (Engine.IsConnected())
				{
					Disconnect();
				}
				else if (Engine.CanConnect())
				{
					Connect();
				}
				else
				{
					Restore(sender);
				}
			};

			MnuTrayAbout.Activated += (object sender, EventArgs e) =>
			{
				ShowAbout();
			};

			MnuTrayPreferences.Activated += (object sender, EventArgs e) =>
			{
				ShowPreferences();
			};

			MnuTrayHome.Activated += (object sender, EventArgs e) =>
			{
				ShowHome();
			};

			MnuTrayClientArea.Activated += (object sender, EventArgs e) =>
			{
				ShowClientArea();
			};

			MnuTrayForwardingPorts.Activated += (object sender, EventArgs e) =>
			{
				ShowForwardingPorts();
			};

			MnuTrayRestore.Activated += (object sender, EventArgs e) =>
			{
				/* // 2.8
				if(Window.IsVisible)
					Minimize();
				else
					*/
                if (Window.IsVisible)
                    Minimize();
                else
				    Restore(sender);
			};

			MnuTrayQuit.Activated += (object sender, EventArgs e) =>
			{
				Shutdown();
			};

			CboServersScoringRule.ToolTip = Messages.TooltipServersScoreType;
			CmdAreasBlackList.ToolTip = Messages.TooltipAreasBlackList;

			Engine.MainWindow = this;
			Engine.UiStart();

			Engine.OnRefreshUi();

			SettingsChanged();

			RequestAttention();
		}

		public bool Shutdown()
		{
			if (Engine.AskExitConfirm())
			{
				bool result = GuiUtils.MessageYesNo(Messages.ExitConfirm);
				if (result == false)
				{
					Engine.Instance.OnExitRejected();
					return false;
				}
			}
			ShutdownConfirmed = true;
			if (windowAbout != null)
				windowAbout.Close();
			if (windowPreferences != null)
				windowPreferences.Close();

			Engine.Instance.RequestStop();
			return true;
		}

		public void RefreshUi(Engine.RefreshUiMode mode)
		{
			try
			{
				if ((mode == Engine.RefreshUiMode.MainMessage) || (mode == Engine.RefreshUiMode.Full))
				{

					if (Engine.CurrentServer != null)
					{
						ImgTopFlag.Image = NSImage.ImageNamed("flag_" + Engine.CurrentServer.CountryCode.ToLowerInvariant() + ".png");
					}
					else
					{
						ImgTopFlag.Image = NSImage.ImageNamed("notconnected.png");
					}

					LblWaiting1.StringValue = Engine.WaitMessage;

					if (Engine.IsWaiting())
					{
						ImgProgress.StartAnimation(this);
						ImgTopPanel.Image = NSImage.ImageNamed("topbar_osx_yellow.png");
						MnuTrayStatus.Image = NSImage.ImageNamed("status_yellow_16.png");
						LblTopStatus.StringValue = Engine.WaitMessage;

						TabOverview.SelectAt(1);

						CmdCancel.Hidden = (Engine.IsWaitingCancelAllowed() == false);
						CmdCancel.Enabled = (Engine.IsWaitingCancelPending() == false);
						MnuTrayConnect.Enabled = CmdCancel.Enabled;
					}
					else if (Engine.IsConnected())
					{
						ImgProgress.StopAnimation(this);
						ImgTopPanel.Image = NSImage.ImageNamed("topbar_osx_green.png");
						MnuTrayStatus.Image = NSImage.ImageNamed("status_green_16.png");
						LblTopStatus.StringValue = MessagesFormatter.Format(MessagesUi.TopBarConnected, Engine.CurrentServer.DisplayName);

						TabOverview.SelectAt(2);

						LblConnectedServerName.StringValue = Engine.CurrentServer.DisplayName;
						LblConnectedLocation.StringValue = Engine.CurrentServer.GetLocationForList();
						TxtConnectedExitIp.StringValue = Engine.ConnectionActive.ExitIPs.ToString();
						ImgConnectedCountry.Image = NSImage.ImageNamed("flag_" + Engine.CurrentServer.CountryCode.ToLowerInvariant() + ".png");
					}
					else
					{
						ImgProgress.StopAnimation(this);
						ImgTopPanel.Image = NSImage.ImageNamed("topbar_osx_red.png");
						MnuTrayStatus.Image = NSImage.ImageNamed("status_red_16.png");
						if (Engine.Instance.NetworkLockManager.IsActive())
							LblTopStatus.StringValue = MessagesUi.TopBarNotConnectedLocked;
						else
							LblTopStatus.StringValue = MessagesUi.TopBarNotConnectedExposed;

						TabOverview.SelectAt(0);
					}

					EnabledUI();
				}

				if ((mode == Engine.RefreshUiMode.Log) || (mode == Engine.RefreshUiMode.Full))
				{

					lock (Engine.LogsPending)
					{
						while (Engine.LogsPending.Count > 0)
						{
							LogEntry l = Engine.LogsPending[0];
							Engine.LogsPending.RemoveAt(0);

							Log(l);
						}
					}
					LblWaiting2.StringValue = Engine.Logs.GetLogDetailTitle();
				}

				if ((mode == Engine.RefreshUiMode.Stats) || (mode == Engine.RefreshUiMode.Full))
				{
					if (Engine.IsConnected())
					{
						TxtConnectedSince.StringValue = Engine.Stats.GetValue("VpnStart");

						TxtConnectedDownload.StringValue = UtilsString.FormatBytes(Engine.ConnectionActive.BytesLastDownloadStep, true, false);
						TxtConnectedUpload.StringValue = UtilsString.FormatBytes(Engine.ConnectionActive.BytesLastUploadStep, true, false);
					}
				}

				if ((mode == Engine.RefreshUiMode.Full))
				{
					if (TableServersController != null)
						TableServersController.RefreshUI();
					if (TableAreasController != null)
						TableAreasController.RefreshUI();
				}



			}
			catch (Exception)
			{
				// TOFIX: macOS sometime throw an useless exception in closing phase
			}
		}

		public void SetColor(string color)
		{
			string colorMode = Engine.Storage.Get("gui.osx.style");

			if (color == "green")
			{
				StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_green.png");
				NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon.png");
				MnuTrayConnect.Title = Messages.CommandDisconnect;
			}
			else if (color == "yellow")
			{
				StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_yellow.png");
				NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon_gray.png");
				MnuTrayConnect.Title = Messages.CommandCancel;
			}
			else if (color == "red")
			{
				StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_red.png");
				NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon_gray.png");
				MnuTrayConnect.Title = Messages.CommandConnect;
			}
		}

		public void ShowNotification(string title)
		{
			RequestAttention();

			if (Engine.Instance.Storage.GetBool("gui.notifications") == false)
				return;

			// First we create our notification and customize as needed
			NSUserNotification not = null;

			try
			{
				not = new NSUserNotification();
			}
			catch
			{
				// This API was introduced in OS X Mountain Lion (10.8)
				return;
			}

			//not.Title = title;
			//not.InformativeText = Eddie.Common.Constants.Name;
			not.InformativeText = title;
			not.DeliveryDate = NSDate.Now;
			not.SoundName = NSUserNotification.NSUserNotificationDefaultSoundName;

			// We get the Default notification Center
			NSUserNotificationCenter center = NSUserNotificationCenter.DefaultUserNotificationCenter;

			// If we return true here, Notification will show up even if your app is TopMost.
			center.ShouldPresentNotification = (c, n) => { return true; };

			center.ScheduleNotification(not);
		}

		public void Log(LogEntry l)
		{
			string msg = l.Message;

			TableLogsController.AddLog(l);

			if (l.Type >= LogType.InfoImportant)
				RequestAttention();

			if (l.Type == LogType.Fatal)
				GuiUtils.MessageBoxError(msg);
		}

		public void SetStatus(string textFull, string textShort)
		{
			StatusItem.ToolTip = textFull;
			StatusItem.Title = textShort;
			Window.Title = Constants.Name + " - " + textFull;
            MnuTrayStatus.Title = "> " + textFull;
		}

		public void EnabledUI()
		{
			ConnectionInfo selectedConnection = null;
			if (TableServers.SelectedRowCount == 1)
			{
				selectedConnection = TableServersController.GetRelatedItem((int)TableServers.SelectedRow);
			}

			bool connected = Engine.IsConnected();
			bool waiting = Engine.IsWaiting();


			if (Engine.Instance.AirVPN != null)
			{
				LblLoginIcon.Hidden = false;
				LblLogin.Hidden = false;
				TxtLogin.Hidden = false;
				LblPassword.Hidden = false;
				TxtPassword.Hidden = false;
				CmdLogin.Hidden = false;
				ChkRemember.Hidden = false;

				bool airvpnLogged = Engine.IsLogged();

				if (airvpnLogged == false)
					CmdLogin.Title = Messages.CommandLoginButton;
				else
					CmdLogin.Title = Messages.CommandLogout;

				CmdLogin.Enabled = ((waiting == false) && (connected == false) && (TxtLogin.StringValue.Trim() != "") && (TxtPassword.StringValue.Trim() != ""));

				TxtLogin.Enabled = (airvpnLogged == false);
				TxtPassword.Enabled = (airvpnLogged == false);
				LblKey.Hidden = ((airvpnLogged == false) || (CboKey.ItemCount < 2));
				CboKey.Hidden = LblKey.Hidden;
			}
			else
			{
				LblLoginIcon.Hidden = true;
				LblLogin.Hidden = true;
				TxtLogin.Hidden = true;
				LblPassword.Hidden = true;
				TxtPassword.Hidden = true;
				CmdLogin.Hidden = true;
				LblKey.Hidden = true;
				CboKey.Hidden = true;
				ChkRemember.Hidden = true;
			}

			if (this.Window.IsVisible)
                MnuTrayRestore.Title = Messages.WindowsMainHide;
            else
                MnuTrayRestore.Title = Messages.WindowsMainShow;

			CmdConnect.Enabled = Engine.Instance.CanConnect();

			CmdProviderAdd.Enabled = true;
			CmdProviderRemove.Enabled = (TableProviders.SelectedRowCount > 0);
			CmdProviderEdit.Enabled = (TableProviders.SelectedRowCount > 0);

			CmdServersConnect.Enabled = ((selectedConnection != null) && (selectedConnection.CanConnect()));
			CmdServersWhiteList.Enabled = (TableServers.SelectedRowCount > 0);
			CmdServersBlackList.Enabled = CmdServersWhiteList.Enabled;
			CmdServersUndefined.Enabled = CmdServersWhiteList.Enabled;
			MnuServersConnect.Enabled = CmdServersConnect.Enabled;
			MnuServersWhitelist.Enabled = CmdServersWhiteList.Enabled;
			MnuServersBlacklist.Enabled = CmdServersBlackList.Enabled;
			MnuServersUndefined.Enabled = CmdServersUndefined.Enabled;

			CmdServersMore.Enabled = (TableServers.SelectedRowCount == 1);
			MnuServersMore.Enabled = CmdServersMore.Enabled;

			CmdServersRename.Enabled = ((selectedConnection != null) && (selectedConnection.Provider is Core.Providers.OpenVPN));
			MnuServersRename.Enabled = CmdServersRename.Enabled;

			CmdAreasWhiteList.Enabled = (TableAreas.SelectedRowCount > 0);
			CmdAreasBlackList.Enabled = CmdAreasWhiteList.Enabled;
			CmdAreasUndefined.Enabled = CmdAreasWhiteList.Enabled;
			MnuAreasWhitelist.Enabled = CmdAreasWhiteList.Enabled;
			MnuAreasBlacklist.Enabled = CmdAreasBlackList.Enabled;
			MnuAreasUndefined.Enabled = CmdAreasUndefined.Enabled;

			CmdLogsCommand.Hidden = (Engine.Storage.GetBool("advanced.expert") == false);

			if (Engine.Instance.NetworkLockManager != null)
			{
				CmdNetworkLock.Hidden = (Engine.Instance.NetworkLockManager.CanEnabled() == false);
				ImgNetworkLock.Hidden = CmdNetworkLock.Hidden;
				if (Engine.Instance.NetworkLockManager.IsActive())
				{
					CmdNetworkLock.Title = Messages.NetworkLockButtonActive;
					ImgNetworkLock.Image = NSImage.ImageNamed("netlock_on.png");

					LblNetLockStatus.Image = NSImage.ImageNamed("netlock_status_on.png");
					LblNetLockStatus.ToolTip = Messages.NetworkLockStatusActive;

				}
				else
				{
					CmdNetworkLock.Title = Messages.NetworkLockButtonDeactive;
					ImgNetworkLock.Image = NSImage.ImageNamed("netlock_off.png");

					LblNetLockStatus.Image = NSImage.ImageNamed("netlock_status_off.png");
					LblNetLockStatus.ToolTip = Messages.NetworkLockStatusDeactive;
				}
			}

			if (Engine.Instance.Storage.GetBool("advanced.providers"))
			{
				if (TabMain.Items[1] != TabProviders)
				{
					TabMain.Insert(TabProviders, 1);
				}
			}
			else
			{
				if (TabMain.Items[1] == TabProviders)
				{
					TabMain.Remove(TabProviders);
				}
			}
		}

		public void ProviderManifestFailed(Provider provider)
		{
			WindowProviderNoBootstrapController w = new WindowProviderNoBootstrapController();
			w.Provider = provider;
			NSApplication.SharedApplication.RunModalForWindow(w.Window);
		}

		public void SettingsChanged()
		{
			// Commented in 2.8, see this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/
			/*
			bool showInDock = Engine.Storage.GetBool ("gui.osx.dock");
			if (showInDock)
				SwitchToRegular ();
			else
				SwitchToAccessory ();
			*/
		}

		/*
		public void SwitchToRegular()
		{
			NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
		}

		public void SwitchToAccessory()
		{
			NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;
		}
		*/

		public void FrontMessage(string message)
		{
			WindowFrontMessageController w = new WindowFrontMessageController();
			(Engine.Instance as Engine).WindowsOpen.Add(w);
			w.Message = message;
			GuiUtils.ShowWindowWithFocus(w, this);
		}

		public void PostManifestUpdate()
		{
			MnuServersRefresh.Enabled = true;
			CmdServersRefresh.Enabled = true;
		}

		public void LoggedUpdate(XmlElement xmlKeys)
		{
			CboKey.RemoveAllItems();
			foreach (XmlElement xmlKey in xmlKeys.ChildNodes)
			{
				string keyName = xmlKey.GetAttribute("name");
				CboKey.AddItem(keyName);
			}
			string currentKey = Engine.Instance.Storage.Get("key");
			int currentIndex = (int)CboKey.IndexOfItem(currentKey);
			if (currentIndex != -1)
			{
				CboKey.SelectItem(currentIndex);
			}
			else
			{
				if (CboKey.ItemCount > 0)
				{
					CboKey.SelectItem(0);
					Engine.Instance.Storage.Set("key", CboKey.ItemAtIndex(0).Title);
				}
				else
				{
					Engine.Instance.Storage.Set("key", "");
				}
			}

		}

		public void RequestAttention()
		{
			NSApplication.SharedApplication.RequestUserAttention(NSRequestUserAttentionType.InformationalRequest);


		}


		public bool NetworkLockKnowledge()
		{
			string msg = Messages.NetworkLockWarning;

			return (GuiUtils.MessageYesNo(msg));
		}

		public void CreateMenuBarIcon()
		{
			StatusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
			//StatusItem.Menu = notifyMenu;
			StatusItem.Menu = MnuTray;
			StatusItem.Image = NSImage.ImageNamed("menubar_light_red.png");
			StatusItem.HighlightMode = true;

			NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon_gray.png");
		}



		void Login()
		{
			Engine.Storage.Set("login", TxtLogin.StringValue);
			Engine.Storage.Set("password", TxtPassword.StringValue);

			if (TermsOfServiceCheck(false) == false)
				return;

			Engine.Login();
		}

		void Logout()
		{
			Engine.Logout();
		}

		void Connect()
		{
			if ((Engine.CanConnect() == true) && (Engine.IsConnected() == false) && (Engine.IsWaiting() == false))
			{
				TabMain.SelectAt(0);
				Engine.Connect();
			}
		}

		void ConnectManual()
		{
			if (TableServers.SelectedRows.Count == 1)
			{
				ConnectionInfo s = TableServersController.GetRelatedItem((int)TableServers.SelectedRow);
				if (s.CanConnect())
				{
					Engine.NextServer = s;
					Connect();
				}
			}
		}

		void Disconnect()
		{
			CmdCancel.Enabled = false;
			MnuTrayConnect.Enabled = false;

			Engine.Disconnect();
		}

		void NetworkLockActivation()
		{
			if (NetworkLockKnowledge())
			{
				Engine.Instance.NetLockIn();
			}

		}

		void NetworkLockDeactivation()
		{
			Engine.NetLockOut();
		}

		bool TermsOfServiceCheck(bool force)
		{
			bool show = force;
			if (show == false)
				show = (Engine.Storage.GetBool("gui.tos") == false);

			if (show)
			{
				WindowTosController tos = new WindowTosController();
				tos.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow(tos.Window);
				tos.Window.Close();

				if (tos.Accepted)
				{
					Engine.Storage.SetBool("gui.tos", true);
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		void ProviderAdd()
		{
			WindowProviderAddController w = new WindowProviderAddController();
			NSApplication.SharedApplication.RunModalForWindow(w.Window);
			if (w.Provider != "")
			{
				Engine.Instance.ProvidersManager.AddProvider(w.Provider, null);
				Engine.Instance.ProvidersManager.Refresh();

				TableProvidersController.RefreshUI();
				EnabledUI();
			}
		}

		void ProviderRemove()
		{
			foreach (int i in TableProviders.SelectedRows)
			{
				Provider p = Engine.Instance.ProvidersManager.Providers[i];
				Engine.Instance.ProvidersManager.Remove(p);
				Engine.Instance.ProvidersManager.Refresh();
			}

			TableProvidersController.RefreshUI();
			EnabledUI();
		}

		void ProviderEdit()
		{
			foreach (int i in TableProviders.SelectedRows)
			{
				bool updated = false;
				Provider p = Engine.Instance.ProvidersManager.Providers[i];
				if (p is Core.Providers.OpenVPN)
				{
					WindowProviderEditOpenVPNController w = new WindowProviderEditOpenVPNController();
					w.Provider = p as Core.Providers.OpenVPN;
					NSApplication.SharedApplication.RunModalForWindow(w.Window);
					if (w.Provider != null)
						updated = true;
				}
				else if (p is Core.Providers.Service)
				{
					WindowProviderEditManifestController w = new WindowProviderEditManifestController();
					w.Provider = p as Core.Providers.Service;
					NSApplication.SharedApplication.RunModalForWindow(w.Window);
					if (w.Provider != null)
						updated = true;
				}

				if (updated)
				{
					Engine.Instance.ProvidersManager.Refresh();

					TableProvidersController.RefreshUI();
					EnabledUI();
				}
				break; // Only one
			}
		}

		void ServersWhiteList()
		{
			foreach (int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ConnectionInfo.UserListType.WhiteList;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI();
		}

		void ServersBlackList()
		{
			foreach (int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ConnectionInfo.UserListType.BlackList;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI();
		}

		void ServersUndefinedList()
		{
			foreach (int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ConnectionInfo.UserListType.None;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI();
		}

		void ServersRename()
		{
			if (TableServers.SelectedRowCount != 1)
				return;

			foreach (int i in TableServers.SelectedRows)
			{
				ConnectionInfo connection = TableServersController.GetRelatedItem(i);

				WindowConnectionRenameController w = new WindowConnectionRenameController();
				w.Body = connection.DisplayName;
				NSApplication.SharedApplication.RunModalForWindow(w.Window);
				if (w.Body != "")
				{
					connection.DisplayName = w.Body;
					connection.Provider.OnChangeConnection(connection);
					TableServersController.RefreshUI();
				}
			}
		}

		void ServersMore()
		{
			if (TableServers.SelectedRowCount != 1)
				return;

			foreach (int i in TableServers.SelectedRows)
			{
				ConnectionInfo connection = TableServersController.GetRelatedItem(i);

				WindowConnectionController w = new WindowConnectionController();
				w.Connection = connection;
				NSApplication.SharedApplication.RunModalForWindow(w.Window);
			}
		}

		void ServersRefresh()
		{
			MnuServersRefresh.Enabled = false;
			CmdServersRefresh.Enabled = false;

			Engine.Instance.RefreshInvalidateConnections();
		}

		void AreasWhiteList()
		{
			foreach (int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.WhiteList;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI();
		}

		void AreasBlackList()
		{
			foreach (int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.BlackList;
			}
			Engine.UpdateSettings();
			TableAreasController.RefreshUI();
		}

		void AreasUndefinedList()
		{
			foreach (int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.None;
			}
			Engine.UpdateSettings();
			TableAreasController.RefreshUI();
		}

		void SupportReport()
		{
			Engine.Instance.GenerateSystemReport();
		}

		void LogsDoCopy(bool selectedOnly)
		{
			string t = TableLogsController.GetBody(selectedOnly);
			if (t != "")
			{
				string[] pboardTypes = new string[] { "NSStringPboardType" };
				NSPasteboard.GeneralPasteboard.DeclareTypes(pboardTypes, null);
				NSPasteboard.GeneralPasteboard.SetStringForType(t, pboardTypes[0]);
				GuiUtils.MessageBoxInfo(Messages.LogsCopyClipboardDone);
			}
		}

		void LogsDoSave(bool selectedOnly)
		{
			string t = TableLogsController.GetBody(selectedOnly);
			if (t.Trim() != "")
			{
				//string filename = "AirVPN_" + DateTime.Now.ToString ("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".txt"; // TOCLEAN
				string filename = Engine.Logs.GetLogSuggestedFileName();

				NSSavePanel panel = new NSSavePanel();
				panel.NameFieldStringValue = filename;
				panel.CanCreateDirectories = true;
				nint result = panel.RunModal();
				if (result == 1)
				{
					Core.Platform.Instance.FileContentsWriteText(panel.Url.Path, t);

					GuiUtils.MessageBoxInfo(Messages.LogsSaveToFileDone);
				}
			}
		}

		public void ShowAbout()
		{
			if ((windowAbout == null) || (windowAbout.Window.IsVisible == false))
				windowAbout = new WindowAboutController();
			GuiUtils.ShowWindowWithFocus(windowAbout, this);
		}

		public void ShowPreferences()
		{
			if ((windowPreferences == null) || (windowPreferences.Window.IsVisible == false))
				windowPreferences = new WindowPreferencesController();
			GuiUtils.ShowWindowWithFocus(windowPreferences, this);
		}

		public void ShowHome()
		{
			Core.UI.App.OpenUrl(Core.UI.App.Manifest["links"]["help"]["website"].Value as string);
		}

		public void ShowClientArea()
		{
			Core.UI.App.OpenUrl("https://airvpn.org/client/");
		}

		public void ShowForwardingPorts()
		{
			Core.UI.App.OpenUrl("https://airvpn.org/ports/");
		}

		public void ShowText(NSWindow parent, string title, string data)
		{
			WindowTextViewerController textViewer = new WindowTextViewerController();
			Engine.WindowsOpen.Add(textViewer);
			textViewer.Title = title;
			textViewer.Body = data;
			textViewer.ShowWindow(parent);
		}

		public void Minimize()
		{
			Window.Miniaturize(this);
			EnabledUI();
		}

		public void Restore(object sender)
		{

			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

			GuiUtils.ShowWindowWithFocus(this, this);
			Window.MakeMainWindow();
			EnabledUI();
			/*
			ShowWindow (this);
			Window.MakeMainWindow ();
			Window.Deminiaturize (this);
			EnabledUI ();
			Window.MakeKeyAndOrderFront (this);
			*/

		}
	}
}


