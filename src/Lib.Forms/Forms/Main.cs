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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using AirVPN.Core;
using AirVPN.Gui.Controls;

namespace AirVPN.Gui.Forms
{
    public partial class Main : AirVPN.Gui.Form
    {
		private Controls.ChartSpeed m_pnlCharts;
		private Controls.MenuButton m_cmdMainMenu;
		private Controls.ProgressInfinite m_imgProgressInfinite;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
		
		private ListViewServers m_listViewServers;
		private ListViewAreas m_listViewAreas;

		private Dictionary<string, ListViewItemStats> m_statsItems = new Dictionary<string, ListViewItemStats>();

		private Font m_topBarFont = new Font("Verdana", 10);
		
		private int m_windowMinimumWidth = 500;
		private int m_windowMinimumHeight = 300;
		private int m_windowDefaultWidth = 600;
		private int m_windowDefaultHeight = 400;
		private bool m_lockCoordUpdate = false;		
        private int m_topHeaderHeight = 30;

		private bool m_formReady = false;
		private bool m_closing = false;

		private System.Timers.Timer timerMonoDelayedRedraw = null;

        public Main()
        {
            InitializeComponent();
        }

        private delegate void DeInitDelegate();
        public void DeInit()
        {
            if (this.InvokeRequired)
            {
                DeInitDelegate inv = new DeInitDelegate(this.DeInit);

                this.Invoke(inv, new object[] {  });
            }
            else
            {
		        if (m_notifyIcon != null)
                {
                    m_notifyIcon.Dispose();
                    m_notifyIcon = null;
                }

				//DoDispose();
				
				m_closing = true;
                Close();
            }
        }

		/*
		private void DoDispose()
		{
			// Workaround experiment for Mono bug.
			// Mono bug https://bugs.debian.org/cgi-bin/bugreport.cgi?bug=742774
			// Mono bug https://bugs.debian.org/cgi-bin/bugreport.cgi?bug=727651

			DoDispose(lstLogs.ContextMenuStrip);
			lstLogs.ContextMenuStrip = null;

			DoDispose(m_listViewServers.ContextMenuStrip);
			m_listViewServers.ContextMenuStrip = null;

			DoDispose(m_listViewAreas.ContextMenuStrip);
			m_listViewAreas.ContextMenuStrip = null;

			DoDispose(this.ContextMenuStrip);
			this.ContextMenuStrip = null;

			DoDispose(this.mnuStatus);
			DoDispose(this.mnuConnect);
			DoDispose(this.mnuHomePage);
			DoDispose(this.mnuUser);
			DoDispose(this.mnuPorts);
			DoDispose(this.mnuSpeedTest);
			DoDispose(this.mnuSettings);
			DoDispose(this.mnuDevelopers);
			DoDispose(this.mnuDevelopersManText);
			DoDispose(this.mnuDevelopersManBBCode);
			DoDispose(this.mnuDevelopersUpdateManifest);
			DoDispose(this.mnuDevelopersDefaultManifest);
			DoDispose(this.mnuDevelopersReset);
			DoDispose(this.mnuTools);
			DoDispose(this.mnuToolsPortForwarding);
			DoDispose(this.mnuToolsNetworkMonitor);
			DoDispose(this.mnuAbout);
			DoDispose(this.mnuRestore);
			DoDispose(this.mnuExit);
			DoDispose(this.mnuStatus);
			DoDispose(this.mnuConnect);
			DoDispose(this.mnuHomePage);
			DoDispose(this.mnuUser);
			DoDispose(this.mnuPorts);
			DoDispose(this.mnuSpeedTest);
			DoDispose(this.mnuSettings);
			DoDispose(this.mnuDevelopers);
			DoDispose(this.mnuDevelopersManText);
			DoDispose(this.mnuDevelopersManBBCode);
			DoDispose(this.mnuDevelopersUpdateManifest);
			DoDispose(this.mnuDevelopersDefaultManifest);
			DoDispose(this.mnuDevelopersReset);
			DoDispose(this.mnuTools);
			DoDispose(this.mnuToolsPortForwarding);
			DoDispose(this.mnuToolsNetworkMonitor);
			DoDispose(this.mnuAbout);
			DoDispose(this.mnuRestore);
			DoDispose(this.mnuExit);
			DoDispose(this.mnuServersConnect);
			DoDispose(this.mnuServersWhiteList);
			DoDispose(this.mnuServersBlackList);
			DoDispose(this.mnuServersUndefined);
			DoDispose(this.mnuAreasWhiteList);
			DoDispose(this.mnuAreasBlackList);
			DoDispose(this.mnuAreasUndefined);
			DoDispose(this.mnuServersRefresh);
		}
		*/

		private void DoDispose(IDisposable o)
		{
			if (o != null)
			{
				o.Dispose();
				o = null;
			}
		}

		private void DoDispose(System.Windows.Forms.ToolStripMenuItem o)
		{			
			o.Dispose();
			o = null;
		}

