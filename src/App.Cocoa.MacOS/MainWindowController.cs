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
using System.Xml;
using Foundation;
using AppKit;
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

        private string m_mainActionCommand = "";

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

			LblVersion.StringValue = "Version " + Constants.VersionDesc;
            CmdUpdater.Hidden = true;
            MnuTrayUpdate.Hidden = true;

			ChkRemember.State = Engine.Options.GetBool("remember") ? NSCellStateValue.On : NSCellStateValue.Off;
			ChkServersShowAll.State = NSCellStateValue.Off;
			GuiUtils.SetCheck(ChkServersLockCurrent, Engine.Options.GetBool("servers.locklast"));
			GuiUtils.SetSelected(CboServersScoringRule, Engine.Options.Get("servers.scoretype"));

			CboSpeedResolutions.RemoveAllItems();
			CboSpeedResolutions.AddItem(LanguageManager.GetText("WindowsMainSpeedResolution1"));
			CboSpeedResolutions.AddItem(LanguageManager.GetText("WindowsMainSpeedResolution2"));
			CboSpeedResolutions.AddItem(LanguageManager.GetText("WindowsMainSpeedResolution3"));
			CboSpeedResolutions.AddItem(LanguageManager.GetText("WindowsMainSpeedResolution4"));
			CboSpeedResolutions.AddItem(LanguageManager.GetText("WindowsMainSpeedResolution5"));
			CboSpeedResolutions.SelectItem(0);

			CmdConnect.Title = LanguageManager.GetText("CommandConnect");
			LblConnect.StringValue = LanguageManager.GetText("CommandConnectSubtitle");
			CmdDisconnect.Title = LanguageManager.GetText("CommandDisconnect");
			CmdCancel.Title = LanguageManager.GetText("CommandCancel");

			CboServersScoringRule.ToolTip = LanguageManager.GetText("TooltipServersScoreType");
			ChkServersLockCurrent.ToolTip = LanguageManager.GetText("TooltipServersLockCurrent");
			ChkServersShowAll.ToolTip = LanguageManager.GetText("TooltipServersShowAll");
			CmdServersConnect.ToolTip = LanguageManager.GetText("TooltipServersConnect");
			CmdServersUndefined.ToolTip = LanguageManager.GetText("TooltipServersUndefined");
			CmdServersBlackList.ToolTip = LanguageManager.GetText("TooltipServersBlackList");
			CmdServersWhiteList.ToolTip = LanguageManager.GetText("TooltipServersWhiteList");
			CmdServersRename.ToolTip = LanguageManager.GetText("TooltipServersRename");
			CmdServersMore.ToolTip = LanguageManager.GetText("TooltipServersMore");
			CmdServersRefresh.ToolTip = LanguageManager.GetText("TooltipServersRefresh");
			CmdAreasUndefined.ToolTip = LanguageManager.GetText("TooltipAreasUndefined");
			CmdAreasBlackList.ToolTip = LanguageManager.GetText("TooltipAreasBlackList");
			CmdAreasWhiteList.ToolTip = LanguageManager.GetText("TooltipAreasWhiteList");
			CmdLogsCommand.ToolTip = LanguageManager.GetText("TooltipLogsCommand");
			CmdLogsClean.ToolTip = LanguageManager.GetText("TooltipLogsClean");
			CmdLogsCopy.ToolTip = LanguageManager.GetText("TooltipLogsCopy");
			CmdLogsSave.ToolTip = LanguageManager.GetText("TooltipLogsSave");
			CmdLogsSupport.ToolTip = LanguageManager.GetText("TooltipLogsSupport");

			if (Engine.Options.GetBool("remember"))
			{
				ChkRemember.State = NSCellStateValue.On;
				TxtLogin.StringValue = Engine.Options.Get("login");
				TxtPassword.StringValue = Engine.Options.Get("password");

			}
            
            CmdMainMenu.Activated += (object sender, EventArgs e) =>
            {
                CoreGraphics.CGPoint p = new CoreGraphics.CGPoint(this.Window.Frame.Left + 10, this.Window.Frame.Bottom - 40);
                //CoreGraphics.CGPoint p = new CoreGraphics.CGPoint(CmdMainMenu.Frame.Left + 10, CmdMainMenu.Frame.Top + 10);
                MnuTray.PopUpMenu(MnuTrayStatus, p, null);
            };

            CmdUpdater.Activated += (object sender, EventArgs e) =>
            {
                Core.Platform.Instance.OpenUrl(Constants.WebSite + "/" + Core.Platform.Instance.GetCode().ToLowerInvariant() + "/");
            };

            ChkRemember.Activated += (object sender, EventArgs e) =>
			{
				Engine.Options.SetBool("remember", ChkRemember.State == NSCellStateValue.On);
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

			TxtLogin.Changed += (object sender, EventArgs e) =>
			{
				EnabledUI();
			};

			TxtPassword.Activated += (object sender, EventArgs e) =>
			{
				EnabledUI();
			};

			TxtPassword.Changed += (object sender, EventArgs e) =>
			{
				EnabledUI();
			};

			CboKey.Activated += (object sender, EventArgs e) =>
			{
                if(Engine.Instance.Options.Get("key") != CboKey.SelectedItem.Title)
				    Engine.Instance.Options.Set("key", CboKey.SelectedItem.Title);
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
				Engine.Options.SetBool("servers.locklast", ChkServersLockCurrent.State == NSCellStateValue.On);
			};

			CboServersScoringRule.Activated += (object sender, EventArgs e) =>
			{
				Engine.Options.Set("servers.scoretype", GuiUtils.GetSelected(CboServersScoringRule));

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
                    UiClient.Instance.Command(w.Command);
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
                if (m_mainActionCommand == "")
                    Restore(sender);
                else
                    UiClient.Instance.Command(m_mainActionCommand);
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

			CboServersScoringRule.ToolTip = LanguageManager.GetText("TooltipServersScoreType");
			CmdAreasBlackList.ToolTip = LanguageManager.GetText("TooltipAreasBlackList");

			Engine.OnRefreshUi();

			SettingsChanged();

			RequestAttention();
		}

        private void Window_WillClose(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool Shutdown()
		{
			if (Engine.AskExitConfirm())
			{
				bool result = GuiUtils.MessageYesNo(LanguageManager.GetText("ExitConfirm"));
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
						LblTopStatus.StringValue = LanguageManager.GetText("TopBarConnected", Engine.CurrentServer.DisplayName);


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
                            LblTopStatus.StringValue = LanguageManager.GetText("TopBarNotConnectedLocked");
						else
                            LblTopStatus.StringValue = LanguageManager.GetText("TopBarNotConnectedExposed");

						TabOverview.SelectAt(0);
					}
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

                        TxtConnectedDownload.StringValue = LanguageManager.FormatBytes(Engine.ConnectionActive.BytesLastDownloadStep, true, false);
                        TxtConnectedUpload.StringValue = LanguageManager.FormatBytes(Engine.ConnectionActive.BytesLastUploadStep, true, false);
					}
				}

				if ((mode == Engine.RefreshUiMode.Full))
				{
					if (TableServersController != null)
						TableServersController.RefreshUI();
					if (TableAreasController != null)
						TableAreasController.RefreshUI();

                    CboKey.RemoveAllItems();
                    if( (Engine.Instance != null) && (Engine.Instance.AirVPN != null) && (Engine.Instance.AirVPN.User != null) )
                    {
                        foreach (XmlElement xmlKey in Engine.Instance.AirVPN.User.SelectNodes("keys/key"))
                        {
                            string keyName = xmlKey.GetAttribute("name");
                            CboKey.AddItem(keyName);
                        }
                        string currentKey = Engine.Instance.Options.Get("key");
                        int currentIndex = (int)CboKey.IndexOfItem(currentKey);
                        if (currentIndex != -1)
                        {
                            CboKey.SelectItem(currentIndex);
                        }
                    }
                }

                if ((mode == Engine.RefreshUiMode.MainMessage) || (mode == Engine.RefreshUiMode.Full))
                {
                    EnabledUI();
                }


            }
			catch (Exception)
			{
				// TOFIX: macOS sometime throw an useless exception in closing phase
			}
		}

        public string GetMacColorMode() // Must return 'light' or 'dark'
        {
            try
            {
                //string colorMode = Engine.Instance.StartCommandLine.Get("gui.osx.style", "light");
                string colorMode = NSUserDefaults.StandardUserDefaults.StringForKey("AppleInterfaceStyle").ToString();
                if (colorMode.ToLowerInvariant() == "dark")
                    return "dark";
            }
            catch
            {
            }

            return "light";
        }

        public void SetMainStatus(string appIcon, string appColor, string mainIcon, string mainActionCommand, string mainActionText)
        {
            string colorMode = GetMacColorMode();

            if(appIcon == "normal")
                NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon.png");
            else
                NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon_gray.png");

            if (appColor == "green")
			{
				StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_green.png");
				MnuTrayConnect.Title = LanguageManager.GetText("CommandDisconnect");
			}
			else if (appColor == "yellow")
			{
				StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_yellow.png");
				
				MnuTrayConnect.Title = LanguageManager.GetText("CommandCancel");
			}
			else if (appColor == "red")
			{
				StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_red.png");
				MnuTrayConnect.Title = LanguageManager.GetText("CommandConnect");
			}

            MnuTrayConnect.Title = mainActionText;
            MnuTrayConnect.Enabled = (mainActionCommand != "");

            m_mainActionCommand = mainActionCommand;
		}

		public void ShowNotification(string title, string level)
		{
			RequestAttention();

			if (Engine.Instance.Options.GetBool("gui.notifications") == false)
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
            not.Title = Eddie.Core.Constants.Name + " " + Eddie.Core.Constants.VersionDesc;
            not.InformativeText = title;
			not.DeliveryDate = NSDate.Now;
			not.SoundName = NSUserNotification.NSUserNotificationDefaultSoundName;
            not.ContentImage = NSImage.ImageNamed("log_" + level.ToLowerInvariant() + ".png");

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
		}

        public void MessageError(string message)
        {
            new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
			{
                GuiUtils.MessageBoxError(message);
            });
        }

		public void SetStatus(string textFull, string textShort)
		{
            if (StatusItem == null)
                return;

            StatusItem.ToolTip = textFull;
            StatusItem.Title = textShort;
            Window.Title = Constants.Name + " - " + textFull;
            MnuTrayStatus.Title = "> " + textFull;
		}

        public void ShowUpdater()
        {
            CmdUpdater.Hidden = false;
            MnuTrayUpdate.Hidden = false;

            CoreGraphics.CGSize s = LblVersion.Frame.Size;
            s.Width -= CmdUpdater.Frame.Width;
            LblVersion.SetFrameSize(s);
        }

        public void ChangeVisibility(bool vis)
        {
            if (vis)
                MnuTrayRestore.Title = LanguageManager.GetText("WindowsMainHide");
            else
                MnuTrayRestore.Title = LanguageManager.GetText("WindowsMainShow");
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
					CmdLogin.Title = LanguageManager.GetText("CommandLoginButton");
				else
					CmdLogin.Title = LanguageManager.GetText("CommandLogout");

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

            ChangeVisibility(this.Window.IsVisible);

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

			CmdLogsCommand.Hidden = (Engine.Options.GetBool("advanced.expert") == false);

			if (Engine.Instance.NetworkLockManager != null)
			{
				CmdNetworkLock.Hidden = (Engine.Instance.NetworkLockManager.CanEnabled() == false);
				ImgNetworkLock.Hidden = CmdNetworkLock.Hidden;
				if (Engine.Instance.NetworkLockManager.IsActive())
				{
					CmdNetworkLock.Title = LanguageManager.GetText("NetworkLockButtonActive");
					ImgNetworkLock.Image = NSImage.ImageNamed("netlock_on.png");

					LblNetLockStatus.Image = NSImage.ImageNamed("netlock_status_on.png");
					LblNetLockStatus.ToolTip = LanguageManager.GetText("NetworkLockStatusActive");

				}
				else
				{
					CmdNetworkLock.Title = LanguageManager.GetText("NetworkLockButtonDeactive");
					ImgNetworkLock.Image = NSImage.ImageNamed("netlock_off.png");

					LblNetLockStatus.Image = NSImage.ImageNamed("netlock_status_off.png");
					LblNetLockStatus.ToolTip = LanguageManager.GetText("NetworkLockStatusDeactive");
				}
			}

			if (Engine.Instance.Options.GetBool("advanced.providers"))
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
            if(WindowProviderNoBootstrapController.Singleton == null)
            {
                WindowProviderNoBootstrapController w = new WindowProviderNoBootstrapController();
                w.Provider = provider;
                NSApplication.SharedApplication.RunModalForWindow(w.Window);
            }
		}

		public void SettingsChanged()
		{
			// Commented in 2.8, see this FAQ: https://airvpn.org/topic/13331-its-possible-to-hide-the-icon-in-dock-bar-under-os-x/			
		}

		public void FrontMessage(Json jMessage)
		{
			WindowFrontMessageController w = new WindowFrontMessageController();
			(Engine.Instance as Engine).WindowsOpen.Add(w);
			w.Message = jMessage;
			GuiUtils.ShowWindowWithFocus(w, this);
		}

		public void PostManifestUpdate()
		{
			MnuServersRefresh.Enabled = true;
			CmdServersRefresh.Enabled = true;
		}

		public void RequestAttention()
		{
			NSApplication.SharedApplication.RequestUserAttention(NSRequestUserAttentionType.InformationalRequest);
		}

		public bool NetworkLockKnowledge()
		{
			string msg = LanguageManager.GetText("NetworkLockWarning");

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
			Engine.Options.Set("login", TxtLogin.StringValue);
			Engine.Options.Set("password", TxtPassword.StringValue);

			Engine.Login();
		}

		void Logout()
		{
			Engine.Logout();
		}

		void Connect()
		{
			Engine.Connect();			
		}

		void ConnectManual()
		{
			if (TableServers.SelectedRows.Count == 1)
			{
				ConnectionInfo s = TableServersController.GetRelatedItem((int)TableServers.SelectedRow);
				if (s.CanConnect())
				{
					Engine.NextServer = s;
                    TabMain.SelectAt(0);
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

		void ProviderAdd()
		{
			WindowProviderAddController w = new WindowProviderAddController();
			NSApplication.SharedApplication.RunModalForWindow(w.Window);
			if (w.Provider != "")
			{
				Engine.Instance.ProvidersManager.AddProvider(w.Provider, null);
                Engine.Instance.JobsManager.ProvidersRefresh.CheckNow();

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
                Engine.Instance.JobsManager.ProvidersRefresh.CheckNow();
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
                    Engine.Instance.JobsManager.ProvidersRefresh.CheckNow();

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
				TableServersController.GetRelatedItem(i).UserList = ConnectionInfo.UserListType.Whitelist;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI();
		}

		void ServersBlackList()
		{
			foreach (int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ConnectionInfo.UserListType.Blacklist;
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

			Engine.Instance.RefreshProvidersInvalidateConnections();
		}

		void AreasWhiteList()
		{
			foreach (int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.Whitelist;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI();
		}

		void AreasBlackList()
		{
			foreach (int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.Blacklist;
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
            UiClient.Instance.Command("system.report.start");
		}

		void LogsDoCopy(bool selectedOnly)
		{
			string t = TableLogsController.GetBody(selectedOnly);
			if (t != "")
			{
				string[] pboardTypes = new string[] { "NSStringPboardType" };
				NSPasteboard.GeneralPasteboard.DeclareTypes(pboardTypes, null);
				NSPasteboard.GeneralPasteboard.SetStringForType(t, pboardTypes[0]);
				GuiUtils.MessageBoxInfo(LanguageManager.GetText("LogsCopyClipboardDone"));
			}
		}

		void LogsDoSave(bool selectedOnly)
		{
			string t = TableLogsController.GetBody(selectedOnly);
			if (t.Trim() != "")
			{
				string filename = Engine.Logs.GetLogSuggestedFileName();

				NSSavePanel panel = new NSSavePanel();
				panel.NameFieldStringValue = filename;
				panel.CanCreateDirectories = true;
				nint result = panel.RunModal();
				if (result == 1)
				{
					Core.Platform.Instance.FileContentsWriteText(panel.Url.Path, t, System.Text.Encoding.UTF8);

					GuiUtils.MessageBoxInfo(LanguageManager.GetText("LogsSaveToFileDone"));
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
            GuiUtils.OpenUrl(UiClient.Instance.Data["links"]["help"]["website"].Value as string);
		}

		public void ShowClientArea()
		{
            GuiUtils.OpenUrl("https://airvpn.org/client/");
		}

		public void ShowForwardingPorts()
		{
            GuiUtils.OpenUrl("https://airvpn.org/ports/");
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
		}
	}
}


