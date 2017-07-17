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
using Eddie.Lib.Common;
using Eddie.Core;
using Eddie.Gui.Controls;

namespace Eddie.Gui.Forms
{
    public partial class Main : Eddie.Gui.Form
    {
        private Controls.ToolTip m_toolTip;
        private Controls.TabNavigator m_tabMain;
		private Controls.ChartSpeed m_pnlCharts;
		private Controls.MenuButton m_cmdMainMenu;
		private Controls.ProgressInfinite m_imgProgressInfinite;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
		
		private ListViewServers m_listViewServers;
		private ListViewAreas m_listViewAreas;

        private Dictionary<string, ListViewItemStats> m_statsItems = new Dictionary<string, ListViewItemStats>();

		private int m_windowMinimumWidth = 700;
		private int m_windowMinimumHeight = 300;
		private int m_windowDefaultWidth = 800;
		private int m_windowDefaultHeight = 400;
		private bool m_lockCoordUpdate = false;		
        private int m_topHeaderHeight = 30;

		private bool m_formReady = false;
		private bool m_closing = false;
        private bool m_windowStateSetByShortcut = false;

        public Main()
        {
            Gui.Skin.SkinReference.Load(Engine.Instance.Storage.Get("gui.skin"));

            OnPreInitializeComponent();
            InitializeComponent();
            OnInitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
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

        public override void OnInitializeComponent()
        {
            base.OnInitializeComponent();
        }

        public override void OnApplySkin()
        {
            base.OnApplySkin();

            cmdLogin.Font = Skin.FontBig;
            cmdConnect.Font = Skin.FontBig;
            cmdLockedNetwork.Font = Skin.FontBig;
            cmdDisconnect.Font = Skin.FontBig;
            cmdCancel.Font = Skin.FontBig;

            mnuMain.Font = Skin.FontNormal;
            mnuServers.Font = Skin.FontNormal;
            mnuAreas.Font = Skin.FontNormal;
            mnuLogsContext.Font = Skin.FontNormal;

            if (m_tabMain != null)
            {
                m_tabMain.TabsFont = Skin.FontBig;
                m_tabMain.ComputeSizes();
            }

            lblWait1.Font = Skin.FontBig;
            lblWait2.Font = Skin.FontNormal;
            lblConnectedServerName.Font = Skin.FontBig;
            txtConnectedDownload.Font = Skin.FontMonoBig;
            txtConnectedUpload.Font = Skin.FontMonoBig;

            mnuMain.ImageScalingSize = Skin.MenuImageSize;
            mnuServers.ImageScalingSize = Skin.MenuImageSize;
            mnuAreas.ImageScalingSize = Skin.MenuImageSize;
            mnuLogsContext.ImageScalingSize = Skin.MenuImageSize;

            GuiUtils.FixHeightVs(this.txtLogin, lblLogin);
            GuiUtils.FixHeightVs(this.txtPassword, lblPassword);
            GuiUtils.FixHeightVs(this.cboKey, lblKey);

            GuiUtils.FixHeightVs(this.cboScoreType, this.chkShowAll);
            GuiUtils.FixHeightVs(this.cboScoreType, this.lblScoreType);
            GuiUtils.FixHeightVs(this.cboScoreType, this.chkLockLast);

            GuiUtils.FixHeightVs(this.cboSpeedResolution, this.lblSpeedResolution);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        public void LoadPhase()
        {
            m_lockCoordUpdate = true;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            MinimumSize = new Size(m_windowMinimumWidth, m_windowMinimumHeight);

            KeyPreview = true;  // 2.10.1

            m_formReady = false;

            Visible = false;

            //base.OnLoad(e);

            CommonInit("");

            if (Platform.Instance.IsTraySupported())
            {
                m_notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
                m_notifyIcon.Icon = this.Icon;
                m_notifyIcon.Text = Constants.Name;
                m_notifyIcon.Visible = true;
                m_notifyIcon.BalloonTipTitle = Constants.Name;

                m_notifyIcon.MouseDoubleClick += new MouseEventHandler(notifyIcon_MouseDoubleClick);
                //m_notifyIcon.Click += new EventHandler(notifyIcon_Click);				
                m_notifyIcon.ContextMenuStrip = mnuMain;
            }

            m_tabMain = new TabNavigator();
            m_tabMain.ImportTabControl(tabMain);
            if (m_tabMain.Pages.Count != 0)
            {
                m_tabMain.Pages[0].Icon = "maintab_overview";
                m_tabMain.Pages[1].Icon = "maintab_servers";
                m_tabMain.Pages[2].Icon = "maintab_countries";
                m_tabMain.Pages[3].Icon = "maintab_speed";
                m_tabMain.Pages[4].Icon = "maintab_stats";
                m_tabMain.Pages[5].Icon = "maintab_logs";
                m_tabMain.TabsFont = Skin.FontBig;
                m_tabMain.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
                m_tabMain.TabSwitch += tabMain_TabSwitch;
                this.Controls.Add(m_tabMain);
            }

            m_imgProgressInfinite = new ProgressInfinite();
            this.pnlWaiting.Controls.Add(m_imgProgressInfinite);

            // Controls initialization
            mnuDevelopers.Visible = Engine.Instance.DevelopmentEnvironment;
            mnuTools.Visible = Engine.Instance.DevelopmentEnvironment;

            chkRemember.BackColor = Color.Transparent;

            if (Platform.IsWindows())
            {
                // TOFIX: Under Mono crash...
                m_toolTip = new Controls.ToolTip();
                Controls.Add(m_toolTip);
            }

            m_pnlCharts = new ChartSpeed();
            m_pnlCharts.Left = holSpeedChart.Left;
            m_pnlCharts.Top = holSpeedChart.Top;
            m_pnlCharts.Width = holSpeedChart.Width;
            m_pnlCharts.Height = holSpeedChart.Height;
            m_pnlCharts.Anchor = holSpeedChart.Anchor;
            holSpeedChart.Visible = false;
            if (m_tabMain.Pages.Count != 0)
                m_tabMain.Pages[3].Controls.Add(m_pnlCharts);

            m_cmdMainMenu = new MenuButton();
            m_cmdMainMenu.Left = 0;
            m_cmdMainMenu.Top = 0;
            m_cmdMainMenu.Click += cmdMenu_Click;
            Controls.Add(m_cmdMainMenu);

            m_listViewServers = new ListViewServers();
            m_listViewServers.ContextMenuStrip = mnuServers;
            m_listViewServers.Dock = DockStyle.Fill;
            m_listViewServers.ResizeColumnString(0, 20);
            m_listViewServers.ResizeColumnString(1, 5);
            m_listViewServers.ResizeColumnString(2, 20);
            m_listViewServers.ResizeColumnString(3, 8);
            m_listViewServers.ResizeColumnString(4, 20);
            m_listViewServers.ResizeColumnString(5, 5);
            m_listViewServers.AllowColumnReorder = true;
            pnlServers.Controls.Add(m_listViewServers);

            m_listViewAreas = new ListViewAreas();
            m_listViewAreas.ContextMenuStrip = mnuAreas;
            m_listViewAreas.Dock = DockStyle.Fill;
            m_listViewAreas.ResizeColumnString(0, 14);
            m_listViewAreas.ResizeColumnString(1, 8);
            m_listViewAreas.ResizeColumnString(2, 20);
            m_listViewAreas.ResizeColumnString(3, 6);
            m_listViewAreas.AllowColumnReorder = true;
            pnlAreas.Controls.Add(m_listViewAreas);

            m_listViewServers.MouseDoubleClick += new MouseEventHandler(m_listViewServers_MouseDoubleClick);
            m_listViewServers.SelectedIndexChanged += new EventHandler(m_listViewServers_SelectedIndexChanged);
            m_listViewAreas.SelectedIndexChanged += new EventHandler(m_listViewAreas_SelectedIndexChanged);

            lstStats.ImageIconResourcePrefix = "stats_";
            lstStats.ResizeColumnMax(1);

            lstLogs.ImageIconResourcePrefix = "log_";
            lstLogs.ResizeColumnString(0, 2);
            lstLogs.ResizeColumnString(1, LogEntry.GetDateForListSample());
            lstLogs.ResizeColumnMax(2);

            chkShowAll.Checked = false;
            chkLockLast.Checked = Engine.Storage.GetBool("servers.locklast");
            cboScoreType.Text = Engine.Storage.Get("servers.scoretype");


            //ApplySkin();

            bool forceMinimized = false;
            if (Engine.Storage.GetBool("gui.windows.start_minimized"))
                forceMinimized = true;
            if ((m_windowStateSetByShortcut) && (WindowState == FormWindowState.Minimized))
                forceMinimized = true;
            bool forceMaximized = false;
            if ((m_windowStateSetByShortcut) && (WindowState == FormWindowState.Maximized))
                forceMaximized = true;
            SetFormLayout(Engine.Storage.Get("gui.window.main"), forceMinimized, forceMaximized, MinimizeInTray(), new Size(m_windowDefaultWidth, m_windowDefaultHeight));
            m_listViewServers.SetUserPrefs(Engine.Storage.Get("gui.list.servers"));
            m_listViewAreas.SetUserPrefs(Engine.Storage.Get("gui.list.areas"));
            lstLogs.SetUserPrefs(Engine.Storage.Get("gui.list.logs"));

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

            lstStats.ResizeColumnAuto(0);

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

            if (m_toolTip != null)
            {
                m_toolTip.Connect(this.cboScoreType, Messages.TooltipServersScoreType);
                m_toolTip.Connect(this.chkLockLast, Messages.TooltipServersLockCurrent);
                m_toolTip.Connect(this.chkShowAll, Messages.TooltipServersShowAll);
                m_toolTip.Connect(this.cboScoreType, Messages.TooltipServersScoreType);
                m_toolTip.Connect(this.chkLockLast, Messages.TooltipServersLockCurrent);
                m_toolTip.Connect(this.chkShowAll, Messages.TooltipServersShowAll);
                m_toolTip.Connect(this.cmdServersConnect, Messages.TooltipServersConnect);
                m_toolTip.Connect(this.cmdServersUndefined, Messages.TooltipServersUndefined);
                m_toolTip.Connect(this.cmdServersBlackList, Messages.TooltipServersBlackList);
                m_toolTip.Connect(this.cmdServersWhiteList, Messages.TooltipServersWhiteList);
                m_toolTip.Connect(this.cmdAreasUndefined, Messages.TooltipAreasUndefined);
                m_toolTip.Connect(this.cmdAreasBlackList, Messages.TooltipAreasBlackList);
                m_toolTip.Connect(this.cmdAreasWhiteList, Messages.TooltipAreasWhiteList);
                m_toolTip.Connect(this.cmdLogsOpenVpnManagement, Messages.TooltipLogsOpenVpnManagement);
                m_toolTip.Connect(this.cmdLogsClean, Messages.TooltipLogsClean);
                m_toolTip.Connect(this.cmdLogsCopy, Messages.TooltipLogsCopy);
                m_toolTip.Connect(this.cmdLogsSave, Messages.TooltipLogsSave);
                m_toolTip.Connect(this.cmdLogsSupport, Messages.TooltipLogsSupport);
                                
                Controls.SetChildIndex(m_toolTip, 0);
            }

            // Start
            if (Engine.Storage.GetBool("remember"))
            {
                chkRemember.Checked = true;
                txtLogin.Text = Engine.Storage.Get("login");
                txtPassword.Text = Engine.Storage.Get("password");
            }

            m_lockCoordUpdate = false;

            Resizing();

            // base.OnLoad(e); // Removed in 2.11.9

            m_formReady = true;

            Engine.OnRefreshUi();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
        }

        protected override void WndProc(ref Message m)
        {
            /*WM_SIZE*/
            if (m.Msg == 0x0005)
            {
                // This will be set to true if the shortcut uses the Maximized or Minimized
                // options because then it runs before OnLoad.
                m_windowStateSetByShortcut = true;                
            }
            else
                base.WndProc(ref m);
        }

        protected override void OnKeyDown(KeyEventArgs e) // 2.10.1
		{
			base.OnKeyDown(e);

			if (e.Control && e.KeyCode == Keys.M)
			{
                Engine.Instance.Command("ui.show.menu");
            }

			if (e.Control && e.KeyCode == Keys.A)
			{
                Engine.Instance.Command("ui.show.about");
            }

			if (e.Control && e.KeyCode == Keys.P)
			{
				Engine.Instance.Command("ui.show.preferences");
            }
		}
        
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                // Another Mono Workaround. On some system, randomly ignore resized controls.
                if (Platform.IsUnix())
                {
                    if (m_tabMain.Width != ClientSize.Width)
                    {
                        Resizing();
                    }
                }

                Skin.GraphicsCommon(e.Graphics);

                int iconHeight = m_topHeaderHeight;
                int iconDistance = 3;
                
                Rectangle rectHeader = new Rectangle(m_cmdMainMenu.Width, 0, ClientSize.Width - m_cmdMainMenu.Width, m_topHeaderHeight);
				Rectangle rectHeaderText = new Rectangle(m_cmdMainMenu.Width, 0, ClientSize.Width - m_cmdMainMenu.Width - iconDistance - iconHeight, m_topHeaderHeight);
                
                Form.DrawImage(e.Graphics, Skin.MainBackImage, new Rectangle(0, 0, ClientSize.Width, m_topHeaderHeight));
                
                Image iconFlag = null;
				if (Engine.CurrentServer != null)
				{
					string iconFlagCode = Engine.CurrentServer.CountryCode;
					if (imgCountries.Images.ContainsKey(iconFlagCode))
						iconFlag = imgCountries.Images[iconFlagCode];

					if (iconFlag != null)
					{
                        rectHeaderText.Width -= iconHeight;
                        rectHeaderText.Width -= iconDistance;
                    }
				}
                
                if (Engine.IsWaiting())
				{
                    DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_yellow"), rectHeader);
                    Form.DrawStringOutline(e.Graphics, Engine.WaitMessage, Skin.FontBig, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);										
				}
				else if (Engine.IsConnected())
				{	
					string serverName = Engine.CurrentServer.DisplayName;

					DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_green"), rectHeader);

					Form.DrawStringOutline(e.Graphics, MessagesFormatter.Format(MessagesUi.TopBarConnected, serverName), Skin.FontBig, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);
				}
				else
				{
                    DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_red"), rectHeader);
                    if( (Engine.Instance.NetworkLockManager != null) && (Engine.Instance.NetworkLockManager.IsActive()) ) 
					{
						Form.DrawStringOutline(e.Graphics, MessagesUi.TopBarNotConnectedLocked, Skin.FontBig, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);
					}
					else
					{
						Form.DrawStringOutline(e.Graphics, MessagesUi.TopBarNotConnectedExposed, Skin.FontBig, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);
					}
				}

                Rectangle rectNetLock = new Rectangle(rectHeader.Right - iconHeight, 0, iconHeight, iconHeight);
                Image iconNetLock = null;
                if ((Engine.Instance.NetworkLockManager != null) && (Engine.Instance.NetworkLockManager.IsActive()))
                {
                    iconNetLock = Lib.Forms.Properties.Resources.netlock_status_on;
                }
                else
                {
                    iconNetLock = Lib.Forms.Properties.Resources.netlock_status_off;
                }
                DrawImageContain(e.Graphics, iconNetLock, rectNetLock, 20);

                if (iconFlag != null)
                {
                    Rectangle rectFlag = new Rectangle(rectHeader.Right - iconHeight - iconDistance - iconHeight, 0, iconHeight, iconHeight);
                    DrawImageContain(e.Graphics, iconFlag, rectFlag, 20);
                }

                DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_shadow"), rectHeader);                		
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

            if (Engine.Storage.GetBool("gui.exit_confirm") == true)
            {
                if(Engine.Instance.OnAskYesNo(Messages.ExitConfirm) != true)
                    return;
            }
			
			Gui.Engine engine = Engine.Instance as Gui.Engine;

            if (engine.FormMain != null)
            {
                engine.Storage.Set("gui.window.main", engine.FormMain.GetFormLayout());
                engine.Storage.Set("gui.list.servers", m_listViewServers.GetUserPrefs());
                engine.Storage.Set("gui.list.areas", m_listViewAreas.GetUserPrefs());
                engine.Storage.Set("gui.list.logs", lstLogs.GetUserPrefs());
            }

            Engine.RequestStop();

            base.OnClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if(MinimizeInTray())
            {
                if (FormWindowState.Minimized == WindowState)
                {
                    Hide();
                    EnabledUi();
                }
            }

            Resizing();			
        }

		#region UI Controls Events

        void notifyIcon_Click(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.menu");
        }

        void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Restore();
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.about");
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
            Engine.Instance.Command("ui.show.ports");
        }