        protected override void OnLoad(EventArgs e)
        {
			m_lockCoordUpdate = true;

			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			MinimumSize = new Size(m_windowMinimumWidth, m_windowMinimumHeight);

			KeyPreview = true;  // 2.10.1

			m_formReady = false;

			Visible = false;

            base.OnLoad(e);

            CommonInit("");

			if (Platform.Instance.IsTraySupported())
            {
                m_notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
                m_notifyIcon.Icon = this.Icon;
                m_notifyIcon.Text = "AirVPN";
                m_notifyIcon.Visible = true;
                m_notifyIcon.BalloonTipTitle = Constants.Name;

                m_notifyIcon.MouseDoubleClick += new MouseEventHandler(notifyIcon_MouseDoubleClick);
                //m_notifyIcon.Click += new EventHandler(notifyIcon_Click);				
				m_notifyIcon.ContextMenuStrip = mnuMain;
            }

			m_imgProgressInfinite = new ProgressInfinite();
			this.pnlWaiting.Controls.Add(m_imgProgressInfinite);
			
			// Controls initialization
			mnuDevelopers.Visible = Engine.Instance.DevelopmentEnvironment;
			mnuTools.Visible = Engine.Instance.DevelopmentEnvironment;

			chkRemember.BackColor = Color.Transparent;
			
			m_pnlCharts = new ChartSpeed();
			m_pnlCharts.Left = holSpeedChart.Left;
			m_pnlCharts.Top = holSpeedChart.Top;
			m_pnlCharts.Width = holSpeedChart.Width;
			m_pnlCharts.Height = holSpeedChart.Height;
			m_pnlCharts.Anchor = holSpeedChart.Anchor;
			holSpeedChart.Visible = false;
			tabSpeed.Controls.Add(m_pnlCharts);

			m_cmdMainMenu = new MenuButton();
			m_cmdMainMenu.Left = 0;
			m_cmdMainMenu.Top = 0;
			m_cmdMainMenu.Width = 150;
			m_cmdMainMenu.Height = m_topHeaderHeight;
			m_cmdMainMenu.Click += cmdMenu_Click;
			Controls.Add(m_cmdMainMenu);

			m_listViewServers = new ListViewServers();
			m_listViewServers.ContextMenuStrip = mnuServers;
			m_listViewServers.Dock = DockStyle.Fill;
			pnlServers.Controls.Add(m_listViewServers);
						
			m_listViewAreas = new ListViewAreas();
			m_listViewAreas.ContextMenuStrip = mnuAreas;
			m_listViewAreas.Dock = DockStyle.Fill;
			pnlAreas.Controls.Add(m_listViewAreas);

			m_listViewServers.MouseDoubleClick += new MouseEventHandler(m_listViewServers_MouseDoubleClick);
			m_listViewServers.SelectedIndexChanged += new EventHandler(m_listViewServers_SelectedIndexChanged);
			m_listViewAreas.SelectedIndexChanged += new EventHandler(m_listViewAreas_SelectedIndexChanged);

			lstStats.ImageIconResourcePrefix = "stats_";

			lstLogs.ImageIconResourcePrefix = "log_";

			chkShowAll.Checked = false;
			chkLockLast.Checked = Engine.Storage.GetBool("servers.locklast");
			cboScoreType.Text = Engine.Storage.Get("servers.scoretype");

            Form.ChangeSkin(Engine.Storage.Get("gui.skin"));
            ApplySkin();

			SetFormLayout(Engine.Storage.Get("forms.main"), true, true, new Size(m_windowDefaultWidth, m_windowDefaultHeight));
			
			foreach (StatsEntry statsEntry in Engine.Stats.List)
			{
				ListViewItemStats statsEntryItem = new ListViewItemStats();
				statsEntryItem.Entry = statsEntry;
				statsEntryItem.Text = statsEntry.Caption;
				statsEntryItem.ImageKey = statsEntry.Icon;

				lstStats.Items.Add(statsEntryItem);
				m_statsItems[statsEntry.Key] = statsEntryItem;

				StatsChange(statsEntry); // Without this, glitch in listview under Linux
			}

			cboSpeedResolution.Items.Clear();
			cboSpeedResolution.Items.Add(Messages.WindowsMainSpeedResolution1);
			cboSpeedResolution.Items.Add(Messages.WindowsMainSpeedResolution2);
			cboSpeedResolution.Items.Add(Messages.WindowsMainSpeedResolution3);
			cboSpeedResolution.Items.Add(Messages.WindowsMainSpeedResolution4);
			cboSpeedResolution.Items.Add(Messages.WindowsMainSpeedResolution5);
			cboSpeedResolution.SelectedIndex = 0;

			// Tooltips
			cmdConnect.Text = Messages.CommandConnect;
			lblConnectSubtitle.Text = Messages.CommandConnectSubtitle;
			cmdDisconnect.Text = Messages.CommandDisconnect;
			cmdCancel.Text = Messages.CommandCancel;
			this.tip.SetToolTip(this.cboScoreType, Messages.TooltipServersScoreType);
			this.tip.SetToolTip(this.chkLockLast, Messages.TooltipServersLockCurrent);
			this.tip.SetToolTip(this.chkShowAll, Messages.TooltipServersShowAll);
			this.tip.SetToolTip(this.cmdServersConnect, Messages.TooltipServersConnect);
			this.tip.SetToolTip(this.cmdServersUndefined, Messages.TooltipServersUndefined);
			this.tip.SetToolTip(this.cmdServersBlackList, Messages.TooltipServersBlackList);
			this.tip.SetToolTip(this.cmdServersWhiteList, Messages.TooltipServersWhiteList);
			this.tip.SetToolTip(this.cmdAreasUndefined, Messages.TooltipAreasUndefined);
			this.tip.SetToolTip(this.cmdAreasBlackList, Messages.TooltipAreasBlackList);
			this.tip.SetToolTip(this.cmdAreasWhiteList, Messages.TooltipAreasWhiteList);
			this.tip.SetToolTip(this.cmdLogsOpenVpnManagement, Messages.TooltipLogsOpenVpnManagement);
			this.tip.SetToolTip(this.cmdLogsClean, Messages.TooltipLogsClean);
			this.tip.SetToolTip(this.cmdLogsCopy, Messages.TooltipLogsCopy);
			this.tip.SetToolTip(this.cmdLogsSave, Messages.TooltipLogsSave);
			this.tip.SetToolTip(this.cmdLogsSupport, Messages.TooltipLogsSupport);


			// Start
			if (Engine.Storage.GetBool("remember"))
			{
				chkRemember.Checked = true;
				txtLogin.Text = Engine.Storage.Get("login");
				txtPassword.Text = Engine.Storage.Get("password");				
			}

			m_lockCoordUpdate = false;

			Resizing();
            Show();

			m_formReady = true;

			Engine.OnRefreshUi();

			if (Platform.IsUnix())
			{
				// Mono Bug, issue on start drawing in some systems like Mint
				timerMonoDelayedRedraw = new System.Timers.Timer();
				timerMonoDelayedRedraw.Elapsed += new System.Timers.ElapsedEventHandler(OnMonoDelayedRedraw);
				timerMonoDelayedRedraw.Interval = 1000;
				timerMonoDelayedRedraw.Enabled = true;
			}
        }

