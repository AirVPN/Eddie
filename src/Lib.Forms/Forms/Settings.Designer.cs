namespace Eddie.Forms.Forms
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			Eddie.Forms.Skin.TabPage tabAdvanced;
			Eddie.Forms.Skin.TabPage tabDirectives;
			Eddie.Forms.Skin.TabPage tabEvents;
			Eddie.Forms.Skin.ColumnHeader columnHeader1;
			Eddie.Forms.Skin.ColumnHeader columnHeader2;
			Eddie.Forms.Skin.ColumnHeader columnHeader5;
			this.lblHummingbirdPrefer = new Eddie.Forms.Skin.Label();
			this.chkHummingbirdPrefer = new Eddie.Forms.Skin.CheckBox();
			this.lblAdvancedUpdaterChannel = new Eddie.Forms.Skin.Label();
			this.cboAdvancedUpdaterChannel = new Eddie.Forms.Skin.ComboBox();
			this.chkAdvancedSkipAlreadyRun = new Eddie.Forms.Skin.CheckBox();
			this.lblAdvancedProviders = new Eddie.Forms.Skin.Label();
			this.lblAdvancedSkipAlreadyRun = new Eddie.Forms.Skin.Label();
			this.chkAdvancedProviders = new Eddie.Forms.Skin.CheckBox();
			this.lnkAdvancedHelp = new Eddie.Forms.Skin.LinkLabel();
			this.lblExpert = new Eddie.Forms.Skin.Label();
			this.lblAdvancedPingerEnabled = new Eddie.Forms.Skin.Label();
			this.lblAdvancedCheckRoute = new Eddie.Forms.Skin.Label();
			this.lblAdvancedManifestRefresh = new Eddie.Forms.Skin.Label();
			this.cboAdvancedManifestRefresh = new Eddie.Forms.Skin.ComboBox();
			this.pnlAdvancedGeneralWindowsOnly = new System.Windows.Forms.GroupBox();
			this.cmdAdvancedUninstallDriverWintun = new Eddie.Forms.Skin.Button();
			this.chkWindowsSshPlinkForce = new Eddie.Forms.Skin.CheckBox();
			this.chkWindowsDisableDriverUpgrade = new Eddie.Forms.Skin.CheckBox();
			this.chkWindowsDebugWorkaround = new Eddie.Forms.Skin.CheckBox();
			this.chkWindowsTapUp = new Eddie.Forms.Skin.CheckBox();
			this.cmdAdvancedUninstallDriverTap = new Eddie.Forms.Skin.Button();
			this.chkAdvancedPingerEnabled = new Eddie.Forms.Skin.CheckBox();
			this.cmdExeBrowse = new Eddie.Forms.Skin.Button();
			this.txtExePath = new Eddie.Forms.Skin.TextBox();
			this.lblExePath = new Eddie.Forms.Skin.Label();
			this.chkAdvancedCheckRoute = new Eddie.Forms.Skin.CheckBox();
			this.chkExpert = new Eddie.Forms.Skin.CheckBox();
			this.chkOpenVpnDirectivesAllowScriptSecurity = new Eddie.Forms.Skin.CheckBox();
			this.cmdOpenVpnDirectivesCustomPathBrowse = new Eddie.Forms.Skin.Button();
			this.txtOpenVpnDirectivesCustomPath = new Eddie.Forms.Skin.TextBox();
			this.label8 = new Eddie.Forms.Skin.Label();
			this.lnkOpenVpnDirectivesHelp = new Eddie.Forms.Skin.LinkLabel();
			this.cboOpenVpnDirectivesDefaultSkip = new Eddie.Forms.Skin.ComboBox();
			this.label3 = new Eddie.Forms.Skin.Label();
			this.label2 = new Eddie.Forms.Skin.Label();
			this.txtOpenVpnDirectivesBase = new Eddie.Forms.Skin.TextBox();
			this.txtOpenVpnDirectivesCustom = new Eddie.Forms.Skin.TextBox();
			this.chkShellExternalRecommended = new Eddie.Forms.Skin.CheckBox();
			this.cmdShellExternalClear = new Eddie.Forms.Skin.Button();
			this.cmdShellExternalView = new Eddie.Forms.Skin.Button();
			this.lblShellExternal = new Eddie.Forms.Skin.Label();
			this.cmdAdvancedEventsEdit = new Eddie.Forms.Skin.Button();
			this.cmdAdvancedEventsClear = new Eddie.Forms.Skin.Button();
			this.lstAdvancedEvents = new Eddie.Forms.Skin.ListView();
			this.columnHeader3 = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.columnHeader4 = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.label1 = new Eddie.Forms.Skin.Label();
			this.tabNetworking = new Eddie.Forms.Skin.TabPage();
			this.lblNetworkIPv4AutoSwitch = new Eddie.Forms.Skin.Label();
			this.chkNetworkIPv4AutoSwitch = new Eddie.Forms.Skin.CheckBox();
			this.lblNetworkIPv6AutoSwitch = new Eddie.Forms.Skin.Label();
			this.chkNetworkIPv6AutoSwitch = new Eddie.Forms.Skin.CheckBox();
			this.lblNetworkIPv6Mode = new Eddie.Forms.Skin.Label();
			this.cboNetworkIPv6Mode = new Eddie.Forms.Skin.ComboBox();
			this.lblNetworkIPv4Mode = new Eddie.Forms.Skin.Label();
			this.cboNetworkIPv4Mode = new Eddie.Forms.Skin.ComboBox();
			this.lblNetworkEntryInterface = new Eddie.Forms.Skin.Label();
			this.cboNetworkEntryInterface = new Eddie.Forms.Skin.ComboBox();
			this.lblOpenVpnRcvbuf = new Eddie.Forms.Skin.Label();
			this.cboOpenVpnRcvbuf = new Eddie.Forms.Skin.ComboBox();
			this.lblOpenVpnSndbuf = new Eddie.Forms.Skin.Label();
			this.cboOpenVpnSndbuf = new Eddie.Forms.Skin.ComboBox();
			this.lblNetworkEntryIpLayer = new Eddie.Forms.Skin.Label();
			this.cboNetworkEntryIpLayer = new Eddie.Forms.Skin.ComboBox();
			this.mnuRoutes = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mnuRoutesAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuRoutesRemove = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuRoutesEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.colLogDate = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.colLogMessage = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.pnlCommands = new Eddie.Forms.Skin.Panel();
			this.cmdCancel = new Eddie.Forms.Skin.Button();
			this.cmdOk = new Eddie.Forms.Skin.Button();
			this.tabSettings = new Eddie.Forms.Skin.TabControl();
			this.tabGeneral = new Eddie.Forms.Skin.TabPage();
			this.txtStoragePasswordConfirm = new Eddie.Forms.Skin.TextBox();
			this.lblStoragePasswordConfirm = new Eddie.Forms.Skin.Label();
			this.lblStoragePassword = new Eddie.Forms.Skin.Label();
			this.lblSystemService = new Eddie.Forms.Skin.Label();
			this.chkSystemService = new Eddie.Forms.Skin.CheckBox();
			this.lblStorageBrowser = new Eddie.Forms.Skin.Label();
			this.chkStorageBrowser = new Eddie.Forms.Skin.CheckBox();
			this.txtStoragePassword = new Eddie.Forms.Skin.TextBox();
			this.lblStorageMode = new Eddie.Forms.Skin.Label();
			this.cboStorageMode = new Eddie.Forms.Skin.ComboBox();
			this.chkSystemStart = new Eddie.Forms.Skin.CheckBox();
			this.label13 = new Eddie.Forms.Skin.Label();
			this.chkOsSingleInstance = new Eddie.Forms.Skin.CheckBox();
			this.cmdResetToDefault = new Eddie.Forms.Skin.Button();
			this.lblConnect = new Eddie.Forms.Skin.Label();
			this.lblNetLock = new Eddie.Forms.Skin.Label();
			this.chkConnect = new Eddie.Forms.Skin.CheckBox();
			this.lblGeneralStartLast = new Eddie.Forms.Skin.Label();
			this.chkNetLock = new Eddie.Forms.Skin.CheckBox();
			this.chkGeneralStartLast = new Eddie.Forms.Skin.CheckBox();
			this.tabUI = new Eddie.Forms.Skin.TabPage();
			this.lbUiSkipPromotional = new Eddie.Forms.Skin.Label();
			this.chkUiSkipPromotional = new Eddie.Forms.Skin.CheckBox();
			this.lblUiStartMinimized = new Eddie.Forms.Skin.Label();
			this.chkUiStartMinimized = new Eddie.Forms.Skin.CheckBox();
			this.lblUiTrayMinimized = new Eddie.Forms.Skin.Label();
			this.chkUiTrayMinimized = new Eddie.Forms.Skin.CheckBox();
			this.lblUiTrayShow = new Eddie.Forms.Skin.Label();
			this.chkUiTrayShow = new Eddie.Forms.Skin.CheckBox();
			this.lblUiSystemNotifications = new Eddie.Forms.Skin.Label();
			this.chkUiIEC = new Eddie.Forms.Skin.CheckBox();
			this.chkUiSystemNotifications = new Eddie.Forms.Skin.CheckBox();
			this.chkUiFontGeneralTitle = new Eddie.Forms.Skin.Label();
			this.chkUiFontGeneral = new Eddie.Forms.Skin.CheckBox();
			this.lblUiExitConfirm = new Eddie.Forms.Skin.Label();
			this.chkUiExitConfirm = new Eddie.Forms.Skin.CheckBox();
			this.lblUiUnit = new Eddie.Forms.Skin.Label();
			this.cboUiUnit = new Eddie.Forms.Skin.ComboBox();
			this.cmdUiFontGeneral = new Eddie.Forms.Skin.Button();
			this.lblUiFontGeneral = new Eddie.Forms.Skin.Label();
			this.lblUiSkipProviderManifestFailed = new Eddie.Forms.Skin.Label();
			this.chkUiSkipProviderManifestFailed = new Eddie.Forms.Skin.CheckBox();
			this.tabProtocols = new Eddie.Forms.Skin.TabPage();
			this.lblProtocolsAvailable = new Eddie.Forms.Skin.Label();
			this.lnkProtocolsHelp2 = new Eddie.Forms.Skin.LinkLabel();
			this.lnkProtocolsHelp1 = new Eddie.Forms.Skin.LinkLabel();
			this.chkProtocolsAutomatic = new System.Windows.Forms.CheckBox();
			this.lstProtocols = new Eddie.Forms.Skin.ListView();
			this.colProtocolsProtocol = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.colProtocolsPort = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.colProtocolsEntry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colProtocolsDescription = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.colProtocolsTech = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabProxy = new Eddie.Forms.Skin.TabPage();
			this.lblProxyWhen = new Eddie.Forms.Skin.Label();
			this.cboProxyWhen = new Eddie.Forms.Skin.ComboBox();
			this.lnkProxyTorHelp = new Eddie.Forms.Skin.LinkLabel();
			this.txtProxyTorControlPassword = new Eddie.Forms.Skin.TextBox();
			this.lblProxyTorControlPassword = new Eddie.Forms.Skin.Label();
			this.cmdProxyTorTest = new Eddie.Forms.Skin.Button();
			this.txtProxyTorControlPort = new Eddie.Forms.Skin.TextBox();
			this.lblProxyTorControlPort = new Eddie.Forms.Skin.Label();
			this.label17 = new Eddie.Forms.Skin.Label();
			this.label12 = new Eddie.Forms.Skin.Label();
			this.lblProxyAuthentication = new Eddie.Forms.Skin.Label();
			this.cboProxyAuthentication = new Eddie.Forms.Skin.ComboBox();
			this.txtProxyPassword = new Eddie.Forms.Skin.TextBox();
			this.lblProxyPassword = new Eddie.Forms.Skin.Label();
			this.txtProxyLogin = new Eddie.Forms.Skin.TextBox();
			this.lblProxyLogin = new Eddie.Forms.Skin.Label();
			this.lblProxyType = new Eddie.Forms.Skin.Label();
			this.cboProxyMode = new Eddie.Forms.Skin.ComboBox();
			this.txtProxyPort = new Eddie.Forms.Skin.TextBox();
			this.lblProxyPort = new Eddie.Forms.Skin.Label();
			this.txtProxyHost = new Eddie.Forms.Skin.TextBox();
			this.lblProxyHost = new Eddie.Forms.Skin.Label();
			this.tabRoutes = new Eddie.Forms.Skin.TabPage();
			this.cmdRouteEdit = new Eddie.Forms.Skin.Button();
			this.cmdRouteRemove = new Eddie.Forms.Skin.Button();
			this.cmdRouteAdd = new Eddie.Forms.Skin.Button();
			this.label6 = new Eddie.Forms.Skin.Label();
			this.lstRoutes = new Eddie.Forms.Skin.ListView();
			this.colRoutesIp = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.colRoutesAction = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.colRoutesNotes = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.tabDNS = new Eddie.Forms.Skin.TabPage();
			this.pnlDnsWindowsOnly = new System.Windows.Forms.GroupBox();
			this.lblDnsIgnoreDNS6 = new Eddie.Forms.Skin.Label();
			this.lblDnsEnsureLock = new Eddie.Forms.Skin.Label();
			this.lblDnsForceAllInterfaces = new Eddie.Forms.Skin.Label();
			this.chkDnsIgnoreDNS6 = new Eddie.Forms.Skin.CheckBox();
			this.chkDnsEnsureLock = new Eddie.Forms.Skin.CheckBox();
			this.chkDnsForceAllInterfaces = new Eddie.Forms.Skin.CheckBox();
			this.label10 = new Eddie.Forms.Skin.Label();
			this.lblDnsCheck = new Eddie.Forms.Skin.Label();
			this.lblDnsServers = new Eddie.Forms.Skin.Label();
			this.cmdDnsEdit = new Eddie.Forms.Skin.Button();
			this.cmdDnsRemove = new Eddie.Forms.Skin.Button();
			this.cmdDnsAdd = new Eddie.Forms.Skin.Button();
			this.lstDnsServers = new Eddie.Forms.Skin.ListView();
			this.lblDnsSwitchMode = new Eddie.Forms.Skin.Label();
			this.cboDnsSwitchMode = new Eddie.Forms.Skin.ComboBox();
			this.chkDnsCheck = new Eddie.Forms.Skin.CheckBox();
			this.tabNetworkLock = new Eddie.Forms.Skin.TabPage();
			this.lblLockWhiteListOutgoingIPs = new Eddie.Forms.Skin.Label();
			this.txtLockWhiteListOutgoingIPs = new Eddie.Forms.Skin.TextBox();
			this.lblLockAllowDHCP = new Eddie.Forms.Skin.Label();
			this.chkLockAllowDHCP = new Eddie.Forms.Skin.CheckBox();
			this.lblLockOutgoing = new Eddie.Forms.Skin.Label();
			this.cboLockOutgoing = new Eddie.Forms.Skin.ComboBox();
			this.lblLockIncoming = new Eddie.Forms.Skin.Label();
			this.cboLockIncoming = new Eddie.Forms.Skin.ComboBox();
			this.lblLockAllowDNS = new Eddie.Forms.Skin.Label();
			this.chkLockAllowDNS = new Eddie.Forms.Skin.CheckBox();
			this.lblLockAllowPing = new Eddie.Forms.Skin.Label();
			this.lblLockAllowPrivate = new Eddie.Forms.Skin.Label();
			this.lnkLockHelp = new Eddie.Forms.Skin.LinkLabel();
			this.chkLockAllowPing = new Eddie.Forms.Skin.CheckBox();
			this.chkLockAllowPrivate = new Eddie.Forms.Skin.CheckBox();
			this.lblLockRoutingOutWarning = new Eddie.Forms.Skin.Label();
			this.lblLockWhiteListIncomingIPs = new Eddie.Forms.Skin.Label();
			this.txtLockWhiteListIncomingIPs = new Eddie.Forms.Skin.TextBox();
			this.lblLockMode = new Eddie.Forms.Skin.Label();
			this.cboLockMode = new Eddie.Forms.Skin.ComboBox();
			this.tabLogging = new Eddie.Forms.Skin.TabPage();
			this.cmdLoggingOpen = new Eddie.Forms.Skin.Button();
			this.chkLogLevelDebug = new Eddie.Forms.Skin.CheckBox();
			this.TxtLoggingPathComputed = new Eddie.Forms.Skin.Label();
			this.lblLoggingHelp = new Eddie.Forms.Skin.Label();
			this.txtLogPath = new Eddie.Forms.Skin.TextBox();
			this.lblLogPath = new Eddie.Forms.Skin.Label();
			this.chkLoggingEnabled = new Eddie.Forms.Skin.CheckBox();
			this.tabExperimentals = new System.Windows.Forms.TabPage();
			this.chkWindowsWintun = new Eddie.Forms.Skin.CheckBox();
			tabAdvanced = new Eddie.Forms.Skin.TabPage();
			tabDirectives = new Eddie.Forms.Skin.TabPage();
			tabEvents = new Eddie.Forms.Skin.TabPage();
			columnHeader1 = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			columnHeader2 = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			columnHeader5 = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			tabAdvanced.SuspendLayout();
			this.pnlAdvancedGeneralWindowsOnly.SuspendLayout();
			tabDirectives.SuspendLayout();
			tabEvents.SuspendLayout();
			this.tabNetworking.SuspendLayout();
			this.mnuRoutes.SuspendLayout();
			this.pnlCommands.SuspendLayout();
			this.tabSettings.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.tabUI.SuspendLayout();
			this.tabProtocols.SuspendLayout();
			this.tabProxy.SuspendLayout();
			this.tabRoutes.SuspendLayout();
			this.tabDNS.SuspendLayout();
			this.pnlDnsWindowsOnly.SuspendLayout();
			this.tabNetworkLock.SuspendLayout();
			this.tabLogging.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabAdvanced
			// 
			tabAdvanced.BackColor = System.Drawing.Color.White;
			tabAdvanced.Controls.Add(this.lblHummingbirdPrefer);
			tabAdvanced.Controls.Add(this.chkHummingbirdPrefer);
			tabAdvanced.Controls.Add(this.lblAdvancedUpdaterChannel);
			tabAdvanced.Controls.Add(this.cboAdvancedUpdaterChannel);
			tabAdvanced.Controls.Add(this.chkAdvancedSkipAlreadyRun);
			tabAdvanced.Controls.Add(this.lblAdvancedProviders);
			tabAdvanced.Controls.Add(this.lblAdvancedSkipAlreadyRun);
			tabAdvanced.Controls.Add(this.chkAdvancedProviders);
			tabAdvanced.Controls.Add(this.lnkAdvancedHelp);
			tabAdvanced.Controls.Add(this.lblExpert);
			tabAdvanced.Controls.Add(this.lblAdvancedPingerEnabled);
			tabAdvanced.Controls.Add(this.lblAdvancedCheckRoute);
			tabAdvanced.Controls.Add(this.lblAdvancedManifestRefresh);
			tabAdvanced.Controls.Add(this.cboAdvancedManifestRefresh);
			tabAdvanced.Controls.Add(this.pnlAdvancedGeneralWindowsOnly);
			tabAdvanced.Controls.Add(this.chkAdvancedPingerEnabled);
			tabAdvanced.Controls.Add(this.cmdExeBrowse);
			tabAdvanced.Controls.Add(this.txtExePath);
			tabAdvanced.Controls.Add(this.lblExePath);
			tabAdvanced.Controls.Add(this.chkAdvancedCheckRoute);
			tabAdvanced.Controls.Add(this.chkExpert);
			tabAdvanced.Location = new System.Drawing.Point(4, 24);
			tabAdvanced.Margin = new System.Windows.Forms.Padding(4);
			tabAdvanced.Name = "tabAdvanced";
			tabAdvanced.Padding = new System.Windows.Forms.Padding(4);
			tabAdvanced.Size = new System.Drawing.Size(1014, 635);
			tabAdvanced.TabIndex = 0;
			tabAdvanced.Text = "Advanced";
			// 
			// lblHummingbirdPrefer
			// 
			this.lblHummingbirdPrefer.BackColor = System.Drawing.Color.Transparent;
			this.lblHummingbirdPrefer.ForeColor = System.Drawing.Color.Black;
			this.lblHummingbirdPrefer.Location = new System.Drawing.Point(21, 523);
			this.lblHummingbirdPrefer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHummingbirdPrefer.Name = "lblHummingbirdPrefer";
			this.lblHummingbirdPrefer.Size = new System.Drawing.Size(330, 32);
			this.lblHummingbirdPrefer.TabIndex = 95;
			this.lblHummingbirdPrefer.Text = "Use Hummingbird if available";
			this.lblHummingbirdPrefer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkHummingbirdPrefer
			// 
			this.chkHummingbirdPrefer.BackColor = System.Drawing.Color.Transparent;
			this.chkHummingbirdPrefer.ForeColor = System.Drawing.Color.Black;
			this.chkHummingbirdPrefer.Location = new System.Drawing.Point(369, 526);
			this.chkHummingbirdPrefer.Margin = new System.Windows.Forms.Padding(4);
			this.chkHummingbirdPrefer.Name = "chkHummingbirdPrefer";
			this.chkHummingbirdPrefer.Size = new System.Drawing.Size(248, 28);
			this.chkHummingbirdPrefer.TabIndex = 94;
			this.chkHummingbirdPrefer.UseVisualStyleBackColor = false;
			// 
			// lblAdvancedUpdaterChannel
			// 
			this.lblAdvancedUpdaterChannel.BackColor = System.Drawing.Color.Transparent;
			this.lblAdvancedUpdaterChannel.ForeColor = System.Drawing.Color.Black;
			this.lblAdvancedUpdaterChannel.Location = new System.Drawing.Point(24, 207);
			this.lblAdvancedUpdaterChannel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblAdvancedUpdaterChannel.Name = "lblAdvancedUpdaterChannel";
			this.lblAdvancedUpdaterChannel.Size = new System.Drawing.Size(330, 32);
			this.lblAdvancedUpdaterChannel.TabIndex = 93;
			this.lblAdvancedUpdaterChannel.Text = "Update channel:";
			this.lblAdvancedUpdaterChannel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboAdvancedUpdaterChannel
			// 
			this.cboAdvancedUpdaterChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboAdvancedUpdaterChannel.FormattingEnabled = true;
			this.cboAdvancedUpdaterChannel.Items.AddRange(new object[] {
            "Automatic",
            "Never",
            "Every minute",
            "Every ten minute",
            "Every one hour"});
			this.cboAdvancedUpdaterChannel.Location = new System.Drawing.Point(369, 207);
			this.cboAdvancedUpdaterChannel.Margin = new System.Windows.Forms.Padding(4);
			this.cboAdvancedUpdaterChannel.Name = "cboAdvancedUpdaterChannel";
			this.cboAdvancedUpdaterChannel.Size = new System.Drawing.Size(198, 28);
			this.cboAdvancedUpdaterChannel.TabIndex = 92;
			// 
			// chkAdvancedSkipAlreadyRun
			// 
			this.chkAdvancedSkipAlreadyRun.BackColor = System.Drawing.Color.Transparent;
			this.chkAdvancedSkipAlreadyRun.ForeColor = System.Drawing.Color.Black;
			this.chkAdvancedSkipAlreadyRun.Location = new System.Drawing.Point(369, 436);
			this.chkAdvancedSkipAlreadyRun.Margin = new System.Windows.Forms.Padding(4);
			this.chkAdvancedSkipAlreadyRun.Name = "chkAdvancedSkipAlreadyRun";
			this.chkAdvancedSkipAlreadyRun.Size = new System.Drawing.Size(248, 28);
			this.chkAdvancedSkipAlreadyRun.TabIndex = 91;
			this.chkAdvancedSkipAlreadyRun.UseVisualStyleBackColor = false;
			// 
			// lblAdvancedProviders
			// 
			this.lblAdvancedProviders.BackColor = System.Drawing.Color.Transparent;
			this.lblAdvancedProviders.ForeColor = System.Drawing.Color.Black;
			this.lblAdvancedProviders.Location = new System.Drawing.Point(21, 478);
			this.lblAdvancedProviders.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblAdvancedProviders.Name = "lblAdvancedProviders";
			this.lblAdvancedProviders.Size = new System.Drawing.Size(330, 32);
			this.lblAdvancedProviders.TabIndex = 90;
			this.lblAdvancedProviders.Text = "Multi-providers support:";
			this.lblAdvancedProviders.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblAdvancedSkipAlreadyRun
			// 
			this.lblAdvancedSkipAlreadyRun.BackColor = System.Drawing.Color.Transparent;
			this.lblAdvancedSkipAlreadyRun.ForeColor = System.Drawing.Color.Black;
			this.lblAdvancedSkipAlreadyRun.Location = new System.Drawing.Point(21, 433);
			this.lblAdvancedSkipAlreadyRun.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblAdvancedSkipAlreadyRun.Name = "lblAdvancedSkipAlreadyRun";
			this.lblAdvancedSkipAlreadyRun.Size = new System.Drawing.Size(330, 32);
			this.lblAdvancedSkipAlreadyRun.TabIndex = 89;
			this.lblAdvancedSkipAlreadyRun.Text = "Skip processes checking:";
			this.lblAdvancedSkipAlreadyRun.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkAdvancedProviders
			// 
			this.chkAdvancedProviders.BackColor = System.Drawing.Color.Transparent;
			this.chkAdvancedProviders.ForeColor = System.Drawing.Color.Black;
			this.chkAdvancedProviders.Location = new System.Drawing.Point(369, 481);
			this.chkAdvancedProviders.Margin = new System.Windows.Forms.Padding(4);
			this.chkAdvancedProviders.Name = "chkAdvancedProviders";
			this.chkAdvancedProviders.Size = new System.Drawing.Size(248, 28);
			this.chkAdvancedProviders.TabIndex = 88;
			this.chkAdvancedProviders.UseVisualStyleBackColor = false;
			// 
			// lnkAdvancedHelp
			// 
			this.lnkAdvancedHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkAdvancedHelp.BackColor = System.Drawing.Color.Maroon;
			this.lnkAdvancedHelp.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkAdvancedHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkAdvancedHelp.Location = new System.Drawing.Point(20, 572);
			this.lnkAdvancedHelp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lnkAdvancedHelp.Name = "lnkAdvancedHelp";
			this.lnkAdvancedHelp.Size = new System.Drawing.Size(975, 33);
			this.lnkAdvancedHelp.TabIndex = 87;
			this.lnkAdvancedHelp.TabStop = true;
			this.lnkAdvancedHelp.Text = "More about Advanced Features";
			this.lnkAdvancedHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lnkAdvancedHelp.Click += new System.EventHandler(this.lnkAdvancedHelp_LinkClicked);
			// 
			// lblExpert
			// 
			this.lblExpert.BackColor = System.Drawing.Color.Transparent;
			this.lblExpert.ForeColor = System.Drawing.Color.Black;
			this.lblExpert.Location = new System.Drawing.Point(24, 27);
			this.lblExpert.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblExpert.Name = "lblExpert";
			this.lblExpert.Size = new System.Drawing.Size(332, 33);
			this.lblExpert.TabIndex = 79;
			this.lblExpert.Text = "Expert Mode:";
			this.lblExpert.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblAdvancedPingerEnabled
			// 
			this.lblAdvancedPingerEnabled.BackColor = System.Drawing.Color.Transparent;
			this.lblAdvancedPingerEnabled.ForeColor = System.Drawing.Color.Black;
			this.lblAdvancedPingerEnabled.Location = new System.Drawing.Point(21, 276);
			this.lblAdvancedPingerEnabled.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblAdvancedPingerEnabled.Name = "lblAdvancedPingerEnabled";
			this.lblAdvancedPingerEnabled.Size = new System.Drawing.Size(330, 33);
			this.lblAdvancedPingerEnabled.TabIndex = 78;
			this.lblAdvancedPingerEnabled.Text = "Enable latency tests:";
			this.lblAdvancedPingerEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblAdvancedCheckRoute
			// 
			this.lblAdvancedCheckRoute.BackColor = System.Drawing.Color.Transparent;
			this.lblAdvancedCheckRoute.ForeColor = System.Drawing.Color.Black;
			this.lblAdvancedCheckRoute.Location = new System.Drawing.Point(24, 72);
			this.lblAdvancedCheckRoute.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblAdvancedCheckRoute.Name = "lblAdvancedCheckRoute";
			this.lblAdvancedCheckRoute.Size = new System.Drawing.Size(332, 33);
			this.lblAdvancedCheckRoute.TabIndex = 77;
			this.lblAdvancedCheckRoute.Text = "Check if the tunnel works:";
			this.lblAdvancedCheckRoute.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblAdvancedManifestRefresh
			// 
			this.lblAdvancedManifestRefresh.BackColor = System.Drawing.Color.Transparent;
			this.lblAdvancedManifestRefresh.ForeColor = System.Drawing.Color.Black;
			this.lblAdvancedManifestRefresh.Location = new System.Drawing.Point(24, 162);
			this.lblAdvancedManifestRefresh.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblAdvancedManifestRefresh.Name = "lblAdvancedManifestRefresh";
			this.lblAdvancedManifestRefresh.Size = new System.Drawing.Size(330, 32);
			this.lblAdvancedManifestRefresh.TabIndex = 74;
			this.lblAdvancedManifestRefresh.Text = "Servers list update every:";
			this.lblAdvancedManifestRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboAdvancedManifestRefresh
			// 
			this.cboAdvancedManifestRefresh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboAdvancedManifestRefresh.FormattingEnabled = true;
			this.cboAdvancedManifestRefresh.Items.AddRange(new object[] {
            "Automatic",
            "Never",
            "Every minute",
            "Every ten minute",
            "Every one hour"});
			this.cboAdvancedManifestRefresh.Location = new System.Drawing.Point(369, 162);
			this.cboAdvancedManifestRefresh.Margin = new System.Windows.Forms.Padding(4);
			this.cboAdvancedManifestRefresh.Name = "cboAdvancedManifestRefresh";
			this.cboAdvancedManifestRefresh.Size = new System.Drawing.Size(198, 28);
			this.cboAdvancedManifestRefresh.TabIndex = 73;
			// 
			// pnlAdvancedGeneralWindowsOnly
			// 
			this.pnlAdvancedGeneralWindowsOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsWintun);
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.cmdAdvancedUninstallDriverWintun);
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsSshPlinkForce);
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsDisableDriverUpgrade);
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsDebugWorkaround);
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsTapUp);
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.cmdAdvancedUninstallDriverTap);
			this.pnlAdvancedGeneralWindowsOnly.Location = new System.Drawing.Point(604, 16);
			this.pnlAdvancedGeneralWindowsOnly.Margin = new System.Windows.Forms.Padding(4);
			this.pnlAdvancedGeneralWindowsOnly.Name = "pnlAdvancedGeneralWindowsOnly";
			this.pnlAdvancedGeneralWindowsOnly.Padding = new System.Windows.Forms.Padding(4);
			this.pnlAdvancedGeneralWindowsOnly.Size = new System.Drawing.Size(388, 362);
			this.pnlAdvancedGeneralWindowsOnly.TabIndex = 69;
			this.pnlAdvancedGeneralWindowsOnly.TabStop = false;
			this.pnlAdvancedGeneralWindowsOnly.Text = "Microsoft Windows Only";
			// 
			// cmdAdvancedUninstallDriverWintun
			// 
			this.cmdAdvancedUninstallDriverWintun.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdAdvancedUninstallDriverWintun.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdAdvancedUninstallDriverWintun.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdAdvancedUninstallDriverWintun.FlatAppearance.BorderSize = 0;
			this.cmdAdvancedUninstallDriverWintun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAdvancedUninstallDriverWintun.Location = new System.Drawing.Point(24, 296);
			this.cmdAdvancedUninstallDriverWintun.Margin = new System.Windows.Forms.Padding(4);
			this.cmdAdvancedUninstallDriverWintun.Name = "cmdAdvancedUninstallDriverWintun";
			this.cmdAdvancedUninstallDriverWintun.Size = new System.Drawing.Size(356, 40);
			this.cmdAdvancedUninstallDriverWintun.TabIndex = 89;
			this.cmdAdvancedUninstallDriverWintun.Text = "Uninstall wintun driver";
			this.cmdAdvancedUninstallDriverWintun.UseVisualStyleBackColor = true;
			this.cmdAdvancedUninstallDriverWintun.Click += new System.EventHandler(this.cmdAdvancedUninstallDriverWintun_Click);
			// 
			// chkWindowsSshPlinkForce
			// 
			this.chkWindowsSshPlinkForce.BackColor = System.Drawing.Color.Transparent;
			this.chkWindowsSshPlinkForce.ForeColor = System.Drawing.Color.Black;
			this.chkWindowsSshPlinkForce.Location = new System.Drawing.Point(24, 108);
			this.chkWindowsSshPlinkForce.Margin = new System.Windows.Forms.Padding(4);
			this.chkWindowsSshPlinkForce.Name = "chkWindowsSshPlinkForce";
			this.chkWindowsSshPlinkForce.Size = new System.Drawing.Size(356, 33);
			this.chkWindowsSshPlinkForce.TabIndex = 88;
			this.chkWindowsSshPlinkForce.Text = "Force usage of bundled plink.exe";
			this.chkWindowsSshPlinkForce.UseVisualStyleBackColor = false;
			// 
			// chkWindowsDisableDriverUpgrade
			// 
			this.chkWindowsDisableDriverUpgrade.BackColor = System.Drawing.Color.Transparent;
			this.chkWindowsDisableDriverUpgrade.ForeColor = System.Drawing.Color.Black;
			this.chkWindowsDisableDriverUpgrade.Location = new System.Drawing.Point(24, 203);
			this.chkWindowsDisableDriverUpgrade.Margin = new System.Windows.Forms.Padding(4);
			this.chkWindowsDisableDriverUpgrade.Name = "chkWindowsDisableDriverUpgrade";
			this.chkWindowsDisableDriverUpgrade.Size = new System.Drawing.Size(356, 33);
			this.chkWindowsDisableDriverUpgrade.TabIndex = 66;
			this.chkWindowsDisableDriverUpgrade.Text = "Disable driver upgrade";
			this.chkWindowsDisableDriverUpgrade.UseVisualStyleBackColor = false;
			// 
			// chkWindowsDebugWorkaround
			// 
			this.chkWindowsDebugWorkaround.BackColor = System.Drawing.Color.Transparent;
			this.chkWindowsDebugWorkaround.ForeColor = System.Drawing.Color.Black;
			this.chkWindowsDebugWorkaround.Location = new System.Drawing.Point(24, 153);
			this.chkWindowsDebugWorkaround.Margin = new System.Windows.Forms.Padding(4);
			this.chkWindowsDebugWorkaround.Name = "chkWindowsDebugWorkaround";
			this.chkWindowsDebugWorkaround.Size = new System.Drawing.Size(248, 28);
			this.chkWindowsDebugWorkaround.TabIndex = 86;
			this.chkWindowsDebugWorkaround.Text = "Experimental workarounds";
			this.chkWindowsDebugWorkaround.UseVisualStyleBackColor = false;
			// 
			// chkWindowsTapUp
			// 
			this.chkWindowsTapUp.BackColor = System.Drawing.Color.Transparent;
			this.chkWindowsTapUp.ForeColor = System.Drawing.Color.Black;
			this.chkWindowsTapUp.Location = new System.Drawing.Point(24, 67);
			this.chkWindowsTapUp.Margin = new System.Windows.Forms.Padding(4);
			this.chkWindowsTapUp.Name = "chkWindowsTapUp";
			this.chkWindowsTapUp.Size = new System.Drawing.Size(356, 33);
			this.chkWindowsTapUp.TabIndex = 55;
			this.chkWindowsTapUp.Text = "Force TAP interface UP";
			this.chkWindowsTapUp.UseVisualStyleBackColor = false;
			// 
			// cmdAdvancedUninstallDriverTap
			// 
			this.cmdAdvancedUninstallDriverTap.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdAdvancedUninstallDriverTap.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdAdvancedUninstallDriverTap.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdAdvancedUninstallDriverTap.FlatAppearance.BorderSize = 0;
			this.cmdAdvancedUninstallDriverTap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAdvancedUninstallDriverTap.Location = new System.Drawing.Point(24, 248);
			this.cmdAdvancedUninstallDriverTap.Margin = new System.Windows.Forms.Padding(4);
			this.cmdAdvancedUninstallDriverTap.Name = "cmdAdvancedUninstallDriverTap";
			this.cmdAdvancedUninstallDriverTap.Size = new System.Drawing.Size(356, 40);
			this.cmdAdvancedUninstallDriverTap.TabIndex = 65;
			this.cmdAdvancedUninstallDriverTap.Text = "Uninstall standard driver";
			this.cmdAdvancedUninstallDriverTap.UseVisualStyleBackColor = true;
			this.cmdAdvancedUninstallDriverTap.Click += new System.EventHandler(this.cmdAdvancedUninstallDriver_Click);
			// 
			// chkAdvancedPingerEnabled
			// 
			this.chkAdvancedPingerEnabled.BackColor = System.Drawing.Color.Transparent;
			this.chkAdvancedPingerEnabled.ForeColor = System.Drawing.Color.Black;
			this.chkAdvancedPingerEnabled.Location = new System.Drawing.Point(369, 276);
			this.chkAdvancedPingerEnabled.Margin = new System.Windows.Forms.Padding(4);
			this.chkAdvancedPingerEnabled.Name = "chkAdvancedPingerEnabled";
			this.chkAdvancedPingerEnabled.Size = new System.Drawing.Size(201, 38);
			this.chkAdvancedPingerEnabled.TabIndex = 66;
			this.chkAdvancedPingerEnabled.UseVisualStyleBackColor = false;
			// 
			// cmdExeBrowse
			// 
			this.cmdExeBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdExeBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdExeBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdExeBrowse.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdExeBrowse.FlatAppearance.BorderSize = 0;
			this.cmdExeBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdExeBrowse.Image = global::Eddie.Forms.Properties.Resources.browse;
			this.cmdExeBrowse.Location = new System.Drawing.Point(942, 388);
			this.cmdExeBrowse.Margin = new System.Windows.Forms.Padding(4);
			this.cmdExeBrowse.Name = "cmdExeBrowse";
			this.cmdExeBrowse.Size = new System.Drawing.Size(50, 30);
			this.cmdExeBrowse.TabIndex = 60;
			this.cmdExeBrowse.UseVisualStyleBackColor = true;
			this.cmdExeBrowse.Click += new System.EventHandler(this.cmdExeBrowse_Click);
			// 
			// txtExePath
			// 
			this.txtExePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtExePath.Location = new System.Drawing.Point(369, 388);
			this.txtExePath.Margin = new System.Windows.Forms.Padding(4);
			this.txtExePath.Name = "txtExePath";
			this.txtExePath.Size = new System.Drawing.Size(565, 26);
			this.txtExePath.TabIndex = 59;
			// 
			// lblExePath
			// 
			this.lblExePath.BackColor = System.Drawing.Color.Transparent;
			this.lblExePath.ForeColor = System.Drawing.Color.Black;
			this.lblExePath.Location = new System.Drawing.Point(21, 388);
			this.lblExePath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblExePath.Name = "lblExePath";
			this.lblExePath.Size = new System.Drawing.Size(330, 32);
			this.lblExePath.TabIndex = 58;
			this.lblExePath.Text = "OpenVPN Custom Path:";
			this.lblExePath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkAdvancedCheckRoute
			// 
			this.chkAdvancedCheckRoute.BackColor = System.Drawing.Color.Transparent;
			this.chkAdvancedCheckRoute.ForeColor = System.Drawing.Color.Black;
			this.chkAdvancedCheckRoute.Location = new System.Drawing.Point(369, 72);
			this.chkAdvancedCheckRoute.Margin = new System.Windows.Forms.Padding(4);
			this.chkAdvancedCheckRoute.Name = "chkAdvancedCheckRoute";
			this.chkAdvancedCheckRoute.Size = new System.Drawing.Size(201, 38);
			this.chkAdvancedCheckRoute.TabIndex = 57;
			this.chkAdvancedCheckRoute.UseVisualStyleBackColor = false;
			// 
			// chkExpert
			// 
			this.chkExpert.BackColor = System.Drawing.Color.Transparent;
			this.chkExpert.ForeColor = System.Drawing.Color.Black;
			this.chkExpert.Location = new System.Drawing.Point(369, 27);
			this.chkExpert.Margin = new System.Windows.Forms.Padding(4);
			this.chkExpert.Name = "chkExpert";
			this.chkExpert.Size = new System.Drawing.Size(201, 33);
			this.chkExpert.TabIndex = 54;
			this.chkExpert.UseVisualStyleBackColor = false;
			// 
			// tabDirectives
			// 
			tabDirectives.BackColor = System.Drawing.Color.White;
			tabDirectives.Controls.Add(this.chkOpenVpnDirectivesAllowScriptSecurity);
			tabDirectives.Controls.Add(this.cmdOpenVpnDirectivesCustomPathBrowse);
			tabDirectives.Controls.Add(this.txtOpenVpnDirectivesCustomPath);
			tabDirectives.Controls.Add(this.label8);
			tabDirectives.Controls.Add(this.lnkOpenVpnDirectivesHelp);
			tabDirectives.Controls.Add(this.cboOpenVpnDirectivesDefaultSkip);
			tabDirectives.Controls.Add(this.label3);
			tabDirectives.Controls.Add(this.label2);
			tabDirectives.Controls.Add(this.txtOpenVpnDirectivesBase);
			tabDirectives.Controls.Add(this.txtOpenVpnDirectivesCustom);
			tabDirectives.Location = new System.Drawing.Point(4, 24);
			tabDirectives.Margin = new System.Windows.Forms.Padding(4);
			tabDirectives.Name = "tabDirectives";
			tabDirectives.Padding = new System.Windows.Forms.Padding(4);
			tabDirectives.Size = new System.Drawing.Size(1014, 635);
			tabDirectives.TabIndex = 1;
			tabDirectives.Text = "OVPN directives";
			// 
			// chkOpenVpnDirectivesAllowScriptSecurity
			// 
			this.chkOpenVpnDirectivesAllowScriptSecurity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkOpenVpnDirectivesAllowScriptSecurity.BackColor = System.Drawing.Color.Transparent;
			this.chkOpenVpnDirectivesAllowScriptSecurity.ForeColor = System.Drawing.Color.Black;
			this.chkOpenVpnDirectivesAllowScriptSecurity.Location = new System.Drawing.Point(568, 530);
			this.chkOpenVpnDirectivesAllowScriptSecurity.Margin = new System.Windows.Forms.Padding(4);
			this.chkOpenVpnDirectivesAllowScriptSecurity.Name = "chkOpenVpnDirectivesAllowScriptSecurity";
			this.chkOpenVpnDirectivesAllowScriptSecurity.Size = new System.Drawing.Size(425, 31);
			this.chkOpenVpnDirectivesAllowScriptSecurity.TabIndex = 92;
			this.chkOpenVpnDirectivesAllowScriptSecurity.Text = "Allow script-security directives";
			this.chkOpenVpnDirectivesAllowScriptSecurity.UseVisualStyleBackColor = false;
			// 
			// cmdOpenVpnDirectivesCustomPathBrowse
			// 
			this.cmdOpenVpnDirectivesCustomPathBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOpenVpnDirectivesCustomPathBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOpenVpnDirectivesCustomPathBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOpenVpnDirectivesCustomPathBrowse.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdOpenVpnDirectivesCustomPathBrowse.FlatAppearance.BorderSize = 0;
			this.cmdOpenVpnDirectivesCustomPathBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOpenVpnDirectivesCustomPathBrowse.Image = global::Eddie.Forms.Properties.Resources.browse;
			this.cmdOpenVpnDirectivesCustomPathBrowse.Location = new System.Drawing.Point(442, 531);
			this.cmdOpenVpnDirectivesCustomPathBrowse.Margin = new System.Windows.Forms.Padding(4);
			this.cmdOpenVpnDirectivesCustomPathBrowse.Name = "cmdOpenVpnDirectivesCustomPathBrowse";
			this.cmdOpenVpnDirectivesCustomPathBrowse.Size = new System.Drawing.Size(50, 30);
			this.cmdOpenVpnDirectivesCustomPathBrowse.TabIndex = 91;
			this.cmdOpenVpnDirectivesCustomPathBrowse.UseVisualStyleBackColor = true;
			this.cmdOpenVpnDirectivesCustomPathBrowse.Click += new System.EventHandler(this.cmdOpenVpnDirectivesCustomPathBrowse_Click);
			// 
			// txtOpenVpnDirectivesCustomPath
			// 
			this.txtOpenVpnDirectivesCustomPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOpenVpnDirectivesCustomPath.Location = new System.Drawing.Point(192, 531);
			this.txtOpenVpnDirectivesCustomPath.Margin = new System.Windows.Forms.Padding(4);
			this.txtOpenVpnDirectivesCustomPath.Name = "txtOpenVpnDirectivesCustomPath";
			this.txtOpenVpnDirectivesCustomPath.Size = new System.Drawing.Size(239, 26);
			this.txtOpenVpnDirectivesCustomPath.TabIndex = 90;
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label8.BackColor = System.Drawing.Color.Transparent;
			this.label8.ForeColor = System.Drawing.Color.Black;
			this.label8.Location = new System.Drawing.Point(20, 531);
			this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(164, 30);
			this.label8.TabIndex = 89;
			this.label8.Text = "External Path:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lnkOpenVpnDirectivesHelp
			// 
			this.lnkOpenVpnDirectivesHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkOpenVpnDirectivesHelp.BackColor = System.Drawing.Color.Maroon;
			this.lnkOpenVpnDirectivesHelp.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkOpenVpnDirectivesHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkOpenVpnDirectivesHelp.Location = new System.Drawing.Point(20, 572);
			this.lnkOpenVpnDirectivesHelp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lnkOpenVpnDirectivesHelp.Name = "lnkOpenVpnDirectivesHelp";
			this.lnkOpenVpnDirectivesHelp.Size = new System.Drawing.Size(975, 33);
			this.lnkOpenVpnDirectivesHelp.TabIndex = 88;
			this.lnkOpenVpnDirectivesHelp.TabStop = true;
			this.lnkOpenVpnDirectivesHelp.Text = "More about OpenVPN directives";
			this.lnkOpenVpnDirectivesHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lnkOpenVpnDirectivesHelp.Click += new System.EventHandler(this.lnkOpenVpnDirectivesHelp_Click);
			// 
			// cboOpenVpnDirectivesDefaultSkip
			// 
			this.cboOpenVpnDirectivesDefaultSkip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cboOpenVpnDirectivesDefaultSkip.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboOpenVpnDirectivesDefaultSkip.FormattingEnabled = true;
			this.cboOpenVpnDirectivesDefaultSkip.Items.AddRange(new object[] {
            "Disabled",
            "Automatic",
            "Resolvconf (Linux only)",
            "Renaming (Linux only)"});
			this.cboOpenVpnDirectivesDefaultSkip.Location = new System.Drawing.Point(501, 480);
			this.cboOpenVpnDirectivesDefaultSkip.Margin = new System.Windows.Forms.Padding(4);
			this.cboOpenVpnDirectivesDefaultSkip.Name = "cboOpenVpnDirectivesDefaultSkip";
			this.cboOpenVpnDirectivesDefaultSkip.Size = new System.Drawing.Size(492, 28);
			this.cboOpenVpnDirectivesDefaultSkip.TabIndex = 75;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(501, 10);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(496, 30);
			this.label3.TabIndex = 61;
			this.label3.Text = "Base directives:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.ForeColor = System.Drawing.Color.Black;
			this.label2.Location = new System.Drawing.Point(20, 10);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(472, 30);
			this.label2.TabIndex = 60;
			this.label2.Text = "Custom directives:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// txtOpenVpnDirectivesBase
			// 
			this.txtOpenVpnDirectivesBase.AcceptsReturn = true;
			this.txtOpenVpnDirectivesBase.AcceptsTab = true;
			this.txtOpenVpnDirectivesBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOpenVpnDirectivesBase.Location = new System.Drawing.Point(501, 45);
			this.txtOpenVpnDirectivesBase.Margin = new System.Windows.Forms.Padding(4);
			this.txtOpenVpnDirectivesBase.Multiline = true;
			this.txtOpenVpnDirectivesBase.Name = "txtOpenVpnDirectivesBase";
			this.txtOpenVpnDirectivesBase.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtOpenVpnDirectivesBase.Size = new System.Drawing.Size(492, 427);
			this.txtOpenVpnDirectivesBase.TabIndex = 58;
			// 
			// txtOpenVpnDirectivesCustom
			// 
			this.txtOpenVpnDirectivesCustom.AcceptsReturn = true;
			this.txtOpenVpnDirectivesCustom.AcceptsTab = true;
			this.txtOpenVpnDirectivesCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOpenVpnDirectivesCustom.Location = new System.Drawing.Point(20, 45);
			this.txtOpenVpnDirectivesCustom.Margin = new System.Windows.Forms.Padding(4);
			this.txtOpenVpnDirectivesCustom.Multiline = true;
			this.txtOpenVpnDirectivesCustom.Name = "txtOpenVpnDirectivesCustom";
			this.txtOpenVpnDirectivesCustom.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtOpenVpnDirectivesCustom.Size = new System.Drawing.Size(470, 427);
			this.txtOpenVpnDirectivesCustom.TabIndex = 57;
			// 
			// tabEvents
			// 
			tabEvents.BackColor = System.Drawing.Color.White;
			tabEvents.Controls.Add(this.chkShellExternalRecommended);
			tabEvents.Controls.Add(this.cmdShellExternalClear);
			tabEvents.Controls.Add(this.cmdShellExternalView);
			tabEvents.Controls.Add(this.lblShellExternal);
			tabEvents.Controls.Add(this.cmdAdvancedEventsEdit);
			tabEvents.Controls.Add(this.cmdAdvancedEventsClear);
			tabEvents.Controls.Add(this.lstAdvancedEvents);
			tabEvents.Controls.Add(this.label1);
			tabEvents.Location = new System.Drawing.Point(4, 24);
			tabEvents.Margin = new System.Windows.Forms.Padding(4);
			tabEvents.Name = "tabEvents";
			tabEvents.Size = new System.Drawing.Size(1014, 635);
			tabEvents.TabIndex = 2;
			tabEvents.Text = "Events";
			// 
			// chkShellExternalRecommended
			// 
			this.chkShellExternalRecommended.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkShellExternalRecommended.BackColor = System.Drawing.Color.Transparent;
			this.chkShellExternalRecommended.ForeColor = System.Drawing.Color.Black;
			this.chkShellExternalRecommended.Location = new System.Drawing.Point(15, 566);
			this.chkShellExternalRecommended.Margin = new System.Windows.Forms.Padding(4);
			this.chkShellExternalRecommended.Name = "chkShellExternalRecommended";
			this.chkShellExternalRecommended.Size = new System.Drawing.Size(356, 33);
			this.chkShellExternalRecommended.TabIndex = 72;
			this.chkShellExternalRecommended.Text = "Use recommended in bundle";
			this.chkShellExternalRecommended.UseVisualStyleBackColor = false;
			// 
			// cmdShellExternalClear
			// 
			this.cmdShellExternalClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cmdShellExternalClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdShellExternalClear.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdShellExternalClear.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdShellExternalClear.FlatAppearance.BorderSize = 0;
			this.cmdShellExternalClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdShellExternalClear.Location = new System.Drawing.Point(789, 561);
			this.cmdShellExternalClear.Margin = new System.Windows.Forms.Padding(4);
			this.cmdShellExternalClear.Name = "cmdShellExternalClear";
			this.cmdShellExternalClear.Size = new System.Drawing.Size(150, 40);
			this.cmdShellExternalClear.TabIndex = 71;
			this.cmdShellExternalClear.Text = "Clear";
			this.cmdShellExternalClear.UseVisualStyleBackColor = true;
			this.cmdShellExternalClear.Click += new System.EventHandler(this.cmdShellExternalClear_Click);
			// 
			// cmdShellExternalView
			// 
			this.cmdShellExternalView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cmdShellExternalView.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdShellExternalView.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdShellExternalView.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdShellExternalView.FlatAppearance.BorderSize = 0;
			this.cmdShellExternalView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdShellExternalView.Location = new System.Drawing.Point(379, 561);
			this.cmdShellExternalView.Margin = new System.Windows.Forms.Padding(4);
			this.cmdShellExternalView.Name = "cmdShellExternalView";
			this.cmdShellExternalView.Size = new System.Drawing.Size(402, 40);
			this.cmdShellExternalView.TabIndex = 70;
			this.cmdShellExternalView.Text = "View accepted rules";
			this.cmdShellExternalView.UseVisualStyleBackColor = true;
			this.cmdShellExternalView.Click += new System.EventHandler(this.cmdShellExternalView_Click);
			// 
			// lblShellExternal
			// 
			this.lblShellExternal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblShellExternal.BackColor = System.Drawing.Color.Transparent;
			this.lblShellExternal.ForeColor = System.Drawing.Color.Black;
			this.lblShellExternal.Location = new System.Drawing.Point(11, 522);
			this.lblShellExternal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblShellExternal.Name = "lblShellExternal";
			this.lblShellExternal.Size = new System.Drawing.Size(492, 30);
			this.lblShellExternal.TabIndex = 68;
			this.lblShellExternal.Text = "External program whitelist:";
			this.lblShellExternal.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// cmdAdvancedEventsEdit
			// 
			this.cmdAdvancedEventsEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdAdvancedEventsEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdAdvancedEventsEdit.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdAdvancedEventsEdit.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdAdvancedEventsEdit.FlatAppearance.BorderSize = 0;
			this.cmdAdvancedEventsEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAdvancedEventsEdit.Image = global::Eddie.Forms.Properties.Resources.edit;
			this.cmdAdvancedEventsEdit.Location = new System.Drawing.Point(950, 96);
			this.cmdAdvancedEventsEdit.Margin = new System.Windows.Forms.Padding(4);
			this.cmdAdvancedEventsEdit.Name = "cmdAdvancedEventsEdit";
			this.cmdAdvancedEventsEdit.Size = new System.Drawing.Size(42, 42);
			this.cmdAdvancedEventsEdit.TabIndex = 59;
			this.cmdAdvancedEventsEdit.UseVisualStyleBackColor = true;
			this.cmdAdvancedEventsEdit.Click += new System.EventHandler(this.cmdAdvancedEventsEdit_Click);
			// 
			// cmdAdvancedEventsClear
			// 
			this.cmdAdvancedEventsClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdAdvancedEventsClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdAdvancedEventsClear.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdAdvancedEventsClear.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdAdvancedEventsClear.FlatAppearance.BorderSize = 0;
			this.cmdAdvancedEventsClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAdvancedEventsClear.Image = global::Eddie.Forms.Properties.Resources.delete;
			this.cmdAdvancedEventsClear.Location = new System.Drawing.Point(950, 45);
			this.cmdAdvancedEventsClear.Margin = new System.Windows.Forms.Padding(4);
			this.cmdAdvancedEventsClear.Name = "cmdAdvancedEventsClear";
			this.cmdAdvancedEventsClear.Size = new System.Drawing.Size(42, 42);
			this.cmdAdvancedEventsClear.TabIndex = 58;
			this.cmdAdvancedEventsClear.UseVisualStyleBackColor = true;
			this.cmdAdvancedEventsClear.Click += new System.EventHandler(this.cmdAdvancedEventsClear_Click);
			// 
			// lstAdvancedEvents
			// 
			this.lstAdvancedEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstAdvancedEvents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
			this.lstAdvancedEvents.FullRowSelect = true;
			this.lstAdvancedEvents.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstAdvancedEvents.HideSelection = false;
			this.lstAdvancedEvents.Location = new System.Drawing.Point(15, 45);
			this.lstAdvancedEvents.Margin = new System.Windows.Forms.Padding(4);
			this.lstAdvancedEvents.MultiSelect = false;
			this.lstAdvancedEvents.Name = "lstAdvancedEvents";
			this.lstAdvancedEvents.OwnerDraw = true;
			this.lstAdvancedEvents.Size = new System.Drawing.Size(924, 463);
			this.lstAdvancedEvents.TabIndex = 57;
			this.lstAdvancedEvents.UseCompatibleStateImageBehavior = false;
			this.lstAdvancedEvents.View = System.Windows.Forms.View.Details;
			this.lstAdvancedEvents.SelectedIndexChanged += new System.EventHandler(this.lstAdvancedEvents_SelectedIndexChanged);
			this.lstAdvancedEvents.DoubleClick += new System.EventHandler(this.lstAdvancedEvents_DoubleClick);
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Event";
			columnHeader1.Width = 100;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "File name";
			columnHeader2.Width = 220;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Arguments";
			this.columnHeader3.Width = 180;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Wait End";
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(9, 10);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(519, 30);
			this.label1.TabIndex = 56;
			this.label1.Text = "External shell:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "IP Address";
			columnHeader5.Width = 150;
			// 
			// tabNetworking
			// 
			this.tabNetworking.Controls.Add(this.lblNetworkIPv4AutoSwitch);
			this.tabNetworking.Controls.Add(this.chkNetworkIPv4AutoSwitch);
			this.tabNetworking.Controls.Add(this.lblNetworkIPv6AutoSwitch);
			this.tabNetworking.Controls.Add(this.chkNetworkIPv6AutoSwitch);
			this.tabNetworking.Controls.Add(this.lblNetworkIPv6Mode);
			this.tabNetworking.Controls.Add(this.cboNetworkIPv6Mode);
			this.tabNetworking.Controls.Add(this.lblNetworkIPv4Mode);
			this.tabNetworking.Controls.Add(this.cboNetworkIPv4Mode);
			this.tabNetworking.Controls.Add(this.lblNetworkEntryInterface);
			this.tabNetworking.Controls.Add(this.cboNetworkEntryInterface);
			this.tabNetworking.Controls.Add(this.lblOpenVpnRcvbuf);
			this.tabNetworking.Controls.Add(this.cboOpenVpnRcvbuf);
			this.tabNetworking.Controls.Add(this.lblOpenVpnSndbuf);
			this.tabNetworking.Controls.Add(this.cboOpenVpnSndbuf);
			this.tabNetworking.Controls.Add(this.lblNetworkEntryIpLayer);
			this.tabNetworking.Controls.Add(this.cboNetworkEntryIpLayer);
			this.tabNetworking.Location = new System.Drawing.Point(4, 24);
			this.tabNetworking.Margin = new System.Windows.Forms.Padding(4);
			this.tabNetworking.Name = "tabNetworking";
			this.tabNetworking.Size = new System.Drawing.Size(1014, 635);
			this.tabNetworking.TabIndex = 6;
			this.tabNetworking.Text = "Networking";
			// 
			// lblNetworkIPv4AutoSwitch
			// 
			this.lblNetworkIPv4AutoSwitch.BackColor = System.Drawing.Color.Transparent;
			this.lblNetworkIPv4AutoSwitch.ForeColor = System.Drawing.Color.Black;
			this.lblNetworkIPv4AutoSwitch.Location = new System.Drawing.Point(28, 70);
			this.lblNetworkIPv4AutoSwitch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNetworkIPv4AutoSwitch.Name = "lblNetworkIPv4AutoSwitch";
			this.lblNetworkIPv4AutoSwitch.Size = new System.Drawing.Size(423, 33);
			this.lblNetworkIPv4AutoSwitch.TabIndex = 111;
			this.lblNetworkIPv4AutoSwitch.Text = "Switch to \'Block\' if issue is detected:";
			this.lblNetworkIPv4AutoSwitch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkNetworkIPv4AutoSwitch
			// 
			this.chkNetworkIPv4AutoSwitch.BackColor = System.Drawing.Color.Transparent;
			this.chkNetworkIPv4AutoSwitch.ForeColor = System.Drawing.Color.Black;
			this.chkNetworkIPv4AutoSwitch.Location = new System.Drawing.Point(464, 70);
			this.chkNetworkIPv4AutoSwitch.Margin = new System.Windows.Forms.Padding(4);
			this.chkNetworkIPv4AutoSwitch.Name = "chkNetworkIPv4AutoSwitch";
			this.chkNetworkIPv4AutoSwitch.Size = new System.Drawing.Size(201, 38);
			this.chkNetworkIPv4AutoSwitch.TabIndex = 110;
			this.chkNetworkIPv4AutoSwitch.UseVisualStyleBackColor = false;
			// 
			// lblNetworkIPv6AutoSwitch
			// 
			this.lblNetworkIPv6AutoSwitch.BackColor = System.Drawing.Color.Transparent;
			this.lblNetworkIPv6AutoSwitch.ForeColor = System.Drawing.Color.Black;
			this.lblNetworkIPv6AutoSwitch.Location = new System.Drawing.Point(28, 183);
			this.lblNetworkIPv6AutoSwitch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNetworkIPv6AutoSwitch.Name = "lblNetworkIPv6AutoSwitch";
			this.lblNetworkIPv6AutoSwitch.Size = new System.Drawing.Size(423, 33);
			this.lblNetworkIPv6AutoSwitch.TabIndex = 109;
			this.lblNetworkIPv6AutoSwitch.Text = "Switch to \'Block\' if issue is detected:";
			this.lblNetworkIPv6AutoSwitch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkNetworkIPv6AutoSwitch
			// 
			this.chkNetworkIPv6AutoSwitch.BackColor = System.Drawing.Color.Transparent;
			this.chkNetworkIPv6AutoSwitch.ForeColor = System.Drawing.Color.Black;
			this.chkNetworkIPv6AutoSwitch.Location = new System.Drawing.Point(464, 183);
			this.chkNetworkIPv6AutoSwitch.Margin = new System.Windows.Forms.Padding(4);
			this.chkNetworkIPv6AutoSwitch.Name = "chkNetworkIPv6AutoSwitch";
			this.chkNetworkIPv6AutoSwitch.Size = new System.Drawing.Size(201, 38);
			this.chkNetworkIPv6AutoSwitch.TabIndex = 108;
			this.chkNetworkIPv6AutoSwitch.UseVisualStyleBackColor = false;
			// 
			// lblNetworkIPv6Mode
			// 
			this.lblNetworkIPv6Mode.BackColor = System.Drawing.Color.Transparent;
			this.lblNetworkIPv6Mode.ForeColor = System.Drawing.Color.Black;
			this.lblNetworkIPv6Mode.Location = new System.Drawing.Point(28, 138);
			this.lblNetworkIPv6Mode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNetworkIPv6Mode.Name = "lblNetworkIPv6Mode";
			this.lblNetworkIPv6Mode.Size = new System.Drawing.Size(422, 32);
			this.lblNetworkIPv6Mode.TabIndex = 107;
			this.lblNetworkIPv6Mode.Text = "Layer IPv6:";
			this.lblNetworkIPv6Mode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboNetworkIPv6Mode
			// 
			this.cboNetworkIPv6Mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboNetworkIPv6Mode.FormattingEnabled = true;
			this.cboNetworkIPv6Mode.Items.AddRange(new object[] {
            "None",
            "Disable"});
			this.cboNetworkIPv6Mode.Location = new System.Drawing.Point(464, 138);
			this.cboNetworkIPv6Mode.Margin = new System.Windows.Forms.Padding(4);
			this.cboNetworkIPv6Mode.Name = "cboNetworkIPv6Mode";
			this.cboNetworkIPv6Mode.Size = new System.Drawing.Size(484, 28);
			this.cboNetworkIPv6Mode.TabIndex = 106;
			// 
			// lblNetworkIPv4Mode
			// 
			this.lblNetworkIPv4Mode.BackColor = System.Drawing.Color.Transparent;
			this.lblNetworkIPv4Mode.ForeColor = System.Drawing.Color.Black;
			this.lblNetworkIPv4Mode.Location = new System.Drawing.Point(28, 26);
			this.lblNetworkIPv4Mode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNetworkIPv4Mode.Name = "lblNetworkIPv4Mode";
			this.lblNetworkIPv4Mode.Size = new System.Drawing.Size(424, 32);
			this.lblNetworkIPv4Mode.TabIndex = 105;
			this.lblNetworkIPv4Mode.Text = "Layer IPv4:";
			this.lblNetworkIPv4Mode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboNetworkIPv4Mode
			// 
			this.cboNetworkIPv4Mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboNetworkIPv4Mode.FormattingEnabled = true;
			this.cboNetworkIPv4Mode.Items.AddRange(new object[] {
            "None",
            "Disable"});
			this.cboNetworkIPv4Mode.Location = new System.Drawing.Point(464, 26);
			this.cboNetworkIPv4Mode.Margin = new System.Windows.Forms.Padding(4);
			this.cboNetworkIPv4Mode.Name = "cboNetworkIPv4Mode";
			this.cboNetworkIPv4Mode.Size = new System.Drawing.Size(484, 28);
			this.cboNetworkIPv4Mode.TabIndex = 104;
			// 
			// lblNetworkEntryInterface
			// 
			this.lblNetworkEntryInterface.BackColor = System.Drawing.Color.Transparent;
			this.lblNetworkEntryInterface.ForeColor = System.Drawing.Color.Black;
			this.lblNetworkEntryInterface.Location = new System.Drawing.Point(28, 290);
			this.lblNetworkEntryInterface.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNetworkEntryInterface.Name = "lblNetworkEntryInterface";
			this.lblNetworkEntryInterface.Size = new System.Drawing.Size(422, 32);
			this.lblNetworkEntryInterface.TabIndex = 103;
			this.lblNetworkEntryInterface.Text = "Interface used for connection:";
			this.lblNetworkEntryInterface.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboNetworkEntryInterface
			// 
			this.cboNetworkEntryInterface.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboNetworkEntryInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboNetworkEntryInterface.FormattingEnabled = true;
			this.cboNetworkEntryInterface.Items.AddRange(new object[] {
            "None",
            "Disable"});
			this.cboNetworkEntryInterface.Location = new System.Drawing.Point(464, 290);
			this.cboNetworkEntryInterface.Margin = new System.Windows.Forms.Padding(4);
			this.cboNetworkEntryInterface.Name = "cboNetworkEntryInterface";
			this.cboNetworkEntryInterface.Size = new System.Drawing.Size(484, 28);
			this.cboNetworkEntryInterface.TabIndex = 102;
			// 
			// lblOpenVpnRcvbuf
			// 
			this.lblOpenVpnRcvbuf.BackColor = System.Drawing.Color.Transparent;
			this.lblOpenVpnRcvbuf.ForeColor = System.Drawing.Color.Black;
			this.lblOpenVpnRcvbuf.Location = new System.Drawing.Point(28, 417);
			this.lblOpenVpnRcvbuf.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblOpenVpnRcvbuf.Name = "lblOpenVpnRcvbuf";
			this.lblOpenVpnRcvbuf.Size = new System.Drawing.Size(422, 32);
			this.lblOpenVpnRcvbuf.TabIndex = 99;
			this.lblOpenVpnRcvbuf.Text = "TCP/UDP receive buffer size:";
			this.lblOpenVpnRcvbuf.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboOpenVpnRcvbuf
			// 
			this.cboOpenVpnRcvbuf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboOpenVpnRcvbuf.FormattingEnabled = true;
			this.cboOpenVpnRcvbuf.Items.AddRange(new object[] {
            "Automatic",
            "Never",
            "Every minute",
            "Every ten minute",
            "Every one hour"});
			this.cboOpenVpnRcvbuf.Location = new System.Drawing.Point(462, 417);
			this.cboOpenVpnRcvbuf.Margin = new System.Windows.Forms.Padding(4);
			this.cboOpenVpnRcvbuf.Name = "cboOpenVpnRcvbuf";
			this.cboOpenVpnRcvbuf.Size = new System.Drawing.Size(198, 28);
			this.cboOpenVpnRcvbuf.TabIndex = 98;
			// 
			// lblOpenVpnSndbuf
			// 
			this.lblOpenVpnSndbuf.BackColor = System.Drawing.Color.Transparent;
			this.lblOpenVpnSndbuf.ForeColor = System.Drawing.Color.Black;
			this.lblOpenVpnSndbuf.Location = new System.Drawing.Point(28, 372);
			this.lblOpenVpnSndbuf.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblOpenVpnSndbuf.Name = "lblOpenVpnSndbuf";
			this.lblOpenVpnSndbuf.Size = new System.Drawing.Size(422, 32);
			this.lblOpenVpnSndbuf.TabIndex = 97;
			this.lblOpenVpnSndbuf.Text = "TCP/UDP send buffer size:";
			this.lblOpenVpnSndbuf.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboOpenVpnSndbuf
			// 
			this.cboOpenVpnSndbuf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboOpenVpnSndbuf.FormattingEnabled = true;
			this.cboOpenVpnSndbuf.Items.AddRange(new object[] {
            "Automatic",
            "Never",
            "Every minute",
            "Every ten minute",
            "Every one hour"});
			this.cboOpenVpnSndbuf.Location = new System.Drawing.Point(462, 372);
			this.cboOpenVpnSndbuf.Margin = new System.Windows.Forms.Padding(4);
			this.cboOpenVpnSndbuf.Name = "cboOpenVpnSndbuf";
			this.cboOpenVpnSndbuf.Size = new System.Drawing.Size(198, 28);
			this.cboOpenVpnSndbuf.TabIndex = 96;
			// 
			// lblNetworkEntryIpLayer
			// 
			this.lblNetworkEntryIpLayer.BackColor = System.Drawing.Color.Transparent;
			this.lblNetworkEntryIpLayer.ForeColor = System.Drawing.Color.Black;
			this.lblNetworkEntryIpLayer.Location = new System.Drawing.Point(28, 244);
			this.lblNetworkEntryIpLayer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNetworkEntryIpLayer.Name = "lblNetworkEntryIpLayer";
			this.lblNetworkEntryIpLayer.Size = new System.Drawing.Size(422, 32);
			this.lblNetworkEntryIpLayer.TabIndex = 95;
			this.lblNetworkEntryIpLayer.Text = "Internet Protocol used for connection:";
			this.lblNetworkEntryIpLayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboNetworkEntryIpLayer
			// 
			this.cboNetworkEntryIpLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboNetworkEntryIpLayer.FormattingEnabled = true;
			this.cboNetworkEntryIpLayer.Location = new System.Drawing.Point(464, 244);
			this.cboNetworkEntryIpLayer.Margin = new System.Windows.Forms.Padding(4);
			this.cboNetworkEntryIpLayer.Name = "cboNetworkEntryIpLayer";
			this.cboNetworkEntryIpLayer.Size = new System.Drawing.Size(198, 28);
			this.cboNetworkEntryIpLayer.TabIndex = 94;
			// 
			// mnuRoutes
			// 
			this.mnuRoutes.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.mnuRoutes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRoutesAdd,
            this.mnuRoutesRemove,
            this.mnuRoutesEdit});
			this.mnuRoutes.Name = "mnuServers";
			this.mnuRoutes.Size = new System.Drawing.Size(153, 94);
			// 
			// mnuRoutesAdd
			// 
			this.mnuRoutesAdd.BackColor = System.Drawing.SystemColors.Control;
			this.mnuRoutesAdd.ForeColor = System.Drawing.SystemColors.ControlText;
			this.mnuRoutesAdd.Image = global::Eddie.Forms.Properties.Resources.add;
			this.mnuRoutesAdd.Name = "mnuRoutesAdd";
			this.mnuRoutesAdd.Size = new System.Drawing.Size(152, 30);
			this.mnuRoutesAdd.Text = "Add";
			this.mnuRoutesAdd.Click += new System.EventHandler(this.mnuRoutesAdd_Click);
			// 
			// mnuRoutesRemove
			// 
			this.mnuRoutesRemove.Image = global::Eddie.Forms.Properties.Resources.delete;
			this.mnuRoutesRemove.Name = "mnuRoutesRemove";
			this.mnuRoutesRemove.Size = new System.Drawing.Size(152, 30);
			this.mnuRoutesRemove.Text = "Remove";
			this.mnuRoutesRemove.Click += new System.EventHandler(this.mnuRoutesRemove_Click);
			// 
			// mnuRoutesEdit
			// 
			this.mnuRoutesEdit.Image = global::Eddie.Forms.Properties.Resources.edit;
			this.mnuRoutesEdit.Name = "mnuRoutesEdit";
			this.mnuRoutesEdit.Size = new System.Drawing.Size(152, 30);
			this.mnuRoutesEdit.Text = "Edit";
			this.mnuRoutesEdit.Click += new System.EventHandler(this.mnuRoutesEdit_Click);
			// 
			// colLogDate
			// 
			this.colLogDate.Text = "Date";
			// 
			// colLogMessage
			// 
			this.colLogMessage.Text = "Message";
			this.colLogMessage.Width = 600;
			// 
			// pnlCommands
			// 
			this.pnlCommands.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.pnlCommands.BackColor = System.Drawing.Color.Transparent;
			this.pnlCommands.Controls.Add(this.cmdCancel);
			this.pnlCommands.Controls.Add(this.cmdOk);
			this.pnlCommands.Location = new System.Drawing.Point(402, 672);
			this.pnlCommands.Margin = new System.Windows.Forms.Padding(4);
			this.pnlCommands.Name = "pnlCommands";
			this.pnlCommands.Size = new System.Drawing.Size(495, 52);
			this.pnlCommands.TabIndex = 42;
			// 
			// cmdCancel
			// 
			this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdCancel.FlatAppearance.BorderSize = 0;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdCancel.Location = new System.Drawing.Point(249, 4);
			this.cmdCancel.Margin = new System.Windows.Forms.Padding(4);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(240, 45);
			this.cmdCancel.TabIndex = 41;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// cmdOk
			// 
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.Location = new System.Drawing.Point(4, 4);
			this.cmdOk.Margin = new System.Windows.Forms.Padding(4);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(240, 45);
			this.cmdOk.TabIndex = 40;
			this.cmdOk.Text = "Save";
			this.cmdOk.UseVisualStyleBackColor = true;
			this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
			// 
			// tabSettings
			// 
			this.tabSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabSettings.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabSettings.Controls.Add(this.tabGeneral);
			this.tabSettings.Controls.Add(this.tabUI);
			this.tabSettings.Controls.Add(this.tabProtocols);
			this.tabSettings.Controls.Add(this.tabProxy);
			this.tabSettings.Controls.Add(this.tabRoutes);
			this.tabSettings.Controls.Add(this.tabDNS);
			this.tabSettings.Controls.Add(this.tabNetworking);
			this.tabSettings.Controls.Add(this.tabNetworkLock);
			this.tabSettings.Controls.Add(tabAdvanced);
			this.tabSettings.Controls.Add(this.tabLogging);
			this.tabSettings.Controls.Add(tabDirectives);
			this.tabSettings.Controls.Add(tabEvents);
			this.tabSettings.Controls.Add(this.tabExperimentals);
			this.tabSettings.ItemSize = new System.Drawing.Size(80, 20);
			this.tabSettings.Location = new System.Drawing.Point(274, 0);
			this.tabSettings.Margin = new System.Windows.Forms.Padding(4);
			this.tabSettings.Name = "tabSettings";
			this.tabSettings.SelectedIndex = 0;
			this.tabSettings.Size = new System.Drawing.Size(1022, 663);
			this.tabSettings.TabIndex = 41;
			// 
			// tabGeneral
			// 
			this.tabGeneral.BackColor = System.Drawing.Color.White;
			this.tabGeneral.Controls.Add(this.txtStoragePasswordConfirm);
			this.tabGeneral.Controls.Add(this.lblStoragePasswordConfirm);
			this.tabGeneral.Controls.Add(this.lblStoragePassword);
			this.tabGeneral.Controls.Add(this.lblSystemService);
			this.tabGeneral.Controls.Add(this.chkSystemService);
			this.tabGeneral.Controls.Add(this.lblStorageBrowser);
			this.tabGeneral.Controls.Add(this.chkStorageBrowser);
			this.tabGeneral.Controls.Add(this.txtStoragePassword);
			this.tabGeneral.Controls.Add(this.lblStorageMode);
			this.tabGeneral.Controls.Add(this.cboStorageMode);
			this.tabGeneral.Controls.Add(this.chkSystemStart);
			this.tabGeneral.Controls.Add(this.label13);
			this.tabGeneral.Controls.Add(this.chkOsSingleInstance);
			this.tabGeneral.Controls.Add(this.cmdResetToDefault);
			this.tabGeneral.Controls.Add(this.lblConnect);
			this.tabGeneral.Controls.Add(this.lblNetLock);
			this.tabGeneral.Controls.Add(this.chkConnect);
			this.tabGeneral.Controls.Add(this.lblGeneralStartLast);
			this.tabGeneral.Controls.Add(this.chkNetLock);
			this.tabGeneral.Controls.Add(this.chkGeneralStartLast);
			this.tabGeneral.Location = new System.Drawing.Point(4, 24);
			this.tabGeneral.Margin = new System.Windows.Forms.Padding(4);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Size = new System.Drawing.Size(1014, 635);
			this.tabGeneral.TabIndex = 0;
			this.tabGeneral.Text = "General";
			// 
			// txtStoragePasswordConfirm
			// 
			this.txtStoragePasswordConfirm.Location = new System.Drawing.Point(597, 370);
			this.txtStoragePasswordConfirm.Margin = new System.Windows.Forms.Padding(4);
			this.txtStoragePasswordConfirm.Name = "txtStoragePasswordConfirm";
			this.txtStoragePasswordConfirm.Size = new System.Drawing.Size(378, 26);
			this.txtStoragePasswordConfirm.TabIndex = 112;
			this.txtStoragePasswordConfirm.UseSystemPasswordChar = true;
			// 
			// lblStoragePasswordConfirm
			// 
			this.lblStoragePasswordConfirm.BackColor = System.Drawing.Color.Transparent;
			this.lblStoragePasswordConfirm.ForeColor = System.Drawing.Color.Black;
			this.lblStoragePasswordConfirm.Location = new System.Drawing.Point(428, 366);
			this.lblStoragePasswordConfirm.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStoragePasswordConfirm.Name = "lblStoragePasswordConfirm";
			this.lblStoragePasswordConfirm.Size = new System.Drawing.Size(161, 34);
			this.lblStoragePasswordConfirm.TabIndex = 111;
			this.lblStoragePasswordConfirm.Text = "Confirm:";
			this.lblStoragePasswordConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblStoragePassword
			// 
			this.lblStoragePassword.BackColor = System.Drawing.Color.Transparent;
			this.lblStoragePassword.ForeColor = System.Drawing.Color.Black;
			this.lblStoragePassword.Location = new System.Drawing.Point(428, 321);
			this.lblStoragePassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStoragePassword.Name = "lblStoragePassword";
			this.lblStoragePassword.Size = new System.Drawing.Size(161, 34);
			this.lblStoragePassword.TabIndex = 110;
			this.lblStoragePassword.Text = "Password:";
			this.lblStoragePassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblSystemService
			// 
			this.lblSystemService.BackColor = System.Drawing.Color.Transparent;
			this.lblSystemService.ForeColor = System.Drawing.Color.Black;
			this.lblSystemService.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.lblSystemService.Location = new System.Drawing.Point(587, 133);
			this.lblSystemService.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSystemService.Name = "lblSystemService";
			this.lblSystemService.Size = new System.Drawing.Size(388, 89);
			this.lblSystemService.TabIndex = 109;
			this.lblSystemService.Text = "Type:";
			this.lblSystemService.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkSystemService
			// 
			this.chkSystemService.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkSystemService.BackColor = System.Drawing.Color.Transparent;
			this.chkSystemService.ForeColor = System.Drawing.Color.Black;
			this.chkSystemService.Location = new System.Drawing.Point(591, 87);
			this.chkSystemService.Margin = new System.Windows.Forms.Padding(4);
			this.chkSystemService.Name = "chkSystemService";
			this.chkSystemService.Size = new System.Drawing.Size(384, 46);
			this.chkSystemService.TabIndex = 108;
			this.chkSystemService.Text = "Don\'t ask elevation every run";
			this.chkSystemService.UseVisualStyleBackColor = false;
			// 
			// lblStorageBrowser
			// 
			this.lblStorageBrowser.BackColor = System.Drawing.Color.Transparent;
			this.lblStorageBrowser.ForeColor = System.Drawing.Color.Black;
			this.lblStorageBrowser.Location = new System.Drawing.Point(26, 450);
			this.lblStorageBrowser.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStorageBrowser.Name = "lblStorageBrowser";
			this.lblStorageBrowser.Size = new System.Drawing.Size(398, 34);
			this.lblStorageBrowser.TabIndex = 107;
			this.lblStorageBrowser.Text = "Show profiles at startup:";
			this.lblStorageBrowser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblStorageBrowser.Visible = false;
			// 
			// chkStorageBrowser
			// 
			this.chkStorageBrowser.BackColor = System.Drawing.Color.Transparent;
			this.chkStorageBrowser.ForeColor = System.Drawing.Color.Black;
			this.chkStorageBrowser.Location = new System.Drawing.Point(432, 450);
			this.chkStorageBrowser.Margin = new System.Windows.Forms.Padding(4);
			this.chkStorageBrowser.Name = "chkStorageBrowser";
			this.chkStorageBrowser.Size = new System.Drawing.Size(147, 34);
			this.chkStorageBrowser.TabIndex = 106;
			this.chkStorageBrowser.UseVisualStyleBackColor = false;
			this.chkStorageBrowser.Visible = false;
			// 
			// txtStoragePassword
			// 
			this.txtStoragePassword.Location = new System.Drawing.Point(597, 325);
			this.txtStoragePassword.Margin = new System.Windows.Forms.Padding(4);
			this.txtStoragePassword.Name = "txtStoragePassword";
			this.txtStoragePassword.Size = new System.Drawing.Size(378, 26);
			this.txtStoragePassword.TabIndex = 105;
			this.txtStoragePassword.UseSystemPasswordChar = true;
			// 
			// lblStorageMode
			// 
			this.lblStorageMode.BackColor = System.Drawing.Color.Transparent;
			this.lblStorageMode.ForeColor = System.Drawing.Color.Black;
			this.lblStorageMode.Location = new System.Drawing.Point(26, 277);
			this.lblStorageMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStorageMode.Name = "lblStorageMode";
			this.lblStorageMode.Size = new System.Drawing.Size(398, 34);
			this.lblStorageMode.TabIndex = 104;
			this.lblStorageMode.Text = "Profile data protection:";
			this.lblStorageMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboStorageMode
			// 
			this.cboStorageMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboStorageMode.FormattingEnabled = true;
			this.cboStorageMode.Location = new System.Drawing.Point(432, 281);
			this.cboStorageMode.Margin = new System.Windows.Forms.Padding(4);
			this.cboStorageMode.Name = "cboStorageMode";
			this.cboStorageMode.Size = new System.Drawing.Size(421, 28);
			this.cboStorageMode.TabIndex = 103;
			this.cboStorageMode.SelectedIndexChanged += new System.EventHandler(this.cboStorageMode_SelectedIndexChanged);
			// 
			// chkSystemStart
			// 
			this.chkSystemStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkSystemStart.BackColor = System.Drawing.Color.Transparent;
			this.chkSystemStart.ForeColor = System.Drawing.Color.Black;
			this.chkSystemStart.Location = new System.Drawing.Point(591, 33);
			this.chkSystemStart.Margin = new System.Windows.Forms.Padding(4);
			this.chkSystemStart.Name = "chkSystemStart";
			this.chkSystemStart.Size = new System.Drawing.Size(384, 46);
			this.chkSystemStart.TabIndex = 31;
			this.chkSystemStart.Text = "Start with System";
			this.chkSystemStart.UseVisualStyleBackColor = false;
			// 
			// label13
			// 
			this.label13.BackColor = System.Drawing.Color.Transparent;
			this.label13.ForeColor = System.Drawing.Color.Black;
			this.label13.Location = new System.Drawing.Point(26, 162);
			this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(398, 34);
			this.label13.TabIndex = 90;
			this.label13.Text = "Single instance:";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkOsSingleInstance
			// 
			this.chkOsSingleInstance.BackColor = System.Drawing.Color.Transparent;
			this.chkOsSingleInstance.ForeColor = System.Drawing.Color.Black;
			this.chkOsSingleInstance.Location = new System.Drawing.Point(432, 162);
			this.chkOsSingleInstance.Margin = new System.Windows.Forms.Padding(4);
			this.chkOsSingleInstance.Name = "chkOsSingleInstance";
			this.chkOsSingleInstance.Size = new System.Drawing.Size(147, 34);
			this.chkOsSingleInstance.TabIndex = 41;
			this.chkOsSingleInstance.UseVisualStyleBackColor = false;
			// 
			// cmdResetToDefault
			// 
			this.cmdResetToDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cmdResetToDefault.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdResetToDefault.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdResetToDefault.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdResetToDefault.FlatAppearance.BorderSize = 0;
			this.cmdResetToDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdResetToDefault.Location = new System.Drawing.Point(30, 549);
			this.cmdResetToDefault.Margin = new System.Windows.Forms.Padding(4);
			this.cmdResetToDefault.Name = "cmdResetToDefault";
			this.cmdResetToDefault.Size = new System.Drawing.Size(321, 45);
			this.cmdResetToDefault.TabIndex = 88;
			this.cmdResetToDefault.Text = "Reset to default settings";
			this.cmdResetToDefault.UseVisualStyleBackColor = true;
			this.cmdResetToDefault.Click += new System.EventHandler(this.cmdResetToDefault_Click);
			// 
			// lblConnect
			// 
			this.lblConnect.BackColor = System.Drawing.Color.Transparent;
			this.lblConnect.ForeColor = System.Drawing.Color.Black;
			this.lblConnect.Location = new System.Drawing.Point(26, 27);
			this.lblConnect.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnect.Name = "lblConnect";
			this.lblConnect.Size = new System.Drawing.Size(398, 34);
			this.lblConnect.TabIndex = 85;
			this.lblConnect.Text = "Connect at startup:";
			this.lblConnect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblNetLock
			// 
			this.lblNetLock.BackColor = System.Drawing.Color.Transparent;
			this.lblNetLock.ForeColor = System.Drawing.Color.Black;
			this.lblNetLock.Location = new System.Drawing.Point(26, 117);
			this.lblNetLock.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNetLock.Name = "lblNetLock";
			this.lblNetLock.Size = new System.Drawing.Size(398, 34);
			this.lblNetLock.TabIndex = 85;
			this.lblNetLock.Text = "Activate Network Lock at startup:";
			this.lblNetLock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkConnect
			// 
			this.chkConnect.BackColor = System.Drawing.Color.Transparent;
			this.chkConnect.ForeColor = System.Drawing.Color.Black;
			this.chkConnect.Location = new System.Drawing.Point(432, 27);
			this.chkConnect.Margin = new System.Windows.Forms.Padding(4);
			this.chkConnect.Name = "chkConnect";
			this.chkConnect.Size = new System.Drawing.Size(200, 34);
			this.chkConnect.TabIndex = 84;
			this.chkConnect.UseVisualStyleBackColor = false;
			// 
			// lblGeneralStartLast
			// 
			this.lblGeneralStartLast.BackColor = System.Drawing.Color.Transparent;
			this.lblGeneralStartLast.ForeColor = System.Drawing.Color.Black;
			this.lblGeneralStartLast.Location = new System.Drawing.Point(26, 72);
			this.lblGeneralStartLast.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblGeneralStartLast.Name = "lblGeneralStartLast";
			this.lblGeneralStartLast.Size = new System.Drawing.Size(398, 34);
			this.lblGeneralStartLast.TabIndex = 87;
			this.lblGeneralStartLast.Text = "Reconnect to last server at start:";
			this.lblGeneralStartLast.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkNetLock
			// 
			this.chkNetLock.BackColor = System.Drawing.Color.Transparent;
			this.chkNetLock.ForeColor = System.Drawing.Color.Black;
			this.chkNetLock.Location = new System.Drawing.Point(432, 117);
			this.chkNetLock.Margin = new System.Windows.Forms.Padding(4);
			this.chkNetLock.Name = "chkNetLock";
			this.chkNetLock.Size = new System.Drawing.Size(200, 34);
			this.chkNetLock.TabIndex = 84;
			this.chkNetLock.UseVisualStyleBackColor = false;
			// 
			// chkGeneralStartLast
			// 
			this.chkGeneralStartLast.BackColor = System.Drawing.Color.Transparent;
			this.chkGeneralStartLast.ForeColor = System.Drawing.Color.Black;
			this.chkGeneralStartLast.Location = new System.Drawing.Point(432, 72);
			this.chkGeneralStartLast.Margin = new System.Windows.Forms.Padding(4);
			this.chkGeneralStartLast.Name = "chkGeneralStartLast";
			this.chkGeneralStartLast.Size = new System.Drawing.Size(42, 34);
			this.chkGeneralStartLast.TabIndex = 86;
			this.chkGeneralStartLast.UseVisualStyleBackColor = false;
			// 
			// tabUI
			// 
			this.tabUI.BackColor = System.Drawing.Color.White;
			this.tabUI.Controls.Add(this.lbUiSkipPromotional);
			this.tabUI.Controls.Add(this.chkUiSkipPromotional);
			this.tabUI.Controls.Add(this.lblUiStartMinimized);
			this.tabUI.Controls.Add(this.chkUiStartMinimized);
			this.tabUI.Controls.Add(this.lblUiTrayMinimized);
			this.tabUI.Controls.Add(this.chkUiTrayMinimized);
			this.tabUI.Controls.Add(this.lblUiTrayShow);
			this.tabUI.Controls.Add(this.chkUiTrayShow);
			this.tabUI.Controls.Add(this.lblUiSystemNotifications);
			this.tabUI.Controls.Add(this.chkUiIEC);
			this.tabUI.Controls.Add(this.chkUiSystemNotifications);
			this.tabUI.Controls.Add(this.chkUiFontGeneralTitle);
			this.tabUI.Controls.Add(this.chkUiFontGeneral);
			this.tabUI.Controls.Add(this.lblUiExitConfirm);
			this.tabUI.Controls.Add(this.chkUiExitConfirm);
			this.tabUI.Controls.Add(this.lblUiUnit);
			this.tabUI.Controls.Add(this.cboUiUnit);
			this.tabUI.Controls.Add(this.cmdUiFontGeneral);
			this.tabUI.Controls.Add(this.lblUiFontGeneral);
			this.tabUI.Controls.Add(this.lblUiSkipProviderManifestFailed);
			this.tabUI.Controls.Add(this.chkUiSkipProviderManifestFailed);
			this.tabUI.Location = new System.Drawing.Point(4, 24);
			this.tabUI.Margin = new System.Windows.Forms.Padding(4);
			this.tabUI.Name = "tabUI";
			this.tabUI.Size = new System.Drawing.Size(1014, 635);
			this.tabUI.TabIndex = 3;
			this.tabUI.Text = "UI";
			// 
			// lbUiSkipPromotional
			// 
			this.lbUiSkipPromotional.BackColor = System.Drawing.Color.Transparent;
			this.lbUiSkipPromotional.ForeColor = System.Drawing.Color.Black;
			this.lbUiSkipPromotional.Location = new System.Drawing.Point(26, 575);
			this.lbUiSkipPromotional.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lbUiSkipPromotional.Name = "lbUiSkipPromotional";
			this.lbUiSkipPromotional.Size = new System.Drawing.Size(398, 34);
			this.lbUiSkipPromotional.TabIndex = 116;
			this.lbUiSkipPromotional.Text = "Skip promotional messages";
			this.lbUiSkipPromotional.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkUiSkipPromotional
			// 
			this.chkUiSkipPromotional.BackColor = System.Drawing.Color.Transparent;
			this.chkUiSkipPromotional.ForeColor = System.Drawing.Color.Black;
			this.chkUiSkipPromotional.Location = new System.Drawing.Point(432, 575);
			this.chkUiSkipPromotional.Margin = new System.Windows.Forms.Padding(4);
			this.chkUiSkipPromotional.Name = "chkUiSkipPromotional";
			this.chkUiSkipPromotional.Size = new System.Drawing.Size(147, 34);
			this.chkUiSkipPromotional.TabIndex = 115;
			this.chkUiSkipPromotional.UseVisualStyleBackColor = false;
			// 
			// lblUiStartMinimized
			// 
			this.lblUiStartMinimized.BackColor = System.Drawing.Color.Transparent;
			this.lblUiStartMinimized.ForeColor = System.Drawing.Color.Black;
			this.lblUiStartMinimized.Location = new System.Drawing.Point(26, 252);
			this.lblUiStartMinimized.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblUiStartMinimized.Name = "lblUiStartMinimized";
			this.lblUiStartMinimized.Size = new System.Drawing.Size(398, 34);
			this.lblUiStartMinimized.TabIndex = 113;
			this.lblUiStartMinimized.Text = "Start minimized:";
			this.lblUiStartMinimized.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkUiStartMinimized
			// 
			this.chkUiStartMinimized.BackColor = System.Drawing.Color.Transparent;
			this.chkUiStartMinimized.ForeColor = System.Drawing.Color.Black;
			this.chkUiStartMinimized.Location = new System.Drawing.Point(432, 252);
			this.chkUiStartMinimized.Margin = new System.Windows.Forms.Padding(4);
			this.chkUiStartMinimized.Name = "chkUiStartMinimized";
			this.chkUiStartMinimized.Size = new System.Drawing.Size(42, 34);
			this.chkUiStartMinimized.TabIndex = 112;
			this.chkUiStartMinimized.UseVisualStyleBackColor = false;
			// 
			// lblUiTrayMinimized
			// 
			this.lblUiTrayMinimized.BackColor = System.Drawing.Color.Transparent;
			this.lblUiTrayMinimized.ForeColor = System.Drawing.Color.Black;
			this.lblUiTrayMinimized.Location = new System.Drawing.Point(483, 207);
			this.lblUiTrayMinimized.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblUiTrayMinimized.Name = "lblUiTrayMinimized";
			this.lblUiTrayMinimized.Size = new System.Drawing.Size(264, 34);
			this.lblUiTrayMinimized.TabIndex = 111;
			this.lblUiTrayMinimized.Text = "Minimize in Tray:";
			this.lblUiTrayMinimized.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkUiTrayMinimized
			// 
			this.chkUiTrayMinimized.BackColor = System.Drawing.Color.Transparent;
			this.chkUiTrayMinimized.ForeColor = System.Drawing.Color.Black;
			this.chkUiTrayMinimized.Location = new System.Drawing.Point(756, 208);
			this.chkUiTrayMinimized.Margin = new System.Windows.Forms.Padding(4);
			this.chkUiTrayMinimized.Name = "chkUiTrayMinimized";
			this.chkUiTrayMinimized.Size = new System.Drawing.Size(42, 34);
			this.chkUiTrayMinimized.TabIndex = 110;
			this.chkUiTrayMinimized.UseVisualStyleBackColor = false;
			// 
			// lblUiTrayShow
			// 
			this.lblUiTrayShow.BackColor = System.Drawing.Color.Transparent;
			this.lblUiTrayShow.ForeColor = System.Drawing.Color.Black;
			this.lblUiTrayShow.Location = new System.Drawing.Point(26, 207);
			this.lblUiTrayShow.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblUiTrayShow.Name = "lblUiTrayShow";
			this.lblUiTrayShow.Size = new System.Drawing.Size(398, 34);
			this.lblUiTrayShow.TabIndex = 109;
			this.lblUiTrayShow.Text = "Show Tray Icon:";
			this.lblUiTrayShow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkUiTrayShow
			// 
			this.chkUiTrayShow.BackColor = System.Drawing.Color.Transparent;
			this.chkUiTrayShow.ForeColor = System.Drawing.Color.Black;
			this.chkUiTrayShow.Location = new System.Drawing.Point(432, 207);
			this.chkUiTrayShow.Margin = new System.Windows.Forms.Padding(4);
			this.chkUiTrayShow.Name = "chkUiTrayShow";
			this.chkUiTrayShow.Size = new System.Drawing.Size(42, 34);
			this.chkUiTrayShow.TabIndex = 108;
			this.chkUiTrayShow.UseVisualStyleBackColor = false;
			// 
			// lblUiSystemNotifications
			// 
			this.lblUiSystemNotifications.BackColor = System.Drawing.Color.Transparent;
			this.lblUiSystemNotifications.ForeColor = System.Drawing.Color.Black;
			this.lblUiSystemNotifications.Location = new System.Drawing.Point(26, 27);
			this.lblUiSystemNotifications.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblUiSystemNotifications.Name = "lblUiSystemNotifications";
			this.lblUiSystemNotifications.Size = new System.Drawing.Size(398, 34);
			this.lblUiSystemNotifications.TabIndex = 107;
			this.lblUiSystemNotifications.Text = "System notifications:";
			this.lblUiSystemNotifications.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkUiIEC
			// 
			this.chkUiIEC.AutoSize = true;
			this.chkUiIEC.BackColor = System.Drawing.Color.Transparent;
			this.chkUiIEC.ForeColor = System.Drawing.Color.Black;
			this.chkUiIEC.Location = new System.Drawing.Point(843, 78);
			this.chkUiIEC.Margin = new System.Windows.Forms.Padding(4);
			this.chkUiIEC.Name = "chkUiIEC";
			this.chkUiIEC.Size = new System.Drawing.Size(62, 24);
			this.chkUiIEC.TabIndex = 98;
			this.chkUiIEC.Text = "IEC";
			this.chkUiIEC.UseVisualStyleBackColor = false;
			// 
			// chkUiSystemNotifications
			// 
			this.chkUiSystemNotifications.BackColor = System.Drawing.Color.Transparent;
			this.chkUiSystemNotifications.ForeColor = System.Drawing.Color.Black;
			this.chkUiSystemNotifications.Location = new System.Drawing.Point(432, 27);
			this.chkUiSystemNotifications.Margin = new System.Windows.Forms.Padding(4);
			this.chkUiSystemNotifications.Name = "chkUiSystemNotifications";
			this.chkUiSystemNotifications.Size = new System.Drawing.Size(182, 34);
			this.chkUiSystemNotifications.TabIndex = 97;
			this.chkUiSystemNotifications.UseVisualStyleBackColor = false;
			// 
			// chkUiFontGeneralTitle
			// 
			this.chkUiFontGeneralTitle.BackColor = System.Drawing.Color.Transparent;
			this.chkUiFontGeneralTitle.ForeColor = System.Drawing.Color.Black;
			this.chkUiFontGeneralTitle.Location = new System.Drawing.Point(26, 162);
			this.chkUiFontGeneralTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.chkUiFontGeneralTitle.Name = "chkUiFontGeneralTitle";
			this.chkUiFontGeneralTitle.Size = new System.Drawing.Size(398, 34);
			this.chkUiFontGeneralTitle.TabIndex = 106;
			this.chkUiFontGeneralTitle.Text = "UI Main Font:";
			this.chkUiFontGeneralTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkUiFontGeneral
			// 
			this.chkUiFontGeneral.BackColor = System.Drawing.Color.Transparent;
			this.chkUiFontGeneral.ForeColor = System.Drawing.Color.Black;
			this.chkUiFontGeneral.Location = new System.Drawing.Point(432, 162);
			this.chkUiFontGeneral.Margin = new System.Windows.Forms.Padding(4);
			this.chkUiFontGeneral.Name = "chkUiFontGeneral";
			this.chkUiFontGeneral.Size = new System.Drawing.Size(42, 34);
			this.chkUiFontGeneral.TabIndex = 105;
			this.chkUiFontGeneral.UseVisualStyleBackColor = false;
			this.chkUiFontGeneral.CheckedChanged += new System.EventHandler(this.chkUiFontGeneral_CheckedChanged_1);
			// 
			// lblUiExitConfirm
			// 
			this.lblUiExitConfirm.BackColor = System.Drawing.Color.Transparent;
			this.lblUiExitConfirm.ForeColor = System.Drawing.Color.Black;
			this.lblUiExitConfirm.Location = new System.Drawing.Point(26, 117);
			this.lblUiExitConfirm.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblUiExitConfirm.Name = "lblUiExitConfirm";
			this.lblUiExitConfirm.Size = new System.Drawing.Size(398, 34);
			this.lblUiExitConfirm.TabIndex = 104;
			this.lblUiExitConfirm.Text = "Exit confirmation prompt:";
			this.lblUiExitConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkUiExitConfirm
			// 
			this.chkUiExitConfirm.BackColor = System.Drawing.Color.Transparent;
			this.chkUiExitConfirm.ForeColor = System.Drawing.Color.Black;
			this.chkUiExitConfirm.Location = new System.Drawing.Point(432, 117);
			this.chkUiExitConfirm.Margin = new System.Windows.Forms.Padding(4);
			this.chkUiExitConfirm.Name = "chkUiExitConfirm";
			this.chkUiExitConfirm.Size = new System.Drawing.Size(200, 34);
			this.chkUiExitConfirm.TabIndex = 103;
			this.chkUiExitConfirm.UseVisualStyleBackColor = false;
			// 
			// lblUiUnit
			// 
			this.lblUiUnit.BackColor = System.Drawing.Color.Transparent;
			this.lblUiUnit.ForeColor = System.Drawing.Color.Black;
			this.lblUiUnit.Location = new System.Drawing.Point(26, 72);
			this.lblUiUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblUiUnit.Name = "lblUiUnit";
			this.lblUiUnit.Size = new System.Drawing.Size(398, 34);
			this.lblUiUnit.TabIndex = 102;
			this.lblUiUnit.Text = "Data units:";
			this.lblUiUnit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboUiUnit
			// 
			this.cboUiUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboUiUnit.FormattingEnabled = true;
			this.cboUiUnit.Items.AddRange(new object[] {
            "Disabled",
            "Automatic",
            "Resolvconf (Linux only)",
            "Renaming (Linux only)"});
			this.cboUiUnit.Location = new System.Drawing.Point(432, 74);
			this.cboUiUnit.Margin = new System.Windows.Forms.Padding(4);
			this.cboUiUnit.Name = "cboUiUnit";
			this.cboUiUnit.Size = new System.Drawing.Size(400, 28);
			this.cboUiUnit.TabIndex = 101;
			// 
			// cmdUiFontGeneral
			// 
			this.cmdUiFontGeneral.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdUiFontGeneral.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdUiFontGeneral.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdUiFontGeneral.FlatAppearance.BorderSize = 0;
			this.cmdUiFontGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdUiFontGeneral.Location = new System.Drawing.Point(843, 162);
			this.cmdUiFontGeneral.Margin = new System.Windows.Forms.Padding(4);
			this.cmdUiFontGeneral.Name = "cmdUiFontGeneral";
			this.cmdUiFontGeneral.Size = new System.Drawing.Size(51, 34);
			this.cmdUiFontGeneral.TabIndex = 100;
			this.cmdUiFontGeneral.Text = "...";
			this.cmdUiFontGeneral.UseVisualStyleBackColor = true;
			this.cmdUiFontGeneral.Click += new System.EventHandler(this.cmdUiFontGeneral_Click);
			// 
			// lblUiFontGeneral
			// 
			this.lblUiFontGeneral.BackColor = System.Drawing.Color.Transparent;
			this.lblUiFontGeneral.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblUiFontGeneral.ForeColor = System.Drawing.Color.Black;
			this.lblUiFontGeneral.Location = new System.Drawing.Point(483, 162);
			this.lblUiFontGeneral.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblUiFontGeneral.Name = "lblUiFontGeneral";
			this.lblUiFontGeneral.Size = new System.Drawing.Size(350, 34);
			this.lblUiFontGeneral.TabIndex = 99;
			this.lblUiFontGeneral.Text = "Default";
			this.lblUiFontGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblUiSkipProviderManifestFailed
			// 
			this.lblUiSkipProviderManifestFailed.BackColor = System.Drawing.Color.Transparent;
			this.lblUiSkipProviderManifestFailed.ForeColor = System.Drawing.Color.Black;
			this.lblUiSkipProviderManifestFailed.Location = new System.Drawing.Point(26, 297);
			this.lblUiSkipProviderManifestFailed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblUiSkipProviderManifestFailed.Name = "lblUiSkipProviderManifestFailed";
			this.lblUiSkipProviderManifestFailed.Size = new System.Drawing.Size(398, 34);
			this.lblUiSkipProviderManifestFailed.TabIndex = 87;
			this.lblUiSkipProviderManifestFailed.Text = "Hide bootstrap failure window:";
			this.lblUiSkipProviderManifestFailed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkUiSkipProviderManifestFailed
			// 
			this.chkUiSkipProviderManifestFailed.BackColor = System.Drawing.Color.Transparent;
			this.chkUiSkipProviderManifestFailed.ForeColor = System.Drawing.Color.Black;
			this.chkUiSkipProviderManifestFailed.Location = new System.Drawing.Point(432, 297);
			this.chkUiSkipProviderManifestFailed.Margin = new System.Windows.Forms.Padding(4);
			this.chkUiSkipProviderManifestFailed.Name = "chkUiSkipProviderManifestFailed";
			this.chkUiSkipProviderManifestFailed.Size = new System.Drawing.Size(42, 34);
			this.chkUiSkipProviderManifestFailed.TabIndex = 86;
			this.chkUiSkipProviderManifestFailed.UseVisualStyleBackColor = false;
			// 
			// tabProtocols
			// 
			this.tabProtocols.BackColor = System.Drawing.Color.White;
			this.tabProtocols.Controls.Add(this.lblProtocolsAvailable);
			this.tabProtocols.Controls.Add(this.lnkProtocolsHelp2);
			this.tabProtocols.Controls.Add(this.lnkProtocolsHelp1);
			this.tabProtocols.Controls.Add(this.chkProtocolsAutomatic);
			this.tabProtocols.Controls.Add(this.lstProtocols);
			this.tabProtocols.Location = new System.Drawing.Point(4, 24);
			this.tabProtocols.Margin = new System.Windows.Forms.Padding(4);
			this.tabProtocols.Name = "tabProtocols";
			this.tabProtocols.Size = new System.Drawing.Size(1014, 635);
			this.tabProtocols.TabIndex = 3;
			this.tabProtocols.Text = "Protocols";
			// 
			// lblProtocolsAvailable
			// 
			this.lblProtocolsAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblProtocolsAvailable.BackColor = System.Drawing.Color.Transparent;
			this.lblProtocolsAvailable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.lblProtocolsAvailable.Location = new System.Drawing.Point(336, 24);
			this.lblProtocolsAvailable.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProtocolsAvailable.Name = "lblProtocolsAvailable";
			this.lblProtocolsAvailable.Size = new System.Drawing.Size(658, 30);
			this.lblProtocolsAvailable.TabIndex = 73;
			this.lblProtocolsAvailable.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lnkProtocolsHelp2
			// 
			this.lnkProtocolsHelp2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkProtocolsHelp2.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkProtocolsHelp2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkProtocolsHelp2.Location = new System.Drawing.Point(20, 572);
			this.lnkProtocolsHelp2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lnkProtocolsHelp2.Name = "lnkProtocolsHelp2";
			this.lnkProtocolsHelp2.Size = new System.Drawing.Size(975, 33);
			this.lnkProtocolsHelp2.TabIndex = 72;
			this.lnkProtocolsHelp2.TabStop = true;
			this.lnkProtocolsHelp2.Text = "UDP vs TCP?";
			this.lnkProtocolsHelp2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lnkProtocolsHelp2.Click += new System.EventHandler(this.lnkProtocolsHelp2_LinkClicked);
			// 
			// lnkProtocolsHelp1
			// 
			this.lnkProtocolsHelp1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkProtocolsHelp1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkProtocolsHelp1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkProtocolsHelp1.Location = new System.Drawing.Point(20, 538);
			this.lnkProtocolsHelp1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lnkProtocolsHelp1.Name = "lnkProtocolsHelp1";
			this.lnkProtocolsHelp1.Size = new System.Drawing.Size(975, 33);
			this.lnkProtocolsHelp1.TabIndex = 71;
			this.lnkProtocolsHelp1.TabStop = true;
			this.lnkProtocolsHelp1.Text = "What is the difference between protocols?";
			this.lnkProtocolsHelp1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lnkProtocolsHelp1.Click += new System.EventHandler(this.lnkProtocolsHelp1_LinkClicked);
			// 
			// chkProtocolsAutomatic
			// 
			this.chkProtocolsAutomatic.ForeColor = System.Drawing.Color.Black;
			this.chkProtocolsAutomatic.Location = new System.Drawing.Point(20, 21);
			this.chkProtocolsAutomatic.Margin = new System.Windows.Forms.Padding(4);
			this.chkProtocolsAutomatic.Name = "chkProtocolsAutomatic";
			this.chkProtocolsAutomatic.Size = new System.Drawing.Size(975, 33);
			this.chkProtocolsAutomatic.TabIndex = 70;
			this.chkProtocolsAutomatic.Text = "Automatic";
			this.chkProtocolsAutomatic.UseVisualStyleBackColor = true;
			this.chkProtocolsAutomatic.CheckedChanged += new System.EventHandler(this.chkProtocolsAutomatic_CheckedChanged);
			// 
			// lstProtocols
			// 
			this.lstProtocols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstProtocols.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colProtocolsProtocol,
            this.colProtocolsPort,
            this.colProtocolsEntry,
            this.colProtocolsDescription,
            this.colProtocolsTech});
			this.lstProtocols.FullRowSelect = true;
			this.lstProtocols.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstProtocols.HideSelection = false;
			this.lstProtocols.Location = new System.Drawing.Point(20, 57);
			this.lstProtocols.Margin = new System.Windows.Forms.Padding(4);
			this.lstProtocols.MultiSelect = false;
			this.lstProtocols.Name = "lstProtocols";
			this.lstProtocols.OwnerDraw = true;
			this.lstProtocols.Size = new System.Drawing.Size(973, 475);
			this.lstProtocols.TabIndex = 69;
			this.lstProtocols.UseCompatibleStateImageBehavior = false;
			this.lstProtocols.View = System.Windows.Forms.View.Details;
			// 
			// colProtocolsProtocol
			// 
			this.colProtocolsProtocol.Text = "Protocol";
			// 
			// colProtocolsPort
			// 
			this.colProtocolsPort.Text = "Port";
			// 
			// colProtocolsEntry
			// 
			this.colProtocolsEntry.Text = "IP";
			// 
			// colProtocolsDescription
			// 
			this.colProtocolsDescription.Text = "Description";
			this.colProtocolsDescription.Width = 200;
			// 
			// colProtocolsTech
			// 
			this.colProtocolsTech.Text = "Specs";
			// 
			// tabProxy
			// 
			this.tabProxy.BackColor = System.Drawing.Color.White;
			this.tabProxy.Controls.Add(this.lblProxyWhen);
			this.tabProxy.Controls.Add(this.cboProxyWhen);
			this.tabProxy.Controls.Add(this.lnkProxyTorHelp);
			this.tabProxy.Controls.Add(this.txtProxyTorControlPassword);
			this.tabProxy.Controls.Add(this.lblProxyTorControlPassword);
			this.tabProxy.Controls.Add(this.cmdProxyTorTest);
			this.tabProxy.Controls.Add(this.txtProxyTorControlPort);
			this.tabProxy.Controls.Add(this.lblProxyTorControlPort);
			this.tabProxy.Controls.Add(this.label17);
			this.tabProxy.Controls.Add(this.label12);
			this.tabProxy.Controls.Add(this.lblProxyAuthentication);
			this.tabProxy.Controls.Add(this.cboProxyAuthentication);
			this.tabProxy.Controls.Add(this.txtProxyPassword);
			this.tabProxy.Controls.Add(this.lblProxyPassword);
			this.tabProxy.Controls.Add(this.txtProxyLogin);
			this.tabProxy.Controls.Add(this.lblProxyLogin);
			this.tabProxy.Controls.Add(this.lblProxyType);
			this.tabProxy.Controls.Add(this.cboProxyMode);
			this.tabProxy.Controls.Add(this.txtProxyPort);
			this.tabProxy.Controls.Add(this.lblProxyPort);
			this.tabProxy.Controls.Add(this.txtProxyHost);
			this.tabProxy.Controls.Add(this.lblProxyHost);
			this.tabProxy.Location = new System.Drawing.Point(4, 24);
			this.tabProxy.Margin = new System.Windows.Forms.Padding(4);
			this.tabProxy.Name = "tabProxy";
			this.tabProxy.Padding = new System.Windows.Forms.Padding(4);
			this.tabProxy.Size = new System.Drawing.Size(1014, 635);
			this.tabProxy.TabIndex = 1;
			this.tabProxy.Text = "Proxy / Tor";
			// 
			// lblProxyWhen
			// 
			this.lblProxyWhen.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyWhen.ForeColor = System.Drawing.Color.Black;
			this.lblProxyWhen.Location = new System.Drawing.Point(21, 68);
			this.lblProxyWhen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProxyWhen.Name = "lblProxyWhen";
			this.lblProxyWhen.Size = new System.Drawing.Size(270, 32);
			this.lblProxyWhen.TabIndex = 76;
			this.lblProxyWhen.Text = "When:";
			this.lblProxyWhen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboProxyWhen
			// 
			this.cboProxyWhen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboProxyWhen.FormattingEnabled = true;
			this.cboProxyWhen.Location = new System.Drawing.Point(300, 68);
			this.cboProxyWhen.Margin = new System.Windows.Forms.Padding(4);
			this.cboProxyWhen.Name = "cboProxyWhen";
			this.cboProxyWhen.Size = new System.Drawing.Size(268, 28);
			this.cboProxyWhen.TabIndex = 75;
			// 
			// lnkProxyTorHelp
			// 
			this.lnkProxyTorHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkProxyTorHelp.BackColor = System.Drawing.Color.Transparent;
			this.lnkProxyTorHelp.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkProxyTorHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkProxyTorHelp.Location = new System.Drawing.Point(26, 572);
			this.lnkProxyTorHelp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lnkProxyTorHelp.Name = "lnkProxyTorHelp";
			this.lnkProxyTorHelp.Size = new System.Drawing.Size(975, 33);
			this.lnkProxyTorHelp.TabIndex = 74;
			this.lnkProxyTorHelp.TabStop = true;
			this.lnkProxyTorHelp.Text = "More about Tor over VPN";
			this.lnkProxyTorHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lnkProxyTorHelp.Click += new System.EventHandler(this.lnkProxyTorHelp_LinkClicked);
			// 
			// txtProxyTorControlPassword
			// 
			this.txtProxyTorControlPassword.Location = new System.Drawing.Point(300, 402);
			this.txtProxyTorControlPassword.Margin = new System.Windows.Forms.Padding(4);
			this.txtProxyTorControlPassword.Name = "txtProxyTorControlPassword";
			this.txtProxyTorControlPassword.Size = new System.Drawing.Size(268, 26);
			this.txtProxyTorControlPassword.TabIndex = 73;
			// 
			// lblProxyTorControlPassword
			// 
			this.lblProxyTorControlPassword.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyTorControlPassword.ForeColor = System.Drawing.Color.Black;
			this.lblProxyTorControlPassword.Location = new System.Drawing.Point(21, 402);
			this.lblProxyTorControlPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProxyTorControlPassword.Name = "lblProxyTorControlPassword";
			this.lblProxyTorControlPassword.Size = new System.Drawing.Size(270, 30);
			this.lblProxyTorControlPassword.TabIndex = 72;
			this.lblProxyTorControlPassword.Text = "Tor Control Password:";
			this.lblProxyTorControlPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmdProxyTorTest
			// 
			this.cmdProxyTorTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdProxyTorTest.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdProxyTorTest.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdProxyTorTest.FlatAppearance.BorderSize = 0;
			this.cmdProxyTorTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdProxyTorTest.Location = new System.Drawing.Point(300, 447);
			this.cmdProxyTorTest.Margin = new System.Windows.Forms.Padding(4);
			this.cmdProxyTorTest.Name = "cmdProxyTorTest";
			this.cmdProxyTorTest.Size = new System.Drawing.Size(270, 33);
			this.cmdProxyTorTest.TabIndex = 71;
			this.cmdProxyTorTest.Text = "Test";
			this.cmdProxyTorTest.UseVisualStyleBackColor = true;
			this.cmdProxyTorTest.Click += new System.EventHandler(this.cmdProxyTorTest_Click);
			// 
			// txtProxyTorControlPort
			// 
			this.txtProxyTorControlPort.Location = new System.Drawing.Point(300, 357);
			this.txtProxyTorControlPort.Margin = new System.Windows.Forms.Padding(4);
			this.txtProxyTorControlPort.Name = "txtProxyTorControlPort";
			this.txtProxyTorControlPort.Size = new System.Drawing.Size(268, 26);
			this.txtProxyTorControlPort.TabIndex = 70;
			// 
			// lblProxyTorControlPort
			// 
			this.lblProxyTorControlPort.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyTorControlPort.ForeColor = System.Drawing.Color.Black;
			this.lblProxyTorControlPort.Location = new System.Drawing.Point(21, 357);
			this.lblProxyTorControlPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProxyTorControlPort.Name = "lblProxyTorControlPort";
			this.lblProxyTorControlPort.Size = new System.Drawing.Size(270, 30);
			this.lblProxyTorControlPort.TabIndex = 69;
			this.lblProxyTorControlPort.Text = "Tor Control Port:";
			this.lblProxyTorControlPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label17
			// 
			this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label17.BackColor = System.Drawing.Color.Transparent;
			this.label17.ForeColor = System.Drawing.Color.Black;
			this.label17.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.label17.Location = new System.Drawing.Point(628, 316);
			this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(366, 123);
			this.label17.TabIndex = 45;
			this.label17.Text = "If you use Tor as proxy, \r\nyou need to set up \r\nyour real proxy\r\ninside Tor confi" +
    "guration";
			this.label17.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label12
			// 
			this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label12.BackColor = System.Drawing.Color.Transparent;
			this.label12.ForeColor = System.Drawing.Color.Black;
			this.label12.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.label12.Location = new System.Drawing.Point(633, 22);
			this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(362, 180);
			this.label12.TabIndex = 44;
			this.label12.Text = "UDP, SSH and SSL connections \r\nwill not be available\r\nif you use a proxy.";
			this.label12.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblProxyAuthentication
			// 
			this.lblProxyAuthentication.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyAuthentication.ForeColor = System.Drawing.Color.Black;
			this.lblProxyAuthentication.Location = new System.Drawing.Point(21, 212);
			this.lblProxyAuthentication.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProxyAuthentication.Name = "lblProxyAuthentication";
			this.lblProxyAuthentication.Size = new System.Drawing.Size(270, 32);
			this.lblProxyAuthentication.TabIndex = 43;
			this.lblProxyAuthentication.Text = "Authentication:";
			this.lblProxyAuthentication.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboProxyAuthentication
			// 
			this.cboProxyAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboProxyAuthentication.FormattingEnabled = true;
			this.cboProxyAuthentication.Items.AddRange(new object[] {
            "None",
            "Basic",
            "NTLM"});
			this.cboProxyAuthentication.Location = new System.Drawing.Point(300, 212);
			this.cboProxyAuthentication.Margin = new System.Windows.Forms.Padding(4);
			this.cboProxyAuthentication.Name = "cboProxyAuthentication";
			this.cboProxyAuthentication.Size = new System.Drawing.Size(268, 28);
			this.cboProxyAuthentication.TabIndex = 42;
			this.cboProxyAuthentication.SelectedIndexChanged += new System.EventHandler(this.cboProxyAuthentication_SelectedIndexChanged);
			// 
			// txtProxyPassword
			// 
			this.txtProxyPassword.Location = new System.Drawing.Point(300, 302);
			this.txtProxyPassword.Margin = new System.Windows.Forms.Padding(4);
			this.txtProxyPassword.Name = "txtProxyPassword";
			this.txtProxyPassword.Size = new System.Drawing.Size(268, 26);
			this.txtProxyPassword.TabIndex = 41;
			// 
			// lblProxyPassword
			// 
			this.lblProxyPassword.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyPassword.ForeColor = System.Drawing.Color.Black;
			this.lblProxyPassword.Location = new System.Drawing.Point(21, 302);
			this.lblProxyPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProxyPassword.Name = "lblProxyPassword";
			this.lblProxyPassword.Size = new System.Drawing.Size(270, 30);
			this.lblProxyPassword.TabIndex = 40;
			this.lblProxyPassword.Text = "Password:";
			this.lblProxyPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtProxyLogin
			// 
			this.txtProxyLogin.Location = new System.Drawing.Point(300, 256);
			this.txtProxyLogin.Margin = new System.Windows.Forms.Padding(4);
			this.txtProxyLogin.Name = "txtProxyLogin";
			this.txtProxyLogin.Size = new System.Drawing.Size(268, 26);
			this.txtProxyLogin.TabIndex = 39;
			// 
			// lblProxyLogin
			// 
			this.lblProxyLogin.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyLogin.ForeColor = System.Drawing.Color.Black;
			this.lblProxyLogin.Location = new System.Drawing.Point(21, 256);
			this.lblProxyLogin.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProxyLogin.Name = "lblProxyLogin";
			this.lblProxyLogin.Size = new System.Drawing.Size(270, 30);
			this.lblProxyLogin.TabIndex = 38;
			this.lblProxyLogin.Text = "Login:";
			this.lblProxyLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblProxyType
			// 
			this.lblProxyType.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyType.ForeColor = System.Drawing.Color.Black;
			this.lblProxyType.Location = new System.Drawing.Point(21, 27);
			this.lblProxyType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProxyType.Name = "lblProxyType";
			this.lblProxyType.Size = new System.Drawing.Size(270, 32);
			this.lblProxyType.TabIndex = 37;
			this.lblProxyType.Text = "Type:";
			this.lblProxyType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboProxyMode
			// 
			this.cboProxyMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboProxyMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cboProxyMode.FormattingEnabled = true;
			this.cboProxyMode.Items.AddRange(new object[] {
            "None",
            "Http",
            "Socks",
            "Tor"});
			this.cboProxyMode.Location = new System.Drawing.Point(300, 27);
			this.cboProxyMode.Margin = new System.Windows.Forms.Padding(4);
			this.cboProxyMode.Name = "cboProxyMode";
			this.cboProxyMode.Size = new System.Drawing.Size(268, 28);
			this.cboProxyMode.TabIndex = 36;
			this.cboProxyMode.SelectedIndexChanged += new System.EventHandler(this.cboProxyMode_SelectedIndexChanged);
			// 
			// txtProxyPort
			// 
			this.txtProxyPort.Location = new System.Drawing.Point(300, 156);
			this.txtProxyPort.Margin = new System.Windows.Forms.Padding(4);
			this.txtProxyPort.Name = "txtProxyPort";
			this.txtProxyPort.Size = new System.Drawing.Size(268, 26);
			this.txtProxyPort.TabIndex = 35;
			// 
			// lblProxyPort
			// 
			this.lblProxyPort.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyPort.ForeColor = System.Drawing.Color.Black;
			this.lblProxyPort.Location = new System.Drawing.Point(21, 156);
			this.lblProxyPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProxyPort.Name = "lblProxyPort";
			this.lblProxyPort.Size = new System.Drawing.Size(270, 30);
			this.lblProxyPort.TabIndex = 34;
			this.lblProxyPort.Text = "Port:";
			this.lblProxyPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtProxyHost
			// 
			this.txtProxyHost.Location = new System.Drawing.Point(300, 112);
			this.txtProxyHost.Margin = new System.Windows.Forms.Padding(4);
			this.txtProxyHost.Name = "txtProxyHost";
			this.txtProxyHost.Size = new System.Drawing.Size(268, 26);
			this.txtProxyHost.TabIndex = 33;
			// 
			// lblProxyHost
			// 
			this.lblProxyHost.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyHost.ForeColor = System.Drawing.Color.Black;
			this.lblProxyHost.Location = new System.Drawing.Point(21, 112);
			this.lblProxyHost.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblProxyHost.Name = "lblProxyHost";
			this.lblProxyHost.Size = new System.Drawing.Size(270, 30);
			this.lblProxyHost.TabIndex = 32;
			this.lblProxyHost.Text = "Host:";
			this.lblProxyHost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabRoutes
			// 
			this.tabRoutes.BackColor = System.Drawing.Color.White;
			this.tabRoutes.Controls.Add(this.cmdRouteEdit);
			this.tabRoutes.Controls.Add(this.cmdRouteRemove);
			this.tabRoutes.Controls.Add(this.cmdRouteAdd);
			this.tabRoutes.Controls.Add(this.label6);
			this.tabRoutes.Controls.Add(this.lstRoutes);
			this.tabRoutes.Location = new System.Drawing.Point(4, 24);
			this.tabRoutes.Margin = new System.Windows.Forms.Padding(4);
			this.tabRoutes.Name = "tabRoutes";
			this.tabRoutes.Size = new System.Drawing.Size(1014, 635);
			this.tabRoutes.TabIndex = 5;
			this.tabRoutes.Text = "Routes";
			// 
			// cmdRouteEdit
			// 
			this.cmdRouteEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdRouteEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdRouteEdit.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdRouteEdit.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdRouteEdit.FlatAppearance.BorderSize = 0;
			this.cmdRouteEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdRouteEdit.Image = global::Eddie.Forms.Properties.Resources.edit;
			this.cmdRouteEdit.Location = new System.Drawing.Point(948, 154);
			this.cmdRouteEdit.Margin = new System.Windows.Forms.Padding(4);
			this.cmdRouteEdit.Name = "cmdRouteEdit";
			this.cmdRouteEdit.Size = new System.Drawing.Size(42, 42);
			this.cmdRouteEdit.TabIndex = 41;
			this.cmdRouteEdit.UseVisualStyleBackColor = true;
			this.cmdRouteEdit.Click += new System.EventHandler(this.cmdRouteEdit_Click);
			// 
			// cmdRouteRemove
			// 
			this.cmdRouteRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdRouteRemove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdRouteRemove.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdRouteRemove.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdRouteRemove.FlatAppearance.BorderSize = 0;
			this.cmdRouteRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdRouteRemove.Image = global::Eddie.Forms.Properties.Resources.delete;
			this.cmdRouteRemove.Location = new System.Drawing.Point(948, 104);
			this.cmdRouteRemove.Margin = new System.Windows.Forms.Padding(4);
			this.cmdRouteRemove.Name = "cmdRouteRemove";
			this.cmdRouteRemove.Size = new System.Drawing.Size(42, 42);
			this.cmdRouteRemove.TabIndex = 40;
			this.cmdRouteRemove.UseVisualStyleBackColor = true;
			this.cmdRouteRemove.Click += new System.EventHandler(this.cmdRouteRemove_Click);
			// 
			// cmdRouteAdd
			// 
			this.cmdRouteAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdRouteAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdRouteAdd.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdRouteAdd.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdRouteAdd.FlatAppearance.BorderSize = 0;
			this.cmdRouteAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdRouteAdd.Image = global::Eddie.Forms.Properties.Resources.add;
			this.cmdRouteAdd.Location = new System.Drawing.Point(948, 52);
			this.cmdRouteAdd.Margin = new System.Windows.Forms.Padding(4);
			this.cmdRouteAdd.Name = "cmdRouteAdd";
			this.cmdRouteAdd.Size = new System.Drawing.Size(42, 42);
			this.cmdRouteAdd.TabIndex = 39;
			this.cmdRouteAdd.UseVisualStyleBackColor = true;
			this.cmdRouteAdd.Click += new System.EventHandler(this.cmdRouteAdd_Click);
			// 
			// label6
			// 
			this.label6.BackColor = System.Drawing.Color.Transparent;
			this.label6.ForeColor = System.Drawing.Color.Black;
			this.label6.Location = new System.Drawing.Point(9, 15);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(981, 33);
			this.label6.TabIndex = 3;
			this.label6.Text = "Network routing rules about what destination must be in the VPN tunnel or not.";
			// 
			// lstRoutes
			// 
			this.lstRoutes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstRoutes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colRoutesIp,
            this.colRoutesAction,
            this.colRoutesNotes});
			this.lstRoutes.ContextMenuStrip = this.mnuRoutes;
			this.lstRoutes.FullRowSelect = true;
			this.lstRoutes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstRoutes.HideSelection = false;
			this.lstRoutes.Location = new System.Drawing.Point(9, 52);
			this.lstRoutes.Margin = new System.Windows.Forms.Padding(4);
			this.lstRoutes.MultiSelect = false;
			this.lstRoutes.Name = "lstRoutes";
			this.lstRoutes.OwnerDraw = true;
			this.lstRoutes.Size = new System.Drawing.Size(930, 566);
			this.lstRoutes.TabIndex = 0;
			this.lstRoutes.UseCompatibleStateImageBehavior = false;
			this.lstRoutes.View = System.Windows.Forms.View.Details;
			this.lstRoutes.SelectedIndexChanged += new System.EventHandler(this.lstRoutes_SelectedIndexChanged);
			this.lstRoutes.DoubleClick += new System.EventHandler(this.lstRoutes_DoubleClick);
			// 
			// colRoutesIp
			// 
			this.colRoutesIp.Text = "IP / Host / Range";
			this.colRoutesIp.Width = 200;
			// 
			// colRoutesAction
			// 
			this.colRoutesAction.Text = "When connected";
			this.colRoutesAction.Width = 160;
			// 
			// colRoutesNotes
			// 
			this.colRoutesNotes.Text = "Notes";
			this.colRoutesNotes.Width = 1000;
			// 
			// tabDNS
			// 
			this.tabDNS.Controls.Add(this.pnlDnsWindowsOnly);
			this.tabDNS.Controls.Add(this.label10);
			this.tabDNS.Controls.Add(this.lblDnsCheck);
			this.tabDNS.Controls.Add(this.lblDnsServers);
			this.tabDNS.Controls.Add(this.cmdDnsEdit);
			this.tabDNS.Controls.Add(this.cmdDnsRemove);
			this.tabDNS.Controls.Add(this.cmdDnsAdd);
			this.tabDNS.Controls.Add(this.lstDnsServers);
			this.tabDNS.Controls.Add(this.lblDnsSwitchMode);
			this.tabDNS.Controls.Add(this.cboDnsSwitchMode);
			this.tabDNS.Controls.Add(this.chkDnsCheck);
			this.tabDNS.Location = new System.Drawing.Point(4, 24);
			this.tabDNS.Margin = new System.Windows.Forms.Padding(4);
			this.tabDNS.Name = "tabDNS";
			this.tabDNS.Padding = new System.Windows.Forms.Padding(4);
			this.tabDNS.Size = new System.Drawing.Size(1014, 635);
			this.tabDNS.TabIndex = 5;
			this.tabDNS.Text = "DNS";
			this.tabDNS.UseVisualStyleBackColor = true;
			// 
			// pnlDnsWindowsOnly
			// 
			this.pnlDnsWindowsOnly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlDnsWindowsOnly.Controls.Add(this.lblDnsIgnoreDNS6);
			this.pnlDnsWindowsOnly.Controls.Add(this.lblDnsEnsureLock);
			this.pnlDnsWindowsOnly.Controls.Add(this.lblDnsForceAllInterfaces);
			this.pnlDnsWindowsOnly.Controls.Add(this.chkDnsIgnoreDNS6);
			this.pnlDnsWindowsOnly.Controls.Add(this.chkDnsEnsureLock);
			this.pnlDnsWindowsOnly.Controls.Add(this.chkDnsForceAllInterfaces);
			this.pnlDnsWindowsOnly.Location = new System.Drawing.Point(9, 448);
			this.pnlDnsWindowsOnly.Margin = new System.Windows.Forms.Padding(4);
			this.pnlDnsWindowsOnly.Name = "pnlDnsWindowsOnly";
			this.pnlDnsWindowsOnly.Padding = new System.Windows.Forms.Padding(4);
			this.pnlDnsWindowsOnly.Size = new System.Drawing.Size(982, 164);
			this.pnlDnsWindowsOnly.TabIndex = 83;
			this.pnlDnsWindowsOnly.TabStop = false;
			this.pnlDnsWindowsOnly.Text = "Microsoft Windows Only";
			// 
			// lblDnsIgnoreDNS6
			// 
			this.lblDnsIgnoreDNS6.BackColor = System.Drawing.Color.Transparent;
			this.lblDnsIgnoreDNS6.ForeColor = System.Drawing.Color.Black;
			this.lblDnsIgnoreDNS6.Location = new System.Drawing.Point(16, 118);
			this.lblDnsIgnoreDNS6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDnsIgnoreDNS6.Name = "lblDnsIgnoreDNS6";
			this.lblDnsIgnoreDNS6.Size = new System.Drawing.Size(362, 34);
			this.lblDnsIgnoreDNS6.TabIndex = 94;
			this.lblDnsIgnoreDNS6.Text = "Ignore IPv6 DNS pushed by server:";
			this.lblDnsIgnoreDNS6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblDnsEnsureLock
			// 
			this.lblDnsEnsureLock.BackColor = System.Drawing.Color.Transparent;
			this.lblDnsEnsureLock.ForeColor = System.Drawing.Color.Black;
			this.lblDnsEnsureLock.Location = new System.Drawing.Point(16, 78);
			this.lblDnsEnsureLock.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDnsEnsureLock.Name = "lblDnsEnsureLock";
			this.lblDnsEnsureLock.Size = new System.Drawing.Size(362, 34);
			this.lblDnsEnsureLock.TabIndex = 93;
			this.lblDnsEnsureLock.Text = "Ensure DNS Lock:";
			this.lblDnsEnsureLock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblDnsForceAllInterfaces
			// 
			this.lblDnsForceAllInterfaces.BackColor = System.Drawing.Color.Transparent;
			this.lblDnsForceAllInterfaces.ForeColor = System.Drawing.Color.Black;
			this.lblDnsForceAllInterfaces.Location = new System.Drawing.Point(16, 34);
			this.lblDnsForceAllInterfaces.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDnsForceAllInterfaces.Name = "lblDnsForceAllInterfaces";
			this.lblDnsForceAllInterfaces.Size = new System.Drawing.Size(362, 34);
			this.lblDnsForceAllInterfaces.TabIndex = 84;
			this.lblDnsForceAllInterfaces.Text = "Force all network interfaces for DNS:";
			this.lblDnsForceAllInterfaces.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkDnsIgnoreDNS6
			// 
			this.chkDnsIgnoreDNS6.BackColor = System.Drawing.Color.Transparent;
			this.chkDnsIgnoreDNS6.ForeColor = System.Drawing.Color.Black;
			this.chkDnsIgnoreDNS6.Location = new System.Drawing.Point(387, 120);
			this.chkDnsIgnoreDNS6.Margin = new System.Windows.Forms.Padding(4);
			this.chkDnsIgnoreDNS6.Name = "chkDnsIgnoreDNS6";
			this.chkDnsIgnoreDNS6.Size = new System.Drawing.Size(586, 33);
			this.chkDnsIgnoreDNS6.TabIndex = 92;
			this.chkDnsIgnoreDNS6.UseVisualStyleBackColor = false;
			// 
			// chkDnsEnsureLock
			// 
			this.chkDnsEnsureLock.BackColor = System.Drawing.Color.Transparent;
			this.chkDnsEnsureLock.ForeColor = System.Drawing.Color.Black;
			this.chkDnsEnsureLock.Location = new System.Drawing.Point(387, 78);
			this.chkDnsEnsureLock.Margin = new System.Windows.Forms.Padding(4);
			this.chkDnsEnsureLock.Name = "chkDnsEnsureLock";
			this.chkDnsEnsureLock.Size = new System.Drawing.Size(586, 33);
			this.chkDnsEnsureLock.TabIndex = 91;
			this.chkDnsEnsureLock.UseVisualStyleBackColor = false;
			// 
			// chkDnsForceAllInterfaces
			// 
			this.chkDnsForceAllInterfaces.BackColor = System.Drawing.Color.Transparent;
			this.chkDnsForceAllInterfaces.ForeColor = System.Drawing.Color.Black;
			this.chkDnsForceAllInterfaces.Location = new System.Drawing.Point(387, 36);
			this.chkDnsForceAllInterfaces.Margin = new System.Windows.Forms.Padding(4);
			this.chkDnsForceAllInterfaces.Name = "chkDnsForceAllInterfaces";
			this.chkDnsForceAllInterfaces.Size = new System.Drawing.Size(586, 33);
			this.chkDnsForceAllInterfaces.TabIndex = 90;
			this.chkDnsForceAllInterfaces.UseVisualStyleBackColor = false;
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label10.BackColor = System.Drawing.Color.Transparent;
			this.label10.ForeColor = System.Drawing.Color.Black;
			this.label10.Location = new System.Drawing.Point(396, 399);
			this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(544, 45);
			this.label10.TabIndex = 82;
			this.label10.Text = "Leave empty to use DNS servers recommended by the VPN.";
			this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblDnsCheck
			// 
			this.lblDnsCheck.BackColor = System.Drawing.Color.Transparent;
			this.lblDnsCheck.ForeColor = System.Drawing.Color.Black;
			this.lblDnsCheck.Location = new System.Drawing.Point(26, 75);
			this.lblDnsCheck.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDnsCheck.Name = "lblDnsCheck";
			this.lblDnsCheck.Size = new System.Drawing.Size(362, 34);
			this.lblDnsCheck.TabIndex = 81;
			this.lblDnsCheck.Text = "Check AirVPN DNS:";
			this.lblDnsCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblDnsServers
			// 
			this.lblDnsServers.BackColor = System.Drawing.Color.Transparent;
			this.lblDnsServers.ForeColor = System.Drawing.Color.Black;
			this.lblDnsServers.Location = new System.Drawing.Point(30, 120);
			this.lblDnsServers.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDnsServers.Name = "lblDnsServers";
			this.lblDnsServers.Size = new System.Drawing.Size(357, 33);
			this.lblDnsServers.TabIndex = 80;
			this.lblDnsServers.Text = "DNS Servers:";
			this.lblDnsServers.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmdDnsEdit
			// 
			this.cmdDnsEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdDnsEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdDnsEdit.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdDnsEdit.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdDnsEdit.FlatAppearance.BorderSize = 0;
			this.cmdDnsEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdDnsEdit.Image = global::Eddie.Forms.Properties.Resources.edit;
			this.cmdDnsEdit.Location = new System.Drawing.Point(950, 222);
			this.cmdDnsEdit.Margin = new System.Windows.Forms.Padding(4);
			this.cmdDnsEdit.Name = "cmdDnsEdit";
			this.cmdDnsEdit.Size = new System.Drawing.Size(42, 42);
			this.cmdDnsEdit.TabIndex = 79;
			this.cmdDnsEdit.UseVisualStyleBackColor = true;
			this.cmdDnsEdit.Click += new System.EventHandler(this.cmdDnsEdit_Click);
			// 
			// cmdDnsRemove
			// 
			this.cmdDnsRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdDnsRemove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdDnsRemove.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdDnsRemove.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdDnsRemove.FlatAppearance.BorderSize = 0;
			this.cmdDnsRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdDnsRemove.Image = global::Eddie.Forms.Properties.Resources.delete;
			this.cmdDnsRemove.Location = new System.Drawing.Point(950, 171);
			this.cmdDnsRemove.Margin = new System.Windows.Forms.Padding(4);
			this.cmdDnsRemove.Name = "cmdDnsRemove";
			this.cmdDnsRemove.Size = new System.Drawing.Size(42, 42);
			this.cmdDnsRemove.TabIndex = 78;
			this.cmdDnsRemove.UseVisualStyleBackColor = true;
			this.cmdDnsRemove.Click += new System.EventHandler(this.cmdDnsRemove_Click);
			// 
			// cmdDnsAdd
			// 
			this.cmdDnsAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdDnsAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdDnsAdd.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdDnsAdd.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdDnsAdd.FlatAppearance.BorderSize = 0;
			this.cmdDnsAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdDnsAdd.Image = global::Eddie.Forms.Properties.Resources.add;
			this.cmdDnsAdd.Location = new System.Drawing.Point(950, 120);
			this.cmdDnsAdd.Margin = new System.Windows.Forms.Padding(4);
			this.cmdDnsAdd.Name = "cmdDnsAdd";
			this.cmdDnsAdd.Size = new System.Drawing.Size(42, 42);
			this.cmdDnsAdd.TabIndex = 77;
			this.cmdDnsAdd.UseVisualStyleBackColor = true;
			this.cmdDnsAdd.Click += new System.EventHandler(this.cmdDnsAdd_Click);
			// 
			// lstDnsServers
			// 
			this.lstDnsServers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstDnsServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader5});
			this.lstDnsServers.ContextMenuStrip = this.mnuRoutes;
			this.lstDnsServers.FullRowSelect = true;
			this.lstDnsServers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.lstDnsServers.HideSelection = false;
			this.lstDnsServers.Location = new System.Drawing.Point(396, 120);
			this.lstDnsServers.Margin = new System.Windows.Forms.Padding(4);
			this.lstDnsServers.MultiSelect = false;
			this.lstDnsServers.Name = "lstDnsServers";
			this.lstDnsServers.OwnerDraw = true;
			this.lstDnsServers.Size = new System.Drawing.Size(542, 272);
			this.lstDnsServers.TabIndex = 76;
			this.lstDnsServers.UseCompatibleStateImageBehavior = false;
			this.lstDnsServers.View = System.Windows.Forms.View.Details;
			this.lstDnsServers.SelectedIndexChanged += new System.EventHandler(this.lstDnsServers_SelectedIndexChanged);
			this.lstDnsServers.DoubleClick += new System.EventHandler(this.lstDnsServers_DoubleClick);
			// 
			// lblDnsSwitchMode
			// 
			this.lblDnsSwitchMode.BackColor = System.Drawing.Color.Transparent;
			this.lblDnsSwitchMode.ForeColor = System.Drawing.Color.Black;
			this.lblDnsSwitchMode.Location = new System.Drawing.Point(26, 27);
			this.lblDnsSwitchMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDnsSwitchMode.Name = "lblDnsSwitchMode";
			this.lblDnsSwitchMode.Size = new System.Drawing.Size(362, 34);
			this.lblDnsSwitchMode.TabIndex = 75;
			this.lblDnsSwitchMode.Text = "DNS Switch mode:";
			this.lblDnsSwitchMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboDnsSwitchMode
			// 
			this.cboDnsSwitchMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDnsSwitchMode.FormattingEnabled = true;
			this.cboDnsSwitchMode.Items.AddRange(new object[] {
            "Disabled",
            "Automatic"});
			this.cboDnsSwitchMode.Location = new System.Drawing.Point(396, 28);
			this.cboDnsSwitchMode.Margin = new System.Windows.Forms.Padding(4);
			this.cboDnsSwitchMode.Name = "cboDnsSwitchMode";
			this.cboDnsSwitchMode.Size = new System.Drawing.Size(542, 28);
			this.cboDnsSwitchMode.TabIndex = 74;
			// 
			// chkDnsCheck
			// 
			this.chkDnsCheck.BackColor = System.Drawing.Color.Transparent;
			this.chkDnsCheck.ForeColor = System.Drawing.Color.Black;
			this.chkDnsCheck.Location = new System.Drawing.Point(396, 75);
			this.chkDnsCheck.Margin = new System.Windows.Forms.Padding(4);
			this.chkDnsCheck.Name = "chkDnsCheck";
			this.chkDnsCheck.Size = new System.Drawing.Size(544, 34);
			this.chkDnsCheck.TabIndex = 73;
			this.chkDnsCheck.UseVisualStyleBackColor = false;
			// 
			// tabNetworkLock
			// 
			this.tabNetworkLock.Controls.Add(this.lblLockWhiteListOutgoingIPs);
			this.tabNetworkLock.Controls.Add(this.txtLockWhiteListOutgoingIPs);
			this.tabNetworkLock.Controls.Add(this.lblLockAllowDHCP);
			this.tabNetworkLock.Controls.Add(this.chkLockAllowDHCP);
			this.tabNetworkLock.Controls.Add(this.lblLockOutgoing);
			this.tabNetworkLock.Controls.Add(this.cboLockOutgoing);
			this.tabNetworkLock.Controls.Add(this.lblLockIncoming);
			this.tabNetworkLock.Controls.Add(this.cboLockIncoming);
			this.tabNetworkLock.Controls.Add(this.lblLockAllowDNS);
			this.tabNetworkLock.Controls.Add(this.chkLockAllowDNS);
			this.tabNetworkLock.Controls.Add(this.lblLockAllowPing);
			this.tabNetworkLock.Controls.Add(this.lblLockAllowPrivate);
			this.tabNetworkLock.Controls.Add(this.lnkLockHelp);
			this.tabNetworkLock.Controls.Add(this.chkLockAllowPing);
			this.tabNetworkLock.Controls.Add(this.chkLockAllowPrivate);
			this.tabNetworkLock.Controls.Add(this.lblLockRoutingOutWarning);
			this.tabNetworkLock.Controls.Add(this.lblLockWhiteListIncomingIPs);
			this.tabNetworkLock.Controls.Add(this.txtLockWhiteListIncomingIPs);
			this.tabNetworkLock.Controls.Add(this.lblLockMode);
			this.tabNetworkLock.Controls.Add(this.cboLockMode);
			this.tabNetworkLock.Location = new System.Drawing.Point(4, 24);
			this.tabNetworkLock.Margin = new System.Windows.Forms.Padding(4);
			this.tabNetworkLock.Name = "tabNetworkLock";
			this.tabNetworkLock.Size = new System.Drawing.Size(1014, 635);
			this.tabNetworkLock.TabIndex = 4;
			this.tabNetworkLock.Text = "Network lock";
			this.tabNetworkLock.UseVisualStyleBackColor = true;
			// 
			// lblLockWhiteListOutgoingIPs
			// 
			this.lblLockWhiteListOutgoingIPs.BackColor = System.Drawing.Color.Transparent;
			this.lblLockWhiteListOutgoingIPs.ForeColor = System.Drawing.Color.Black;
			this.lblLockWhiteListOutgoingIPs.Location = new System.Drawing.Point(418, 388);
			this.lblLockWhiteListOutgoingIPs.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockWhiteListOutgoingIPs.Name = "lblLockWhiteListOutgoingIPs";
			this.lblLockWhiteListOutgoingIPs.Size = new System.Drawing.Size(262, 38);
			this.lblLockWhiteListOutgoingIPs.TabIndex = 107;
			this.lblLockWhiteListOutgoingIPs.Text = "IPs allowed for outgoing:";
			this.lblLockWhiteListOutgoingIPs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtLockWhiteListOutgoingIPs
			// 
			this.txtLockWhiteListOutgoingIPs.AcceptsReturn = true;
			this.txtLockWhiteListOutgoingIPs.AcceptsTab = true;
			this.txtLockWhiteListOutgoingIPs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.txtLockWhiteListOutgoingIPs.Location = new System.Drawing.Point(421, 427);
			this.txtLockWhiteListOutgoingIPs.Margin = new System.Windows.Forms.Padding(4);
			this.txtLockWhiteListOutgoingIPs.Multiline = true;
			this.txtLockWhiteListOutgoingIPs.Name = "txtLockWhiteListOutgoingIPs";
			this.txtLockWhiteListOutgoingIPs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLockWhiteListOutgoingIPs.Size = new System.Drawing.Size(386, 141);
			this.txtLockWhiteListOutgoingIPs.TabIndex = 106;
			// 
			// lblLockAllowDHCP
			// 
			this.lblLockAllowDHCP.BackColor = System.Drawing.Color.Transparent;
			this.lblLockAllowDHCP.ForeColor = System.Drawing.Color.Black;
			this.lblLockAllowDHCP.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblLockAllowDHCP.Location = new System.Drawing.Point(22, 224);
			this.lblLockAllowDHCP.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockAllowDHCP.Name = "lblLockAllowDHCP";
			this.lblLockAllowDHCP.Size = new System.Drawing.Size(261, 38);
			this.lblLockAllowDHCP.TabIndex = 105;
			this.lblLockAllowDHCP.Text = "Allow DHCP:";
			this.lblLockAllowDHCP.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkLockAllowDHCP
			// 
			this.chkLockAllowDHCP.BackColor = System.Drawing.Color.Transparent;
			this.chkLockAllowDHCP.ForeColor = System.Drawing.Color.Black;
			this.chkLockAllowDHCP.Location = new System.Drawing.Point(292, 224);
			this.chkLockAllowDHCP.Margin = new System.Windows.Forms.Padding(4);
			this.chkLockAllowDHCP.Name = "chkLockAllowDHCP";
			this.chkLockAllowDHCP.Size = new System.Drawing.Size(244, 38);
			this.chkLockAllowDHCP.TabIndex = 104;
			this.chkLockAllowDHCP.UseVisualStyleBackColor = false;
			// 
			// lblLockOutgoing
			// 
			this.lblLockOutgoing.BackColor = System.Drawing.Color.Transparent;
			this.lblLockOutgoing.ForeColor = System.Drawing.Color.Black;
			this.lblLockOutgoing.Location = new System.Drawing.Point(21, 117);
			this.lblLockOutgoing.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockOutgoing.Name = "lblLockOutgoing";
			this.lblLockOutgoing.Size = new System.Drawing.Size(261, 32);
			this.lblLockOutgoing.TabIndex = 103;
			this.lblLockOutgoing.Text = "Outgoing:";
			this.lblLockOutgoing.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboLockOutgoing
			// 
			this.cboLockOutgoing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLockOutgoing.FormattingEnabled = true;
			this.cboLockOutgoing.Items.AddRange(new object[] {
            "Block",
            "Allow"});
			this.cboLockOutgoing.Location = new System.Drawing.Point(291, 118);
			this.cboLockOutgoing.Margin = new System.Windows.Forms.Padding(4);
			this.cboLockOutgoing.Name = "cboLockOutgoing";
			this.cboLockOutgoing.Size = new System.Drawing.Size(342, 28);
			this.cboLockOutgoing.TabIndex = 102;
			// 
			// lblLockIncoming
			// 
			this.lblLockIncoming.BackColor = System.Drawing.Color.Transparent;
			this.lblLockIncoming.ForeColor = System.Drawing.Color.Black;
			this.lblLockIncoming.Location = new System.Drawing.Point(21, 76);
			this.lblLockIncoming.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockIncoming.Name = "lblLockIncoming";
			this.lblLockIncoming.Size = new System.Drawing.Size(261, 32);
			this.lblLockIncoming.TabIndex = 101;
			this.lblLockIncoming.Text = "Incoming:";
			this.lblLockIncoming.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboLockIncoming
			// 
			this.cboLockIncoming.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLockIncoming.FormattingEnabled = true;
			this.cboLockIncoming.Items.AddRange(new object[] {
            "Block",
            "Allow"});
			this.cboLockIncoming.Location = new System.Drawing.Point(291, 78);
			this.cboLockIncoming.Margin = new System.Windows.Forms.Padding(4);
			this.cboLockIncoming.Name = "cboLockIncoming";
			this.cboLockIncoming.Size = new System.Drawing.Size(342, 28);
			this.cboLockIncoming.TabIndex = 100;
			// 
			// lblLockAllowDNS
			// 
			this.lblLockAllowDNS.BackColor = System.Drawing.Color.Transparent;
			this.lblLockAllowDNS.ForeColor = System.Drawing.Color.Black;
			this.lblLockAllowDNS.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblLockAllowDNS.Location = new System.Drawing.Point(21, 316);
			this.lblLockAllowDNS.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockAllowDNS.Name = "lblLockAllowDNS";
			this.lblLockAllowDNS.Size = new System.Drawing.Size(261, 38);
			this.lblLockAllowDNS.TabIndex = 86;
			this.lblLockAllowDNS.Text = "Allow detected DNS:";
			this.lblLockAllowDNS.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkLockAllowDNS
			// 
			this.chkLockAllowDNS.BackColor = System.Drawing.Color.Transparent;
			this.chkLockAllowDNS.ForeColor = System.Drawing.Color.Black;
			this.chkLockAllowDNS.Location = new System.Drawing.Point(291, 316);
			this.chkLockAllowDNS.Margin = new System.Windows.Forms.Padding(4);
			this.chkLockAllowDNS.Name = "chkLockAllowDNS";
			this.chkLockAllowDNS.Size = new System.Drawing.Size(244, 38);
			this.chkLockAllowDNS.TabIndex = 85;
			this.chkLockAllowDNS.UseVisualStyleBackColor = false;
			// 
			// lblLockAllowPing
			// 
			this.lblLockAllowPing.BackColor = System.Drawing.Color.Transparent;
			this.lblLockAllowPing.ForeColor = System.Drawing.Color.Black;
			this.lblLockAllowPing.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblLockAllowPing.Location = new System.Drawing.Point(21, 270);
			this.lblLockAllowPing.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockAllowPing.Name = "lblLockAllowPing";
			this.lblLockAllowPing.Size = new System.Drawing.Size(261, 38);
			this.lblLockAllowPing.TabIndex = 84;
			this.lblLockAllowPing.Text = "Allow ping:";
			this.lblLockAllowPing.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblLockAllowPrivate
			// 
			this.lblLockAllowPrivate.BackColor = System.Drawing.Color.Transparent;
			this.lblLockAllowPrivate.ForeColor = System.Drawing.Color.Black;
			this.lblLockAllowPrivate.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblLockAllowPrivate.Location = new System.Drawing.Point(21, 178);
			this.lblLockAllowPrivate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockAllowPrivate.Name = "lblLockAllowPrivate";
			this.lblLockAllowPrivate.Size = new System.Drawing.Size(261, 38);
			this.lblLockAllowPrivate.TabIndex = 83;
			this.lblLockAllowPrivate.Text = "Allow lan/private:";
			this.lblLockAllowPrivate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lnkLockHelp
			// 
			this.lnkLockHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkLockHelp.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkLockHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkLockHelp.Location = new System.Drawing.Point(20, 572);
			this.lnkLockHelp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lnkLockHelp.Name = "lnkLockHelp";
			this.lnkLockHelp.Size = new System.Drawing.Size(975, 33);
			this.lnkLockHelp.TabIndex = 82;
			this.lnkLockHelp.TabStop = true;
			this.lnkLockHelp.Text = "More about Network Lock";
			this.lnkLockHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lnkLockHelp.Click += new System.EventHandler(this.lnkLockHelp_LinkClicked);
			// 
			// chkLockAllowPing
			// 
			this.chkLockAllowPing.BackColor = System.Drawing.Color.Transparent;
			this.chkLockAllowPing.ForeColor = System.Drawing.Color.Black;
			this.chkLockAllowPing.Location = new System.Drawing.Point(291, 270);
			this.chkLockAllowPing.Margin = new System.Windows.Forms.Padding(4);
			this.chkLockAllowPing.Name = "chkLockAllowPing";
			this.chkLockAllowPing.Size = new System.Drawing.Size(244, 38);
			this.chkLockAllowPing.TabIndex = 81;
			this.chkLockAllowPing.UseVisualStyleBackColor = false;
			// 
			// chkLockAllowPrivate
			// 
			this.chkLockAllowPrivate.BackColor = System.Drawing.Color.Transparent;
			this.chkLockAllowPrivate.ForeColor = System.Drawing.Color.Black;
			this.chkLockAllowPrivate.Location = new System.Drawing.Point(291, 178);
			this.chkLockAllowPrivate.Margin = new System.Windows.Forms.Padding(4);
			this.chkLockAllowPrivate.Name = "chkLockAllowPrivate";
			this.chkLockAllowPrivate.Size = new System.Drawing.Size(244, 38);
			this.chkLockAllowPrivate.TabIndex = 80;
			this.chkLockAllowPrivate.UseVisualStyleBackColor = false;
			// 
			// lblLockRoutingOutWarning
			// 
			this.lblLockRoutingOutWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblLockRoutingOutWarning.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.lblLockRoutingOutWarning.ForeColor = System.Drawing.Color.White;
			this.lblLockRoutingOutWarning.Location = new System.Drawing.Point(644, 15);
			this.lblLockRoutingOutWarning.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockRoutingOutWarning.Name = "lblLockRoutingOutWarning";
			this.lblLockRoutingOutWarning.Size = new System.Drawing.Size(351, 128);
			this.lblLockRoutingOutWarning.TabIndex = 78;
			this.lblLockRoutingOutWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblLockWhiteListIncomingIPs
			// 
			this.lblLockWhiteListIncomingIPs.BackColor = System.Drawing.Color.Transparent;
			this.lblLockWhiteListIncomingIPs.ForeColor = System.Drawing.Color.Black;
			this.lblLockWhiteListIncomingIPs.Location = new System.Drawing.Point(21, 388);
			this.lblLockWhiteListIncomingIPs.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockWhiteListIncomingIPs.Name = "lblLockWhiteListIncomingIPs";
			this.lblLockWhiteListIncomingIPs.Size = new System.Drawing.Size(262, 38);
			this.lblLockWhiteListIncomingIPs.TabIndex = 76;
			this.lblLockWhiteListIncomingIPs.Text = "IPs allowed for incoming:";
			this.lblLockWhiteListIncomingIPs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtLockWhiteListIncomingIPs
			// 
			this.txtLockWhiteListIncomingIPs.AcceptsReturn = true;
			this.txtLockWhiteListIncomingIPs.AcceptsTab = true;
			this.txtLockWhiteListIncomingIPs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.txtLockWhiteListIncomingIPs.Location = new System.Drawing.Point(24, 427);
			this.txtLockWhiteListIncomingIPs.Margin = new System.Windows.Forms.Padding(4);
			this.txtLockWhiteListIncomingIPs.Multiline = true;
			this.txtLockWhiteListIncomingIPs.Name = "txtLockWhiteListIncomingIPs";
			this.txtLockWhiteListIncomingIPs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLockWhiteListIncomingIPs.Size = new System.Drawing.Size(386, 141);
			this.txtLockWhiteListIncomingIPs.TabIndex = 75;
			// 
			// lblLockMode
			// 
			this.lblLockMode.BackColor = System.Drawing.Color.Transparent;
			this.lblLockMode.ForeColor = System.Drawing.Color.Black;
			this.lblLockMode.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblLockMode.Location = new System.Drawing.Point(21, 15);
			this.lblLockMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLockMode.Name = "lblLockMode";
			this.lblLockMode.Size = new System.Drawing.Size(261, 32);
			this.lblLockMode.TabIndex = 74;
			this.lblLockMode.Text = "Mode:";
			this.lblLockMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cboLockMode
			// 
			this.cboLockMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLockMode.FormattingEnabled = true;
			this.cboLockMode.Location = new System.Drawing.Point(291, 15);
			this.cboLockMode.Margin = new System.Windows.Forms.Padding(4);
			this.cboLockMode.Name = "cboLockMode";
			this.cboLockMode.Size = new System.Drawing.Size(342, 28);
			this.cboLockMode.TabIndex = 73;
			// 
			// tabLogging
			// 
			this.tabLogging.Controls.Add(this.cmdLoggingOpen);
			this.tabLogging.Controls.Add(this.chkLogLevelDebug);
			this.tabLogging.Controls.Add(this.TxtLoggingPathComputed);
			this.tabLogging.Controls.Add(this.lblLoggingHelp);
			this.tabLogging.Controls.Add(this.txtLogPath);
			this.tabLogging.Controls.Add(this.lblLogPath);
			this.tabLogging.Controls.Add(this.chkLoggingEnabled);
			this.tabLogging.Location = new System.Drawing.Point(4, 24);
			this.tabLogging.Margin = new System.Windows.Forms.Padding(4);
			this.tabLogging.Name = "tabLogging";
			this.tabLogging.Size = new System.Drawing.Size(1014, 635);
			this.tabLogging.TabIndex = 3;
			this.tabLogging.Text = "Logging";
			this.tabLogging.UseVisualStyleBackColor = true;
			// 
			// cmdLoggingOpen
			// 
			this.cmdLoggingOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdLoggingOpen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdLoggingOpen.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdLoggingOpen.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdLoggingOpen.FlatAppearance.BorderSize = 0;
			this.cmdLoggingOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdLoggingOpen.Location = new System.Drawing.Point(676, 296);
			this.cmdLoggingOpen.Margin = new System.Windows.Forms.Padding(4);
			this.cmdLoggingOpen.Name = "cmdLoggingOpen";
			this.cmdLoggingOpen.Size = new System.Drawing.Size(321, 45);
			this.cmdLoggingOpen.TabIndex = 64;
			this.cmdLoggingOpen.Text = "Open in file-manager";
			this.cmdLoggingOpen.UseVisualStyleBackColor = true;
			this.cmdLoggingOpen.Click += new System.EventHandler(this.cmdLoggingOpen_Click);
			// 
			// chkLogLevelDebug
			// 
			this.chkLogLevelDebug.BackColor = System.Drawing.Color.Transparent;
			this.chkLogLevelDebug.ForeColor = System.Drawing.Color.Black;
			this.chkLogLevelDebug.Location = new System.Drawing.Point(26, 75);
			this.chkLogLevelDebug.Margin = new System.Windows.Forms.Padding(4);
			this.chkLogLevelDebug.Name = "chkLogLevelDebug";
			this.chkLogLevelDebug.Size = new System.Drawing.Size(972, 38);
			this.chkLogLevelDebug.TabIndex = 63;
			this.chkLogLevelDebug.Text = "Log debug (for troubleshooting)";
			this.chkLogLevelDebug.UseVisualStyleBackColor = false;
			// 
			// TxtLoggingPathComputed
			// 
			this.TxtLoggingPathComputed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TxtLoggingPathComputed.BackColor = System.Drawing.Color.Transparent;
			this.TxtLoggingPathComputed.ForeColor = System.Drawing.Color.Black;
			this.TxtLoggingPathComputed.Location = new System.Drawing.Point(111, 153);
			this.TxtLoggingPathComputed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.TxtLoggingPathComputed.Name = "TxtLoggingPathComputed";
			this.TxtLoggingPathComputed.Size = new System.Drawing.Size(879, 130);
			this.TxtLoggingPathComputed.TabIndex = 62;
			this.TxtLoggingPathComputed.Text = "Current Computed Path";
			this.TxtLoggingPathComputed.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblLoggingHelp
			// 
			this.lblLoggingHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblLoggingHelp.BackColor = System.Drawing.Color.Transparent;
			this.lblLoggingHelp.ForeColor = System.Drawing.Color.Black;
			this.lblLoggingHelp.Location = new System.Drawing.Point(26, 345);
			this.lblLoggingHelp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLoggingHelp.Name = "lblLoggingHelp";
			this.lblLoggingHelp.Size = new System.Drawing.Size(951, 144);
			this.lblLoggingHelp.TabIndex = 61;
			this.lblLoggingHelp.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// txtLogPath
			// 
			this.txtLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLogPath.Location = new System.Drawing.Point(111, 118);
			this.txtLogPath.Margin = new System.Windows.Forms.Padding(4);
			this.txtLogPath.Name = "txtLogPath";
			this.txtLogPath.Size = new System.Drawing.Size(884, 26);
			this.txtLogPath.TabIndex = 61;
			this.txtLogPath.TextChanged += new System.EventHandler(this.TxtLoggingPath_TextChanged);
			// 
			// lblLogPath
			// 
			this.lblLogPath.BackColor = System.Drawing.Color.Transparent;
			this.lblLogPath.ForeColor = System.Drawing.Color.Black;
			this.lblLogPath.Location = new System.Drawing.Point(21, 123);
			this.lblLogPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLogPath.Name = "lblLogPath";
			this.lblLogPath.Size = new System.Drawing.Size(81, 27);
			this.lblLogPath.TabIndex = 60;
			this.lblLogPath.Text = "Path:";
			// 
			// chkLoggingEnabled
			// 
			this.chkLoggingEnabled.BackColor = System.Drawing.Color.Transparent;
			this.chkLoggingEnabled.ForeColor = System.Drawing.Color.Black;
			this.chkLoggingEnabled.Location = new System.Drawing.Point(26, 30);
			this.chkLoggingEnabled.Margin = new System.Windows.Forms.Padding(4);
			this.chkLoggingEnabled.Name = "chkLoggingEnabled";
			this.chkLoggingEnabled.Size = new System.Drawing.Size(964, 38);
			this.chkLoggingEnabled.TabIndex = 55;
			this.chkLoggingEnabled.Text = "Logging on file enabled";
			this.chkLoggingEnabled.UseVisualStyleBackColor = false;
			// 
			// tabExperimentals
			// 
			this.tabExperimentals.Location = new System.Drawing.Point(4, 24);
			this.tabExperimentals.Margin = new System.Windows.Forms.Padding(4);
			this.tabExperimentals.Name = "tabExperimentals";
			this.tabExperimentals.Size = new System.Drawing.Size(1014, 635);
			this.tabExperimentals.TabIndex = 7;
			this.tabExperimentals.Text = "Experimentals";
			this.tabExperimentals.UseVisualStyleBackColor = true;
			// 
			// chkWindowsWintun
			// 
			this.chkWindowsWintun.BackColor = System.Drawing.Color.Transparent;
			this.chkWindowsWintun.ForeColor = System.Drawing.Color.Black;
			this.chkWindowsWintun.Location = new System.Drawing.Point(24, 27);
			this.chkWindowsWintun.Margin = new System.Windows.Forms.Padding(4);
			this.chkWindowsWintun.Name = "chkWindowsWintun";
			this.chkWindowsWintun.Size = new System.Drawing.Size(356, 33);
			this.chkWindowsWintun.TabIndex = 90;
			this.chkWindowsWintun.Text = "Use wintun driver (OpenVPN>=2.5)";
			this.chkWindowsWintun.UseVisualStyleBackColor = false;
			// 
			// Settings
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(1296, 732);
			this.Controls.Add(this.pnlCommands);
			this.Controls.Add(this.tabSettings);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(1309, 762);
			this.Name = "Settings";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			tabAdvanced.ResumeLayout(false);
			tabAdvanced.PerformLayout();
			this.pnlAdvancedGeneralWindowsOnly.ResumeLayout(false);
			tabDirectives.ResumeLayout(false);
			tabDirectives.PerformLayout();
			tabEvents.ResumeLayout(false);
			this.tabNetworking.ResumeLayout(false);
			this.mnuRoutes.ResumeLayout(false);
			this.pnlCommands.ResumeLayout(false);
			this.tabSettings.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.tabGeneral.PerformLayout();
			this.tabUI.ResumeLayout(false);
			this.tabUI.PerformLayout();
			this.tabProtocols.ResumeLayout(false);
			this.tabProxy.ResumeLayout(false);
			this.tabProxy.PerformLayout();
			this.tabRoutes.ResumeLayout(false);
			this.tabDNS.ResumeLayout(false);
			this.pnlDnsWindowsOnly.ResumeLayout(false);
			this.tabNetworkLock.ResumeLayout(false);
			this.tabNetworkLock.PerformLayout();
			this.tabLogging.ResumeLayout(false);
			this.tabLogging.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Skin.ColumnHeader colLogDate;
        private Skin.ColumnHeader colLogMessage;
        private Skin.CheckBox chkSystemStart;
        private Skin.Label lblProxyType;
        private Skin.ComboBox cboProxyMode;
        private Skin.TextBox txtProxyPort;
        private Skin.Label lblProxyPort;
        private Skin.TextBox txtProxyHost;
        private Skin.Label lblProxyHost;
        private Skin.Label lblProxyAuthentication;
        private Skin.ComboBox cboProxyAuthentication;
        private Skin.TextBox txtProxyPassword;
        private Skin.Label lblProxyPassword;
        private Skin.TextBox txtProxyLogin;
		private Skin.Label lblProxyLogin;        
        private Skin.Label label6;
        private Skin.ListView lstRoutes;
        private Skin.ColumnHeader colRoutesIp;
        private Skin.ColumnHeader colRoutesNotes;
        private Skin.ColumnHeader colRoutesAction;
        private Skin.Label label12;
        private Skin.Button cmdRouteEdit;
        private Skin.Button cmdRouteRemove;
        private Skin.Button cmdRouteAdd;
        private Skin.Panel pnlCommands;
        private Skin.Button cmdCancel;
        private Skin.Button cmdOk;
        private System.Windows.Forms.ContextMenuStrip mnuRoutes;
        private System.Windows.Forms.ToolStripMenuItem mnuRoutesAdd;
        private System.Windows.Forms.ToolStripMenuItem mnuRoutesRemove;
		private System.Windows.Forms.ToolStripMenuItem mnuRoutesEdit;
        private Skin.TabControl tabSettings;
        private Skin.TabPage tabGeneral;
		private Skin.TabPage tabUI;
		private Skin.TabPage tabProtocols;
        private Skin.TabPage tabProxy;
        private Skin.TabPage tabRoutes;
        private Skin.Button cmdExeBrowse;
        private Skin.TextBox txtExePath;
        private Skin.Label lblExePath;
		private Skin.CheckBox chkAdvancedCheckRoute;
        private Skin.CheckBox chkWindowsTapUp;
        private Skin.CheckBox chkExpert;
        private Skin.TextBox txtOpenVpnDirectivesBase;
        private Skin.TextBox txtOpenVpnDirectivesCustom;
        private Skin.Button cmdAdvancedEventsEdit;
        private Skin.Button cmdAdvancedEventsClear;
        private Skin.ListView lstAdvancedEvents;
        private Skin.Label label1;
        private Skin.Label label3;
        private Skin.Label label2;
        private Skin.ColumnHeader columnHeader3;
        private Skin.ColumnHeader columnHeader4;
		private Skin.Button cmdAdvancedUninstallDriverTap;
		private Skin.CheckBox chkAdvancedPingerEnabled;
		private System.Windows.Forms.GroupBox pnlAdvancedGeneralWindowsOnly;
		private Skin.TabPage tabLogging;
		private Skin.Label TxtLoggingPathComputed;
		private Skin.Label lblLoggingHelp;
		private Skin.TextBox txtLogPath;
		private Skin.Label lblLogPath;
		private Skin.CheckBox chkLoggingEnabled;
		private Skin.TabPage tabNetworkLock;
		private Skin.Label lblLockWhiteListIncomingIPs;
		private Skin.TextBox txtLockWhiteListIncomingIPs;
		private Skin.Label lblLockMode;
		private Skin.ComboBox cboLockMode;
		private Skin.Label lblLockRoutingOutWarning;
		private Skin.Label label17;
		private Skin.Label lblAdvancedManifestRefresh;
		private Skin.ComboBox cboAdvancedManifestRefresh;
		private Skin.TabPage tabDNS;
		private Skin.Label lblDnsSwitchMode;
		private Skin.ComboBox cboDnsSwitchMode;
		private Skin.CheckBox chkDnsCheck;
		private Skin.CheckBox chkLockAllowPing;
		private Skin.CheckBox chkLockAllowPrivate;
		private Skin.Label lblAdvancedCheckRoute;
		private Skin.Label lblExpert;
		private Skin.Label lblAdvancedPingerEnabled;
		private Skin.Label lblDnsServers;
		private Skin.Button cmdDnsEdit;
		private Skin.Button cmdDnsRemove;
		private Skin.Button cmdDnsAdd;
		private Skin.ListView lstDnsServers;
		private Skin.CheckBox chkLogLevelDebug;
		private Skin.CheckBox chkWindowsDisableDriverUpgrade;
        private Skin.ListView lstProtocols;
        private Skin.ColumnHeader colProtocolsDescription;
        private Skin.ColumnHeader colProtocolsProtocol;
        private Skin.ColumnHeader colProtocolsPort;
        private System.Windows.Forms.ColumnHeader colProtocolsEntry;
        private Skin.LinkLabel lnkProtocolsHelp2;
        private Skin.LinkLabel lnkProtocolsHelp1;
        private System.Windows.Forms.CheckBox chkProtocolsAutomatic;
        private Skin.CheckBox chkWindowsDebugWorkaround;
        private Skin.TextBox txtProxyTorControlPassword;
        private Skin.Label lblProxyTorControlPassword;
        private Skin.Button cmdProxyTorTest;
        private Skin.TextBox txtProxyTorControlPort;
        private Skin.Label lblProxyTorControlPort;
        private Skin.LinkLabel lnkProxyTorHelp;
        private Skin.LinkLabel lnkLockHelp;
        private Skin.LinkLabel lnkAdvancedHelp;
        private Skin.Label lblDnsCheck;
        private Skin.Label label10;
        private Skin.ComboBox cboOpenVpnDirectivesDefaultSkip;
        private Skin.Label lblLockAllowPing;
        private Skin.Label lblLockAllowPrivate;
        private Skin.LinkLabel lnkOpenVpnDirectivesHelp;
        private Skin.CheckBox chkOsSingleInstance;
        private Skin.Label lblConnect;
        private Skin.Label lblNetLock;
        private Skin.CheckBox chkConnect;
        private Skin.Label lblGeneralStartLast;
        private Skin.CheckBox chkNetLock;
        private Skin.CheckBox chkGeneralStartLast;
        private Skin.Button cmdResetToDefault;
        private Skin.Button cmdLoggingOpen;
		private Skin.Label lblLockAllowDNS;
		private Skin.CheckBox chkLockAllowDNS;
		private System.Windows.Forms.ColumnHeader colProtocolsTech;
		private System.Windows.Forms.TabPage tabExperimentals;
		private Skin.CheckBox chkAdvancedProviders;
		private Skin.Button cmdOpenVpnDirectivesCustomPathBrowse;
		private Skin.TextBox txtOpenVpnDirectivesCustomPath;
		private Skin.Label label8;
		private Skin.CheckBox chkAdvancedSkipAlreadyRun;
		private Skin.Label lblAdvancedProviders;
		private Skin.Label lblAdvancedSkipAlreadyRun;
		private Skin.Label lblProxyWhen;
		private Skin.ComboBox cboProxyWhen;
		private Skin.Label lblProtocolsAvailable;
		private Skin.Label label13;
		private Skin.TabPage tabNetworking;
		private Skin.Label lblNetworkEntryInterface;
		private Skin.ComboBox cboNetworkEntryInterface;
		private Skin.Label lblOpenVpnRcvbuf;
		private Skin.ComboBox cboOpenVpnRcvbuf;
		private Skin.Label lblOpenVpnSndbuf;
		private Skin.ComboBox cboOpenVpnSndbuf;
		private Skin.Label lblNetworkEntryIpLayer;
		private Skin.ComboBox cboNetworkEntryIpLayer;
		private Skin.Label lblLockOutgoing;
		private Skin.ComboBox cboLockOutgoing;
		private Skin.Label lblLockIncoming;
		private Skin.ComboBox cboLockIncoming;
		private System.Windows.Forms.GroupBox pnlDnsWindowsOnly;
		private Skin.CheckBox chkDnsIgnoreDNS6;
		private Skin.CheckBox chkDnsEnsureLock;
		private Skin.CheckBox chkDnsForceAllInterfaces;
		private Skin.Label lblNetworkIPv6Mode;
		private Skin.ComboBox cboNetworkIPv6Mode;
		private Skin.Label lblNetworkIPv4Mode;
		private Skin.ComboBox cboNetworkIPv4Mode;
		private Skin.Label lblDnsIgnoreDNS6;
		private Skin.Label lblDnsEnsureLock;
		private Skin.Label lblDnsForceAllInterfaces;
		private Skin.Label lblUiSkipProviderManifestFailed;
		private Skin.CheckBox chkUiSkipProviderManifestFailed;
		private Skin.Label lblUiStartMinimized;
		private Skin.CheckBox chkUiStartMinimized;
		private Skin.Label lblUiTrayMinimized;
		private Skin.CheckBox chkUiTrayMinimized;
		private Skin.Label lblUiTrayShow;
		private Skin.CheckBox chkUiTrayShow;
		private Skin.Label lblUiSystemNotifications;
		private Skin.CheckBox chkUiIEC;
		private Skin.CheckBox chkUiSystemNotifications;
		private Skin.Label chkUiFontGeneralTitle;
		private Skin.CheckBox chkUiFontGeneral;
		private Skin.Label lblUiExitConfirm;
		private Skin.CheckBox chkUiExitConfirm;
		private Skin.Label lblUiUnit;
		private Skin.ComboBox cboUiUnit;
		private Skin.Button cmdUiFontGeneral;
		private Skin.Label lblUiFontGeneral;
		private Skin.Label lblNetworkIPv6AutoSwitch;
		private Skin.CheckBox chkNetworkIPv6AutoSwitch;
		private Skin.Label lblNetworkIPv4AutoSwitch;
		private Skin.CheckBox chkNetworkIPv4AutoSwitch;
		private Skin.CheckBox chkWindowsSshPlinkForce;
		private Skin.Label lblAdvancedUpdaterChannel;
		private Skin.ComboBox cboAdvancedUpdaterChannel;
		private Skin.Label lblLockAllowDHCP;
		private Skin.CheckBox chkLockAllowDHCP;
		private Skin.CheckBox chkOpenVpnDirectivesAllowScriptSecurity;
		private Skin.Label lblStorageBrowser;
		private Skin.CheckBox chkStorageBrowser;
		private Skin.TextBox txtStoragePassword;
		private Skin.Label lblStorageMode;
		private Skin.ComboBox cboStorageMode;
		private Skin.Button cmdShellExternalClear;
		private Skin.Button cmdShellExternalView;
		private Skin.Label lblShellExternal;
		private Skin.CheckBox chkShellExternalRecommended;
		private Skin.Label lblLockWhiteListOutgoingIPs;
		private Skin.TextBox txtLockWhiteListOutgoingIPs;
		private Skin.CheckBox chkSystemService;
		private Skin.Label lblSystemService;
		private Skin.TextBox txtStoragePasswordConfirm;
		private Skin.Label lblStoragePasswordConfirm;
		private Skin.Label lblStoragePassword;
		private Skin.Label lbUiSkipPromotional;
		private Skin.CheckBox chkUiSkipPromotional;
		private Skin.Label lblHummingbirdPrefer;
		private Skin.CheckBox chkHummingbirdPrefer;
		private Skin.Button cmdAdvancedUninstallDriverWintun;
		private Skin.CheckBox chkWindowsWintun;
	}
}