        private void mnuUser_Click(object sender, EventArgs e)
        {
			Engine.Instance.Command("ui.show.clientarea");
        }

        private void mnuSpeedTest_Click(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.speedtest");
        }

        private void mnuHomePage_Click(object sender, EventArgs e)
        {
			Engine.Instance.Command("ui.show.website");
        }

        private void mnuSettings_Click(object sender, EventArgs e)
        {
            Engine.Instance.Command("ui.show.preferences");
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
            Engine.Instance.Command("ui.show.menu");
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
			Dlg.Body = Engine.Instance.Storage.GetMan("text");
			Dlg.ShowDialog();
		}

		private void mnuDevelopersManBBCode_Click(object sender, EventArgs e)
		{
			Forms.TextViewer Dlg = new TextViewer();
			Dlg.Title = "Man";
			Dlg.Body = Engine.Instance.Storage.GetMan("bbc");
			Dlg.ShowDialog();
		}

		private void mnuDevelopersReset_Click(object sender, EventArgs e)
		{
			Dictionary<string, ConnectionInfo> servers;
			lock (Engine.Connections)
				servers = new Dictionary<string, ConnectionInfo>(Engine.Connections);
			foreach (ConnectionInfo infoServer in servers.Values)
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
				item.Info.UserList = ConnectionInfo.UserListType.WhiteList;
			}
			Engine.UpdateSettings();
			DeselectServersListItem();
			//m_listViewServers.UpdateList();
            Engine.OnRefreshUi();
        }