		void OnMonoDelayedRedraw(object sender, System.Timers.ElapsedEventArgs e)
		{
			timerMonoDelayedRedraw.Enabled = false;

			Refresh();
		}

		protected override void OnKeyDown(KeyEventArgs e) // 2.10.1
		{
			base.OnKeyDown(e);

			if (e.Control && e.KeyCode == Keys.M)
			{
				ShowMenu();
			}

			if (e.Control && e.KeyCode == Keys.A)
			{
				ShowAbout();
			}

			if (e.Control && e.KeyCode == Keys.P)
			{
				ShowPreferences();
			}
		}
        
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
			try
            {   
                Skin.GraphicsCommon(e.Graphics);

				Rectangle rectHeader = new Rectangle(m_cmdMainMenu.Width, 0, ClientSize.Width - m_cmdMainMenu.Width, m_topHeaderHeight);
				Rectangle rectHeaderText = new Rectangle(m_cmdMainMenu.Width, 0, ClientSize.Width - m_cmdMainMenu.Width - 10 - 30 - 10, m_topHeaderHeight);

				Form.DrawImage(e.Graphics, Skin.MainBackImage, new Rectangle(0, 0, ClientSize.Width, m_topHeaderHeight));

				Image iconFlag = null;
				if (Engine.CurrentServer != null)
				{
					string iconFlagCode = Engine.CurrentServer.CountryCode;
					if (imgCountries.Images.ContainsKey(iconFlagCode))
						iconFlag = imgCountries.Images[iconFlagCode];

					if (iconFlag != null)
					{
						rectHeaderText.Width -= iconFlag.Width + 5;
					}
				}

				if (Engine.IsWaiting())
				{					
					DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_yellow"), rectHeader);
					Form.DrawStringOutline(e.Graphics, Engine.WaitMessage, m_topBarFont, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);										
				}
				else if (Engine.IsConnected())
				{	
					string serverName = Engine.CurrentServer.PublicName;

					DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_green"), rectHeader);

					Form.DrawStringOutline(e.Graphics, Messages.Format(Messages.TopBarConnected, serverName), m_topBarFont, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);
				}
				else
				{
					DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_red"), rectHeader);
					if (Engine.Instance.NetworkLockManager.IsActive())
					{
						Form.DrawStringOutline(e.Graphics, Messages.TopBarNotConnectedLocked, m_topBarFont, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);
					}
					else
					{
						Form.DrawStringOutline(e.Graphics, Messages.TopBarNotConnectedExposed, m_topBarFont, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);
					}
				}

				if (iconFlag != null)
				{
					Rectangle rectFlag = new Rectangle(rectHeader.Right - 30 - iconFlag.Width - 10, 5, iconFlag.Width, iconFlag.Height);
					DrawImage(e.Graphics, iconFlag, rectFlag);					
				}

				DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_shadow"), rectHeader);
                
				/*
				{
					String msg = "Developer mode";
					msg += " - " + Core.RandomGenerator.GetInt(0, 999999).ToString();
					//msg += " - " + ClientSize.ToString();
					//msg += " - " + m_windowPanel1Height.ToString() + ">" + m_windowPanel2Height.ToString();
					Rectangle r = new Rectangle(160, 0, ClientSize.Width, ClientSize.Height);					
					DrawString(e.Graphics, msg, Font, Brushes.Black, r, GuiUtils.StringFormatLeftTop);
				}
				*/				
            }
            catch (Exception ex)
            {
                Core.Debug.Trace(ex);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
			if (m_closing)
				return;

			e.Cancel = true;

			if(Engine.Storage.GetBool("gui.exit_confirm") == true)
				if (MessageBox.Show(this, Messages.ExitConfirm, Constants.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
					return;
			
			Gui.Engine engine = Engine.Instance as Gui.Engine;

            if (engine.FormMain != null)
            {
                engine.Storage.Set("forms.main", engine.FormMain.GetFormLayout());
            }

            Engine.RequestStop();

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);            
        }
              
                
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

			if (Engine.Storage != null)
			{
				if (Engine.Storage.GetBool("gui.windows.tray"))
				{
					if (Platform.Instance.IsTraySupported())
					{
						if (FormWindowState.Minimized == WindowState)
						{
							Hide();
							EnabledUi();
						}
					}
				}

				Resizing();
			}            
			
        }

		#region UI Controls Events

        void notifyIcon_Click(object sender, EventArgs e)
        {
            ShowMenu();
        }

        void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Restore();
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            ShowAbout();
        }        

