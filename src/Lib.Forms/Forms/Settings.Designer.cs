namespace Eddie.Gui.Forms
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
            Eddie.Gui.Skin.TabPage tabAdvanced;
            Eddie.Gui.Skin.TabPage tabDirectives;
            Eddie.Gui.Skin.TabPage tabEventsw;
            Eddie.Gui.Skin.ColumnHeader columnHeader1;
            Eddie.Gui.Skin.ColumnHeader columnHeader2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            Eddie.Gui.Skin.ColumnHeader columnHeader5;
            this.lnkAdvancedHelp = new Eddie.Gui.Skin.LinkLabel();
            this.lblOpenVpnRcvbuf = new Eddie.Gui.Skin.Label();
            this.cboOpenVpnRcvbuf = new Eddie.Gui.Skin.ComboBox();
            this.lblOpenVpnSndbuf = new Eddie.Gui.Skin.Label();
            this.cboOpenVpnSndbuf = new Eddie.Gui.Skin.ComboBox();
            this.lblRouteRemoveDefault = new Eddie.Gui.Skin.Label();
            this.chkRouteRemoveDefault = new Eddie.Gui.Skin.CheckBox();
            this.lblExpert = new Eddie.Gui.Skin.Label();
            this.lblAdvancedPingerEnabled = new Eddie.Gui.Skin.Label();
            this.lblAdvancedCheckRoute = new Eddie.Gui.Skin.Label();
            this.lblIpV6 = new Eddie.Gui.Skin.Label();
            this.cboIpV6 = new Eddie.Gui.Skin.ComboBox();
            this.lblAdvancedManifestRefresh = new Eddie.Gui.Skin.Label();
            this.cboAdvancedManifestRefresh = new Eddie.Gui.Skin.ComboBox();
            this.pnlAdvancedGeneralWindowsOnly = new System.Windows.Forms.GroupBox();
            this.chkWindowsDnsLock = new Eddie.Gui.Skin.CheckBox();
            this.chkWindowsDnsForceAllInterfaces = new Eddie.Gui.Skin.CheckBox();
            this.chkWindowsDisableDriverUpgrade = new Eddie.Gui.Skin.CheckBox();
            this.chkWindowsIPv6DisableAtOs = new Eddie.Gui.Skin.CheckBox();
            this.chkWindowsWfp = new Eddie.Gui.Skin.CheckBox();
            this.chkWindowsTapUp = new Eddie.Gui.Skin.CheckBox();
            this.chkWindowsDhcpSwitch = new Eddie.Gui.Skin.CheckBox();
            this.cmdAdvancedUninstallDriver = new Eddie.Gui.Skin.Button();
            this.chkAdvancedPingerEnabled = new Eddie.Gui.Skin.CheckBox();
            this.cmdExeBrowse = new Eddie.Gui.Skin.Button();
            this.txtExePath = new Eddie.Gui.Skin.TextBox();
            this.lblExePath = new Eddie.Gui.Skin.Label();
            this.chkAdvancedCheckRoute = new Eddie.Gui.Skin.CheckBox();
            this.chkExpert = new Eddie.Gui.Skin.CheckBox();
            this.lnkOpenVpnDirectivesHelp = new Eddie.Gui.Skin.LinkLabel();
            this.cboOpenVpnDirectivesDefaultSkip = new Eddie.Gui.Skin.ComboBox();
            this.label3 = new Eddie.Gui.Skin.Label();
            this.label2 = new Eddie.Gui.Skin.Label();
            this.txtOpenVpnDirectivesBase = new Eddie.Gui.Skin.TextBox();
            this.txtOpenVpnDirectivesCustom = new Eddie.Gui.Skin.TextBox();
            this.cmdAdvancedEventsEdit = new Eddie.Gui.Skin.Button();
            this.cmdAdvancedEventsClear = new Eddie.Gui.Skin.Button();
            this.lstAdvancedEvents = new Eddie.Gui.Skin.ListView();
            this.columnHeader3 = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.columnHeader4 = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.imgRoutes = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new Eddie.Gui.Skin.Label();
            this.mnuRoutes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuRoutesAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRoutesRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRoutesEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.colLogDate = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.colLogMessage = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.pnlCommands = new Eddie.Gui.Skin.Panel();
            this.cmdCancel = new Eddie.Gui.Skin.Button();
            this.cmdOk = new Eddie.Gui.Skin.Button();
            this.tabSettings = new Eddie.Gui.Skin.TabControl();
            this.tabGeneral = new Eddie.Gui.Skin.TabPage();
            this.lblConnect = new Eddie.Gui.Skin.Label();
            this.lblNetLock = new Eddie.Gui.Skin.Label();
            this.chkConnect = new Eddie.Gui.Skin.CheckBox();
            this.lblGeneralStartLast = new Eddie.Gui.Skin.Label();
            this.chkNetLock = new Eddie.Gui.Skin.CheckBox();
            this.chkGeneralStartLast = new Eddie.Gui.Skin.CheckBox();
            this.label7 = new Eddie.Gui.Skin.Label();
            this.chkUiFontGeneral = new Eddie.Gui.Skin.CheckBox();
            this.label5 = new Eddie.Gui.Skin.Label();
            this.chkExitConfirm = new Eddie.Gui.Skin.CheckBox();
            this.label4 = new Eddie.Gui.Skin.Label();
            this.cboUiUnit = new Eddie.Gui.Skin.ComboBox();
            this.cmdUiFontGeneral = new Eddie.Gui.Skin.Button();
            this.lblUiFontGeneral = new Eddie.Gui.Skin.Label();
            this.pnlGeneralWindowsOnly = new System.Windows.Forms.GroupBox();
            this.chkOsSingleInstance = new Eddie.Gui.Skin.CheckBox();
            this.chkSystemNotifications = new Eddie.Gui.Skin.CheckBox();
            this.chkSystemStart = new Eddie.Gui.Skin.CheckBox();
            this.chkMinimizeTray = new Eddie.Gui.Skin.CheckBox();
            this.cmdTos = new Eddie.Gui.Skin.Button();
            this.tabProtocols = new Eddie.Gui.Skin.TabPage();
            this.lnkProtocolsHelp2 = new Eddie.Gui.Skin.LinkLabel();
            this.lnkProtocolsHelp1 = new Eddie.Gui.Skin.LinkLabel();
            this.chkProtocolsAutomatic = new System.Windows.Forms.CheckBox();
            this.lstProtocols = new Eddie.Gui.Skin.ListView();
            this.colProtocolsProtocol = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.colProtocolsPort = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.colProtocolsEntry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProtocolsDescription = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.tabProxy = new Eddie.Gui.Skin.TabPage();
            this.lnkProxyTorHelp = new Eddie.Gui.Skin.LinkLabel();
            this.txtProxyTorControlPassword = new Eddie.Gui.Skin.TextBox();
            this.lblProxyTorControlPassword = new Eddie.Gui.Skin.Label();
            this.cmdProxyTorTest = new Eddie.Gui.Skin.Button();
            this.txtProxyTorControlPort = new Eddie.Gui.Skin.TextBox();
            this.lblProxyTorControlPort = new Eddie.Gui.Skin.Label();
            this.label17 = new Eddie.Gui.Skin.Label();
            this.label12 = new Eddie.Gui.Skin.Label();
            this.lblProxyAuthentication = new Eddie.Gui.Skin.Label();
            this.cboProxyAuthentication = new Eddie.Gui.Skin.ComboBox();
            this.txtProxyPassword = new Eddie.Gui.Skin.TextBox();
            this.lblProxyPassword = new Eddie.Gui.Skin.Label();
            this.txtProxyLogin = new Eddie.Gui.Skin.TextBox();
            this.lblProxyLogin = new Eddie.Gui.Skin.Label();
            this.lblProxyType = new Eddie.Gui.Skin.Label();
            this.cboProxyMode = new Eddie.Gui.Skin.ComboBox();
            this.txtProxyPort = new Eddie.Gui.Skin.TextBox();
            this.lblProxyPort = new Eddie.Gui.Skin.Label();
            this.txtProxyHost = new Eddie.Gui.Skin.TextBox();
            this.lblProxyHost = new Eddie.Gui.Skin.Label();
            this.tabRoutes = new Eddie.Gui.Skin.TabPage();
            this.lblRoutesNetworkLockWarning = new Eddie.Gui.Skin.Label();
            this.cmdRouteEdit = new Eddie.Gui.Skin.Button();
            this.cmdRouteRemove = new Eddie.Gui.Skin.Button();
            this.cmdRouteAdd = new Eddie.Gui.Skin.Button();
            this.label6 = new Eddie.Gui.Skin.Label();
            this.cboRoutesOtherwise = new Eddie.Gui.Skin.ComboBox();
            this.lblRoutesOtherwise = new Eddie.Gui.Skin.Label();
            this.lstRoutes = new Eddie.Gui.Skin.ListView();
            this.colRoutesIp = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.colRoutesAction = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.colRoutesNotes = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            this.tabDNS = new Eddie.Gui.Skin.TabPage();
            this.label10 = new Eddie.Gui.Skin.Label();
            this.lblDnsCheck = new Eddie.Gui.Skin.Label();
            this.lblDnsServers = new Eddie.Gui.Skin.Label();
            this.cmdDnsEdit = new Eddie.Gui.Skin.Button();
            this.cmdDnsRemove = new Eddie.Gui.Skin.Button();
            this.cmdDnsAdd = new Eddie.Gui.Skin.Button();
            this.lstDnsServers = new Eddie.Gui.Skin.ListView();
            this.lblDnsSwitchMode = new Eddie.Gui.Skin.Label();
            this.cboDnsSwitchMode = new Eddie.Gui.Skin.ComboBox();
            this.chkDnsCheck = new Eddie.Gui.Skin.CheckBox();
            this.tabNetworkLock = new Eddie.Gui.Skin.TabPage();
            this.lblLockAllowPing = new Eddie.Gui.Skin.Label();
            this.lblLockAllowPrivate = new Eddie.Gui.Skin.Label();
            this.lnkLockHelp = new Eddie.Gui.Skin.LinkLabel();
            this.chkLockAllowPing = new Eddie.Gui.Skin.CheckBox();
            this.chkLockAllowPrivate = new Eddie.Gui.Skin.CheckBox();
            this.lblLockRoutingOutWarning = new Eddie.Gui.Skin.Label();
            this.lblLockAllowedIPS = new Eddie.Gui.Skin.Label();
            this.txtLockAllowedIPS = new Eddie.Gui.Skin.TextBox();
            this.lblLockMode = new Eddie.Gui.Skin.Label();
            this.cboLockMode = new Eddie.Gui.Skin.ComboBox();
            this.tabLogging = new Eddie.Gui.Skin.TabPage();
            this.chkLogLevelDebug = new Eddie.Gui.Skin.CheckBox();
            this.TxtLoggingPathComputed = new Eddie.Gui.Skin.Label();
            this.lblLoggingHelp = new Eddie.Gui.Skin.Label();
            this.txtLogPath = new Eddie.Gui.Skin.TextBox();
            this.lblLogPath = new Eddie.Gui.Skin.Label();
            this.chkLoggingEnabled = new Eddie.Gui.Skin.CheckBox();
            tabAdvanced = new Eddie.Gui.Skin.TabPage();
            tabDirectives = new Eddie.Gui.Skin.TabPage();
            tabEventsw = new Eddie.Gui.Skin.TabPage();
            columnHeader1 = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            columnHeader2 = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            columnHeader5 = ((Eddie.Gui.Skin.ColumnHeader)(new Eddie.Gui.Skin.ColumnHeader()));
            tabAdvanced.SuspendLayout();
            this.pnlAdvancedGeneralWindowsOnly.SuspendLayout();
            tabDirectives.SuspendLayout();
            tabEventsw.SuspendLayout();
            this.mnuRoutes.SuspendLayout();
            this.pnlCommands.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.pnlGeneralWindowsOnly.SuspendLayout();
            this.tabProtocols.SuspendLayout();
            this.tabProxy.SuspendLayout();
            this.tabRoutes.SuspendLayout();
            this.tabDNS.SuspendLayout();
            this.tabNetworkLock.SuspendLayout();
            this.tabLogging.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabAdvanced
            // 
            tabAdvanced.BackColor = System.Drawing.Color.White;
            tabAdvanced.Controls.Add(this.lnkAdvancedHelp);
            tabAdvanced.Controls.Add(this.lblOpenVpnRcvbuf);
            tabAdvanced.Controls.Add(this.cboOpenVpnRcvbuf);
            tabAdvanced.Controls.Add(this.lblOpenVpnSndbuf);
            tabAdvanced.Controls.Add(this.cboOpenVpnSndbuf);
            tabAdvanced.Controls.Add(this.lblRouteRemoveDefault);
            tabAdvanced.Controls.Add(this.chkRouteRemoveDefault);
            tabAdvanced.Controls.Add(this.lblExpert);
            tabAdvanced.Controls.Add(this.lblAdvancedPingerEnabled);
            tabAdvanced.Controls.Add(this.lblAdvancedCheckRoute);
            tabAdvanced.Controls.Add(this.lblIpV6);
            tabAdvanced.Controls.Add(this.cboIpV6);
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
            tabAdvanced.Name = "tabAdvanced";
            tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
            tabAdvanced.Size = new System.Drawing.Size(673, 337);
            tabAdvanced.TabIndex = 0;
            tabAdvanced.Text = "Advanced";
            // 
            // lnkAdvancedHelp
            // 
            this.lnkAdvancedHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkAdvancedHelp.BackColor = System.Drawing.Color.Maroon;
            this.lnkAdvancedHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkAdvancedHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
            this.lnkAdvancedHelp.Location = new System.Drawing.Point(13, 304);
            this.lnkAdvancedHelp.Name = "lnkAdvancedHelp";
            this.lnkAdvancedHelp.Size = new System.Drawing.Size(650, 22);
            this.lnkAdvancedHelp.TabIndex = 87;
            this.lnkAdvancedHelp.TabStop = true;
            this.lnkAdvancedHelp.Text = "More about Advanced Features";
            this.lnkAdvancedHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkAdvancedHelp.Click += new System.EventHandler(this.lnkAdvancedHelp_LinkClicked);
            // 
            // lblOpenVpnRcvbuf
            // 
            this.lblOpenVpnRcvbuf.BackColor = System.Drawing.Color.Transparent;
            this.lblOpenVpnRcvbuf.ForeColor = System.Drawing.Color.Black;
            this.lblOpenVpnRcvbuf.Location = new System.Drawing.Point(14, 228);
            this.lblOpenVpnRcvbuf.Name = "lblOpenVpnRcvbuf";
            this.lblOpenVpnRcvbuf.Size = new System.Drawing.Size(220, 21);
            this.lblOpenVpnRcvbuf.TabIndex = 85;
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
            this.cboOpenVpnRcvbuf.Location = new System.Drawing.Point(246, 228);
            this.cboOpenVpnRcvbuf.Name = "cboOpenVpnRcvbuf";
            this.cboOpenVpnRcvbuf.Size = new System.Drawing.Size(133, 21);
            this.cboOpenVpnRcvbuf.TabIndex = 84;
            // 
            // lblOpenVpnSndbuf
            // 
            this.lblOpenVpnSndbuf.BackColor = System.Drawing.Color.Transparent;
            this.lblOpenVpnSndbuf.ForeColor = System.Drawing.Color.Black;
            this.lblOpenVpnSndbuf.Location = new System.Drawing.Point(14, 198);
            this.lblOpenVpnSndbuf.Name = "lblOpenVpnSndbuf";
            this.lblOpenVpnSndbuf.Size = new System.Drawing.Size(220, 21);
            this.lblOpenVpnSndbuf.TabIndex = 83;
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
            this.cboOpenVpnSndbuf.Location = new System.Drawing.Point(246, 198);
            this.cboOpenVpnSndbuf.Name = "cboOpenVpnSndbuf";
            this.cboOpenVpnSndbuf.Size = new System.Drawing.Size(133, 21);
            this.cboOpenVpnSndbuf.TabIndex = 82;
            // 
            // lblRouteRemoveDefault
            // 
            this.lblRouteRemoveDefault.BackColor = System.Drawing.Color.Transparent;
            this.lblRouteRemoveDefault.ForeColor = System.Drawing.Color.Black;
            this.lblRouteRemoveDefault.Location = new System.Drawing.Point(14, 168);
            this.lblRouteRemoveDefault.Name = "lblRouteRemoveDefault";
            this.lblRouteRemoveDefault.Size = new System.Drawing.Size(220, 22);
            this.lblRouteRemoveDefault.TabIndex = 81;
            this.lblRouteRemoveDefault.Text = "Remove the gateway route:";
            this.lblRouteRemoveDefault.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkRouteRemoveDefault
            // 
            this.chkRouteRemoveDefault.BackColor = System.Drawing.Color.Transparent;
            this.chkRouteRemoveDefault.ForeColor = System.Drawing.Color.Black;
            this.chkRouteRemoveDefault.Location = new System.Drawing.Point(246, 168);
            this.chkRouteRemoveDefault.Name = "chkRouteRemoveDefault";
            this.chkRouteRemoveDefault.Size = new System.Drawing.Size(134, 25);
            this.chkRouteRemoveDefault.TabIndex = 80;
            this.chkRouteRemoveDefault.UseVisualStyleBackColor = false;
            // 
            // lblExpert
            // 
            this.lblExpert.BackColor = System.Drawing.Color.Transparent;
            this.lblExpert.ForeColor = System.Drawing.Color.Black;
            this.lblExpert.Location = new System.Drawing.Point(10, 18);
            this.lblExpert.Name = "lblExpert";
            this.lblExpert.Size = new System.Drawing.Size(227, 22);
            this.lblExpert.TabIndex = 79;
            this.lblExpert.Text = "Expert Mode:";
            this.lblExpert.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAdvancedPingerEnabled
            // 
            this.lblAdvancedPingerEnabled.BackColor = System.Drawing.Color.Transparent;
            this.lblAdvancedPingerEnabled.ForeColor = System.Drawing.Color.Black;
            this.lblAdvancedPingerEnabled.Location = new System.Drawing.Point(14, 138);
            this.lblAdvancedPingerEnabled.Name = "lblAdvancedPingerEnabled";
            this.lblAdvancedPingerEnabled.Size = new System.Drawing.Size(220, 22);
            this.lblAdvancedPingerEnabled.TabIndex = 78;
            this.lblAdvancedPingerEnabled.Text = "Enable Pinger / Latency Tests:";
            this.lblAdvancedPingerEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAdvancedCheckRoute
            // 
            this.lblAdvancedCheckRoute.BackColor = System.Drawing.Color.Transparent;
            this.lblAdvancedCheckRoute.ForeColor = System.Drawing.Color.Black;
            this.lblAdvancedCheckRoute.Location = new System.Drawing.Point(14, 48);
            this.lblAdvancedCheckRoute.Name = "lblAdvancedCheckRoute";
            this.lblAdvancedCheckRoute.Size = new System.Drawing.Size(223, 22);
            this.lblAdvancedCheckRoute.TabIndex = 77;
            this.lblAdvancedCheckRoute.Text = "Check if the tunnel works:";
            this.lblAdvancedCheckRoute.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblIpV6
            // 
            this.lblIpV6.BackColor = System.Drawing.Color.Transparent;
            this.lblIpV6.ForeColor = System.Drawing.Color.Black;
            this.lblIpV6.Location = new System.Drawing.Point(17, 78);
            this.lblIpV6.Name = "lblIpV6";
            this.lblIpV6.Size = new System.Drawing.Size(220, 21);
            this.lblIpV6.TabIndex = 76;
            this.lblIpV6.Text = "IPv6:";
            this.lblIpV6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboIpV6
            // 
            this.cboIpV6.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboIpV6.FormattingEnabled = true;
            this.cboIpV6.Items.AddRange(new object[] {
            "None",
            "Disable"});
            this.cboIpV6.Location = new System.Drawing.Point(246, 78);
            this.cboIpV6.Name = "cboIpV6";
            this.cboIpV6.Size = new System.Drawing.Size(133, 21);
            this.cboIpV6.TabIndex = 75;
            // 
            // lblAdvancedManifestRefresh
            // 
            this.lblAdvancedManifestRefresh.BackColor = System.Drawing.Color.Transparent;
            this.lblAdvancedManifestRefresh.ForeColor = System.Drawing.Color.Black;
            this.lblAdvancedManifestRefresh.Location = new System.Drawing.Point(14, 108);
            this.lblAdvancedManifestRefresh.Name = "lblAdvancedManifestRefresh";
            this.lblAdvancedManifestRefresh.Size = new System.Drawing.Size(220, 21);
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
            this.cboAdvancedManifestRefresh.Location = new System.Drawing.Point(246, 108);
            this.cboAdvancedManifestRefresh.Name = "cboAdvancedManifestRefresh";
            this.cboAdvancedManifestRefresh.Size = new System.Drawing.Size(133, 21);
            this.cboAdvancedManifestRefresh.TabIndex = 73;
            // 
            // pnlAdvancedGeneralWindowsOnly
            // 
            this.pnlAdvancedGeneralWindowsOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsDnsLock);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsDnsForceAllInterfaces);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsDisableDriverUpgrade);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsIPv6DisableAtOs);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsWfp);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsTapUp);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsDhcpSwitch);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.cmdAdvancedUninstallDriver);
            this.pnlAdvancedGeneralWindowsOnly.Location = new System.Drawing.Point(403, 11);
            this.pnlAdvancedGeneralWindowsOnly.Name = "pnlAdvancedGeneralWindowsOnly";
            this.pnlAdvancedGeneralWindowsOnly.Size = new System.Drawing.Size(259, 241);
            this.pnlAdvancedGeneralWindowsOnly.TabIndex = 69;
            this.pnlAdvancedGeneralWindowsOnly.TabStop = false;
            this.pnlAdvancedGeneralWindowsOnly.Text = "Microsoft Windows Only";
            // 
            // chkWindowsDnsLock
            // 
            this.chkWindowsDnsLock.BackColor = System.Drawing.Color.Transparent;
            this.chkWindowsDnsLock.ForeColor = System.Drawing.Color.Black;
            this.chkWindowsDnsLock.Location = new System.Drawing.Point(16, 161);
            this.chkWindowsDnsLock.Name = "chkWindowsDnsLock";
            this.chkWindowsDnsLock.Size = new System.Drawing.Size(237, 22);
            this.chkWindowsDnsLock.TabIndex = 89;
            this.chkWindowsDnsLock.Text = "Ensure DNS Lock";
            this.chkWindowsDnsLock.UseVisualStyleBackColor = false;
            // 
            // chkWindowsDnsForceAllInterfaces
            // 
            this.chkWindowsDnsForceAllInterfaces.BackColor = System.Drawing.Color.Transparent;
            this.chkWindowsDnsForceAllInterfaces.ForeColor = System.Drawing.Color.Black;
            this.chkWindowsDnsForceAllInterfaces.Location = new System.Drawing.Point(16, 133);
            this.chkWindowsDnsForceAllInterfaces.Name = "chkWindowsDnsForceAllInterfaces";
            this.chkWindowsDnsForceAllInterfaces.Size = new System.Drawing.Size(237, 22);
            this.chkWindowsDnsForceAllInterfaces.TabIndex = 88;
            this.chkWindowsDnsForceAllInterfaces.Text = "Force all network interfaces for DNS";
            this.chkWindowsDnsForceAllInterfaces.UseVisualStyleBackColor = false;
            // 
            // chkWindowsDisableDriverUpgrade
            // 
            this.chkWindowsDisableDriverUpgrade.BackColor = System.Drawing.Color.Transparent;
            this.chkWindowsDisableDriverUpgrade.ForeColor = System.Drawing.Color.Black;
            this.chkWindowsDisableDriverUpgrade.Location = new System.Drawing.Point(16, 77);
            this.chkWindowsDisableDriverUpgrade.Name = "chkWindowsDisableDriverUpgrade";
            this.chkWindowsDisableDriverUpgrade.Size = new System.Drawing.Size(237, 22);
            this.chkWindowsDisableDriverUpgrade.TabIndex = 66;
            this.chkWindowsDisableDriverUpgrade.Text = "Disable driver upgrade";
            this.chkWindowsDisableDriverUpgrade.UseVisualStyleBackColor = false;
            // 
            // chkWindowsIPv6DisableAtOs
            // 
            this.chkWindowsIPv6DisableAtOs.BackColor = System.Drawing.Color.Transparent;
            this.chkWindowsIPv6DisableAtOs.ForeColor = System.Drawing.Color.Black;
            this.chkWindowsIPv6DisableAtOs.Location = new System.Drawing.Point(16, 105);
            this.chkWindowsIPv6DisableAtOs.Name = "chkWindowsIPv6DisableAtOs";
            this.chkWindowsIPv6DisableAtOs.Size = new System.Drawing.Size(237, 22);
            this.chkWindowsIPv6DisableAtOs.TabIndex = 87;
            this.chkWindowsIPv6DisableAtOs.Text = "Disable IPv6 at OS level if requested";
            this.chkWindowsIPv6DisableAtOs.UseVisualStyleBackColor = false;
            // 
            // chkWindowsWfp
            // 
            this.chkWindowsWfp.BackColor = System.Drawing.Color.Transparent;
            this.chkWindowsWfp.ForeColor = System.Drawing.Color.Black;
            this.chkWindowsWfp.Location = new System.Drawing.Point(178, 185);
            this.chkWindowsWfp.Name = "chkWindowsWfp";
            this.chkWindowsWfp.Size = new System.Drawing.Size(75, 19);
            this.chkWindowsWfp.TabIndex = 86;
            this.chkWindowsWfp.Text = "WFP";
            this.chkWindowsWfp.UseVisualStyleBackColor = false;
            // 
            // chkWindowsTapUp
            // 
            this.chkWindowsTapUp.BackColor = System.Drawing.Color.Transparent;
            this.chkWindowsTapUp.ForeColor = System.Drawing.Color.Black;
            this.chkWindowsTapUp.Location = new System.Drawing.Point(16, 21);
            this.chkWindowsTapUp.Name = "chkWindowsTapUp";
            this.chkWindowsTapUp.Size = new System.Drawing.Size(237, 22);
            this.chkWindowsTapUp.TabIndex = 55;
            this.chkWindowsTapUp.Text = "Force TAP interface UP";
            this.chkWindowsTapUp.UseVisualStyleBackColor = false;
            // 
            // chkWindowsDhcpSwitch
            // 
            this.chkWindowsDhcpSwitch.BackColor = System.Drawing.Color.Transparent;
            this.chkWindowsDhcpSwitch.ForeColor = System.Drawing.Color.Black;
            this.chkWindowsDhcpSwitch.Location = new System.Drawing.Point(16, 49);
            this.chkWindowsDhcpSwitch.Name = "chkWindowsDhcpSwitch";
            this.chkWindowsDhcpSwitch.Size = new System.Drawing.Size(237, 22);
            this.chkWindowsDhcpSwitch.TabIndex = 64;
            this.chkWindowsDhcpSwitch.Text = "Switch DHCP to Static";
            this.chkWindowsDhcpSwitch.UseVisualStyleBackColor = false;
            // 
            // cmdAdvancedUninstallDriver
            // 
            this.cmdAdvancedUninstallDriver.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdAdvancedUninstallDriver.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdAdvancedUninstallDriver.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdAdvancedUninstallDriver.FlatAppearance.BorderSize = 0;
            this.cmdAdvancedUninstallDriver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdAdvancedUninstallDriver.Location = new System.Drawing.Point(16, 207);
            this.cmdAdvancedUninstallDriver.Name = "cmdAdvancedUninstallDriver";
            this.cmdAdvancedUninstallDriver.Size = new System.Drawing.Size(237, 27);
            this.cmdAdvancedUninstallDriver.TabIndex = 65;
            this.cmdAdvancedUninstallDriver.Text = "Uninstall Driver";
            this.cmdAdvancedUninstallDriver.UseVisualStyleBackColor = true;
            this.cmdAdvancedUninstallDriver.Click += new System.EventHandler(this.cmdAdvancedUninstallDriver_Click);
            // 
            // chkAdvancedPingerEnabled
            // 
            this.chkAdvancedPingerEnabled.BackColor = System.Drawing.Color.Transparent;
            this.chkAdvancedPingerEnabled.ForeColor = System.Drawing.Color.Black;
            this.chkAdvancedPingerEnabled.Location = new System.Drawing.Point(246, 138);
            this.chkAdvancedPingerEnabled.Name = "chkAdvancedPingerEnabled";
            this.chkAdvancedPingerEnabled.Size = new System.Drawing.Size(134, 25);
            this.chkAdvancedPingerEnabled.TabIndex = 66;
            this.chkAdvancedPingerEnabled.UseVisualStyleBackColor = false;
            // 
            // cmdExeBrowse
            // 
            this.cmdExeBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdExeBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdExeBrowse.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdExeBrowse.FlatAppearance.BorderSize = 0;
            this.cmdExeBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdExeBrowse.Image = global::Eddie.Lib.Forms.Properties.Resources.browse;
            this.cmdExeBrowse.Location = new System.Drawing.Point(426, 258);
            this.cmdExeBrowse.Name = "cmdExeBrowse";
            this.cmdExeBrowse.Size = new System.Drawing.Size(33, 20);
            this.cmdExeBrowse.TabIndex = 60;
            this.cmdExeBrowse.UseVisualStyleBackColor = true;
            this.cmdExeBrowse.Click += new System.EventHandler(this.cmdExeBrowse_Click);
            // 
            // txtExePath
            // 
            this.txtExePath.Location = new System.Drawing.Point(246, 258);
            this.txtExePath.Name = "txtExePath";
            this.txtExePath.Size = new System.Drawing.Size(174, 20);
            this.txtExePath.TabIndex = 59;
            // 
            // lblExePath
            // 
            this.lblExePath.BackColor = System.Drawing.Color.Transparent;
            this.lblExePath.ForeColor = System.Drawing.Color.Black;
            this.lblExePath.Location = new System.Drawing.Point(14, 261);
            this.lblExePath.Name = "lblExePath";
            this.lblExePath.Size = new System.Drawing.Size(220, 13);
            this.lblExePath.TabIndex = 58;
            this.lblExePath.Text = "OpenVPN Custom Path:";
            this.lblExePath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkAdvancedCheckRoute
            // 
            this.chkAdvancedCheckRoute.BackColor = System.Drawing.Color.Transparent;
            this.chkAdvancedCheckRoute.ForeColor = System.Drawing.Color.Black;
            this.chkAdvancedCheckRoute.Location = new System.Drawing.Point(246, 48);
            this.chkAdvancedCheckRoute.Name = "chkAdvancedCheckRoute";
            this.chkAdvancedCheckRoute.Size = new System.Drawing.Size(134, 25);
            this.chkAdvancedCheckRoute.TabIndex = 57;
            this.chkAdvancedCheckRoute.UseVisualStyleBackColor = false;
            // 
            // chkExpert
            // 
            this.chkExpert.BackColor = System.Drawing.Color.Transparent;
            this.chkExpert.ForeColor = System.Drawing.Color.Black;
            this.chkExpert.Location = new System.Drawing.Point(246, 18);
            this.chkExpert.Name = "chkExpert";
            this.chkExpert.Size = new System.Drawing.Size(134, 22);
            this.chkExpert.TabIndex = 54;
            this.chkExpert.UseVisualStyleBackColor = false;
            // 
            // tabDirectives
            // 
            tabDirectives.BackColor = System.Drawing.Color.White;
            tabDirectives.Controls.Add(this.lnkOpenVpnDirectivesHelp);
            tabDirectives.Controls.Add(this.cboOpenVpnDirectivesDefaultSkip);
            tabDirectives.Controls.Add(this.label3);
            tabDirectives.Controls.Add(this.label2);
            tabDirectives.Controls.Add(this.txtOpenVpnDirectivesBase);
            tabDirectives.Controls.Add(this.txtOpenVpnDirectivesCustom);
            tabDirectives.Location = new System.Drawing.Point(4, 24);
            tabDirectives.Name = "tabDirectives";
            tabDirectives.Padding = new System.Windows.Forms.Padding(3);
            tabDirectives.Size = new System.Drawing.Size(673, 337);
            tabDirectives.TabIndex = 1;
            tabDirectives.Text = "OVPN directives";
            // 
            // lnkOpenVpnDirectivesHelp
            // 
            this.lnkOpenVpnDirectivesHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkOpenVpnDirectivesHelp.BackColor = System.Drawing.Color.Maroon;
            this.lnkOpenVpnDirectivesHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkOpenVpnDirectivesHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
            this.lnkOpenVpnDirectivesHelp.Location = new System.Drawing.Point(13, 304);
            this.lnkOpenVpnDirectivesHelp.Name = "lnkOpenVpnDirectivesHelp";
            this.lnkOpenVpnDirectivesHelp.Size = new System.Drawing.Size(650, 22);
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
            this.cboOpenVpnDirectivesDefaultSkip.Location = new System.Drawing.Point(334, 279);
            this.cboOpenVpnDirectivesDefaultSkip.Name = "cboOpenVpnDirectivesDefaultSkip";
            this.cboOpenVpnDirectivesDefaultSkip.Size = new System.Drawing.Size(329, 21);
            this.cboOpenVpnDirectivesDefaultSkip.TabIndex = 75;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(334, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(331, 20);
            this.label3.TabIndex = 61;
            this.label3.Text = "Base directives:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(13, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(315, 20);
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
            this.txtOpenVpnDirectivesBase.Location = new System.Drawing.Point(334, 30);
            this.txtOpenVpnDirectivesBase.Multiline = true;
            this.txtOpenVpnDirectivesBase.Name = "txtOpenVpnDirectivesBase";
            this.txtOpenVpnDirectivesBase.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOpenVpnDirectivesBase.Size = new System.Drawing.Size(329, 243);
            this.txtOpenVpnDirectivesBase.TabIndex = 58;
            // 
            // txtOpenVpnDirectivesCustom
            // 
            this.txtOpenVpnDirectivesCustom.AcceptsReturn = true;
            this.txtOpenVpnDirectivesCustom.AcceptsTab = true;
            this.txtOpenVpnDirectivesCustom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtOpenVpnDirectivesCustom.Location = new System.Drawing.Point(13, 30);
            this.txtOpenVpnDirectivesCustom.Multiline = true;
            this.txtOpenVpnDirectivesCustom.Name = "txtOpenVpnDirectivesCustom";
            this.txtOpenVpnDirectivesCustom.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOpenVpnDirectivesCustom.Size = new System.Drawing.Size(315, 243);
            this.txtOpenVpnDirectivesCustom.TabIndex = 57;
            // 
            // tabEventsw
            // 
            tabEventsw.BackColor = System.Drawing.Color.White;
            tabEventsw.Controls.Add(this.cmdAdvancedEventsEdit);
            tabEventsw.Controls.Add(this.cmdAdvancedEventsClear);
            tabEventsw.Controls.Add(this.lstAdvancedEvents);
            tabEventsw.Controls.Add(this.label1);
            tabEventsw.Location = new System.Drawing.Point(4, 24);
            tabEventsw.Name = "tabEventsw";
            tabEventsw.Size = new System.Drawing.Size(673, 337);
            tabEventsw.TabIndex = 2;
            tabEventsw.Text = "Events";
            // 
            // cmdAdvancedEventsEdit
            // 
            this.cmdAdvancedEventsEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdAdvancedEventsEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdAdvancedEventsEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdAdvancedEventsEdit.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdAdvancedEventsEdit.FlatAppearance.BorderSize = 0;
            this.cmdAdvancedEventsEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdAdvancedEventsEdit.Image = global::Eddie.Lib.Forms.Properties.Resources.edit;
            this.cmdAdvancedEventsEdit.Location = new System.Drawing.Point(633, 64);
            this.cmdAdvancedEventsEdit.Name = "cmdAdvancedEventsEdit";
            this.cmdAdvancedEventsEdit.Size = new System.Drawing.Size(28, 28);
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
            this.cmdAdvancedEventsClear.Image = global::Eddie.Lib.Forms.Properties.Resources.delete;
            this.cmdAdvancedEventsClear.Location = new System.Drawing.Point(633, 30);
            this.cmdAdvancedEventsClear.Name = "cmdAdvancedEventsClear";
            this.cmdAdvancedEventsClear.Size = new System.Drawing.Size(28, 28);
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
            this.lstAdvancedEvents.Location = new System.Drawing.Point(10, 30);
            this.lstAdvancedEvents.MultiSelect = false;
            this.lstAdvancedEvents.Name = "lstAdvancedEvents";
            this.lstAdvancedEvents.OwnerDraw = true;
            this.lstAdvancedEvents.Size = new System.Drawing.Size(617, 298);
            this.lstAdvancedEvents.SmallImageList = this.imgRoutes;
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
            // imgRoutes
            // 
            this.imgRoutes.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgRoutes.ImageStream")));
            this.imgRoutes.TransparentColor = System.Drawing.Color.Transparent;
            this.imgRoutes.Images.SetKeyName(0, "in");
            this.imgRoutes.Images.SetKeyName(1, "out");
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(6, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(346, 20);
            this.label1.TabIndex = 56;
            this.label1.Text = "External shell:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "IP Address";
            columnHeader5.Width = 150;
            // 
            // mnuRoutes
            // 
            this.mnuRoutes.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mnuRoutes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRoutesAdd,
            this.mnuRoutesRemove,
            this.mnuRoutesEdit});
            this.mnuRoutes.Name = "mnuServers";
            this.mnuRoutes.Size = new System.Drawing.Size(122, 82);
            // 
            // mnuRoutesAdd
            // 
            this.mnuRoutesAdd.Image = global::Eddie.Lib.Forms.Properties.Resources.add;
            this.mnuRoutesAdd.Name = "mnuRoutesAdd";
            this.mnuRoutesAdd.Size = new System.Drawing.Size(121, 26);
            this.mnuRoutesAdd.Text = "Add";
            this.mnuRoutesAdd.Click += new System.EventHandler(this.mnuRoutesAdd_Click);
            // 
            // mnuRoutesRemove
            // 
            this.mnuRoutesRemove.Image = global::Eddie.Lib.Forms.Properties.Resources.delete;
            this.mnuRoutesRemove.Name = "mnuRoutesRemove";
            this.mnuRoutesRemove.Size = new System.Drawing.Size(121, 26);
            this.mnuRoutesRemove.Text = "Remove";
            this.mnuRoutesRemove.Click += new System.EventHandler(this.mnuRoutesRemove_Click);
            // 
            // mnuRoutesEdit
            // 
            this.mnuRoutesEdit.Image = global::Eddie.Lib.Forms.Properties.Resources.edit;
            this.mnuRoutesEdit.Name = "mnuRoutesEdit";
            this.mnuRoutesEdit.Size = new System.Drawing.Size(121, 26);
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
            this.pnlCommands.Location = new System.Drawing.Point(268, 371);
            this.pnlCommands.Name = "pnlCommands";
            this.pnlCommands.Size = new System.Drawing.Size(330, 35);
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
            this.cmdCancel.Location = new System.Drawing.Point(166, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(160, 30);
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
            this.cmdOk.Location = new System.Drawing.Point(3, 3);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(160, 30);
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
            this.tabSettings.Controls.Add(this.tabProtocols);
            this.tabSettings.Controls.Add(this.tabProxy);
            this.tabSettings.Controls.Add(this.tabRoutes);
            this.tabSettings.Controls.Add(this.tabDNS);
            this.tabSettings.Controls.Add(this.tabNetworkLock);
            this.tabSettings.Controls.Add(tabAdvanced);
            this.tabSettings.Controls.Add(this.tabLogging);
            this.tabSettings.Controls.Add(tabDirectives);
            this.tabSettings.Controls.Add(tabEventsw);
            this.tabSettings.ItemSize = new System.Drawing.Size(80, 20);
            this.tabSettings.Location = new System.Drawing.Point(183, 0);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.SelectedIndex = 0;
            this.tabSettings.Size = new System.Drawing.Size(681, 365);
            this.tabSettings.TabIndex = 41;
            // 
            // tabGeneral
            // 
            this.tabGeneral.BackColor = System.Drawing.Color.White;
            this.tabGeneral.Controls.Add(this.lblConnect);
            this.tabGeneral.Controls.Add(this.lblNetLock);
            this.tabGeneral.Controls.Add(this.chkConnect);
            this.tabGeneral.Controls.Add(this.lblGeneralStartLast);
            this.tabGeneral.Controls.Add(this.chkNetLock);
            this.tabGeneral.Controls.Add(this.chkGeneralStartLast);
            this.tabGeneral.Controls.Add(this.label7);
            this.tabGeneral.Controls.Add(this.chkUiFontGeneral);
            this.tabGeneral.Controls.Add(this.label5);
            this.tabGeneral.Controls.Add(this.chkExitConfirm);
            this.tabGeneral.Controls.Add(this.label4);
            this.tabGeneral.Controls.Add(this.cboUiUnit);
            this.tabGeneral.Controls.Add(this.cmdUiFontGeneral);
            this.tabGeneral.Controls.Add(this.lblUiFontGeneral);
            this.tabGeneral.Controls.Add(this.pnlGeneralWindowsOnly);
            this.tabGeneral.Controls.Add(this.cmdTos);
            this.tabGeneral.Location = new System.Drawing.Point(4, 24);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Size = new System.Drawing.Size(673, 337);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            // 
            // lblConnect
            // 
            this.lblConnect.BackColor = System.Drawing.Color.Transparent;
            this.lblConnect.ForeColor = System.Drawing.Color.Black;
            this.lblConnect.Location = new System.Drawing.Point(6, 43);
            this.lblConnect.Name = "lblConnect";
            this.lblConnect.Size = new System.Drawing.Size(241, 23);
            this.lblConnect.TabIndex = 85;
            this.lblConnect.Text = "Connect at startup:";
            this.lblConnect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblNetLock
            // 
            this.lblNetLock.BackColor = System.Drawing.Color.Transparent;
            this.lblNetLock.ForeColor = System.Drawing.Color.Black;
            this.lblNetLock.Location = new System.Drawing.Point(6, 72);
            this.lblNetLock.Name = "lblNetLock";
            this.lblNetLock.Size = new System.Drawing.Size(241, 23);
            this.lblNetLock.TabIndex = 85;
            this.lblNetLock.Text = "Activate Network Lock at startup:";
            this.lblNetLock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkConnect
            // 
            this.chkConnect.BackColor = System.Drawing.Color.Transparent;
            this.chkConnect.ForeColor = System.Drawing.Color.Black;
            this.chkConnect.Location = new System.Drawing.Point(253, 43);
            this.chkConnect.Name = "chkConnect";
            this.chkConnect.Size = new System.Drawing.Size(133, 23);
            this.chkConnect.TabIndex = 84;
            this.chkConnect.UseVisualStyleBackColor = false;
            // 
            // lblGeneralStartLast
            // 
            this.lblGeneralStartLast.BackColor = System.Drawing.Color.Transparent;
            this.lblGeneralStartLast.ForeColor = System.Drawing.Color.Black;
            this.lblGeneralStartLast.Location = new System.Drawing.Point(3, 103);
            this.lblGeneralStartLast.Name = "lblGeneralStartLast";
            this.lblGeneralStartLast.Size = new System.Drawing.Size(244, 23);
            this.lblGeneralStartLast.TabIndex = 87;
            this.lblGeneralStartLast.Text = "Reconnect to last server at start:";
            this.lblGeneralStartLast.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkNetLock
            // 
            this.chkNetLock.BackColor = System.Drawing.Color.Transparent;
            this.chkNetLock.ForeColor = System.Drawing.Color.Black;
            this.chkNetLock.Location = new System.Drawing.Point(253, 73);
            this.chkNetLock.Name = "chkNetLock";
            this.chkNetLock.Size = new System.Drawing.Size(133, 23);
            this.chkNetLock.TabIndex = 84;
            this.chkNetLock.UseVisualStyleBackColor = false;
            // 
            // chkGeneralStartLast
            // 
            this.chkGeneralStartLast.BackColor = System.Drawing.Color.Transparent;
            this.chkGeneralStartLast.ForeColor = System.Drawing.Color.Black;
            this.chkGeneralStartLast.Location = new System.Drawing.Point(253, 103);
            this.chkGeneralStartLast.Name = "chkGeneralStartLast";
            this.chkGeneralStartLast.Size = new System.Drawing.Size(28, 23);
            this.chkGeneralStartLast.TabIndex = 86;
            this.chkGeneralStartLast.UseVisualStyleBackColor = false;
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(6, 236);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(241, 23);
            this.label7.TabIndex = 85;
            this.label7.Text = "UI Main Font:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkUiFontGeneral
            // 
            this.chkUiFontGeneral.BackColor = System.Drawing.Color.Transparent;
            this.chkUiFontGeneral.ForeColor = System.Drawing.Color.Black;
            this.chkUiFontGeneral.Location = new System.Drawing.Point(253, 236);
            this.chkUiFontGeneral.Name = "chkUiFontGeneral";
            this.chkUiFontGeneral.Size = new System.Drawing.Size(28, 23);
            this.chkUiFontGeneral.TabIndex = 84;
            this.chkUiFontGeneral.UseVisualStyleBackColor = false;
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(6, 206);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(241, 23);
            this.label5.TabIndex = 83;
            this.label5.Text = "Exit confirmation prompt:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkExitConfirm
            // 
            this.chkExitConfirm.BackColor = System.Drawing.Color.Transparent;
            this.chkExitConfirm.ForeColor = System.Drawing.Color.Black;
            this.chkExitConfirm.Location = new System.Drawing.Point(253, 206);
            this.chkExitConfirm.Name = "chkExitConfirm";
            this.chkExitConfirm.Size = new System.Drawing.Size(133, 23);
            this.chkExitConfirm.TabIndex = 82;
            this.chkExitConfirm.UseVisualStyleBackColor = false;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(6, 175);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(241, 23);
            this.label4.TabIndex = 77;
            this.label4.Text = "Data units:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.cboUiUnit.Location = new System.Drawing.Point(253, 176);
            this.cboUiUnit.Name = "cboUiUnit";
            this.cboUiUnit.Size = new System.Drawing.Size(268, 21);
            this.cboUiUnit.TabIndex = 76;
            // 
            // cmdUiFontGeneral
            // 
            this.cmdUiFontGeneral.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdUiFontGeneral.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdUiFontGeneral.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdUiFontGeneral.FlatAppearance.BorderSize = 0;
            this.cmdUiFontGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdUiFontGeneral.Location = new System.Drawing.Point(527, 236);
            this.cmdUiFontGeneral.Name = "cmdUiFontGeneral";
            this.cmdUiFontGeneral.Size = new System.Drawing.Size(34, 23);
            this.cmdUiFontGeneral.TabIndex = 70;
            this.cmdUiFontGeneral.Text = "...";
            this.cmdUiFontGeneral.UseVisualStyleBackColor = true;
            this.cmdUiFontGeneral.Click += new System.EventHandler(this.cmdUiFontGeneral_Click);
            // 
            // lblUiFontGeneral
            // 
            this.lblUiFontGeneral.BackColor = System.Drawing.Color.Transparent;
            this.lblUiFontGeneral.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblUiFontGeneral.ForeColor = System.Drawing.Color.Black;
            this.lblUiFontGeneral.Location = new System.Drawing.Point(287, 236);
            this.lblUiFontGeneral.Name = "lblUiFontGeneral";
            this.lblUiFontGeneral.Size = new System.Drawing.Size(234, 23);
            this.lblUiFontGeneral.TabIndex = 69;
            this.lblUiFontGeneral.Text = "Default";
            this.lblUiFontGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlGeneralWindowsOnly
            // 
            this.pnlGeneralWindowsOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlGeneralWindowsOnly.Controls.Add(this.chkOsSingleInstance);
            this.pnlGeneralWindowsOnly.Controls.Add(this.chkSystemNotifications);
            this.pnlGeneralWindowsOnly.Controls.Add(this.chkSystemStart);
            this.pnlGeneralWindowsOnly.Controls.Add(this.chkMinimizeTray);
            this.pnlGeneralWindowsOnly.Location = new System.Drawing.Point(436, 14);
            this.pnlGeneralWindowsOnly.Name = "pnlGeneralWindowsOnly";
            this.pnlGeneralWindowsOnly.Size = new System.Drawing.Size(214, 122);
            this.pnlGeneralWindowsOnly.TabIndex = 65;
            this.pnlGeneralWindowsOnly.TabStop = false;
            this.pnlGeneralWindowsOnly.Text = "Microsoft Windows Only";
            // 
            // chkOsSingleInstance
            // 
            this.chkOsSingleInstance.AutoSize = true;
            this.chkOsSingleInstance.BackColor = System.Drawing.Color.Transparent;
            this.chkOsSingleInstance.ForeColor = System.Drawing.Color.Black;
            this.chkOsSingleInstance.Location = new System.Drawing.Point(22, 91);
            this.chkOsSingleInstance.Name = "chkOsSingleInstance";
            this.chkOsSingleInstance.Size = new System.Drawing.Size(98, 17);
            this.chkOsSingleInstance.TabIndex = 41;
            this.chkOsSingleInstance.Text = "Single instance";
            this.chkOsSingleInstance.UseVisualStyleBackColor = false;
            // 
            // chkSystemNotifications
            // 
            this.chkSystemNotifications.AutoSize = true;
            this.chkSystemNotifications.BackColor = System.Drawing.Color.Transparent;
            this.chkSystemNotifications.ForeColor = System.Drawing.Color.Black;
            this.chkSystemNotifications.Location = new System.Drawing.Point(22, 68);
            this.chkSystemNotifications.Name = "chkSystemNotifications";
            this.chkSystemNotifications.Size = new System.Drawing.Size(121, 17);
            this.chkSystemNotifications.TabIndex = 40;
            this.chkSystemNotifications.Text = "System Notifications";
            this.chkSystemNotifications.UseVisualStyleBackColor = false;
            // 
            // chkSystemStart
            // 
            this.chkSystemStart.AutoSize = true;
            this.chkSystemStart.BackColor = System.Drawing.Color.Transparent;
            this.chkSystemStart.ForeColor = System.Drawing.Color.Black;
            this.chkSystemStart.Location = new System.Drawing.Point(22, 22);
            this.chkSystemStart.Name = "chkSystemStart";
            this.chkSystemStart.Size = new System.Drawing.Size(117, 17);
            this.chkSystemStart.TabIndex = 31;
            this.chkSystemStart.Text = "Start with Windows";
            this.chkSystemStart.UseVisualStyleBackColor = false;
            // 
            // chkMinimizeTray
            // 
            this.chkMinimizeTray.AutoSize = true;
            this.chkMinimizeTray.BackColor = System.Drawing.Color.Transparent;
            this.chkMinimizeTray.ForeColor = System.Drawing.Color.Black;
            this.chkMinimizeTray.Location = new System.Drawing.Point(22, 46);
            this.chkMinimizeTray.Name = "chkMinimizeTray";
            this.chkMinimizeTray.Size = new System.Drawing.Size(120, 17);
            this.chkMinimizeTray.TabIndex = 39;
            this.chkMinimizeTray.Text = "Minimize in tray icon";
            this.chkMinimizeTray.UseVisualStyleBackColor = false;
            // 
            // cmdTos
            // 
            this.cmdTos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdTos.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdTos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdTos.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdTos.FlatAppearance.BorderSize = 0;
            this.cmdTos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdTos.Location = new System.Drawing.Point(436, 293);
            this.cmdTos.Name = "cmdTos";
            this.cmdTos.Size = new System.Drawing.Size(214, 30);
            this.cmdTos.TabIndex = 38;
            this.cmdTos.Text = "Terms of Service";
            this.cmdTos.UseVisualStyleBackColor = true;
            this.cmdTos.Click += new System.EventHandler(this.cmdTos_Click);
            // 
            // tabProtocols
            // 
            this.tabProtocols.BackColor = System.Drawing.Color.White;
            this.tabProtocols.Controls.Add(this.lnkProtocolsHelp2);
            this.tabProtocols.Controls.Add(this.lnkProtocolsHelp1);
            this.tabProtocols.Controls.Add(this.chkProtocolsAutomatic);
            this.tabProtocols.Controls.Add(this.lstProtocols);
            this.tabProtocols.Location = new System.Drawing.Point(4, 24);
            this.tabProtocols.Name = "tabProtocols";
            this.tabProtocols.Size = new System.Drawing.Size(673, 337);
            this.tabProtocols.TabIndex = 3;
            this.tabProtocols.Text = "Protocols";
            // 
            // lnkProtocolsHelp2
            // 
            this.lnkProtocolsHelp2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkProtocolsHelp2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkProtocolsHelp2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
            this.lnkProtocolsHelp2.Location = new System.Drawing.Point(13, 304);
            this.lnkProtocolsHelp2.Name = "lnkProtocolsHelp2";
            this.lnkProtocolsHelp2.Size = new System.Drawing.Size(650, 22);
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
            this.lnkProtocolsHelp1.Location = new System.Drawing.Point(13, 282);
            this.lnkProtocolsHelp1.Name = "lnkProtocolsHelp1";
            this.lnkProtocolsHelp1.Size = new System.Drawing.Size(650, 22);
            this.lnkProtocolsHelp1.TabIndex = 71;
            this.lnkProtocolsHelp1.TabStop = true;
            this.lnkProtocolsHelp1.Text = "What is the difference between protocols?";
            this.lnkProtocolsHelp1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkProtocolsHelp1.Click += new System.EventHandler(this.lnkProtocolsHelp1_LinkClicked);
            // 
            // chkProtocolsAutomatic
            // 
            this.chkProtocolsAutomatic.Location = new System.Drawing.Point(13, 14);
            this.chkProtocolsAutomatic.Name = "chkProtocolsAutomatic";
            this.chkProtocolsAutomatic.Size = new System.Drawing.Size(650, 22);
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
            this.colProtocolsDescription});
            this.lstProtocols.FullRowSelect = true;
            this.lstProtocols.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstProtocols.HideSelection = false;
            this.lstProtocols.Location = new System.Drawing.Point(13, 38);
            this.lstProtocols.MultiSelect = false;
            this.lstProtocols.Name = "lstProtocols";
            this.lstProtocols.OwnerDraw = true;
            this.lstProtocols.Size = new System.Drawing.Size(650, 241);
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
            this.colProtocolsEntry.Text = "Entry";
            // 
            // colProtocolsDescription
            // 
            this.colProtocolsDescription.Text = "Description";
            this.colProtocolsDescription.Width = 200;
            // 
            // tabProxy
            // 
            this.tabProxy.BackColor = System.Drawing.Color.White;
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
            this.tabProxy.Name = "tabProxy";
            this.tabProxy.Padding = new System.Windows.Forms.Padding(3);
            this.tabProxy.Size = new System.Drawing.Size(673, 337);
            this.tabProxy.TabIndex = 1;
            this.tabProxy.Text = "Proxy";
            // 
            // lnkProxyTorHelp
            // 
            this.lnkProxyTorHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkProxyTorHelp.BackColor = System.Drawing.Color.Transparent;
            this.lnkProxyTorHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkProxyTorHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
            this.lnkProxyTorHelp.Location = new System.Drawing.Point(13, 304);
            this.lnkProxyTorHelp.Name = "lnkProxyTorHelp";
            this.lnkProxyTorHelp.Size = new System.Drawing.Size(650, 22);
            this.lnkProxyTorHelp.TabIndex = 74;
            this.lnkProxyTorHelp.TabStop = true;
            this.lnkProxyTorHelp.Text = "More about Tor over VPN";
            this.lnkProxyTorHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkProxyTorHelp.Click += new System.EventHandler(this.lnkProxyTorHelp_LinkClicked);
            // 
            // txtProxyTorControlPassword
            // 
            this.txtProxyTorControlPassword.Location = new System.Drawing.Point(200, 241);
            this.txtProxyTorControlPassword.Name = "txtProxyTorControlPassword";
            this.txtProxyTorControlPassword.Size = new System.Drawing.Size(180, 20);
            this.txtProxyTorControlPassword.TabIndex = 73;
            // 
            // lblProxyTorControlPassword
            // 
            this.lblProxyTorControlPassword.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyTorControlPassword.ForeColor = System.Drawing.Color.Black;
            this.lblProxyTorControlPassword.Location = new System.Drawing.Point(14, 241);
            this.lblProxyTorControlPassword.Name = "lblProxyTorControlPassword";
            this.lblProxyTorControlPassword.Size = new System.Drawing.Size(180, 20);
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
            this.cmdProxyTorTest.Location = new System.Drawing.Point(200, 271);
            this.cmdProxyTorTest.Name = "cmdProxyTorTest";
            this.cmdProxyTorTest.Size = new System.Drawing.Size(180, 22);
            this.cmdProxyTorTest.TabIndex = 71;
            this.cmdProxyTorTest.Text = "Test";
            this.cmdProxyTorTest.UseVisualStyleBackColor = true;
            this.cmdProxyTorTest.Click += new System.EventHandler(this.cmdProxyTorTest_Click);
            // 
            // txtProxyTorControlPort
            // 
            this.txtProxyTorControlPort.Location = new System.Drawing.Point(200, 211);
            this.txtProxyTorControlPort.Name = "txtProxyTorControlPort";
            this.txtProxyTorControlPort.Size = new System.Drawing.Size(180, 20);
            this.txtProxyTorControlPort.TabIndex = 70;
            // 
            // lblProxyTorControlPort
            // 
            this.lblProxyTorControlPort.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyTorControlPort.ForeColor = System.Drawing.Color.Black;
            this.lblProxyTorControlPort.Location = new System.Drawing.Point(14, 211);
            this.lblProxyTorControlPort.Name = "lblProxyTorControlPort";
            this.lblProxyTorControlPort.Size = new System.Drawing.Size(180, 20);
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
            this.label17.Image = ((System.Drawing.Image)(resources.GetObject("label17.Image")));
            this.label17.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label17.Location = new System.Drawing.Point(419, 211);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(244, 82);
            this.label17.TabIndex = 45;
            this.label17.Text = "       If you use Tor as proxy, you need to set up your real proxy inside Tor con" +
    "figuration";
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.BackColor = System.Drawing.Color.Transparent;
            this.label12.ForeColor = System.Drawing.Color.Black;
            this.label12.Image = ((System.Drawing.Image)(resources.GetObject("label12.Image")));
            this.label12.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label12.Location = new System.Drawing.Point(422, 15);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(241, 120);
            this.label12.TabIndex = 44;
            this.label12.Text = "       UDP, SSH and SSL connections will not be available if you use a proxy.";
            // 
            // lblProxyAuthentication
            // 
            this.lblProxyAuthentication.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyAuthentication.ForeColor = System.Drawing.Color.Black;
            this.lblProxyAuthentication.Location = new System.Drawing.Point(11, 114);
            this.lblProxyAuthentication.Name = "lblProxyAuthentication";
            this.lblProxyAuthentication.Size = new System.Drawing.Size(183, 21);
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
            this.cboProxyAuthentication.Location = new System.Drawing.Point(200, 114);
            this.cboProxyAuthentication.Name = "cboProxyAuthentication";
            this.cboProxyAuthentication.Size = new System.Drawing.Size(180, 21);
            this.cboProxyAuthentication.TabIndex = 42;
            this.cboProxyAuthentication.SelectedIndexChanged += new System.EventHandler(this.cboProxyAuthentication_SelectedIndexChanged);
            // 
            // txtProxyPassword
            // 
            this.txtProxyPassword.Location = new System.Drawing.Point(200, 174);
            this.txtProxyPassword.Name = "txtProxyPassword";
            this.txtProxyPassword.Size = new System.Drawing.Size(180, 20);
            this.txtProxyPassword.TabIndex = 41;
            // 
            // lblProxyPassword
            // 
            this.lblProxyPassword.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyPassword.ForeColor = System.Drawing.Color.Black;
            this.lblProxyPassword.Location = new System.Drawing.Point(14, 174);
            this.lblProxyPassword.Name = "lblProxyPassword";
            this.lblProxyPassword.Size = new System.Drawing.Size(180, 20);
            this.lblProxyPassword.TabIndex = 40;
            this.lblProxyPassword.Text = "Password:";
            this.lblProxyPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtProxyLogin
            // 
            this.txtProxyLogin.Location = new System.Drawing.Point(200, 144);
            this.txtProxyLogin.Name = "txtProxyLogin";
            this.txtProxyLogin.Size = new System.Drawing.Size(180, 20);
            this.txtProxyLogin.TabIndex = 39;
            // 
            // lblProxyLogin
            // 
            this.lblProxyLogin.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyLogin.ForeColor = System.Drawing.Color.Black;
            this.lblProxyLogin.Location = new System.Drawing.Point(14, 144);
            this.lblProxyLogin.Name = "lblProxyLogin";
            this.lblProxyLogin.Size = new System.Drawing.Size(180, 20);
            this.lblProxyLogin.TabIndex = 38;
            this.lblProxyLogin.Text = "Login:";
            this.lblProxyLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblProxyType
            // 
            this.lblProxyType.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyType.ForeColor = System.Drawing.Color.Black;
            this.lblProxyType.Location = new System.Drawing.Point(17, 18);
            this.lblProxyType.Name = "lblProxyType";
            this.lblProxyType.Size = new System.Drawing.Size(177, 21);
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
            "Detect",
            "Http",
            "Socks",
            "Tor"});
            this.cboProxyMode.Location = new System.Drawing.Point(200, 18);
            this.cboProxyMode.Name = "cboProxyMode";
            this.cboProxyMode.Size = new System.Drawing.Size(180, 21);
            this.cboProxyMode.TabIndex = 36;
            this.cboProxyMode.SelectedIndexChanged += new System.EventHandler(this.cboProxyMode_SelectedIndexChanged);
            // 
            // txtProxyPort
            // 
            this.txtProxyPort.Location = new System.Drawing.Point(200, 77);
            this.txtProxyPort.Name = "txtProxyPort";
            this.txtProxyPort.Size = new System.Drawing.Size(180, 20);
            this.txtProxyPort.TabIndex = 35;
            // 
            // lblProxyPort
            // 
            this.lblProxyPort.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyPort.ForeColor = System.Drawing.Color.Black;
            this.lblProxyPort.Location = new System.Drawing.Point(14, 77);
            this.lblProxyPort.Name = "lblProxyPort";
            this.lblProxyPort.Size = new System.Drawing.Size(180, 20);
            this.lblProxyPort.TabIndex = 34;
            this.lblProxyPort.Text = "Port:";
            this.lblProxyPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtProxyHost
            // 
            this.txtProxyHost.Location = new System.Drawing.Point(200, 48);
            this.txtProxyHost.Name = "txtProxyHost";
            this.txtProxyHost.Size = new System.Drawing.Size(180, 20);
            this.txtProxyHost.TabIndex = 33;
            // 
            // lblProxyHost
            // 
            this.lblProxyHost.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyHost.ForeColor = System.Drawing.Color.Black;
            this.lblProxyHost.Location = new System.Drawing.Point(14, 48);
            this.lblProxyHost.Name = "lblProxyHost";
            this.lblProxyHost.Size = new System.Drawing.Size(180, 20);
            this.lblProxyHost.TabIndex = 32;
            this.lblProxyHost.Text = "Host:";
            this.lblProxyHost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabRoutes
            // 
            this.tabRoutes.BackColor = System.Drawing.Color.White;
            this.tabRoutes.Controls.Add(this.lblRoutesNetworkLockWarning);
            this.tabRoutes.Controls.Add(this.cmdRouteEdit);
            this.tabRoutes.Controls.Add(this.cmdRouteRemove);
            this.tabRoutes.Controls.Add(this.cmdRouteAdd);
            this.tabRoutes.Controls.Add(this.label6);
            this.tabRoutes.Controls.Add(this.cboRoutesOtherwise);
            this.tabRoutes.Controls.Add(this.lblRoutesOtherwise);
            this.tabRoutes.Controls.Add(this.lstRoutes);
            this.tabRoutes.Location = new System.Drawing.Point(4, 24);
            this.tabRoutes.Name = "tabRoutes";
            this.tabRoutes.Size = new System.Drawing.Size(673, 337);
            this.tabRoutes.TabIndex = 5;
            this.tabRoutes.Text = "Routes";
            // 
            // lblRoutesNetworkLockWarning
            // 
            this.lblRoutesNetworkLockWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRoutesNetworkLockWarning.BackColor = System.Drawing.Color.Transparent;
            this.lblRoutesNetworkLockWarning.ForeColor = System.Drawing.Color.Black;
            this.lblRoutesNetworkLockWarning.Location = new System.Drawing.Point(9, 302);
            this.lblRoutesNetworkLockWarning.Name = "lblRoutesNetworkLockWarning";
            this.lblRoutesNetworkLockWarning.Size = new System.Drawing.Size(282, 30);
            this.lblRoutesNetworkLockWarning.TabIndex = 43;
            this.lblRoutesNetworkLockWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmdRouteEdit
            // 
            this.cmdRouteEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRouteEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdRouteEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdRouteEdit.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdRouteEdit.FlatAppearance.BorderSize = 0;
            this.cmdRouteEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdRouteEdit.Image = global::Eddie.Lib.Forms.Properties.Resources.edit;
            this.cmdRouteEdit.Location = new System.Drawing.Point(632, 103);
            this.cmdRouteEdit.Name = "cmdRouteEdit";
            this.cmdRouteEdit.Size = new System.Drawing.Size(28, 28);
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
            this.cmdRouteRemove.Image = global::Eddie.Lib.Forms.Properties.Resources.delete;
            this.cmdRouteRemove.Location = new System.Drawing.Point(632, 69);
            this.cmdRouteRemove.Name = "cmdRouteRemove";
            this.cmdRouteRemove.Size = new System.Drawing.Size(28, 28);
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
            this.cmdRouteAdd.Image = global::Eddie.Lib.Forms.Properties.Resources.add;
            this.cmdRouteAdd.Location = new System.Drawing.Point(632, 35);
            this.cmdRouteAdd.Name = "cmdRouteAdd";
            this.cmdRouteAdd.Size = new System.Drawing.Size(28, 28);
            this.cmdRouteAdd.TabIndex = 39;
            this.cmdRouteAdd.UseVisualStyleBackColor = true;
            this.cmdRouteAdd.Click += new System.EventHandler(this.cmdRouteAdd_Click);
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(6, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(654, 22);
            this.label6.TabIndex = 3;
            this.label6.Text = "Network routing rules about what destination must be in the VPN tunnel or not.";
            // 
            // cboRoutesOtherwise
            // 
            this.cboRoutesOtherwise.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cboRoutesOtherwise.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRoutesOtherwise.FormattingEnabled = true;
            this.cboRoutesOtherwise.Location = new System.Drawing.Point(463, 307);
            this.cboRoutesOtherwise.Name = "cboRoutesOtherwise";
            this.cboRoutesOtherwise.Size = new System.Drawing.Size(164, 21);
            this.cboRoutesOtherwise.TabIndex = 2;
            this.cboRoutesOtherwise.SelectedIndexChanged += new System.EventHandler(this.cboRoutesOtherwise_SelectedIndexChanged);
            // 
            // lblRoutesOtherwise
            // 
            this.lblRoutesOtherwise.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRoutesOtherwise.BackColor = System.Drawing.Color.Transparent;
            this.lblRoutesOtherwise.ForeColor = System.Drawing.Color.Black;
            this.lblRoutesOtherwise.Location = new System.Drawing.Point(297, 302);
            this.lblRoutesOtherwise.Name = "lblRoutesOtherwise";
            this.lblRoutesOtherwise.Size = new System.Drawing.Size(160, 30);
            this.lblRoutesOtherwise.TabIndex = 1;
            this.lblRoutesOtherwise.Text = "Not specified routes go: ";
            this.lblRoutesOtherwise.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.lstRoutes.Location = new System.Drawing.Point(6, 35);
            this.lstRoutes.MultiSelect = false;
            this.lstRoutes.Name = "lstRoutes";
            this.lstRoutes.OwnerDraw = true;
            this.lstRoutes.Size = new System.Drawing.Size(621, 264);
            this.lstRoutes.SmallImageList = this.imgRoutes;
            this.lstRoutes.TabIndex = 0;
            this.lstRoutes.UseCompatibleStateImageBehavior = false;
            this.lstRoutes.View = System.Windows.Forms.View.Details;
            this.lstRoutes.SelectedIndexChanged += new System.EventHandler(this.lstRoutes_SelectedIndexChanged);
            this.lstRoutes.DoubleClick += new System.EventHandler(this.lstRoutes_DoubleClick);
            // 
            // colRoutesIp
            // 
            this.colRoutesIp.Text = "IP / Host";
            this.colRoutesIp.Width = 200;
            // 
            // colRoutesAction
            // 
            this.colRoutesAction.Text = "Action";
            this.colRoutesAction.Width = 160;
            // 
            // colRoutesNotes
            // 
            this.colRoutesNotes.Text = "Notes";
            this.colRoutesNotes.Width = 1000;
            // 
            // tabDNS
            // 
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
            this.tabDNS.Name = "tabDNS";
            this.tabDNS.Padding = new System.Windows.Forms.Padding(3);
            this.tabDNS.Size = new System.Drawing.Size(673, 337);
            this.tabDNS.TabIndex = 5;
            this.tabDNS.Text = "DNS";
            this.tabDNS.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.BackColor = System.Drawing.Color.Transparent;
            this.label10.ForeColor = System.Drawing.Color.Black;
            this.label10.Location = new System.Drawing.Point(174, 304);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(453, 30);
            this.label10.TabIndex = 82;
            this.label10.Text = "Leave empty to use DNS servers recommended by the VPN.";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDnsCheck
            // 
            this.lblDnsCheck.BackColor = System.Drawing.Color.Transparent;
            this.lblDnsCheck.ForeColor = System.Drawing.Color.Black;
            this.lblDnsCheck.Location = new System.Drawing.Point(17, 50);
            this.lblDnsCheck.Name = "lblDnsCheck";
            this.lblDnsCheck.Size = new System.Drawing.Size(151, 23);
            this.lblDnsCheck.TabIndex = 81;
            this.lblDnsCheck.Text = "Check AirVPN DNS:";
            this.lblDnsCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDnsServers
            // 
            this.lblDnsServers.BackColor = System.Drawing.Color.Transparent;
            this.lblDnsServers.ForeColor = System.Drawing.Color.Black;
            this.lblDnsServers.Location = new System.Drawing.Point(20, 80);
            this.lblDnsServers.Name = "lblDnsServers";
            this.lblDnsServers.Size = new System.Drawing.Size(148, 22);
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
            this.cmdDnsEdit.Image = global::Eddie.Lib.Forms.Properties.Resources.edit;
            this.cmdDnsEdit.Location = new System.Drawing.Point(633, 148);
            this.cmdDnsEdit.Name = "cmdDnsEdit";
            this.cmdDnsEdit.Size = new System.Drawing.Size(28, 28);
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
            this.cmdDnsRemove.Image = global::Eddie.Lib.Forms.Properties.Resources.delete;
            this.cmdDnsRemove.Location = new System.Drawing.Point(633, 114);
            this.cmdDnsRemove.Name = "cmdDnsRemove";
            this.cmdDnsRemove.Size = new System.Drawing.Size(28, 28);
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
            this.cmdDnsAdd.Image = global::Eddie.Lib.Forms.Properties.Resources.add;
            this.cmdDnsAdd.Location = new System.Drawing.Point(633, 80);
            this.cmdDnsAdd.Name = "cmdDnsAdd";
            this.cmdDnsAdd.Size = new System.Drawing.Size(28, 28);
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
            this.lstDnsServers.Location = new System.Drawing.Point(174, 80);
            this.lstDnsServers.MultiSelect = false;
            this.lstDnsServers.Name = "lstDnsServers";
            this.lstDnsServers.OwnerDraw = true;
            this.lstDnsServers.Size = new System.Drawing.Size(453, 221);
            this.lstDnsServers.SmallImageList = this.imgRoutes;
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
            this.lblDnsSwitchMode.Location = new System.Drawing.Point(17, 18);
            this.lblDnsSwitchMode.Name = "lblDnsSwitchMode";
            this.lblDnsSwitchMode.Size = new System.Drawing.Size(151, 23);
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
            "Automatic",
            "Resolvconf (Linux only)",
            "Renaming (Linux only)"});
            this.cboDnsSwitchMode.Location = new System.Drawing.Point(174, 19);
            this.cboDnsSwitchMode.Name = "cboDnsSwitchMode";
            this.cboDnsSwitchMode.Size = new System.Drawing.Size(133, 21);
            this.cboDnsSwitchMode.TabIndex = 74;
            // 
            // chkDnsCheck
            // 
            this.chkDnsCheck.BackColor = System.Drawing.Color.Transparent;
            this.chkDnsCheck.ForeColor = System.Drawing.Color.Black;
            this.chkDnsCheck.Location = new System.Drawing.Point(174, 50);
            this.chkDnsCheck.Name = "chkDnsCheck";
            this.chkDnsCheck.Size = new System.Drawing.Size(133, 23);
            this.chkDnsCheck.TabIndex = 73;
            this.chkDnsCheck.UseVisualStyleBackColor = false;
            // 
            // tabNetworkLock
            // 
            this.tabNetworkLock.Controls.Add(this.lblLockAllowPing);
            this.tabNetworkLock.Controls.Add(this.lblLockAllowPrivate);
            this.tabNetworkLock.Controls.Add(this.lnkLockHelp);
            this.tabNetworkLock.Controls.Add(this.chkLockAllowPing);
            this.tabNetworkLock.Controls.Add(this.chkLockAllowPrivate);
            this.tabNetworkLock.Controls.Add(this.lblLockRoutingOutWarning);
            this.tabNetworkLock.Controls.Add(this.lblLockAllowedIPS);
            this.tabNetworkLock.Controls.Add(this.txtLockAllowedIPS);
            this.tabNetworkLock.Controls.Add(this.lblLockMode);
            this.tabNetworkLock.Controls.Add(this.cboLockMode);
            this.tabNetworkLock.Location = new System.Drawing.Point(4, 24);
            this.tabNetworkLock.Name = "tabNetworkLock";
            this.tabNetworkLock.Size = new System.Drawing.Size(673, 337);
            this.tabNetworkLock.TabIndex = 4;
            this.tabNetworkLock.Text = "Network lock";
            this.tabNetworkLock.UseVisualStyleBackColor = true;
            // 
            // lblLockAllowPing
            // 
            this.lblLockAllowPing.BackColor = System.Drawing.Color.Transparent;
            this.lblLockAllowPing.ForeColor = System.Drawing.Color.Black;
            this.lblLockAllowPing.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLockAllowPing.Location = new System.Drawing.Point(14, 70);
            this.lblLockAllowPing.Name = "lblLockAllowPing";
            this.lblLockAllowPing.Size = new System.Drawing.Size(144, 25);
            this.lblLockAllowPing.TabIndex = 84;
            this.lblLockAllowPing.Text = "Allow ping:";
            this.lblLockAllowPing.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLockAllowPrivate
            // 
            this.lblLockAllowPrivate.BackColor = System.Drawing.Color.Transparent;
            this.lblLockAllowPrivate.ForeColor = System.Drawing.Color.Black;
            this.lblLockAllowPrivate.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLockAllowPrivate.Location = new System.Drawing.Point(14, 40);
            this.lblLockAllowPrivate.Name = "lblLockAllowPrivate";
            this.lblLockAllowPrivate.Size = new System.Drawing.Size(144, 25);
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
            this.lnkLockHelp.Location = new System.Drawing.Point(13, 304);
            this.lnkLockHelp.Name = "lnkLockHelp";
            this.lnkLockHelp.Size = new System.Drawing.Size(650, 22);
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
            this.chkLockAllowPing.Location = new System.Drawing.Point(164, 70);
            this.chkLockAllowPing.Name = "chkLockAllowPing";
            this.chkLockAllowPing.Size = new System.Drawing.Size(163, 25);
            this.chkLockAllowPing.TabIndex = 81;
            this.chkLockAllowPing.UseVisualStyleBackColor = false;
            // 
            // chkLockAllowPrivate
            // 
            this.chkLockAllowPrivate.BackColor = System.Drawing.Color.Transparent;
            this.chkLockAllowPrivate.ForeColor = System.Drawing.Color.Black;
            this.chkLockAllowPrivate.Location = new System.Drawing.Point(164, 40);
            this.chkLockAllowPrivate.Name = "chkLockAllowPrivate";
            this.chkLockAllowPrivate.Size = new System.Drawing.Size(163, 25);
            this.chkLockAllowPrivate.TabIndex = 80;
            this.chkLockAllowPrivate.UseVisualStyleBackColor = false;
            // 
            // lblLockRoutingOutWarning
            // 
            this.lblLockRoutingOutWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLockRoutingOutWarning.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.lblLockRoutingOutWarning.ForeColor = System.Drawing.Color.White;
            this.lblLockRoutingOutWarning.Location = new System.Drawing.Point(429, 10);
            this.lblLockRoutingOutWarning.Name = "lblLockRoutingOutWarning";
            this.lblLockRoutingOutWarning.Size = new System.Drawing.Size(234, 281);
            this.lblLockRoutingOutWarning.TabIndex = 78;
            this.lblLockRoutingOutWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLockAllowedIPS
            // 
            this.lblLockAllowedIPS.BackColor = System.Drawing.Color.Transparent;
            this.lblLockAllowedIPS.ForeColor = System.Drawing.Color.Black;
            this.lblLockAllowedIPS.Location = new System.Drawing.Point(20, 100);
            this.lblLockAllowedIPS.Name = "lblLockAllowedIPS";
            this.lblLockAllowedIPS.Size = new System.Drawing.Size(138, 20);
            this.lblLockAllowedIPS.TabIndex = 76;
            this.lblLockAllowedIPS.Text = "Addresses allowed:";
            this.lblLockAllowedIPS.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtLockAllowedIPS
            // 
            this.txtLockAllowedIPS.AcceptsReturn = true;
            this.txtLockAllowedIPS.AcceptsTab = true;
            this.txtLockAllowedIPS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLockAllowedIPS.Location = new System.Drawing.Point(164, 100);
            this.txtLockAllowedIPS.Multiline = true;
            this.txtLockAllowedIPS.Name = "txtLockAllowedIPS";
            this.txtLockAllowedIPS.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLockAllowedIPS.Size = new System.Drawing.Size(259, 191);
            this.txtLockAllowedIPS.TabIndex = 75;
            // 
            // lblLockMode
            // 
            this.lblLockMode.BackColor = System.Drawing.Color.Transparent;
            this.lblLockMode.ForeColor = System.Drawing.Color.Black;
            this.lblLockMode.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLockMode.Location = new System.Drawing.Point(14, 10);
            this.lblLockMode.Name = "lblLockMode";
            this.lblLockMode.Size = new System.Drawing.Size(144, 21);
            this.lblLockMode.TabIndex = 74;
            this.lblLockMode.Text = "Mode:";
            this.lblLockMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboLockMode
            // 
            this.cboLockMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLockMode.FormattingEnabled = true;
            this.cboLockMode.Location = new System.Drawing.Point(164, 10);
            this.cboLockMode.Name = "cboLockMode";
            this.cboLockMode.Size = new System.Drawing.Size(259, 21);
            this.cboLockMode.TabIndex = 73;
            // 
            // tabLogging
            // 
            this.tabLogging.Controls.Add(this.chkLogLevelDebug);
            this.tabLogging.Controls.Add(this.TxtLoggingPathComputed);
            this.tabLogging.Controls.Add(this.lblLoggingHelp);
            this.tabLogging.Controls.Add(this.txtLogPath);
            this.tabLogging.Controls.Add(this.lblLogPath);
            this.tabLogging.Controls.Add(this.chkLoggingEnabled);
            this.tabLogging.Location = new System.Drawing.Point(4, 24);
            this.tabLogging.Name = "tabLogging";
            this.tabLogging.Size = new System.Drawing.Size(673, 337);
            this.tabLogging.TabIndex = 3;
            this.tabLogging.Text = "Logging";
            this.tabLogging.UseVisualStyleBackColor = true;
            // 
            // chkLogLevelDebug
            // 
            this.chkLogLevelDebug.AutoSize = true;
            this.chkLogLevelDebug.BackColor = System.Drawing.Color.Transparent;
            this.chkLogLevelDebug.ForeColor = System.Drawing.Color.Black;
            this.chkLogLevelDebug.Location = new System.Drawing.Point(17, 50);
            this.chkLogLevelDebug.Name = "chkLogLevelDebug";
            this.chkLogLevelDebug.Size = new System.Drawing.Size(173, 17);
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
            this.TxtLoggingPathComputed.Location = new System.Drawing.Point(74, 102);
            this.TxtLoggingPathComputed.Name = "TxtLoggingPathComputed";
            this.TxtLoggingPathComputed.Size = new System.Drawing.Size(586, 97);
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
            this.lblLoggingHelp.Location = new System.Drawing.Point(17, 230);
            this.lblLoggingHelp.Name = "lblLoggingHelp";
            this.lblLoggingHelp.Size = new System.Drawing.Size(634, 96);
            this.lblLoggingHelp.TabIndex = 61;
            this.lblLoggingHelp.Text = resources.GetString("lblLoggingHelp.Text");
            this.lblLoggingHelp.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtLogPath
            // 
            this.txtLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLogPath.Location = new System.Drawing.Point(74, 79);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.Size = new System.Drawing.Size(591, 20);
            this.txtLogPath.TabIndex = 61;
            this.txtLogPath.TextChanged += new System.EventHandler(this.TxtLoggingPath_TextChanged);
            // 
            // lblLogPath
            // 
            this.lblLogPath.BackColor = System.Drawing.Color.Transparent;
            this.lblLogPath.ForeColor = System.Drawing.Color.Black;
            this.lblLogPath.Location = new System.Drawing.Point(14, 82);
            this.lblLogPath.Name = "lblLogPath";
            this.lblLogPath.Size = new System.Drawing.Size(54, 18);
            this.lblLogPath.TabIndex = 60;
            this.lblLogPath.Text = "Path:";
            // 
            // chkLoggingEnabled
            // 
            this.chkLoggingEnabled.AutoSize = true;
            this.chkLoggingEnabled.BackColor = System.Drawing.Color.Transparent;
            this.chkLoggingEnabled.ForeColor = System.Drawing.Color.Black;
            this.chkLoggingEnabled.Location = new System.Drawing.Point(17, 20);
            this.chkLoggingEnabled.Name = "chkLoggingEnabled";
            this.chkLoggingEnabled.Size = new System.Drawing.Size(136, 17);
            this.chkLoggingEnabled.TabIndex = 55;
            this.chkLoggingEnabled.Text = "Logging on file enabled";
            this.chkLoggingEnabled.UseVisualStyleBackColor = false;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(864, 411);
            this.Controls.Add(this.pnlCommands);
            this.Controls.Add(this.tabSettings);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(880, 450);
            this.Name = "Settings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            tabAdvanced.ResumeLayout(false);
            tabAdvanced.PerformLayout();
            this.pnlAdvancedGeneralWindowsOnly.ResumeLayout(false);
            tabDirectives.ResumeLayout(false);
            tabDirectives.PerformLayout();
            tabEventsw.ResumeLayout(false);
            this.mnuRoutes.ResumeLayout(false);
            this.pnlCommands.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.pnlGeneralWindowsOnly.ResumeLayout(false);
            this.pnlGeneralWindowsOnly.PerformLayout();
            this.tabProtocols.ResumeLayout(false);
            this.tabProxy.ResumeLayout(false);
            this.tabProxy.PerformLayout();
            this.tabRoutes.ResumeLayout(false);
            this.tabDNS.ResumeLayout(false);
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
        private Skin.Button cmdTos;
        private Skin.Label label6;
        private Skin.ComboBox cboRoutesOtherwise;
        private Skin.Label lblRoutesOtherwise;
        private Skin.ListView lstRoutes;
        private Skin.ColumnHeader colRoutesIp;
        private Skin.ColumnHeader colRoutesNotes;
        private Skin.ColumnHeader colRoutesAction;
        private System.Windows.Forms.ImageList imgRoutes;
		private Skin.CheckBox chkMinimizeTray;
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
		private Skin.CheckBox chkWindowsDhcpSwitch;
		private Skin.Button cmdAdvancedUninstallDriver;
		private Skin.CheckBox chkAdvancedPingerEnabled;
		private System.Windows.Forms.GroupBox pnlGeneralWindowsOnly;
		private System.Windows.Forms.GroupBox pnlAdvancedGeneralWindowsOnly;
		private Skin.TabPage tabLogging;
		private Skin.Label TxtLoggingPathComputed;
		private Skin.Label lblLoggingHelp;
		private Skin.TextBox txtLogPath;
		private Skin.Label lblLogPath;
		private Skin.CheckBox chkLoggingEnabled;
		private Skin.TabPage tabNetworkLock;
		private Skin.Label lblLockAllowedIPS;
		private Skin.TextBox txtLockAllowedIPS;
		private Skin.Label lblLockMode;
		private Skin.ComboBox cboLockMode;
		private Skin.Label lblLockRoutingOutWarning;
		private Skin.Label lblRoutesNetworkLockWarning;
		private Skin.Label label17;
		private Skin.Label lblAdvancedManifestRefresh;
		private Skin.ComboBox cboAdvancedManifestRefresh;
		private Skin.TabPage tabDNS;
		private Skin.Label lblIpV6;
		private Skin.ComboBox cboIpV6;
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
		private Skin.Label lblRouteRemoveDefault;
		private Skin.CheckBox chkRouteRemoveDefault;
		private Skin.CheckBox chkLogLevelDebug;
		private Skin.CheckBox chkWindowsDisableDriverUpgrade;
		private Skin.Label lblOpenVpnSndbuf;
		private Skin.ComboBox cboOpenVpnSndbuf;
		private Skin.Label lblOpenVpnRcvbuf;
		private Skin.ComboBox cboOpenVpnRcvbuf;
		private Skin.CheckBox chkSystemNotifications;
        private Skin.ListView lstProtocols;
        private Skin.ColumnHeader colProtocolsDescription;
        private Skin.ColumnHeader colProtocolsProtocol;
        private Skin.ColumnHeader colProtocolsPort;
        private System.Windows.Forms.ColumnHeader colProtocolsEntry;
        private Skin.LinkLabel lnkProtocolsHelp2;
        private Skin.LinkLabel lnkProtocolsHelp1;
        private System.Windows.Forms.CheckBox chkProtocolsAutomatic;
        private Skin.CheckBox chkWindowsWfp;
        private Skin.Label lblUiFontGeneral;
        private Skin.Button cmdUiFontGeneral;
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
        private Skin.CheckBox chkWindowsIPv6DisableAtOs;
        private Skin.CheckBox chkWindowsDnsForceAllInterfaces;
        private Skin.LinkLabel lnkOpenVpnDirectivesHelp;
        private Skin.CheckBox chkOsSingleInstance;
        private Skin.CheckBox chkWindowsDnsLock;
        private Skin.Label label5;
        private Skin.CheckBox chkExitConfirm;
        private Skin.Label label4;
        private Skin.ComboBox cboUiUnit;
        private Skin.Label label7;
        private Skin.CheckBox chkUiFontGeneral;
        private Skin.Label lblConnect;
        private Skin.Label lblNetLock;
        private Skin.CheckBox chkConnect;
        private Skin.Label lblGeneralStartLast;
        private Skin.CheckBox chkNetLock;
        private Skin.CheckBox chkGeneralStartLast;
    }
}