		private void mnuServersBlacklist_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemServer item in m_listViewServers.SelectedItems)
			{
				item.Info.UserList = ConnectionInfo.UserListType.BlackList;
			}
			Engine.UpdateSettings();
			DeselectServersListItem();
			//m_listViewServers.UpdateList();
            Engine.OnRefreshUi();
        }

		private void mnuServersUndefined_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemServer item in m_listViewServers.SelectedItems)
			{
				item.Info.UserList = ConnectionInfo.UserListType.None;
			}
			Engine.UpdateSettings();
			DeselectServersListItem();
            //m_listViewServers.UpdateList();			
            Engine.OnRefreshUi();
        }

		private void mnuServersViewOVPN_Click(object sender, EventArgs e)
		{
			if(m_listViewServers.SelectedItems.Count == 1)
			{
				ListViewItemServer item = m_listViewServers.SelectedItems[0] as ListViewItemServer;
				string ovpn = item.Info.BuildOVPN(true).Get();
				Engine.Instance.OnShowText(Messages.ConnectionsShowOVPN, ovpn);				
			}
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

		private void cmdServersViewOVPN_Click(object sender, EventArgs e)
		{
			mnuServersViewOVPN_Click(sender, e);
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
            //m_listViewAreas.UpdateList();
            //m_listViewServers.UpdateList();
            Engine.OnRefreshUi();
        }

		private void mnuAreasBlackList_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemArea item in m_listViewAreas.SelectedItems)
			{
				item.Info.UserList = AreaInfo.UserListType.BlackList;
			}
			Engine.UpdateSettings();
            //m_listViewAreas.UpdateList();
            //m_listViewServers.UpdateList();
            Engine.OnRefreshUi();
        }

		private void mnuAreasUndefined_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemArea item in m_listViewAreas.SelectedItems)
			{
				item.Info.UserList = AreaInfo.UserListType.None;
			}
			Engine.UpdateSettings();
            //m_listViewAreas.UpdateList();
            //m_listViewServers.UpdateList();
            Engine.OnRefreshUi();
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
            //m_listViewServers.UpdateList();
            Engine.OnRefreshUi();
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

            Engine.Instance.Command("ui.stats." + item.Entry.Key, true);			
		}

        private void tabMain_TabSwitch()
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
			// ClodoTemp
			byte[] temp = Engine.Instance.FetchUrlEx("http://freegeoip.net", null, "test", false, "");
			Engine.Instance.Logs.Log(LogType.Fatal, temp.Length.ToString());

			LogsSupport();
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

        public bool MinimizeInTray()
        {
            if (Engine.Storage != null)
            {
                if (Engine.Storage.GetBool("gui.windows.tray"))
                {
                    if (Platform.Instance.IsTraySupported())
                        return true;
                }
            }
            return false;
        }

		public void Resizing()
		{
            if (m_lockCoordUpdate)
				return;

            if (m_pnlCharts == null)
                return;

            if (Engine.Storage == null)
                return;

            Graphics g = this.CreateGraphics();

            m_topHeaderHeight = GuiUtils.GetFontSize(g, Skin.FontBig, MessagesUi.TopBarNotConnectedExposed).Height;
            if (m_topHeaderHeight < 30)
                m_topHeaderHeight = 30;

            m_tabMain.Left = 0;
            m_tabMain.Top = m_topHeaderHeight;
            m_tabMain.Width = this.ClientSize.Width;
            m_tabMain.Height = this.ClientSize.Height - m_topHeaderHeight;
            
            m_cmdMainMenu.Width = m_topHeaderHeight*150/30;
            m_cmdMainMenu.Height = m_topHeaderHeight;
            
            Size tabPageRectangle = m_tabMain.GetPageRect().Size;
                
			// Welcome items
			pnlWelcome.Left = tabPageRectangle.Width / 2 - pnlWelcome.Width / 2;
			pnlWelcome.Top = tabPageRectangle.Height / 2 - pnlWelcome.Height / 2;
			pnlConnected.Left = tabPageRectangle.Width / 2 - pnlConnected.Width / 2;
			pnlConnected.Top = tabPageRectangle.Height / 2 - pnlConnected.Height / 2;

			// Waiting items
			pnlWaiting.Left = 0;
			pnlWaiting.Top = 0;
			pnlWaiting.Width = tabPageRectangle.Width;
			pnlWaiting.Height = tabPageRectangle.Height;
			int imgProgressTop = (tabPageRectangle.Height / 2) - (13 / 2);
			Size imgProgressSize = new Size(208, 13);
			if (m_imgProgressInfinite != null)
			{
				m_imgProgressInfinite.Size = imgProgressSize;
				m_imgProgressInfinite.Left = (tabPageRectangle.Width / 2) - (208 / 2);					
				m_imgProgressInfinite.Top = (tabPageRectangle.Height / 2) - (13 / 2);					
			}
            cmdCancel.Width = tabPageRectangle.Width * 2 / 3;
            cmdCancel.Left = tabPageRectangle.Width / 2 - cmdCancel.Width / 2;
            cmdCancel.Top = tabPageRectangle.Height - cmdCancel.Height - 20;
            lblWait1.Left = 0;
			lblWait1.Top = 0;
			lblWait1.Width = tabPageRectangle.Width;
			lblWait1.Height = imgProgressTop - 10;
			lblWait2.Left = 0;
			lblWait2.Top = imgProgressTop + imgProgressSize.Height + 10;
			lblWait2.Width = tabPageRectangle.Width;
			lblWait2.Height = tabPageRectangle.Height - lblWait2.Top - 10 - cmdCancel.Height - 20;

            Invalidate();
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
            m_tabMain.SelectTab(0);

			if ((Engine.IsLogged() == true) && (Engine.IsConnected() == false) && (Engine.IsWaiting() == false))
				Engine.Connect();            
        }

		public void ConnectManual()
		{
			if (m_listViewServers.SelectedItems.Count == 1)
			{
                Eddie.Gui.Controls.ListViewItemServer listViewItem = m_listViewServers.SelectedItems[0] as Eddie.Gui.Controls.ListViewItemServer;

                if (listViewItem.Info.CanConnect())
                {
                    Engine.NextServer = listViewItem.Info;

                    Connect();
                }
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

                    if (l.Type > LogType.Realtime)
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

                    if ((Msg != "") && (l.Type != LogType.Verbose))
                    {
                        String ShortMsg = Msg;
                        if (ShortMsg.Length > 40)
                            ShortMsg = ShortMsg.Substring(0, 40) + "...";

						string notifyText = Constants.Name + " - " + ShortMsg;

                        if (l.Type >= LogType.InfoImportant)
                        {
                            Text = Constants.Name + " - " + Msg;
                        }

						//if(Engine.IsConnected() == false)
						{
							//Text = Constants.Name + " - " + Msg;

							mnuStatus.Text = "> " + Msg;

							if (m_notifyIcon != null)
							{
								m_notifyIcon.Text = notifyText;
								m_notifyIcon.BalloonTipText = Msg;
								if (Engine.Storage.GetBool("gui.windows.notifications"))
								{
									if (l.Type >= LogType.InfoImportant)
									{
										if (l.Type == LogType.Warning)
											m_notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
										else if (l.Type == LogType.Error)
											m_notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
										else if (l.Type == LogType.Fatal)
											m_notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
										else
											m_notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
										m_notifyIcon.ShowBalloonTip(l.BalloonTime);
									}
								}
							}
						}
                    }

                    if (l.Type == LogType.Fatal)
                    {
                        MessageBox.Show(this, Msg, Constants.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

					
                }
            }
        }

        public void EnabledUi()
		{
            if (m_listViewServers == null) // 2.11.4
                return;

            if ((Engine.Storage.GetBool("gui.windows.tray")) && (Platform.Instance.IsTraySupported()))
			{
				mnuRestore.Visible = true;
                mnuRestoreSep.Visible = true;

				if (this.Visible)
					mnuRestore.Text = Messages.WindowsMainHide;
				else
					mnuRestore.Text = Messages.WindowsMainShow;
			}
			else
			{
				mnuRestore.Visible = false;
                mnuRestoreSep.Visible = false;
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
            lblKey.Visible = ((logged == true) && (cboKey.Items.Count > 1));
            cboKey.Visible = ((logged == true) && (cboKey.Items.Count > 1));

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
			cmdServersViewOVPN.Enabled = (m_listViewServers.SelectedItems.Count == 1);
			mnuServersViewOVPN.Enabled = cmdServersViewOVPN.Enabled;

			cmdAreasWhiteList.Enabled = (m_listViewAreas.SelectedItems.Count > 0);
			mnuAreasWhiteList.Enabled = cmdAreasWhiteList.Enabled;
			cmdAreasBlackList.Enabled = cmdAreasWhiteList.Enabled;
			mnuAreasBlackList.Enabled = cmdAreasBlackList.Enabled;
			cmdAreasUndefined.Enabled = cmdAreasWhiteList.Enabled;
			mnuAreasUndefined.Enabled = cmdAreasUndefined.Enabled;
            
			mnuSpeedTest.Enabled = connected;
			cmdLogsOpenVpnManagement.Visible = Engine.Storage.GetBool("advanced.expert");
			cmdLogsOpenVpnManagement.Enabled = Engine.IsConnected();

            if ( (Engine.Instance.NetworkLockManager != null) && (Engine.Instance.NetworkLockManager.IsActive()) )
			{
				cmdLockedNetwork.Text = Messages.NetworkLockButtonActive;
				imgLockedNetwork.Image = Lib.Forms.Properties.Resources.netlock_on;                
            }
            else
			{
				cmdLockedNetwork.Text = Messages.NetworkLockButtonDeactive;
				imgLockedNetwork.Image = Lib.Forms.Properties.Resources.netlock_off;
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

        private delegate void LoggedUpdateDelegate(XmlElement xmlKeys);
        public void LoggedUpdate(XmlElement xmlKeys)
        {
            if (this.InvokeRequired)
            {
                LoggedUpdateDelegate inv = new LoggedUpdateDelegate(this.LoggedUpdate);

                this.BeginInvoke(inv, new object[] { xmlKeys });
            }
            else
            {
                cboKey.Items.Clear();
                foreach (XmlElement xmlKey in xmlKeys.ChildNodes)
                {
                    cboKey.Items.Add(xmlKey.GetAttribute("name"));
                }

                if (cboKey.Items.Contains(Engine.Instance.Storage.Get("key")) == true)
                {
                    cboKey.SelectedItem = Engine.Instance.Storage.Get("key");
                }
                else
                {
                    if (cboKey.Items.Count > 0)
                    {
                        cboKey.SelectedIndex = 0;
                        Engine.Instance.Storage.Set("key", cboKey.Items[0] as string);
                    }
                    else
                    {
                        Engine.Instance.Storage.Set("key", "");
                    }
                }
                
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

							mnuStatus.Image = global::Eddie.Lib.Forms.Properties.Resources.status_yellow;

						}
						else if (Engine.IsConnected())
						{
							pnlWelcome.Visible = false;
							pnlWaiting.Visible = false;
							pnlConnected.Visible = true;

							lblConnectedServerName.Text = Engine.CurrentServer.DisplayName;
							lblConnectedLocation.Text = Engine.CurrentServer.GetLocationForList();
							txtConnectedExitIp.Text = Engine.CurrentServer.IpsExit.ToString();
							string iconFlagCode = Engine.CurrentServer.CountryCode;
							Image iconFlag = null;
							if (imgCountries.Images.ContainsKey(iconFlagCode))
							{
								iconFlag = imgCountries.Images[iconFlagCode];
								lblConnectedCountry.Image = iconFlag;
							}
							else
								lblConnectedCountry.Image = null;

							mnuStatus.Image = global::Eddie.Lib.Forms.Properties.Resources.status_green;
						}
						else
						{
							pnlWelcome.Visible = true;
							pnlWaiting.Visible = false;
							pnlConnected.Visible = false;

							mnuStatus.Image = global::Eddie.Lib.Forms.Properties.Resources.status_red;
						}
						
						// Icon                    
						{
							Icon icon;

							//if(pageView == PageView.Stats)
							if (Engine.IsConnected())
							{
								icon = global::Eddie.Lib.Forms.Properties.Resources.icon1;
							}
							else
							{
								icon = global::Eddie.Lib.Forms.Properties.Resources.icon_gray1;
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
                        }
                        
						if (Engine.IsWaiting())
						{
							lblWait2.Text = Engine.Logs.GetLogDetailTitle();
						}						
					}

                    if( (mode == Core.Engine.RefreshUiMode.Stats) || (mode == Core.Engine.RefreshUiMode.Full))
                    {
						if (Engine.IsConnected())
						{
							txtConnectedSince.Text = Engine.Stats.GetValue("VpnConnectionStart");

							txtConnectedDownload.Text = Core.Utils.FormatBytes(Engine.ConnectedLastDownloadStep, true, false);
							txtConnectedUpload.Text = Core.Utils.FormatBytes(Engine.ConnectedLastUploadStep, true, false);

                            string notifyText = Engine.Instance.GetConnectedTrayText(true, true);
                            string notifyText2 = Constants.Name + " - " + notifyText;
							Text = notifyText2;
							mnuStatus.Text = "> " + notifyText;
							if (m_notifyIcon != null)
							{
                                string tooltipText = notifyText2.Replace(" - ", "\n");
                                if (tooltipText.Length > 127)
                                    tooltipText = tooltipText.Substring(0, 127);                                
                                GuiUtils.SetNotifyIconText(m_notifyIcon, tooltipText);
                            }
						}						
                    }
                                        
					if (mode == Core.Engine.RefreshUiMode.Full)
					{                        
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
                    item.SubItems[1].Text = entry.Text;
                }
			}
		}

		private delegate Credentials AskCredentialsDelegate();
		public Credentials AskCredentials()
		{
			if (this.InvokeRequired)
			{
				AskCredentialsDelegate inv = new AskCredentialsDelegate(this.AskCredentials);
				return this.Invoke(inv, new object[] { }) as Credentials;
			}
			else
			{
				Forms.Login Dlg = new Forms.Login();
				if (Dlg.ShowDialog(this) == DialogResult.OK) // ClodoTemp if parent of MainForm, throw cross-thread-ui exception
					return Dlg.Credentials;
				else
					return null;
			}
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
						buffer.Append((lstLogs.Items[i] as ListViewItemLog).Info.GetStringLines() + "\n");
					}
				}

				return Platform.Instance.NormalizeString(buffer.ToString());
			}
		}

        private void LogsSupport()
        {
            Application.UseWaitCursor = true;

            string report = Engine.Instance.GetSupportReport();

            Clipboard.SetText(report);

            Application.UseWaitCursor = false;

            Engine.Instance.OnMessageInfo(Messages.LogsCopyClipboardDone);			
		}

		private void LogsDoCopy(bool selectedOnly)
		{
			String t = LogsGetBody(selectedOnly);
			if (t.Trim() != "")
			{
				Clipboard.SetText(t);

                Engine.Instance.OnMessageInfo(Messages.LogsCopyClipboardDone);
            }
		}

		private void LogsDoSave(bool selectedOnly)
		{
			String t = LogsGetBody(selectedOnly);
			if (t.Trim() != "")
			{
				SaveFileDialog sd = new SaveFileDialog();

				sd.FileName = Engine.Logs.GetLogSuggestedFileName();
				sd.Filter = Messages.FilterTextFiles;

				if (sd.ShowDialog() == DialogResult.OK)
				{
					using (StreamWriter sw = new StreamWriter(sd.FileName))
					{
						sw.Write(t);
						sw.Flush();
						sw.Close();
					}

                    Engine.Instance.OnMessageInfo(Messages.LogsSaveToFileDone);
				}
			}
		}

		public bool NetworkLockKnowledge()
		{
			string Msg = Messages.NetworkLockWarning;

            return Engine.Instance.OnAskYesNo(Msg);
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
				
				Engine.Instance.Command(command, false);
			}			
		}

        private void cboKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            Engine.Instance.Storage.Set("key", cboKey.SelectedItem as string);
        }		
	}
}