        private void mnuRestore_Click(object sender, EventArgs e)
        {
			if (this.Visible == false)
				Restore();
			else
				this.WindowState = FormWindowState.Minimized;
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuPorts_Click(object sender, EventArgs e)
        {
			Core.UI.Actions.OpenUrlPorts();
        }

        private void mnuUser_Click(object sender, EventArgs e)
        {
			Core.UI.Actions.OpenUrlClient();
        }

        private void mnuSpeedTest_Click(object sender, EventArgs e)
        {
			Core.UI.Actions.OpenUrlSpeedTest();
        }

        private void mnuHomePage_Click(object sender, EventArgs e)
        {
			Core.UI.Actions.OpenUrlWebsite();
        }

        private void mnuSettings_Click(object sender, EventArgs e)
        {
			ShowPreferences(); // 2.10.1
        }

		private void mnuStatus_Click(object sender, EventArgs e)
		{
			Restore();
		}

		private void mnuConnect_Click(object sender, EventArgs e)
		{
			if (Engine.IsWaiting())
			{
				Disconnect();
			}
			else if (Engine.IsConnected())
			{
				Disconnect();
			}
			else if (Engine.IsLogged())
			{
				Connect();
			}
			else
			{
				Restore();
			}
		}
		
		private void chkRemember_CheckedChanged(object sender, EventArgs e)
		{
			Engine.Storage.SetBool("remember", chkRemember.Checked);
		}

		private void cmdLogin_Click(object sender, EventArgs e)
		{
			if(Engine.IsLogged() == false)
				Login();
			else
				Logout();
		}
		
		private void txtLogin_TextChanged(object sender, EventArgs e)
        {
			//Engine.OnRefreshUi();
			EnabledUi();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            //Engine.OnRefreshUi();
			EnabledUi();
        }


		private void cmdConnect_Click(object sender, EventArgs e)
		{
			Connect();
		}


		private void cmdLockedNetwork_Click(object sender, EventArgs e)
		{
			if (Engine.Instance.NetworkLockManager.IsActive())
				NetworkLockDeactivation();
			else
				NetworkLockActivation();
		}

		private void cmdDisconnect_Click(object sender, EventArgs e)
		{
			Disconnect();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{			
			Disconnect();
		}

        private void cmdMenu_Click(object sender, EventArgs e)
        {
            ShowMenu();
        }

        private void mnuToolsPortForwarding_Click(object sender, EventArgs e)
		{
			/*
			Forms.PortForwarding Dlg = new PortForwarding();
			Dlg.ShowDialog();
			*/
		}

		private void mnuToolsNetworkMonitor_Click(object sender, EventArgs e)
		{
			/*
			Forms.NetworkMonitor Dlg = new NetworkMonitor();
			Dlg.ShowDialog();
			*/			
		}

		private void mnuDevelopersUpdateManifest_Click(object sender, EventArgs e)
		{
			Core.Threads.Manifest.Instance.ForceUpdate = true;
		}

		private void mnuDevelopersNetworkMonitor_Click(object sender, EventArgs e)
		{
			/*
			 * NetworkMonitor dlg = new NetworkMonitor();
			dlg.Show();
			*/
		}


		private void mnuDevelopersManText_Click(object sender, EventArgs e)
		{
			Forms.TextViewer Dlg = new TextViewer();
			Dlg.Title = "Man";
			Dlg.Body = Core.UI.Actions.GetMan("text");
			Dlg.ShowDialog();
		}

		private void mnuDevelopersManBBCode_Click(object sender, EventArgs e)
		{
			Forms.TextViewer Dlg = new TextViewer();
			Dlg.Title = "Man";
			Dlg.Body = Core.UI.Actions.GetMan("bbc");
			Dlg.ShowDialog();
		}

		private void mnuDevelopersReset_Click(object sender, EventArgs e)
		{
			Dictionary<string, ServerInfo> servers;
			lock (Engine.Servers)
				servers = new Dictionary<string, ServerInfo>(Engine.Servers);
			foreach (ServerInfo infoServer in servers.Values)
			{
				infoServer.PingTests = 0;
				infoServer.PingFailedConsecutive = 0;
				infoServer.Ping = -1;
				infoServer.LastPingTest = 0;
				infoServer.LastPingResult = 0;
				infoServer.LastPingSuccess = 0;
			}

			Engine.OnRefreshUi();
		}
		
		private void mnuDevelopersDefaultManifest_Click(object sender, EventArgs e)
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode node = xmlDoc.ImportNode(Engine.Storage.Manifest, true);			
			xmlDoc.AppendChild(node);
			xmlDoc.FirstChild.Attributes.RemoveAll();
			xmlDoc.FirstChild.RemoveChild(xmlDoc.SelectSingleNode("//manifest/servers"));
			xmlDoc.FirstChild.RemoveChild(xmlDoc.SelectSingleNode("//manifest/areas"));
			
			using (var sw = new StringWriter())
			{
				using (var xw = new XmlTextWriter(sw))
				{
					xw.Formatting = Formatting.Indented;					
					xw.Indentation = 2; //default is 1. I used 2 to make the indents larger.

					xmlDoc.WriteTo(xw);
				}

				String body = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" + sw.ToString();

				Forms.TextViewer dlg = new TextViewer();
				dlg.Title = "Default manifest";
				dlg.Body = body;
				dlg.ShowDialog();
			}

			
		}
		
		void m_listViewServers_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ConnectManual();
		}

		private void mnuServersConnect_Click(object sender, EventArgs e)
		{
			ConnectManual();
		}

		private void cmdServersConnect_Click(object sender, EventArgs e)
		{
			mnuServersConnect_Click(sender, e);
		}

		private void DeselectServersListItem()
		{
			if (Platform.Instance.IsWindowsSystem() == false)
			{
				// To avoid a Mono ListView crash, reproducible by whitelisting servers
				foreach (ListViewItemServer item in m_listViewServers.Items)
				{
					item.Focused = false;
					item.Selected = false;
				}
			}
		}

