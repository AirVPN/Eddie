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

        private String m_lastLogMessage;
        private int m_logDotCount = 0;

		private bool m_skipNetworkLockedConfirm = false;
		private bool m_FormReady = false;
		private bool m_Closing = false;

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

				// Mono bug https://bugs.debian.org/cgi-bin/bugreport.cgi?bug=742774
				mnuMain.Dispose();
				mnuServers.Dispose();
				mnuAreas.Dispose();
				mnuLogsContext.Dispose();

				m_Closing = true;
                Close();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
			m_lockCoordUpdate = true;

			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			MinimumSize = new Size(m_windowMinimumWidth, m_windowMinimumHeight);

			m_FormReady = false;

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
                m_notifyIcon.Click += new EventHandler(notifyIcon_Click);
            }

			// Controls initialization
			tmrRefreshDetails.Interval = 1000;

			chkLockedNetwork.Visible = Engine.Instance.DevelopmentEnvironment;
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

			cboSpeedResolution.SelectedIndex = 0;

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
			}

			// Tooltips
			cmdConnect.Text = Messages.CommandConnect;
			lblConnectSubtitle.Text = Messages.CommandConnectSubtitle;
			cmdDisconnect.Text = Messages.CommandDisconnect;
			this.tip.SetToolTip(this.cboScoreType, Messages.TooltipScoreType);
			this.tip.SetToolTip(this.chkLockLast, Messages.TooltipLockLast);
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

			m_skipNetworkLockedConfirm = true;
            chkLockedNetwork.Checked = Engine.Storage.GetBool("advanced.locked_security");
			m_skipNetworkLockedConfirm = false;

			/*
            if (cmdConnect.Enabled && (Engine.Storage.GetBool("connect")))
            {
                Connect();
            }
			*/

			m_FormReady = true;

            Engine.OnRefreshUi();


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
				Rectangle rectHeaderText = new Rectangle(m_cmdMainMenu.Width, 0, ClientSize.Width - m_cmdMainMenu.Width - 10, m_topHeaderHeight);

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
					Form.DrawStringOutline(e.Graphics, lblWait1.Text, m_topBarFont, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);										
				}
				else if (Engine.IsConnected())
				{	
					string serverName = Engine.CurrentServer.Name;

					DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_green"), rectHeader);

					Form.DrawStringOutline(e.Graphics, "Connected to " + serverName, m_topBarFont, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);
				}
				else
				{
					DrawImage(e.Graphics, GuiUtils.GetResourceImage("topbar_red"), rectHeader);
					if (NetworkLocking.Instance.GetEnabled())
					{
						Form.DrawStringOutline(e.Graphics, "Not connected. Network locked.", m_topBarFont, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);
					}
					else
					{
						Form.DrawStringOutline(e.Graphics, "Not connected. Network exposed.", m_topBarFont, Skin.ForeBrush, rectHeaderText, GuiUtils.StringFormatRightMiddle);
					}
				}

				if (iconFlag != null)
				{
					Rectangle rectFlag = new Rectangle(rectHeader.Right - iconFlag.Width - 10, 5, iconFlag.Width, iconFlag.Height);
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
			if (m_Closing)
				return;
			
			e.Cancel = true;

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
				if (Engine.Storage.GetBool("gui.tray"))
				{
					if (Platform.Instance.IsTraySupported())
						if (FormWindowState.Minimized == WindowState)
							Hide();
				}

				mnuRestore.Visible = (this.Visible == false);


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
            Restore();
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
            Forms.Settings Dlg = new Forms.Settings();
            Dlg.ShowDialog();            
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
			Engine.OnRefreshUi();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            Engine.OnRefreshUi();
        }


		private void cmdConnect_Click(object sender, EventArgs e)
		{
			Connect();
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

        private void chkLockedSecurity_CheckedChanged(object sender, EventArgs e)
        {
            CheckLockedNetwork();
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
		
		private void mnuDevelopersDefaultManifest_Click(object sender, EventArgs e)
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode node = xmlDoc.ImportNode(Engine.Storage.Manifest, true);			
			xmlDoc.AppendChild(node);
			
			String Body = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" + xmlDoc.OuterXml;

			Forms.TextViewer Dlg = new TextViewer();
			Dlg.Title = "Default manifest";
			Dlg.Body = Body;
			Dlg.ShowDialog();
		}
		
		void m_listViewServers_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			cmdServersConnect_Click(sender, e);
		}

		private void mnuServersConnect_Click(object sender, EventArgs e)
		{
			if (m_listViewServers.SelectedItems.Count == 1)
			{	
				AirVPN.Gui.Controls.ListViewItemServer listViewItem = m_listViewServers.SelectedItems[0] as AirVPN.Gui.Controls.ListViewItemServer;

				Engine.NextServer = listViewItem.Info;
				
				if ( (Engine.IsLogged() == true) && (Engine.IsConnected() == false) && (Engine.IsWaiting() == false))
					Connect();
			}
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
			DeselectServersListItem();
			m_listViewServers.UpdateList();
		}

		private void mnuServersBlacklist_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemServer item in m_listViewServers.SelectedItems)
			{
				item.Info.UserList = ServerInfo.UserListType.BlackList;
			}
			DeselectServersListItem();
			m_listViewServers.UpdateList();			
		}

		private void mnuServersUndefined_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemServer item in m_listViewServers.SelectedItems)
			{
				item.Info.UserList = ServerInfo.UserListType.None;
			}
			DeselectServersListItem();
			m_listViewServers.UpdateList();			
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


		private void mnuAreasWhiteList_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemArea item in m_listViewAreas.SelectedItems)
			{
				item.Info.UserList = AreaInfo.UserListType.WhiteList;
			}
			m_listViewAreas.UpdateList();
			m_listViewServers.UpdateList();
		}

		private void mnuAreasBlackList_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemArea item in m_listViewAreas.SelectedItems)
			{
				item.Info.UserList = AreaInfo.UserListType.BlackList;
			}
			m_listViewAreas.UpdateList();
			m_listViewServers.UpdateList();
		}

		private void mnuAreasUndefined_Click(object sender, EventArgs e)
		{
			foreach (ListViewItemArea item in m_listViewAreas.SelectedItems)
			{
				item.Info.UserList = AreaInfo.UserListType.None;
			}
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
				Dlg.Title = "OVPN Viewer";
				Dlg.Body = Engine.ConnectedOVPN;
				Dlg.ShowDialog();
			}
			else if (item.Entry.Key == "SystemReport")
			{
				Forms.TextViewer Dlg = new TextViewer();
				Dlg.Title = "System Networking Report";
				Dlg.Body = Platform.Instance.GenerateSystemReport();
				Dlg.ShowDialog();
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
				imgProgress.Left = (tabItemWidth / 2) - (208 / 2);
				imgProgress.Top = (tabItemHeight / 2) - (13 / 2);
				lblWait1.Left = 0;
				lblWait1.Top = 0;
				lblWait1.Width = tabItemWidth;
				lblWait1.Height = imgProgress.Top - 10;
				lblWait2.Left = 0;
				lblWait2.Top = imgProgress.Top + imgProgress.Height + 10;
				lblWait2.Width = tabItemWidth;
				lblWait2.Height = tabItemHeight - lblWait2.Top - 10;
				cmdCancel.Width = tabItemHeight * 3 / 2;
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
			
			Engine.Connect();            
        }

        public void Disconnect()
        {
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
                        m_lastLogMessage = l.Message;
                        m_logDotCount += 1;
                        m_logDotCount = m_logDotCount % 10;



						String NewMessage = l.Message;
						NewMessage = NewMessage.Replace("\r", "");
						NewMessage = NewMessage.Replace("\n", " | ");

						ListViewItem Item = new ListViewItem();
						Item.ImageKey = l.Type.ToString().ToLowerInvariant();
						Item.Text = "";
						Item.SubItems.Add(l.Date.ToShortDateString() + " - " + l.Date.ToShortTimeString());
						Item.SubItems.Add(NewMessage);
						Item.ToolTipText = l.Message;

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

						if(Engine.IsConnected() == false)
						{
							Text = Constants.Name + " - " + Msg;

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
			// Welcome
			bool logged = Engine.IsLogged();
			bool connected = Engine.IsConnected();
			bool waiting = Engine.IsWaiting();

			if (logged == false)
				cmdLogin.Text = Messages.CommandLogin;
			else
				cmdLogin.Text = Messages.CommandLogout;

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
				if (m_FormReady == false) // To avoid useless calling that Windows.Forms do when initializing controls 
					return;

				lock (Engine)
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
							cmdCancel.Visible = Engine.IsWaitingCancel();
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
						}
						else
						{
							pnlWelcome.Visible = true;
							pnlWaiting.Visible = false;
							pnlConnected.Visible = false;
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
								String text2 = "";
								if ((Engine.Storage != null) && (Engine.Storage.GetBool("advanced.expert")))
								{
									if (Engine.WaitMessage == m_lastLogMessage)
										text2 = "";
									else
										text2 = m_lastLogMessage;
								}
								else
								{
									for (int i = 0; i < m_logDotCount; i++)
										text2 += ".";
								}
								lblWait2.Text = text2;
							}
						}
					}

					if ((mode == Core.Engine.RefreshUiMode.Quick) || (mode == Core.Engine.RefreshUiMode.Full))
					{
						RefreshStats("ManifestLastUpdate", Utils.FormatTime(Utils.XmlGetAttributeInt64(Engine.Storage.Manifest, "time", 0)));
					}

					if (mode == Core.Engine.RefreshUiMode.Full)
                    {
                        // TOCLEAN

						bool welcome = ((Engine.IsWaiting() == false) && (Engine.IsConnected() == false));
						bool connected = ((Engine.IsWaiting() == false) && (Engine.IsConnected() == true));

                        //imgProgress.Visible = Engine.IsWaiting();
						
                        mnuPorts.Enabled = connected;
                        mnuSpeedTest.Enabled = connected;
						                        
						m_listViewServers.UpdateList();
						m_listViewAreas.UpdateList();

						cmdLogsOpenVpnManagement.Visible = ((Engine.IsConnected()) && (Engine.Storage.GetBool("advanced.expert")));

						if (Engine.IsConnected())
						{
							RefreshStats("ServerName", Engine.CurrentServer.Name);
							RefreshStats("ServerLatency", Engine.CurrentServer.Ping.ToString() + " ms");
							RefreshStats("ServerLocation", Engine.CurrentServer.CountryName + " - " + Engine.CurrentServer.Location);
							RefreshStats("ServerLoad", Engine.CurrentServer.Load().ToString());
							RefreshStats("ServerUsers", Engine.CurrentServer.Users.ToString());

							RefreshStats("VpnIpEntry", Engine.ConnectedEntryIP);
							RefreshStats("VpnIpExit", Engine.CurrentServer.IpExit);
							RefreshStats("VpnProtocol", Engine.ConnectedProtocol);
							RefreshStats("VpnPort", Engine.ConnectedPort.ToString());
							if (Engine.ConnectedRealIp != "")
								RefreshStats("VpnRealIp", Engine.ConnectedRealIp);
							else
								RefreshStats("VpnRealIp", Messages.CheckingRequired);
							RefreshStats("VpnIp", Engine.ConnectedVpnIp);
							RefreshStats("VpnDns", Engine.ConnectedVpnDns);
							RefreshStats("VpnInterface", Engine.ConnectedVpnInterfaceName);
							RefreshStats("VpnGateway", Engine.ConnectedVpnGateway);
							RefreshStats("VpnGeneratedOVPN", Messages.DoubleClickToView);							

							if (Engine.ConnectedServerTime != 0)
								RefreshStats("SystemTimeServerDifference", (Engine.ConnectedServerTime - Engine.ConnectedClientTime).ToString() + " seconds");
							else
								RefreshStats("SystemTimeServerDifference", Messages.CheckingRequired);
						}
						else
						{
							RefreshStats("ServerName", Messages.StatsNotConnected);
							RefreshStats("ServerLatency", Messages.StatsNotConnected);
							RefreshStats("ServerLocation", Messages.StatsNotConnected);
							RefreshStats("ServerLoad", Messages.StatsNotConnected);
							RefreshStats("ServerUsers", Messages.StatsNotConnected);

							RefreshStats("VpnIpEntry", Messages.StatsNotConnected);
							RefreshStats("VpnIpExit", Messages.StatsNotConnected);
							RefreshStats("VpnProtocol", Messages.StatsNotConnected);
							RefreshStats("VpnPort", Messages.StatsNotConnected);
							RefreshStats("VpnRealIp", Messages.StatsNotConnected);
							RefreshStats("VpnIp", Messages.StatsNotConnected);
							RefreshStats("VpnDns", Messages.StatsNotConnected);
							RefreshStats("VpnInterface", Messages.StatsNotConnected);
							RefreshStats("VpnGateway", Messages.StatsNotConnected);
							RefreshStats("VpnGeneratedOVPN", Messages.StatsNotConnected);
							RefreshStats("SystemTimeServerDifference", Messages.StatsNotConnected);
						}
						
						RefreshStats("SystemReport", Messages.DoubleClickToView);
                    }

                    if( (mode == Core.Engine.RefreshUiMode.Full) ||
                        (mode == Core.Engine.RefreshUiMode.Stats) )
                    {
						// TOCLEAN

						if (Engine.IsConnected())
						{
							{
								DateTime DT1 = Engine.Instance.ConnectedSince;
								DateTime DT2 = DateTime.UtcNow;
								TimeSpan TS = DT2 - DT1;
								string TSText = string.Format("{0:00}:{1:00}:{2:00} - {3}", (int)TS.TotalHours, TS.Minutes, TS.Seconds, DT1.ToLocalTime().ToLongDateString() + " " + DT1.ToLocalTime().ToLongTimeString());
								RefreshStats("VpnConnectionStart", TSText);
								txtConnectedSince.Text = TSText;
							}
							RefreshStats("VpnTotalDownload", Core.Utils.FormatBytes(Engine.ConnectedLastRead, false, true));
							RefreshStats("VpnTotalUpload", Core.Utils.FormatBytes(Engine.ConnectedLastWrite, false, true));

							RefreshStats("VpnSpeedDownload", Core.Utils.FormatBytes(Engine.ConnectedLastDownloadStep, true, true));
							RefreshStats("VpnSpeedUpload", Core.Utils.FormatBytes(Engine.ConnectedLastUploadStep, true, true));
							txtConnectedDownload.Text = Core.Utils.FormatBytes(Engine.ConnectedLastDownloadStep, true, false);
							txtConnectedUpload.Text = Core.Utils.FormatBytes(Engine.ConnectedLastUploadStep, true, false);

							string notifyText = Constants.Name + " - " + "Down: " + Core.Utils.FormatBytes(Engine.ConnectedLastDownloadStep, true, false) + " - Up: " + Core.Utils.FormatBytes(Engine.ConnectedLastUploadStep, true, false) + " - " + Engine.CurrentServer.PublicName + " (" + Engine.CurrentServer.CountryCode + ")";
							Text = notifyText;
							if (m_notifyIcon != null)
							{
								if (notifyText.Length > 62)
									notifyText = notifyText.Substring(0, 62);
								m_notifyIcon.Text = notifyText;
							}

							
						}
						else
						{
							RefreshStats("VpnConnectionStart", Messages.StatsNotConnected);
							RefreshStats("VpnTotalDownload", Messages.StatsNotConnected);
							RefreshStats("VpnTotalUpload", Messages.StatsNotConnected);

							RefreshStats("VpnSpeedDownload", Messages.StatsNotConnected);
							RefreshStats("VpnSpeedUpload", Messages.StatsNotConnected);
						}
                        
                    }
                }

                
            }
        }

		public void RefreshStats(string name, string value)
		{
			if (m_statsItems.ContainsKey(name))
			{
				ListViewItemStats item = m_statsItems[name];
				if (item.SubItems.Count == 1)
					item.SubItems.Add("");
				if (item.SubItems[1].Text != value)
					item.SubItems[1].Text = value;
			}
			else
			{				
				throw new Exception("Unknown stats.");				
			}
		}

        public void ShowAbout()
        {
            Forms.About dlg = new Forms.About();
            dlg.ShowDialog();
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
						String line = "";
						for (int j = 0; j < lstLogs.Columns.Count; j++)
						{
							line += lstLogs.Items[i].SubItems[j].Text;
							line += "\t";
						}
						buffer.Append(line.Trim());
						buffer.Append("\n");
					}
				}

				return Platform.Instance.NormalizeString(buffer.ToString());
			}
		}

		private void LogsDoCopy(bool selectedOnly)
		{
			String t = LogsGetBody(selectedOnly);
			if (t != "")
				Clipboard.SetText(t);

			MessageBox.Show(Messages.LogsCopyClipboardDone, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void LogsDoSave(bool selectedOnly)
		{
			SaveFileDialog sd = new SaveFileDialog();

			sd.FileName = "AirVPN_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".txt";
			sd.Filter = Messages.FilterTextFiles;

			if (sd.ShowDialog() == DialogResult.OK)
			{
				using (StreamWriter sw = new StreamWriter(sd.FileName))
				{
					sw.Write(LogsGetBody(selectedOnly));
					sw.Flush();
					sw.Close();
				}
			}

			MessageBox.Show(Messages.LogsSaveToFileDone, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}	

        public void CheckLockedNetwork()
        {
            if (chkLockedNetwork.Checked)
            {
				bool confirmed = false;

				if (m_skipNetworkLockedConfirm)
				{
					confirmed = true;
					m_skipNetworkLockedConfirm = false;
				}
				else
				{
					String Msg = "";
					Msg += "Network Locked Mode\n";
					Msg += "\n";
					Msg += "In this state, any network connections outside AirVPN service & tunnel are unavailable.\n";
					Msg += "Indipendently if you are connected to the VPN or not.\n";
					Msg += "This computer will also unavailable for your local network.\n";
					Msg += "\n";
					Msg += "Warning: Any active connections will be dropped.\n";
					Msg += "\n";
					Msg += "Are you sure do you want to activate this mode?";

					if (MessageBox.Show(this, Msg, this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						confirmed = true;
					}
				}

				if(confirmed)
                {
                    if (NetworkLocking.Instance.Enable() == false)
                    {
						MessageBox.Show(this, Messages.NetworkLockFailed, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }

				chkLockedNetwork.Checked = NetworkLocking.Instance.GetEnabled();
            }
            else
            {
				NetworkLocking.Instance.Disable();
            }

			Engine.Storage.SetBool("advanced.locked_security", NetworkLocking.Instance.GetEnabled());
        }


		

        
    }
}