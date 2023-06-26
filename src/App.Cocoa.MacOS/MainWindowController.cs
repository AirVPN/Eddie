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
            GuiUtils.SetHidden(CmdUpdater, true);
			GuiUtils.SetHidden(MnuTrayUpdate, true);

			ChkRemember.State = Engine.ProfileOptions.GetBool("remember") ? NSCellStateValue.On : NSCellStateValue.Off;
			ChkServersShowAll.State = NSCellStateValue.Off;
			GuiUtils.SetCheck(ChkServersLockCurrent, Engine.ProfileOptions.GetBool("servers.locklast"));
			GuiUtils.SetSelected(CboServersScoringRule, Engine.ProfileOptions.Get("servers.scoretype"));

			CboSpeedResolutions.RemoveAllItems();
			CboSpeedResolutions.AddItem(LanguageManager.GetText(LanguageItems.WindowsMainSpeedResolution1));
			CboSpeedResolutions.AddItem(LanguageManager.GetText(LanguageItems.WindowsMainSpeedResolution2));
			CboSpeedResolutions.AddItem(LanguageManager.GetText(LanguageItems.WindowsMainSpeedResolution3));
			CboSpeedResolutions.AddItem(LanguageManager.GetText(LanguageItems.WindowsMainSpeedResolution4));
			CboSpeedResolutions.AddItem(LanguageManager.GetText(LanguageItems.WindowsMainSpeedResolution5));
			CboSpeedResolutions.SelectItem(0);

			CmdConnect.Title = LanguageManager.GetText(LanguageItems.CommandConnect);
			LblConnect.StringValue = LanguageManager.GetText(LanguageItems.CommandConnectSubtitle);
			CmdDisconnect.Title = LanguageManager.GetText(LanguageItems.CommandDisconnect);
			CmdCancel.Title = LanguageManager.GetText(LanguageItems.CommandCancel);

			CboServersScoringRule.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersScoreType);
			ChkServersLockCurrent.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersLockCurrent);
			ChkServersShowAll.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersShowAll);
			CmdServersConnect.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersConnect);
			CmdServersUndefined.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersUndefined);
			CmdServersDenylist.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersDenylist);
			CmdServersAllowlist.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersAllowlist);
			CmdServersRename.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersRename);
			CmdServersMore.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersMore);
			CmdServersRefresh.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersRefresh);
			CmdAreasUndefined.ToolTip = LanguageManager.GetText(LanguageItems.TooltipAreasUndefined);
			CmdAreasDenylist.ToolTip = LanguageManager.GetText(LanguageItems.TooltipAreasDenylist);
			CmdAreasAllowlist.ToolTip = LanguageManager.GetText(LanguageItems.TooltipAreasAllowlist);
			CmdLogsCommand.ToolTip = LanguageManager.GetText(LanguageItems.TooltipLogsCommand);
			CmdLogsClean.ToolTip = LanguageManager.GetText(LanguageItems.TooltipLogsClean);
			CmdLogsCopy.ToolTip = LanguageManager.GetText(LanguageItems.TooltipLogsCopy);
			CmdLogsSave.ToolTip = LanguageManager.GetText(LanguageItems.TooltipLogsSave);
			CmdLogsSupport.ToolTip = LanguageManager.GetText(LanguageItems.TooltipLogsSupport);

			if (Engine.ProfileOptions.GetBool("remember"))
			{
				ChkRemember.State = NSCellStateValue.On;
				TxtAirU.StringValue = Engine.ProfileOptions.Get("login");
				TxtPassword.StringValue = Engine.ProfileOptions.Get("password");

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
				Engine.ProfileOptions.SetBool("remember", ChkRemember.State == NSCellStateValue.On);
			};

			CmdLogin.Activated += (object sender, EventArgs e) =>
			{
				if (Engine.IsLogged() == false)
					Login();
				else
					Logout();
			};

			TxtAirU.Activated += (object sender, EventArgs e) =>
			{
				EnabledUI();
			};

			TxtAirU.Changed += (object sender, EventArgs e) =>
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
                if(Engine.Instance.ProfileOptions.Get("key") != CboKey.SelectedItem.Title)
				    Engine.Instance.ProfileOptions.Set("key", CboKey.SelectedItem.Title);
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

			CmdServersAllowlist.Activated += (object sender, EventArgs e) =>
			{
				ServersAllowlist();
			};

			CmdServersDenylist.Activated += (object sender, EventArgs e) =>
			{
				ServersDenylist();
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

			MnuServersAllowlist.Activated += (object sender, EventArgs e) =>
			{
				ServersAllowlist();
			};

			MnuServersDenylist.Activated += (object sender, EventArgs e) =>
			{
				ServersDenylist();
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

			CmdAreasAllowlist.Activated += (object sender, EventArgs e) =>
			{
				AreasAllowlist();
			};

			CmdAreasDenylist.Activated += (object sender, EventArgs e) =>
			{
				AreasDenylist();
			};

			CmdAreasUndefined.Activated += (object sender, EventArgs e) =>
			{
				AreasUndefinedList();
			};

			MnuAreasAllowlist.Activated += (object sender, EventArgs e) =>
			{
				AreasAllowlist();
			};

			MnuAreasDenylist.Activated += (object sender, EventArgs e) =>
			{
				AreasDenylist();
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
				Engine.ProfileOptions.SetBool("servers.locklast", ChkServersLockCurrent.State == NSCellStateValue.On);
			};

			CboServersScoringRule.Activated += (object sender, EventArgs e) =>
			{
				Engine.ProfileOptions.Set("servers.scoretype", GuiUtils.GetSelected(CboServersScoringRule));

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

			CboServersScoringRule.ToolTip = LanguageManager.GetText(LanguageItems.TooltipServersScoreType);
			CmdAreasDenylist.ToolTip = LanguageManager.GetText(LanguageItems.TooltipAreasDenylist);

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
				bool result = GuiUtils.MessageYesNo(LanguageManager.GetText(LanguageItems.ExitConfirm));
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

						GuiUtils.SetHidden(CmdCancel, (Engine.IsWaitingCancelAllowed() == false));
						GuiUtils.SetEnabled(CmdCancel, (Engine.IsWaitingCancelPending() == false));
						GuiUtils.SetEnabled(MnuTrayConnect, CmdCancel.Enabled);
					}
					else if (Engine.IsConnected())
					{
						ImgProgress.StopAnimation(this);
						ImgTopPanel.Image = NSImage.ImageNamed("topbar_osx_green.png");
						MnuTrayStatus.Image = NSImage.ImageNamed("status_green_16.png");
						LblTopStatus.StringValue = LanguageManager.GetText(LanguageItems.TopBarConnected, Engine.CurrentServer.DisplayName);


                        TabOverview.SelectAt(2);

						LblConnectedServerName.StringValue = Engine.CurrentServer.DisplayName;
						LblConnectedLocation.StringValue = Engine.CurrentServer.GetLocationForList();
						TxtConnectedExitIp.StringValue = Engine.Connection.ExitIPs.ToString();
						ImgConnectedCountry.Image = NSImage.ImageNamed("flag_" + Engine.CurrentServer.CountryCode.ToLowerInvariant() + ".png");
					}
					else
					{
						ImgProgress.StopAnimation(this);
						ImgTopPanel.Image = NSImage.ImageNamed("topbar_osx_red.png");
						MnuTrayStatus.Image = NSImage.ImageNamed("status_red_16.png");
						if (Engine.Instance.NetworkLockManager.IsActive())
                            LblTopStatus.StringValue = LanguageManager.GetText(LanguageItems.TopBarNotConnectedLocked);
						else
                            LblTopStatus.StringValue = LanguageManager.GetText(LanguageItems.TopBarNotConnectedExposed);

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

                        TxtConnectedDownload.StringValue = LanguageManager.FormatBytes(Engine.Connection.BytesLastDownloadStep, true, false);
                        TxtConnectedUpload.StringValue = LanguageManager.FormatBytes(Engine.Connection.BytesLastUploadStep, true, false);
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
                        string currentKey = Engine.Instance.ProfileOptions.Get("key");
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
				MnuTrayConnect.Title = LanguageManager.GetText(LanguageItems.CommandDisconnect);
			}
			else if (appColor == "yellow")
			{
				StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_yellow.png");
				
				MnuTrayConnect.Title = LanguageManager.GetText(LanguageItems.CommandCancel);
			}
			else if (appColor == "red")
			{
				StatusItem.Image = NSImage.ImageNamed("menubar_" + colorMode.ToLowerInvariant() + "_red.png");
				MnuTrayConnect.Title = LanguageManager.GetText(LanguageItems.CommandConnect);
			}

            MnuTrayConnect.Title = mainActionText;
			GuiUtils.SetEnabled(MnuTrayConnect, (mainActionCommand != ""));

            m_mainActionCommand = mainActionCommand;
		}

		public void ShowNotification(string title, string level)
		{
			RequestAttention();

			if (Engine.Instance.ProfileOptions.GetBool("gui.notifications") == false)
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
			GuiUtils.SetHidden(CmdUpdater, false);
			GuiUtils.SetHidden(MnuTrayUpdate, false);

            CoreGraphics.CGSize s = LblVersion.Frame.Size;
            s.Width -= CmdUpdater.Frame.Width;
            LblVersion.SetFrameSize(s);
        }

        public void ChangeVisibility(bool vis)
        {
            if (vis)
                MnuTrayRestore.Title = LanguageManager.GetText(LanguageItems.WindowsMainHide);
            else
                MnuTrayRestore.Title = LanguageManager.GetText(LanguageItems.WindowsMainShow);
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
				GuiUtils.SetHidden(LblLoginIcon, false);
				GuiUtils.SetHidden(LblLogin, false);
				GuiUtils.SetHidden(TxtAirU, false);
				GuiUtils.SetHidden(LblPassword, false);
				GuiUtils.SetHidden(TxtPassword, false);
				GuiUtils.SetHidden(CmdLogin, false);
				GuiUtils.SetHidden(ChkRemember, false);

				bool airvpnLogged = Engine.IsLogged();

				if (airvpnLogged == false)
					CmdLogin.Title = LanguageManager.GetText(LanguageItems.CommandLoginButton);
				else
					CmdLogin.Title = LanguageManager.GetText(LanguageItems.CommandLogout);

				GuiUtils.SetEnabled(CmdLogin,((waiting == false) && (connected == false) && (TxtAirU.StringValue.Trim() != "") && (TxtPassword.StringValue.Trim() != "")));
				GuiUtils.SetEnabled(TxtAirU, (airvpnLogged == false));
				GuiUtils.SetEnabled(TxtPassword, (airvpnLogged == false));
				GuiUtils.SetHidden(LblKey, ((airvpnLogged == false) || (CboKey.ItemCount < 2)));
				GuiUtils.SetHidden(CboKey, LblKey.Hidden);
			}
			else
			{
				GuiUtils.SetHidden(LblLoginIcon, true);
				GuiUtils.SetHidden(LblLogin, true);
				GuiUtils.SetHidden(TxtAirU, true);
				GuiUtils.SetHidden(LblPassword, true);
				GuiUtils.SetHidden(TxtPassword, true);
				GuiUtils.SetHidden(CmdLogin, true);
				GuiUtils.SetHidden(LblKey, true);
				GuiUtils.SetHidden(CboKey, true);
				GuiUtils.SetHidden(ChkRemember, true);
			}

            ChangeVisibility(this.Window.IsVisible);

			GuiUtils.SetEnabled(CmdConnect, Engine.Instance.CanConnect());

			GuiUtils.SetEnabled(CmdProviderAdd, true);
			GuiUtils.SetEnabled(CmdProviderRemove, (TableProviders.SelectedRowCount > 0));
			GuiUtils.SetEnabled(CmdProviderEdit, (TableProviders.SelectedRowCount > 0));

			GuiUtils.SetEnabled(CmdServersConnect, ((selectedConnection != null) && (selectedConnection.CanConnect())));
			GuiUtils.SetEnabled(CmdServersAllowlist, (TableServers.SelectedRowCount > 0));
			GuiUtils.SetEnabled(CmdServersDenylist, CmdServersAllowlist.Enabled);
			GuiUtils.SetEnabled(CmdServersUndefined, CmdServersAllowlist.Enabled);
			GuiUtils.SetEnabled(MnuServersConnect, CmdServersConnect.Enabled);
			GuiUtils.SetEnabled(MnuServersAllowlist, CmdServersAllowlist.Enabled);
			GuiUtils.SetEnabled(MnuServersDenylist, CmdServersDenylist.Enabled);
			GuiUtils.SetEnabled(MnuServersUndefined, CmdServersUndefined.Enabled);

			GuiUtils.SetEnabled(CmdServersMore, (TableServers.SelectedRowCount == 1));
			GuiUtils.SetEnabled(MnuServersMore, CmdServersMore.Enabled);

			GuiUtils.SetEnabled(CmdServersRename, ((selectedConnection != null) && (selectedConnection.Provider is Core.Providers.OpenVPN)));
			GuiUtils.SetEnabled(MnuServersRename, CmdServersRename.Enabled);

			GuiUtils.SetEnabled(CmdAreasAllowlist, (TableAreas.SelectedRowCount > 0));
			GuiUtils.SetEnabled(CmdAreasDenylist, CmdAreasAllowlist.Enabled);
			GuiUtils.SetEnabled(CmdAreasUndefined, CmdAreasAllowlist.Enabled);
			GuiUtils.SetEnabled(MnuAreasAllowlist, CmdAreasAllowlist.Enabled);
			GuiUtils.SetEnabled(MnuAreasDenylist, CmdAreasDenylist.Enabled);
			GuiUtils.SetEnabled(MnuAreasUndefined, CmdAreasUndefined.Enabled);

			GuiUtils.SetHidden(CmdLogsCommand, (Engine.ProfileOptions.GetBool("advanced.expert") == false));

			if (Engine.Instance.NetworkLockManager != null)
			{
				GuiUtils.SetHidden(CmdNetworkLock, (Engine.Instance.NetworkLockManager.CanEnabled() == false));
				GuiUtils.SetHidden(ImgNetworkLock, CmdNetworkLock.Hidden);
				if (Engine.Instance.NetworkLockManager.IsActive())
				{
					CmdNetworkLock.Title = LanguageManager.GetText(LanguageItems.NetworkLockButtonActive);
					ImgNetworkLock.Image = NSImage.ImageNamed("netlock_on.png");

					LblNetLockStatus.Image = NSImage.ImageNamed("netlock_status_on.png");
					LblNetLockStatus.ToolTip = LanguageManager.GetText(LanguageItems.NetworkLockStatusActive);

				}
				else
				{
					CmdNetworkLock.Title = LanguageManager.GetText(LanguageItems.NetworkLockButtonDeactive);
					ImgNetworkLock.Image = NSImage.ImageNamed("netlock_off.png");

					LblNetLockStatus.Image = NSImage.ImageNamed("netlock_status_off.png");
					LblNetLockStatus.ToolTip = LanguageManager.GetText(LanguageItems.NetworkLockStatusDeactive);
				}
			}

			if (Engine.Instance.ProfileOptions.GetBool("advanced.providers"))
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

		public void ProviderManifestFailed(Core.Providers.IProvider provider)
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
			GuiUtils.SetEnabled(MnuServersRefresh, true);
			GuiUtils.SetEnabled(CmdServersRefresh, true);
		}

		public void RequestAttention()
		{
			NSApplication.SharedApplication.RequestUserAttention(NSRequestUserAttentionType.InformationalRequest);
		}

		public bool NetworkLockKnowledge()
		{
			if (Engine.Instance.ProfileOptions.GetBool("ui.skip.netlock.confirm"))
				return true;

			string msg = LanguageManager.GetText(LanguageItems.NetworkLockWarning);

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
			Engine.ProfileOptions.Set("login", TxtAirU.StringValue);
			Engine.ProfileOptions.Set("password", TxtPassword.StringValue);

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
			GuiUtils.SetEnabled(CmdCancel, false);
			GuiUtils.SetEnabled(MnuTrayConnect, false);

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
				Core.Providers.IProvider p = Engine.Instance.ProvidersManager.Providers[i];
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
				Core.Providers.IProvider p = Engine.Instance.ProvidersManager.Providers[i];
				if (p is Core.Providers.OpenVPN)
				{
					WindowProviderEditOpenVPNController w = new WindowProviderEditOpenVPNController();
					w.Provider = p as Core.Providers.OpenVPN;
					NSApplication.SharedApplication.RunModalForWindow(w.Window);
					if (w.Provider != null)
						updated = true;
				}
				else if (p is Core.Providers.WireGuard)
				{
					WindowProviderEditWireGuardController w = new WindowProviderEditWireGuardController();
					w.Provider = p as Core.Providers.WireGuard;
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

		void ServersAllowlist()
		{
			foreach (int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ConnectionInfo.UserListType.Allowlist;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI();
		}

		void ServersDenylist()
		{
			foreach (int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ConnectionInfo.UserListType.Denylist;
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
			GuiUtils.SetEnabled(MnuServersRefresh, false);
			GuiUtils.SetEnabled(CmdServersRefresh, false);

			Engine.Instance.RefreshProvidersInvalidateConnections();
		}

		void AreasAllowlist()
		{
			foreach (int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.Allowlist;
			}
			Engine.UpdateSettings();
			TableServersController.RefreshUI();
		}

		void AreasDenylist()
		{
			foreach (int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.Denylist;
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
				GuiUtils.MessageBoxInfo(LanguageManager.GetText(LanguageItems.LogsCopyClipboardDone));
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

					GuiUtils.MessageBoxInfo(LanguageManager.GetText(LanguageItems.LogsSaveToFileDone));
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