		private void mnuServersWhitelist_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemServer item in m_listViewServers.SelectedItems)
			{
				item.Info.UserList = ServerInfo.UserListType.WhiteList;
			}
			Engine.UpdateSettings();
			DeselectServersListItem();
			m_listViewServers.UpdateList();
		}

		private void mnuServersBlacklist_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemServer item in m_listViewServers.SelectedItems)
			{
				item.Info.UserList = ServerInfo.UserListType.BlackList;
			}
			Engine.UpdateSettings();
			DeselectServersListItem();
			m_listViewServers.UpdateList();			
		}

		private void mnuServersUndefined_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemServer item in m_listViewServers.SelectedItems)
			{
				item.Info.UserList = ServerInfo.UserListType.None;
			}
			Engine.UpdateSettings();
			DeselectServersListItem();
			m_listViewServers.UpdateList();			
		}

		private void mnuServersRefresh_Click(object sender, EventArgs e)
		{
			mnuServersRefresh.Enabled = false;
			cmdServersRefresh.Enabled = false;				

			Core.Threads.Manifest.Instance.ForceUpdate = true;
		}
				
		private void cmdServersWhiteList_Click(object sender, EventArgs e)
		{
			mnuServersWhitelist_Click(sender, e);
		}

		private void cmdServersBlackList_Click(object sender, EventArgs e)
		{
			mnuServersBlacklist_Click(sender, e);
		}

		private void cmdServersUndefined_Click(object sender, EventArgs e)
		{
			mnuServersUndefined_Click(sender, e);
		}

		private void cmdServersRefresh_Click(object sender, EventArgs e)
		{
			mnuServersRefresh_Click(sender, e);
		}

		private void mnuAreasWhiteList_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemArea item in m_listViewAreas.SelectedItems)
			{
				item.Info.UserList = AreaInfo.UserListType.WhiteList;
			}
			Engine.UpdateSettings();
			m_listViewAreas.UpdateList();
			m_listViewServers.UpdateList();
		}

		private void mnuAreasBlackList_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemArea item in m_listViewAreas.SelectedItems)
			{
				item.Info.UserList = AreaInfo.UserListType.BlackList;
			}
			Engine.UpdateSettings();
			m_listViewAreas.UpdateList();
			m_listViewServers.UpdateList();
		}

		private void mnuAreasUndefined_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemArea item in m_listViewAreas.SelectedItems)
			{
				item.Info.UserList = AreaInfo.UserListType.None;
			}
			Engine.UpdateSettings();
			m_listViewAreas.UpdateList();
			m_listViewServers.UpdateList();
		}


		private void cmdAreasWhiteList_Click(object sender, EventArgs e)
		{
			mnuAreasWhiteList_Click(sender, e);
		}

		private void cmdAreasBlackList_Click(object sender, EventArgs e)
		{
			mnuAreasBlackList_Click(sender, e);
		}

		private void cmdAreasUndefined_Click(object sender, EventArgs e)
		{
			mnuAreasUndefined_Click(sender, e);
		}

		private void chkShowAll_CheckedChanged(object sender, EventArgs e)
		{
			m_listViewServers.ShowAll = chkShowAll.Checked;
			m_listViewServers.UpdateList();
		}

		void m_listViewServers_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnabledUi();
		}

		void m_listViewAreas_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnabledUi();
		}

		private void chkLockCurrent_CheckedChanged(object sender, EventArgs e)
		{
			Engine.Storage.SetBool("servers.locklast", chkLockLast.Checked);
		}

		private void lstStats_DoubleClick(object sender, EventArgs e)
		{
			if (lstStats.SelectedItems.Count != 1)
				return;

			ListViewItemStats item = lstStats.SelectedItems[0] as ListViewItemStats;

			if (item.Entry.Key == "VpnGeneratedOVPN")
			{
				if (Engine.IsConnected() == false)
					return;

				Forms.TextViewer Dlg = new TextViewer();
				Dlg.Title = item.Entry.Caption;
				Dlg.Body = Engine.ConnectedOVPN;
				Dlg.ShowDialog();
			}
			else if (item.Entry.Key == "SystemReport")
			{
				Forms.TextViewer Dlg = new TextViewer();
				Dlg.Title = item.Entry.Caption;
				Dlg.Body = Platform.Instance.GenerateSystemReport();
				Dlg.ShowDialog();
			}
			else if (item.Entry.Key == "ManifestLastUpdate")
			{
				Core.Threads.Manifest.Instance.ForceUpdate = true;
			}
		}

		private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Under Linux, to close context menù.
			if (mnuAreas.Visible)
				mnuAreas.Close();
			if (mnuServers.Visible)
				mnuServers.Close();
		}

		private void cboScoreType_SelectedIndexChanged(object sender, EventArgs e)
		{
			Engine.Storage.Set("servers.scoretype", cboScoreType.Text);

			RefreshUi(Core.Engine.RefreshUiMode.Full);
		}

		private void mnuContextCopyAll_Click(object sender, EventArgs e)
		{
			LogsDoCopy(false);
		}

		private void mnuContextSaveAll_Click(object sender, EventArgs e)
		{
			LogsDoSave(false);
		}

		private void mnuContextCopySelected_Click(object sender, EventArgs e)
		{
			LogsDoCopy(true);
		}

		private void mnuContextSaveSelected_Click(object sender, EventArgs e)
		{
			LogsDoSave(true);
		}

		private void cmdLogsClean_Click(object sender, EventArgs e)
		{
			lstLogs.Items.Clear();
		}

		private void cmdLogsSave_Click(object sender, EventArgs e)
		{
			LogsDoSave(false);
		}

		private void cmdLogsCopy_Click(object sender, EventArgs e)
		{
			LogsDoCopy(false);
		}

		private void cmdLogsSupport_Click(object sender, EventArgs e)
		{
			// Ready for additional logging
			LogsDoCopy(false);
		}

		private void cmdLogsOpenVpnManagement_Click(object sender, EventArgs e)
		{
			if (Engine.IsConnected())
			{
				OpenVpnManagementCommand Dlg = new OpenVpnManagementCommand();
				if (Dlg.ShowDialog() == DialogResult.OK)
				{
					Core.UI.Actions.SendOpenVpnManagementCommand(Dlg.Command);
				}
			}
		}

		private void cboSpeedResolution_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_pnlCharts.Switch(cboSpeedResolution.SelectedIndex);
		}
		
		#endregion

		public void Resizing()
		{
			if (m_lockCoordUpdate)
				return;

			if (m_pnlCharts != null)
			{
				tabMain.Left = 0;
				tabMain.Top = m_topHeaderHeight;
				tabMain.Width = this.ClientSize.Width;
				tabMain.Height = this.ClientSize.Height - m_topHeaderHeight;

				int tabItemWidth = tabMain.SelectedTab.ClientSize.Width;
				int tabItemHeight = tabMain.SelectedTab.ClientSize.Height;
				
				// Welcome items
				pnlWelcome.Left = tabItemWidth / 2 - pnlWelcome.Width / 2;
				pnlWelcome.Top = tabItemHeight / 2 - pnlWelcome.Height / 2;
				pnlConnected.Left = tabItemWidth / 2 - pnlConnected.Width / 2;
				pnlConnected.Top = tabItemHeight / 2 - pnlConnected.Height / 2;

				// Waiting items
				pnlWaiting.Left = 0;
				pnlWaiting.Top = 0;
				pnlWaiting.Width = tabItemWidth;
				pnlWaiting.Height = tabItemHeight;
				int imgProgressTop = (tabItemHeight / 2) - (13 / 2);
				Size imgProgressSize = new Size(208, 13);
				if (m_imgProgressInfinite != null)
				{
					m_imgProgressInfinite.Size = imgProgressSize;
					m_imgProgressInfinite.Left = (tabItemWidth / 2) - (208 / 2);					
					m_imgProgressInfinite.Top = (tabItemHeight / 2) - (13 / 2);					
				}
				lblWait1.Left = 0;
				lblWait1.Top = 0;
				lblWait1.Width = tabItemWidth;
				lblWait1.Height = imgProgressTop - 10;
				lblWait2.Left = 0;
				lblWait2.Top = imgProgressTop + imgProgressSize.Height + 10;
				lblWait2.Width = tabItemWidth;
				lblWait2.Height = tabItemHeight - lblWait2.Top - 10 - 60;
				cmdCancel.Width = tabItemWidth * 2 / 3;
				cmdCancel.Height = 30;
				cmdCancel.Left = tabItemWidth / 2 - cmdCancel.Width / 2;
				cmdCancel.Top = tabItemHeight - 50;				
			}			
		}
        
        public void Restore()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
			EnabledUi();
        }

        public void ShowMenu()
        {
			Point p = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			mnuMain.Show(p);
			/*
            if (mnuMain.Visible)
                mnuMain.Close();
            else
            {
                Point p = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                mnuMain.Show(p);
            } 
			*/
        }

		public void Login()
		{
			Engine.Storage.Set("login", txtLogin.Text);
			Engine.Storage.Set("password", txtPassword.Text);

			if (TermsOfServiceCheck(false) == false)
				return;

			Engine.Login();
		}

		public void Logout()
		{
			Engine.Logout();
		}

        public void Connect()
        {
			tabMain.SelectedIndex = 0;

			if ((Engine.IsLogged() == true) && (Engine.IsConnected() == false) && (Engine.IsWaiting() == false))
				Engine.Connect();            
        }

		public void ConnectManual()
		{
			if (m_listViewServers.SelectedItems.Count == 1)
			{
				AirVPN.Gui.Controls.ListViewItemServer listViewItem = m_listViewServers.SelectedItems[0] as AirVPN.Gui.Controls.ListViewItemServer;

				Engine.NextServer = listViewItem.Info;

				Connect();
			}
		}

        public void Disconnect()
        {
			cmdCancel.Enabled = false;
			mnuConnect.Enabled = false;

			Engine.Disconnect();            
        }
        
        /* -----------------------------------------
         * Logging
         ---------------------------------------- */
        private delegate void LogDelegate(LogEntry l);

        public void Log(LogEntry l)
        {
            if (this.InvokeRequired)
            {

                LogDelegate inv = new LogDelegate(this.Log);

                this.BeginInvoke(inv, new object[] { l });                
            }
            else
            {
                lock (this)
                {
                    string Msg = l.Message;

                    if (l.Type > Engine.LogType.Realtime)
                    {
                        ListViewItemLog Item = new ListViewItemLog();
						Item.ImageKey = l.Type.ToString().ToLowerInvariant();
						Item.Text = "";
						Item.SubItems.Add(l.GetDateForList());
						Item.SubItems.Add(l.GetMessageForList());
						Item.ToolTipText = l.Message;
						Item.Info = l;

						lstLogs.Items.Add(Item);
						Item.EnsureVisible();

						if (lstLogs.Items.Count >= Engine.Storage.GetInt("gui.log_limit"))
							lstLogs.Items.RemoveAt(0);
                    }

                    if ((Msg != "") && (l.Type != Engine.LogType.Verbose))
                    {
                        String ShortMsg = Msg;
                        if (ShortMsg.Length > 40)
                            ShortMsg = ShortMsg.Substring(0, 40) + "...";

						string notifyText = Constants.Name + " - " + ShortMsg;

						//if(Engine.IsConnected() == false)
						{
							Text = Constants.Name + " - " + Msg;

							mnuStatus.Text = "> " + Msg;

							if (m_notifyIcon != null)
							{
								m_notifyIcon.Text = notifyText;
								m_notifyIcon.BalloonTipText = Msg;
								if(l.Type >= Engine.LogType.InfoImportant)
									m_notifyIcon.ShowBalloonTip(l.BalloonTime);
							}
						}
                    }

                    if (l.Type == Engine.LogType.Fatal)
                    {
                        MessageBox.Show(this, Msg, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

					
                }
            }
        }

        public void EnabledUi()
		{
			if ((Engine.Storage.GetBool("gui.windows.tray")) && (Platform.Instance.IsTraySupported()))
			{
				mnuRestore.Visible = true;

				if (this.Visible)
					mnuRestore.Text = Messages.WindowsMainHide;
				else
					mnuRestore.Text = Messages.WindowsMainShow;
			}
			else
			{
				mnuRestore.Visible = false;
			}

			// Welcome
			bool logged = Engine.IsLogged();
			bool connected = Engine.IsConnected();
			bool waiting = Engine.IsWaiting();

			if (logged == false)
			{
				cmdLogin.Text = Messages.CommandLoginButton;				
			}
			else
			{
				cmdLogin.Text = Messages.CommandLogout;
			}

			if (waiting)
			{
				mnuConnect.Text = Messages.CommandCancel;
			}
			else if (connected)
			{
				mnuConnect.Enabled = true;
				mnuConnect.Text = Messages.CommandDisconnect;
			}
			else if (logged)
			{
				mnuConnect.Enabled = true;
				mnuConnect.Text = Messages.CommandConnect;
			}
			else
			{
				mnuConnect.Enabled = true;
				mnuConnect.Text = Messages.CommandLoginMenu;
			}


			cmdLogin.Enabled = ((waiting == false) && (connected == false) && (txtLogin.Text.Trim() != "") && (txtPassword.Text.Trim() != "") );

			txtLogin.Enabled = (logged == false);
			txtPassword.Enabled = (logged == false);

			if (logged)
			{
				cmdConnect.Enabled = true;					
			}
			else
			{
				cmdConnect.Enabled = false;
			}

			cmdServersConnect.Enabled = ( (Engine.IsLogged()) && (m_listViewServers.SelectedItems.Count == 1));
			mnuServersConnect.Enabled = cmdServersConnect.Enabled;

			cmdServersWhiteList.Enabled = (m_listViewServers.SelectedItems.Count > 0);
			mnuServersWhiteList.Enabled = cmdServersWhiteList.Enabled;
			cmdServersBlackList.Enabled = cmdServersWhiteList.Enabled;
			mnuServersBlackList.Enabled = cmdServersBlackList.Enabled;
			cmdServersUndefined.Enabled = cmdServersWhiteList.Enabled;
			mnuServersUndefined.Enabled = cmdServersUndefined.Enabled;

			cmdAreasWhiteList.Enabled = (m_listViewAreas.SelectedItems.Count > 0);
			mnuAreasWhiteList.Enabled = cmdAreasWhiteList.Enabled;
			cmdAreasBlackList.Enabled = cmdAreasWhiteList.Enabled;
			mnuAreasBlackList.Enabled = cmdAreasBlackList.Enabled;
			cmdAreasUndefined.Enabled = cmdAreasWhiteList.Enabled;
			mnuAreasUndefined.Enabled = cmdAreasUndefined.Enabled;

			mnuSpeedTest.Enabled = connected;
			cmdLogsOpenVpnManagement.Visible = Engine.Storage.GetBool("advanced.expert");
			cmdLogsOpenVpnManagement.Enabled = Engine.IsConnected();
			
			if( (Engine.Instance.NetworkLockManager != null) && (Engine.Instance.NetworkLockManager.IsActive()) )
			{
				cmdLockedNetwork.Text = Messages.NetworkLockButtonActive;
				imgLockedNetwork.Image = Lib.Forms.Properties.Resources.netlock_on;

				lblNetLockStatus.Image = Lib.Forms.Properties.Resources.netlock_status_on;
				this.tip.SetToolTip(this.lblNetLockStatus, Messages.NetworkLockStatusActive);
			}
			else
			{
				cmdLockedNetwork.Text = Messages.NetworkLockButtonDeactive;
				imgLockedNetwork.Image = Lib.Forms.Properties.Resources.netlock_off;

				lblNetLockStatus.Image = Lib.Forms.Properties.Resources.netlock_status_off;
				this.tip.SetToolTip(this.lblNetLockStatus, Messages.NetworkLockStatusDeactive);
			}

			bool networkCanEnabled = ( (Engine.Instance.NetworkLockManager != null) && (Engine.Instance.NetworkLockManager.CanEnabled()) );
			cmdLockedNetwork.Visible = networkCanEnabled;
			imgLockedNetwork.Visible = networkCanEnabled;
		}

		private delegate void ShowFrontMessageDelegate(string message);
		public void ShowFrontMessage(string message)
		{
			if (this.InvokeRequired)
			{
				ShowFrontMessageDelegate inv = new ShowFrontMessageDelegate(this.ShowFrontMessage);

				//this.Invoke(inv, new object[] { mode });
				this.BeginInvoke(inv, new object[] { message });
			}
			else
			{
				Gui.Forms.FrontMessage dlg = new Forms.FrontMessage();
				dlg.Message = message;
				dlg.Show();
				dlg.Activate();
			}
		}


		private delegate void PostManifestUpdateDelegate();
		public void PostManifestUpdate()
		{
			if (this.InvokeRequired)
			{
				PostManifestUpdateDelegate inv = new PostManifestUpdateDelegate(this.PostManifestUpdate);

				this.BeginInvoke(inv, new object[] {  });
			}
			else
			{
				mnuServersRefresh.Enabled = true;
				cmdServersRefresh.Enabled = true;				
			}
		}

        // Force when need to update icons, force refresh etc.		
        private delegate void RefreshUiDelegate(Engine.RefreshUiMode mode);
        public void RefreshUi(Engine.RefreshUiMode mode)
        {
            if (this.IsHandleCreated == false)
                return;

			if (m_lockCoordUpdate)
				return;

            if (this.InvokeRequired)
            {
                RefreshUiDelegate inv = new RefreshUiDelegate(this.RefreshUi);

                //this.Invoke(inv, new object[] { mode });
                this.BeginInvoke(inv, new object[] { mode });
            }
            else
            {
				if (m_formReady == false) // To avoid useless calling that Windows.Forms do when initializing controls 
					return;

				// lock (Engine) // TOCLEAN 2.9
                {
					if( (mode == Core.Engine.RefreshUiMode.MainMessage) || (mode == Core.Engine.RefreshUiMode.Full) )
					{
						// Status message
						String text1 = Engine.WaitMessage;
						lblWait1.Text = text1;

						if (Engine.IsWaiting())
						{
							pnlWelcome.Visible = false;
							pnlWaiting.Visible = true;
							pnlConnected.Visible = false;
							cmdCancel.Visible = Engine.IsWaitingCancelAllowed();
							cmdCancel.Enabled = (Engine.IsWaitingCancelPending() == false);
							mnuConnect.Enabled = cmdCancel.Enabled;

							mnuStatus.Image = global::AirVPN.Lib.Forms.Properties.Resources.status_yellow_16;

						}
						else if (Engine.IsConnected())
						{
							pnlWelcome.Visible = false;
							pnlWaiting.Visible = false;
							pnlConnected.Visible = true;

							lblConnectedServerName.Text = Engine.CurrentServer.PublicName;
							lblConnectedLocation.Text = Engine.CurrentServer.CountryName + ", " + Engine.CurrentServer.Location;
							txtConnectedExitIp.Text = Engine.CurrentServer.IpExit;
							string iconFlagCode = Engine.CurrentServer.CountryCode;
							Image iconFlag = null;
							if (imgCountries.Images.ContainsKey(iconFlagCode))
							{
								iconFlag = imgCountries.Images[iconFlagCode];
								lblConnectedCountry.Image = iconFlag;
							}
							else
								lblConnectedCountry.Image = null;

							mnuStatus.Image = global::AirVPN.Lib.Forms.Properties.Resources.status_green_16;
						}
						else
						{
							pnlWelcome.Visible = true;
							pnlWaiting.Visible = false;
							pnlConnected.Visible = false;

							mnuStatus.Image = global::AirVPN.Lib.Forms.Properties.Resources.status_red_16;
						}
						
						// Icon                    
						{
							Icon icon;

							//if(pageView == PageView.Stats)
							if (Engine.IsConnected())
							{
								icon = global::AirVPN.Lib.Forms.Properties.Resources.icon1;
							}
							else
							{
								icon = global::AirVPN.Lib.Forms.Properties.Resources.icon_gray1;
							}

							if (this.Icon != icon)
							{
								this.Icon = icon;								
								if (m_notifyIcon != null)
									m_notifyIcon.Icon = icon;
							}
						}

						// Repaint
						Invalidate();

						EnabledUi();
					}

					if ((mode == Core.Engine.RefreshUiMode.Log) || (mode == Core.Engine.RefreshUiMode.Full))
					{
						lock (Engine.LogEntries)
						{
							while (Engine.LogEntries.Count > 0)
							{
								LogEntry l = Engine.LogEntries[0];
								Engine.LogEntries.RemoveAt(0);

								Log(l);
							}

							if (Engine.IsWaiting())
							{
								lblWait2.Text = Engine.GetLogDetailTitle();
							}
						}
					}

                    if( (mode == Core.Engine.RefreshUiMode.Stats) || (mode == Core.Engine.RefreshUiMode.Full))
                    {
						if (Engine.IsConnected())
						{
							txtConnectedSince.Text = Engine.Stats.GetValue("VpnConnectionStart");

							txtConnectedDownload.Text = Core.Utils.FormatBytes(Engine.ConnectedLastDownloadStep, true, false);
							txtConnectedUpload.Text = Core.Utils.FormatBytes(Engine.ConnectedLastUploadStep, true, false);

							string notifyText = Messages.Format(Messages.StatusTextConnected, Core.Utils.FormatBytes(Engine.ConnectedLastDownloadStep, true, false), Core.Utils.FormatBytes(Engine.ConnectedLastUploadStep, true, false), Engine.CurrentServer.PublicName, Engine.CurrentServer.CountryName);
							string notifyText2 = Constants.Name + " - " + notifyText;
							Text = notifyText2;
							mnuStatus.Text = "> " + notifyText;
							if (m_notifyIcon != null)
							{
								if (notifyText2.Length > 62)
									notifyText2 = notifyText2.Substring(0, 62);
								m_notifyIcon.Text = notifyText2;
							}
						}						
                    }

					if (mode == Core.Engine.RefreshUiMode.Full)
					{
						// TOCLEAN
						//bool welcome = ((Engine.IsWaiting() == false) && (Engine.IsConnected() == false));
						//bool connected = ((Engine.IsWaiting() == false) && (Engine.IsConnected() == true));

						m_listViewServers.UpdateList();
						m_listViewAreas.UpdateList();
					}
                }

                
            }
        }

		private delegate void StatsChangeDelegate(StatsEntry entry);        
		public void StatsChange(StatsEntry entry)
		{
			if(this.InvokeRequired)
			{
				StatsChangeDelegate inv = new StatsChangeDelegate(this.StatsChange);
				this.BeginInvoke(inv, new object[] { entry });
			}
			else
			{
				if (m_statsItems.ContainsKey(entry.Key))
				{
					ListViewItemStats item = m_statsItems[entry.Key];
					if (item.SubItems.Count == 1)
						item.SubItems.Add("");
					item.SubItems[1].Text = entry.Value;
				}
			}
		}

        public void ShowAbout()
        {
			Forms.About dlg = new Forms.About();
            dlg.ShowDialog();			
        }

		public void ShowPreferences()
		{
			Forms.Settings Dlg = new Forms.Settings();
			Dlg.ShowDialog();

			EnabledUi();
		}

        public bool TermsOfServiceCheck(bool force)
        {
            bool show = force;
            if(show == false)
				show = (Engine.Storage.GetBool("gui.tos") == false);
                
            if(show)
            {
                Forms.Tos Dlg = new Forms.Tos();
                if (Dlg.ShowDialog() == DialogResult.OK)
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

		private String LogsGetBody(bool selectedOnly)
		{
			lock (this)
			{
				StringBuilder buffer = new StringBuilder();

				for (int i = 0; i < lstLogs.Items.Count; i++)
				{
					bool skip = false;
					if ((selectedOnly) && (lstLogs.Items[i].Selected == false))
						skip = true;

					if (skip == false)
					{
						/*
						String line = "";
						for (int j = 0; j < lstLogs.Columns.Count; j++)
						{
							line += lstLogs.Items[i].SubItems[j].Text;
							line += "\t";
						}
						buffer.Append(line.Trim());
						buffer.Append("\n");
						*/
						buffer.Append((lstLogs.Items[i] as ListViewItemLog).Info.GetStringLines() + "\n");
					}
				}

				return Platform.Instance.NormalizeString(buffer.ToString());
			}
		}

		private void LogsDoCopy(bool selectedOnly)
		{
			String t = LogsGetBody(selectedOnly);
			if (t.Trim() != "")
			{
				Clipboard.SetText(t);

				MessageBox.Show(Messages.LogsCopyClipboardDone, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void LogsDoSave(bool selectedOnly)
		{
			String t = LogsGetBody(selectedOnly);
			if (t.Trim() != "")
			{
				SaveFileDialog sd = new SaveFileDialog();

				sd.FileName = Engine.GetLogSuggestedFileName();
				sd.Filter = Messages.FilterTextFiles;

				if (sd.ShowDialog() == DialogResult.OK)
				{
					using (StreamWriter sw = new StreamWriter(sd.FileName))
					{
						sw.Write(t);
						sw.Flush();
						sw.Close();
					}

					MessageBox.Show(Messages.LogsSaveToFileDone, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		public bool NetworkLockKnowledge()
		{
			string Msg = Messages.NetworkLockWarning;
			return (MessageBox.Show(this, Msg, Constants.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
		}

		public void NetworkLockActivation()
		{
			if(NetworkLockKnowledge())
			{
				Engine.Instance.NetLockIn();
			}
		}

		public void NetworkLockDeactivation()
		{
			Engine.NetLockOut();
		}

		private void txtCommand_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyValue == 13)
			{
				string command = txtCommand.Text;
				txtCommand.Text = "";
				
				Engine.Instance.Command(command);
			}			
		}

		
		

		
		


		


		

        
    }
}