namespace AirVPN.Gui.Forms
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
            AirVPN.Gui.Skin.TabPage tabAdvanced;
            AirVPN.Gui.Skin.TabPage tabDirectives;
            AirVPN.Gui.Skin.TabPage tabEventsw;
            AirVPN.Gui.Skin.ColumnHeader columnHeader1;
            AirVPN.Gui.Skin.ColumnHeader columnHeader2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            AirVPN.Gui.Skin.ColumnHeader columnHeader5;
            this.lnkAdvancedHelp = new AirVPN.Gui.Skin.LinkLabel();
            this.chkWindowsWfp = new AirVPN.Gui.Skin.CheckBox();
            this.lblOpenVpnRcvbuf = new AirVPN.Gui.Skin.Label();
            this.cboOpenVpnRcvbuf = new AirVPN.Gui.Skin.ComboBox();
            this.lblOpenVpnSndbuf = new AirVPN.Gui.Skin.Label();
            this.cboOpenVpnSndbuf = new AirVPN.Gui.Skin.ComboBox();
            this.label9 = new AirVPN.Gui.Skin.Label();
            this.chkRouteRemoveDefault = new AirVPN.Gui.Skin.CheckBox();
            this.lblExpert = new AirVPN.Gui.Skin.Label();
            this.lblAdvancedPingerEnabled = new AirVPN.Gui.Skin.Label();
            this.lblAdvancedCheckRoute = new AirVPN.Gui.Skin.Label();
            this.lblIpV6 = new AirVPN.Gui.Skin.Label();
            this.cboIpV6 = new AirVPN.Gui.Skin.ComboBox();
            this.lblAdvancedManifestRefresh = new AirVPN.Gui.Skin.Label();
            this.cboAdvancedManifestRefresh = new AirVPN.Gui.Skin.ComboBox();
            this.pnlAdvancedGeneralWindowsOnly = new System.Windows.Forms.GroupBox();
            this.chkWindowsDisableDriverUpgrade = new AirVPN.Gui.Skin.CheckBox();
            this.chkAdvancedWindowsTapUp = new AirVPN.Gui.Skin.CheckBox();
            this.chkAdvancedWindowsDhcpSwitch = new AirVPN.Gui.Skin.CheckBox();
            this.cmdAdvancedUninstallDriver = new AirVPN.Gui.Skin.Button();
            this.chkAdvancedPingerEnabled = new AirVPN.Gui.Skin.CheckBox();
            this.cmdExeBrowse = new AirVPN.Gui.Skin.Button();
            this.txtExePath = new AirVPN.Gui.Skin.TextBox();
            this.label4 = new AirVPN.Gui.Skin.Label();
            this.chkAdvancedCheckRoute = new AirVPN.Gui.Skin.CheckBox();
            this.chkExpert = new AirVPN.Gui.Skin.CheckBox();
            this.label3 = new AirVPN.Gui.Skin.Label();
            this.label2 = new AirVPN.Gui.Skin.Label();
            this.chkAdvancedOpenVpnDirectivesDefaultSkip = new AirVPN.Gui.Skin.CheckBox();
            this.txtAdvancedOpenVpnDirectivesDefault = new AirVPN.Gui.Skin.TextBox();
            this.txtAdvancedOpenVpnDirectivesCustom = new AirVPN.Gui.Skin.TextBox();
            this.cmdAdvancedEventsEdit = new AirVPN.Gui.Skin.Button();
            this.cmdAdvancedEventsClear = new AirVPN.Gui.Skin.Button();
            this.lstAdvancedEvents = new AirVPN.Gui.Skin.ListView();
            this.columnHeader3 = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.columnHeader4 = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.imgRoutes = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new AirVPN.Gui.Skin.Label();
            this.mnuRoutes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuRoutesAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRoutesRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRoutesEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.colLogDate = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.colLogMessage = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.myToolTip = new AirVPN.Gui.Skin.ToolTip(this.components);
            this.pnlCommands = new AirVPN.Gui.Skin.Panel();
            this.cmdCancel = new AirVPN.Gui.Skin.Button();
            this.cmdOk = new AirVPN.Gui.Skin.Button();
            this.tabSettings = new AirVPN.Gui.Skin.TabControl();
            this.tabGeneral = new AirVPN.Gui.Skin.TabPage();
            this.cmdUiFontGeneral = new AirVPN.Gui.Skin.Button();
            this.lblUiFontGeneral = new AirVPN.Gui.Skin.Label();
            this.chkUiFontGeneral = new AirVPN.Gui.Skin.CheckBox();
            this.chkNetLock = new AirVPN.Gui.Skin.CheckBox();
            this.chkExitConfirm = new AirVPN.Gui.Skin.CheckBox();
            this.pnlGeneralWindowsOnly = new System.Windows.Forms.GroupBox();
            this.chkSystemNotifications = new AirVPN.Gui.Skin.CheckBox();
            this.chkSystemStart = new AirVPN.Gui.Skin.CheckBox();
            this.chkMinimizeTray = new AirVPN.Gui.Skin.CheckBox();
            this.lblGeneralTheme = new AirVPN.Gui.Skin.Label();
            this.cboGeneralTheme = new AirVPN.Gui.Skin.ComboBox();
            this.chkGeneralStartLast = new AirVPN.Gui.Skin.CheckBox();
            this.cmdTos = new AirVPN.Gui.Skin.Button();
            this.chkConnect = new AirVPN.Gui.Skin.CheckBox();
            this.tabProtocols = new AirVPN.Gui.Skin.TabPage();
            this.lnkProtocolsHelp2 = new AirVPN.Gui.Skin.LinkLabel();
            this.lnkProtocolsHelp1 = new AirVPN.Gui.Skin.LinkLabel();
            this.chkProtocolsAutomatic = new System.Windows.Forms.CheckBox();
            this.lstProtocols = new AirVPN.Gui.Skin.ListView();
            this.colProtocolsProtocol = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.colProtocolsPort = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.colProtocolsEntry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProtocolsCipher = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProtocolsDescription = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.tabProxy = new AirVPN.Gui.Skin.TabPage();
            this.lnkProxyTorHelp = new AirVPN.Gui.Skin.LinkLabel();
            this.txtProxyTorControlPassword = new AirVPN.Gui.Skin.TextBox();
            this.lblProxyTorControlPassword = new AirVPN.Gui.Skin.Label();
            this.cmdProxyTorTest = new AirVPN.Gui.Skin.Button();
            this.txtProxyTorControlPort = new AirVPN.Gui.Skin.TextBox();
            this.lblProxyTorControlPort = new AirVPN.Gui.Skin.Label();
            this.label17 = new AirVPN.Gui.Skin.Label();
            this.label12 = new AirVPN.Gui.Skin.Label();
            this.lblProxyAuthentication = new AirVPN.Gui.Skin.Label();
            this.cboProxyAuthentication = new AirVPN.Gui.Skin.ComboBox();
            this.txtProxyPassword = new AirVPN.Gui.Skin.TextBox();
            this.lblProxyPassword = new AirVPN.Gui.Skin.Label();
            this.txtProxyLogin = new AirVPN.Gui.Skin.TextBox();
            this.lblProxyLogin = new AirVPN.Gui.Skin.Label();
            this.lblProxyType = new AirVPN.Gui.Skin.Label();
            this.cboProxyMode = new AirVPN.Gui.Skin.ComboBox();
            this.txtProxyPort = new AirVPN.Gui.Skin.TextBox();
            this.lblProxyPort = new AirVPN.Gui.Skin.Label();
            this.txtProxyHost = new AirVPN.Gui.Skin.TextBox();
            this.lblProxyHost = new AirVPN.Gui.Skin.Label();
            this.tabRoutes = new AirVPN.Gui.Skin.TabPage();
            this.lblRoutesNetworkLockWarning = new AirVPN.Gui.Skin.Label();
            this.cmdRouteEdit = new AirVPN.Gui.Skin.Button();
            this.cmdRouteRemove = new AirVPN.Gui.Skin.Button();
            this.cmdRouteAdd = new AirVPN.Gui.Skin.Button();
            this.label6 = new AirVPN.Gui.Skin.Label();
            this.cboRoutesOtherwise = new AirVPN.Gui.Skin.ComboBox();
            this.lblRoutesOtherwise = new AirVPN.Gui.Skin.Label();
            this.lstRoutes = new AirVPN.Gui.Skin.ListView();
            this.colRoutesIp = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.colRoutesAction = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.colRoutesNotes = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            this.tabDNS = new AirVPN.Gui.Skin.TabPage();
            this.lblDnsServers = new AirVPN.Gui.Skin.Label();
            this.cmdDnsEdit = new AirVPN.Gui.Skin.Button();
            this.cmdDnsRemove = new AirVPN.Gui.Skin.Button();
            this.cmdDnsAdd = new AirVPN.Gui.Skin.Button();
            this.lstDnsServers = new AirVPN.Gui.Skin.ListView();
            this.label7 = new AirVPN.Gui.Skin.Label();
            this.cboDnsSwitchMode = new AirVPN.Gui.Skin.ComboBox();
            this.chkDnsCheck = new AirVPN.Gui.Skin.CheckBox();
            this.tabNetworkLock = new AirVPN.Gui.Skin.TabPage();
            this.lnkLockHelp = new AirVPN.Gui.Skin.LinkLabel();
            this.chkLockAllowPing = new AirVPN.Gui.Skin.CheckBox();
            this.chkLockAllowPrivate = new AirVPN.Gui.Skin.CheckBox();
            this.lblLockRoutingOutWarning = new AirVPN.Gui.Skin.Label();
            this.lblLockAllowedIPS = new AirVPN.Gui.Skin.Label();
            this.txtLockAllowedIPS = new AirVPN.Gui.Skin.TextBox();
            this.lblLockMode = new AirVPN.Gui.Skin.Label();
            this.cboLockMode = new AirVPN.Gui.Skin.ComboBox();
            this.tabLogging = new AirVPN.Gui.Skin.TabPage();
            this.chkLogLevelDebug = new AirVPN.Gui.Skin.CheckBox();
            this.TxtLoggingPathComputed = new AirVPN.Gui.Skin.Label();
            this.lblLoggingHelp = new AirVPN.Gui.Skin.Label();
            this.TxtLoggingPath = new AirVPN.Gui.Skin.TextBox();
            this.label8 = new AirVPN.Gui.Skin.Label();
            this.chkLoggingEnabled = new AirVPN.Gui.Skin.CheckBox();
            tabAdvanced = new AirVPN.Gui.Skin.TabPage();
            tabDirectives = new AirVPN.Gui.Skin.TabPage();
            tabEventsw = new AirVPN.Gui.Skin.TabPage();
            columnHeader1 = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            columnHeader2 = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
            columnHeader5 = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
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
            tabAdvanced.Controls.Add(this.chkWindowsWfp);
            tabAdvanced.Controls.Add(this.lblOpenVpnRcvbuf);
            tabAdvanced.Controls.Add(this.cboOpenVpnRcvbuf);
            tabAdvanced.Controls.Add(this.lblOpenVpnSndbuf);
            tabAdvanced.Controls.Add(this.cboOpenVpnSndbuf);
            tabAdvanced.Controls.Add(this.label9);
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
            tabAdvanced.Controls.Add(this.label4);
            tabAdvanced.Controls.Add(this.chkAdvancedCheckRoute);
            tabAdvanced.Controls.Add(this.chkExpert);
            tabAdvanced.Location = new System.Drawing.Point(4, 24);
            tabAdvanced.Name = "tabAdvanced";
            tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
            tabAdvanced.Size = new System.Drawing.Size(673, 286);
            tabAdvanced.TabIndex = 0;
            tabAdvanced.Text = "Advanced";
            // 
            // lnkAdvancedHelp
            // 
            this.lnkAdvancedHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkAdvancedHelp.Location = new System.Drawing.Point(318, 252);
            this.lnkAdvancedHelp.Name = "lnkAdvancedHelp";
            this.lnkAdvancedHelp.Size = new System.Drawing.Size(347, 22);
            this.lnkAdvancedHelp.TabIndex = 87;
            this.lnkAdvancedHelp.TabStop = true;
            this.lnkAdvancedHelp.Text = "Explanation of Advanced Features?";
            this.lnkAdvancedHelp.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.lnkAdvancedHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkAdvancedHelp_LinkClicked);
            // 
            // chkWindowsWfp
            // 
            this.chkWindowsWfp.BackColor = System.Drawing.Color.Transparent;
            this.chkWindowsWfp.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkWindowsWfp.ForeColor = System.Drawing.Color.Black;
            this.chkWindowsWfp.Location = new System.Drawing.Point(513, 174);
            this.chkWindowsWfp.Name = "chkWindowsWfp";
            this.chkWindowsWfp.Size = new System.Drawing.Size(137, 22);
            this.chkWindowsWfp.TabIndex = 86;
            this.chkWindowsWfp.Text = "Experimental: WFP";
            this.chkWindowsWfp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkWindowsWfp.UseVisualStyleBackColor = false;
            // 
            // lblOpenVpnRcvbuf
            // 
            this.lblOpenVpnRcvbuf.BackColor = System.Drawing.Color.Transparent;
            this.lblOpenVpnRcvbuf.ForeColor = System.Drawing.Color.Black;
            this.lblOpenVpnRcvbuf.Location = new System.Drawing.Point(14, 202);
            this.lblOpenVpnRcvbuf.Name = "lblOpenVpnRcvbuf";
            this.lblOpenVpnRcvbuf.Size = new System.Drawing.Size(220, 21);
            this.lblOpenVpnRcvbuf.TabIndex = 85;
            this.lblOpenVpnRcvbuf.Text = "TCP/UDP socket receive buffer size:";
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
            this.cboOpenVpnRcvbuf.Location = new System.Drawing.Point(246, 202);
            this.cboOpenVpnRcvbuf.Name = "cboOpenVpnRcvbuf";
            this.cboOpenVpnRcvbuf.Size = new System.Drawing.Size(133, 21);
            this.cboOpenVpnRcvbuf.TabIndex = 84;
            // 
            // lblOpenVpnSndbuf
            // 
            this.lblOpenVpnSndbuf.BackColor = System.Drawing.Color.Transparent;
            this.lblOpenVpnSndbuf.ForeColor = System.Drawing.Color.Black;
            this.lblOpenVpnSndbuf.Location = new System.Drawing.Point(14, 175);
            this.lblOpenVpnSndbuf.Name = "lblOpenVpnSndbuf";
            this.lblOpenVpnSndbuf.Size = new System.Drawing.Size(220, 21);
            this.lblOpenVpnSndbuf.TabIndex = 83;
            this.lblOpenVpnSndbuf.Text = "TCP/UDP socket send buffer size:";
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
            this.cboOpenVpnSndbuf.Location = new System.Drawing.Point(246, 175);
            this.cboOpenVpnSndbuf.Name = "cboOpenVpnSndbuf";
            this.cboOpenVpnSndbuf.Size = new System.Drawing.Size(133, 21);
            this.cboOpenVpnSndbuf.TabIndex = 82;
            // 
            // label9
            // 
            this.label9.BackColor = System.Drawing.Color.Transparent;
            this.label9.ForeColor = System.Drawing.Color.Black;
            this.label9.Location = new System.Drawing.Point(14, 150);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(220, 22);
            this.label9.TabIndex = 81;
            this.label9.Text = "Remove the default gateway route:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkRouteRemoveDefault
            // 
            this.chkRouteRemoveDefault.BackColor = System.Drawing.Color.Transparent;
            this.chkRouteRemoveDefault.ForeColor = System.Drawing.Color.Black;
            this.chkRouteRemoveDefault.Location = new System.Drawing.Point(246, 150);
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
            this.lblAdvancedPingerEnabled.Location = new System.Drawing.Point(14, 125);
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
            this.lblAdvancedCheckRoute.Location = new System.Drawing.Point(14, 46);
            this.lblAdvancedCheckRoute.Name = "lblAdvancedCheckRoute";
            this.lblAdvancedCheckRoute.Size = new System.Drawing.Size(223, 22);
            this.lblAdvancedCheckRoute.TabIndex = 77;
            this.lblAdvancedCheckRoute.Text = "Check if the tunnel effectively works:";
            this.lblAdvancedCheckRoute.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblIpV6
            // 
            this.lblIpV6.BackColor = System.Drawing.Color.Transparent;
            this.lblIpV6.ForeColor = System.Drawing.Color.Black;
            this.lblIpV6.Location = new System.Drawing.Point(17, 72);
            this.lblIpV6.Name = "lblIpV6";
            this.lblIpV6.Size = new System.Drawing.Size(220, 21);
            this.lblIpV6.TabIndex = 76;
            this.lblIpV6.Text = "IPV6:";
            this.lblIpV6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboIpV6
            // 
            this.cboIpV6.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboIpV6.FormattingEnabled = true;
            this.cboIpV6.Items.AddRange(new object[] {
            "None",
            "Disable"});
            this.cboIpV6.Location = new System.Drawing.Point(246, 72);
            this.cboIpV6.Name = "cboIpV6";
            this.cboIpV6.Size = new System.Drawing.Size(133, 21);
            this.cboIpV6.TabIndex = 75;
            // 
            // lblAdvancedManifestRefresh
            // 
            this.lblAdvancedManifestRefresh.BackColor = System.Drawing.Color.Transparent;
            this.lblAdvancedManifestRefresh.ForeColor = System.Drawing.Color.Black;
            this.lblAdvancedManifestRefresh.Location = new System.Drawing.Point(14, 98);
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
            this.cboAdvancedManifestRefresh.Location = new System.Drawing.Point(246, 98);
            this.cboAdvancedManifestRefresh.Name = "cboAdvancedManifestRefresh";
            this.cboAdvancedManifestRefresh.Size = new System.Drawing.Size(133, 21);
            this.cboAdvancedManifestRefresh.TabIndex = 73;
            // 
            // pnlAdvancedGeneralWindowsOnly
            // 
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkWindowsDisableDriverUpgrade);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkAdvancedWindowsTapUp);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkAdvancedWindowsDhcpSwitch);
            this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.cmdAdvancedUninstallDriver);
            this.pnlAdvancedGeneralWindowsOnly.Location = new System.Drawing.Point(414, 18);
            this.pnlAdvancedGeneralWindowsOnly.Name = "pnlAdvancedGeneralWindowsOnly";
            this.pnlAdvancedGeneralWindowsOnly.Size = new System.Drawing.Size(236, 139);
            this.pnlAdvancedGeneralWindowsOnly.TabIndex = 69;
            this.pnlAdvancedGeneralWindowsOnly.TabStop = false;
            this.pnlAdvancedGeneralWindowsOnly.Text = "Microsoft Windows Only";
            // 
            // chkWindowsDisableDriverUpgrade
            // 
            this.chkWindowsDisableDriverUpgrade.BackColor = System.Drawing.Color.Transparent;
            this.chkWindowsDisableDriverUpgrade.ForeColor = System.Drawing.Color.Black;
            this.chkWindowsDisableDriverUpgrade.Location = new System.Drawing.Point(16, 68);
            this.chkWindowsDisableDriverUpgrade.Name = "chkWindowsDisableDriverUpgrade";
            this.chkWindowsDisableDriverUpgrade.Size = new System.Drawing.Size(207, 22);
            this.chkWindowsDisableDriverUpgrade.TabIndex = 66;
            this.chkWindowsDisableDriverUpgrade.Text = "Disable driver upgrade";
            this.chkWindowsDisableDriverUpgrade.UseVisualStyleBackColor = false;
            // 
            // chkAdvancedWindowsTapUp
            // 
            this.chkAdvancedWindowsTapUp.BackColor = System.Drawing.Color.Transparent;
            this.chkAdvancedWindowsTapUp.ForeColor = System.Drawing.Color.Black;
            this.chkAdvancedWindowsTapUp.Location = new System.Drawing.Point(16, 21);
            this.chkAdvancedWindowsTapUp.Name = "chkAdvancedWindowsTapUp";
            this.chkAdvancedWindowsTapUp.Size = new System.Drawing.Size(207, 22);
            this.chkAdvancedWindowsTapUp.TabIndex = 55;
            this.chkAdvancedWindowsTapUp.Text = "Force TAP interface UP";
            this.chkAdvancedWindowsTapUp.UseVisualStyleBackColor = false;
            // 
            // chkAdvancedWindowsDhcpSwitch
            // 
            this.chkAdvancedWindowsDhcpSwitch.BackColor = System.Drawing.Color.Transparent;
            this.chkAdvancedWindowsDhcpSwitch.ForeColor = System.Drawing.Color.Black;
            this.chkAdvancedWindowsDhcpSwitch.Location = new System.Drawing.Point(16, 44);
            this.chkAdvancedWindowsDhcpSwitch.Name = "chkAdvancedWindowsDhcpSwitch";
            this.chkAdvancedWindowsDhcpSwitch.Size = new System.Drawing.Size(207, 22);
            this.chkAdvancedWindowsDhcpSwitch.TabIndex = 64;
            this.chkAdvancedWindowsDhcpSwitch.Text = "Switch DHCP to Static";
            this.chkAdvancedWindowsDhcpSwitch.UseVisualStyleBackColor = false;
            // 
            // cmdAdvancedUninstallDriver
            // 
            this.cmdAdvancedUninstallDriver.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdAdvancedUninstallDriver.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdAdvancedUninstallDriver.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdAdvancedUninstallDriver.FlatAppearance.BorderSize = 0;
            this.cmdAdvancedUninstallDriver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdAdvancedUninstallDriver.Location = new System.Drawing.Point(10, 98);
            this.cmdAdvancedUninstallDriver.Name = "cmdAdvancedUninstallDriver";
            this.cmdAdvancedUninstallDriver.Size = new System.Drawing.Size(217, 30);
            this.cmdAdvancedUninstallDriver.TabIndex = 65;
            this.cmdAdvancedUninstallDriver.Text = "Uninstall Driver";
            this.cmdAdvancedUninstallDriver.UseVisualStyleBackColor = true;
            this.cmdAdvancedUninstallDriver.Click += new System.EventHandler(this.cmdAdvancedUninstallDriver_Click);
            // 
            // chkAdvancedPingerEnabled
            // 
            this.chkAdvancedPingerEnabled.BackColor = System.Drawing.Color.Transparent;
            this.chkAdvancedPingerEnabled.ForeColor = System.Drawing.Color.Black;
            this.chkAdvancedPingerEnabled.Location = new System.Drawing.Point(246, 125);
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
            this.cmdExeBrowse.Image = global::AirVPN.Lib.Forms.Properties.Resources.browse;
            this.cmdExeBrowse.Location = new System.Drawing.Point(426, 229);
            this.cmdExeBrowse.Name = "cmdExeBrowse";
            this.cmdExeBrowse.Size = new System.Drawing.Size(33, 25);
            this.cmdExeBrowse.TabIndex = 60;
            this.cmdExeBrowse.UseVisualStyleBackColor = true;
            this.cmdExeBrowse.Click += new System.EventHandler(this.cmdExeBrowse_Click);
            // 
            // txtExePath
            // 
            this.txtExePath.Location = new System.Drawing.Point(246, 231);
            this.txtExePath.Name = "txtExePath";
            this.txtExePath.Size = new System.Drawing.Size(174, 20);
            this.txtExePath.TabIndex = 59;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(14, 234);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(220, 13);
            this.label4.TabIndex = 58;
            this.label4.Text = "OpenVPN Custom Path:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkAdvancedCheckRoute
            // 
            this.chkAdvancedCheckRoute.BackColor = System.Drawing.Color.Transparent;
            this.chkAdvancedCheckRoute.ForeColor = System.Drawing.Color.Black;
            this.chkAdvancedCheckRoute.Location = new System.Drawing.Point(246, 46);
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
            tabDirectives.Controls.Add(this.label3);
            tabDirectives.Controls.Add(this.label2);
            tabDirectives.Controls.Add(this.chkAdvancedOpenVpnDirectivesDefaultSkip);
            tabDirectives.Controls.Add(this.txtAdvancedOpenVpnDirectivesDefault);
            tabDirectives.Controls.Add(this.txtAdvancedOpenVpnDirectivesCustom);
            tabDirectives.Location = new System.Drawing.Point(4, 24);
            tabDirectives.Name = "tabDirectives";
            tabDirectives.Padding = new System.Windows.Forms.Padding(3);
            tabDirectives.Size = new System.Drawing.Size(673, 286);
            tabDirectives.TabIndex = 1;
            tabDirectives.Text = "OVPN directives";
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(334, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(331, 20);
            this.label3.TabIndex = 61;
            this.label3.Text = "Default:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(10, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(318, 20);
            this.label2.TabIndex = 60;
            this.label2.Text = "Custom:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // chkAdvancedOpenVpnDirectivesDefaultSkip
            // 
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.BackColor = System.Drawing.Color.Transparent;
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.ForeColor = System.Drawing.Color.Black;
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.Location = new System.Drawing.Point(10, 249);
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.Name = "chkAdvancedOpenVpnDirectivesDefaultSkip";
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.Size = new System.Drawing.Size(655, 31);
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.TabIndex = 59;
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.Text = "Skip the above default directives. If unchecked, your custom directives are appen" +
    "ded.";
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkAdvancedOpenVpnDirectivesDefaultSkip.UseVisualStyleBackColor = false;
            // 
            // txtAdvancedOpenVpnDirectivesDefault
            // 
            this.txtAdvancedOpenVpnDirectivesDefault.AcceptsReturn = true;
            this.txtAdvancedOpenVpnDirectivesDefault.AcceptsTab = true;
            this.txtAdvancedOpenVpnDirectivesDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAdvancedOpenVpnDirectivesDefault.Location = new System.Drawing.Point(334, 30);
            this.txtAdvancedOpenVpnDirectivesDefault.Multiline = true;
            this.txtAdvancedOpenVpnDirectivesDefault.Name = "txtAdvancedOpenVpnDirectivesDefault";
            this.txtAdvancedOpenVpnDirectivesDefault.ReadOnly = true;
            this.txtAdvancedOpenVpnDirectivesDefault.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAdvancedOpenVpnDirectivesDefault.Size = new System.Drawing.Size(331, 213);
            this.txtAdvancedOpenVpnDirectivesDefault.TabIndex = 58;
            // 
            // txtAdvancedOpenVpnDirectivesCustom
            // 
            this.txtAdvancedOpenVpnDirectivesCustom.AcceptsReturn = true;
            this.txtAdvancedOpenVpnDirectivesCustom.AcceptsTab = true;
            this.txtAdvancedOpenVpnDirectivesCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAdvancedOpenVpnDirectivesCustom.Location = new System.Drawing.Point(10, 30);
            this.txtAdvancedOpenVpnDirectivesCustom.Multiline = true;
            this.txtAdvancedOpenVpnDirectivesCustom.Name = "txtAdvancedOpenVpnDirectivesCustom";
            this.txtAdvancedOpenVpnDirectivesCustom.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAdvancedOpenVpnDirectivesCustom.Size = new System.Drawing.Size(318, 213);
            this.txtAdvancedOpenVpnDirectivesCustom.TabIndex = 57;
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
            tabEventsw.Size = new System.Drawing.Size(673, 286);
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
            this.cmdAdvancedEventsEdit.Image = global::AirVPN.Lib.Forms.Properties.Resources.edit;
            this.cmdAdvancedEventsEdit.Location = new System.Drawing.Point(633, 66);
            this.cmdAdvancedEventsEdit.Name = "cmdAdvancedEventsEdit";
            this.cmdAdvancedEventsEdit.Size = new System.Drawing.Size(32, 30);
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
            this.cmdAdvancedEventsClear.Image = global::AirVPN.Lib.Forms.Properties.Resources.delete;
            this.cmdAdvancedEventsClear.Location = new System.Drawing.Point(633, 30);
            this.cmdAdvancedEventsClear.Name = "cmdAdvancedEventsClear";
            this.cmdAdvancedEventsClear.Size = new System.Drawing.Size(32, 30);
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
            this.lstAdvancedEvents.Size = new System.Drawing.Size(617, 247);
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
            this.mnuRoutesAdd.Image = global::AirVPN.Lib.Forms.Properties.Resources.add;
            this.mnuRoutesAdd.Name = "mnuRoutesAdd";
            this.mnuRoutesAdd.Size = new System.Drawing.Size(121, 26);
            this.mnuRoutesAdd.Text = "Add";
            this.mnuRoutesAdd.Click += new System.EventHandler(this.mnuRoutesAdd_Click);
            // 
            // mnuRoutesRemove
            // 
            this.mnuRoutesRemove.Image = global::AirVPN.Lib.Forms.Properties.Resources.delete;
            this.mnuRoutesRemove.Name = "mnuRoutesRemove";
            this.mnuRoutesRemove.Size = new System.Drawing.Size(121, 26);
            this.mnuRoutesRemove.Text = "Remove";
            this.mnuRoutesRemove.Click += new System.EventHandler(this.mnuRoutesRemove_Click);
            // 
            // mnuRoutesEdit
            // 
            this.mnuRoutesEdit.Image = global::AirVPN.Lib.Forms.Properties.Resources.edit;
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
            this.pnlCommands.Location = new System.Drawing.Point(266, 320);
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
            this.tabSettings.Location = new System.Drawing.Point(180, 0);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.SelectedIndex = 0;
            this.tabSettings.Size = new System.Drawing.Size(681, 314);
            this.tabSettings.TabIndex = 41;
            // 
            // tabGeneral
            // 
            this.tabGeneral.BackColor = System.Drawing.Color.White;
            this.tabGeneral.Controls.Add(this.cmdUiFontGeneral);
            this.tabGeneral.Controls.Add(this.lblUiFontGeneral);
            this.tabGeneral.Controls.Add(this.chkUiFontGeneral);
            this.tabGeneral.Controls.Add(this.chkNetLock);
            this.tabGeneral.Controls.Add(this.chkExitConfirm);
            this.tabGeneral.Controls.Add(this.pnlGeneralWindowsOnly);
            this.tabGeneral.Controls.Add(this.lblGeneralTheme);
            this.tabGeneral.Controls.Add(this.cboGeneralTheme);
            this.tabGeneral.Controls.Add(this.chkGeneralStartLast);
            this.tabGeneral.Controls.Add(this.cmdTos);
            this.tabGeneral.Controls.Add(this.chkConnect);
            this.tabGeneral.Location = new System.Drawing.Point(4, 24);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Size = new System.Drawing.Size(673, 286);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            // 
            // cmdUiFontGeneral
            // 
            this.cmdUiFontGeneral.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdUiFontGeneral.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdUiFontGeneral.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdUiFontGeneral.FlatAppearance.BorderSize = 0;
            this.cmdUiFontGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdUiFontGeneral.Location = new System.Drawing.Point(491, 180);
            this.cmdUiFontGeneral.Name = "cmdUiFontGeneral";
            this.cmdUiFontGeneral.Size = new System.Drawing.Size(34, 22);
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
            this.lblUiFontGeneral.Location = new System.Drawing.Point(251, 180);
            this.lblUiFontGeneral.Name = "lblUiFontGeneral";
            this.lblUiFontGeneral.Size = new System.Drawing.Size(234, 22);
            this.lblUiFontGeneral.TabIndex = 69;
            this.lblUiFontGeneral.Text = "Theme:";
            this.lblUiFontGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkUiFontGeneral
            // 
            this.chkUiFontGeneral.BackColor = System.Drawing.Color.Transparent;
            this.chkUiFontGeneral.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkUiFontGeneral.ForeColor = System.Drawing.Color.Black;
            this.chkUiFontGeneral.Location = new System.Drawing.Point(138, 180);
            this.chkUiFontGeneral.Name = "chkUiFontGeneral";
            this.chkUiFontGeneral.Size = new System.Drawing.Size(107, 22);
            this.chkUiFontGeneral.TabIndex = 68;
            this.chkUiFontGeneral.Text = "Custom Font:";
            this.chkUiFontGeneral.UseVisualStyleBackColor = false;
            this.chkUiFontGeneral.CheckedChanged += new System.EventHandler(this.chkUiFontGeneral_CheckedChanged);
            // 
            // chkNetLock
            // 
            this.chkNetLock.AutoSize = true;
            this.chkNetLock.BackColor = System.Drawing.Color.Transparent;
            this.chkNetLock.ForeColor = System.Drawing.Color.Black;
            this.chkNetLock.Location = new System.Drawing.Point(17, 39);
            this.chkNetLock.Name = "chkNetLock";
            this.chkNetLock.Size = new System.Drawing.Size(130, 17);
            this.chkNetLock.TabIndex = 67;
            this.chkNetLock.Text = "*Lock network at start";
            this.chkNetLock.UseVisualStyleBackColor = false;
            this.chkNetLock.CheckedChanged += new System.EventHandler(this.chkNetLock_CheckedChanged);
            // 
            // chkExitConfirm
            // 
            this.chkExitConfirm.BackColor = System.Drawing.Color.Transparent;
            this.chkExitConfirm.ForeColor = System.Drawing.Color.Black;
            this.chkExitConfirm.Location = new System.Drawing.Point(17, 125);
            this.chkExitConfirm.Name = "chkExitConfirm";
            this.chkExitConfirm.Size = new System.Drawing.Size(281, 17);
            this.chkExitConfirm.TabIndex = 66;
            this.chkExitConfirm.Text = "Exit confirmation prompt";
            this.chkExitConfirm.UseVisualStyleBackColor = false;
            // 
            // pnlGeneralWindowsOnly
            // 
            this.pnlGeneralWindowsOnly.Controls.Add(this.chkSystemNotifications);
            this.pnlGeneralWindowsOnly.Controls.Add(this.chkSystemStart);
            this.pnlGeneralWindowsOnly.Controls.Add(this.chkMinimizeTray);
            this.pnlGeneralWindowsOnly.Location = new System.Drawing.Point(436, 14);
            this.pnlGeneralWindowsOnly.Name = "pnlGeneralWindowsOnly";
            this.pnlGeneralWindowsOnly.Size = new System.Drawing.Size(214, 98);
            this.pnlGeneralWindowsOnly.TabIndex = 65;
            this.pnlGeneralWindowsOnly.TabStop = false;
            this.pnlGeneralWindowsOnly.Text = "Microsoft Windows Only";
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
            // lblGeneralTheme
            // 
            this.lblGeneralTheme.BackColor = System.Drawing.Color.Transparent;
            this.lblGeneralTheme.ForeColor = System.Drawing.Color.Black;
            this.lblGeneralTheme.Location = new System.Drawing.Point(18, 254);
            this.lblGeneralTheme.Name = "lblGeneralTheme";
            this.lblGeneralTheme.Size = new System.Drawing.Size(48, 20);
            this.lblGeneralTheme.TabIndex = 43;
            this.lblGeneralTheme.Text = "Theme:";
            this.lblGeneralTheme.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cboGeneralTheme
            // 
            this.cboGeneralTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGeneralTheme.FormattingEnabled = true;
            this.cboGeneralTheme.Items.AddRange(new object[] {
            "Light",
            "Dark"});
            this.cboGeneralTheme.Location = new System.Drawing.Point(70, 250);
            this.cboGeneralTheme.Name = "cboGeneralTheme";
            this.cboGeneralTheme.Size = new System.Drawing.Size(67, 21);
            this.cboGeneralTheme.TabIndex = 42;
            this.cboGeneralTheme.SelectedIndexChanged += new System.EventHandler(this.cboGeneralTheme_SelectedIndexChanged);
            // 
            // chkGeneralStartLast
            // 
            this.chkGeneralStartLast.BackColor = System.Drawing.Color.Transparent;
            this.chkGeneralStartLast.ForeColor = System.Drawing.Color.Black;
            this.chkGeneralStartLast.Location = new System.Drawing.Point(17, 80);
            this.chkGeneralStartLast.Name = "chkGeneralStartLast";
            this.chkGeneralStartLast.Size = new System.Drawing.Size(281, 39);
            this.chkGeneralStartLast.TabIndex = 40;
            this.chkGeneralStartLast.Text = "Force reconnection to last server at startup (otherwise current best server is pi" +
    "cked)";
            this.chkGeneralStartLast.UseVisualStyleBackColor = false;
            // 
            // cmdTos
            // 
            this.cmdTos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdTos.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdTos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdTos.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdTos.FlatAppearance.BorderSize = 0;
            this.cmdTos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdTos.Location = new System.Drawing.Point(436, 242);
            this.cmdTos.Name = "cmdTos";
            this.cmdTos.Size = new System.Drawing.Size(214, 30);
            this.cmdTos.TabIndex = 38;
            this.cmdTos.Text = "Terms of Service";
            this.cmdTos.UseVisualStyleBackColor = true;
            this.cmdTos.Click += new System.EventHandler(this.cmdTos_Click);
            // 
            // chkConnect
            // 
            this.chkConnect.AutoSize = true;
            this.chkConnect.BackColor = System.Drawing.Color.Transparent;
            this.chkConnect.ForeColor = System.Drawing.Color.Black;
            this.chkConnect.Location = new System.Drawing.Point(17, 14);
            this.chkConnect.Name = "chkConnect";
            this.chkConnect.Size = new System.Drawing.Size(146, 17);
            this.chkConnect.TabIndex = 30;
            this.chkConnect.Text = "*Connect when launched";
            this.chkConnect.UseVisualStyleBackColor = false;
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
            this.tabProtocols.Size = new System.Drawing.Size(673, 286);
            this.tabProtocols.TabIndex = 3;
            this.tabProtocols.Text = "Protocols";
            // 
            // lnkProtocolsHelp2
            // 
            this.lnkProtocolsHelp2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkProtocolsHelp2.Location = new System.Drawing.Point(519, 263);
            this.lnkProtocolsHelp2.Name = "lnkProtocolsHelp2";
            this.lnkProtocolsHelp2.Size = new System.Drawing.Size(145, 18);
            this.lnkProtocolsHelp2.TabIndex = 72;
            this.lnkProtocolsHelp2.TabStop = true;
            this.lnkProtocolsHelp2.Text = "UDP vs TCP?";
            this.lnkProtocolsHelp2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkProtocolsHelp2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProtocolsHelp2_LinkClicked);
            // 
            // lnkProtocolsHelp1
            // 
            this.lnkProtocolsHelp1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkProtocolsHelp1.Location = new System.Drawing.Point(382, 245);
            this.lnkProtocolsHelp1.Name = "lnkProtocolsHelp1";
            this.lnkProtocolsHelp1.Size = new System.Drawing.Size(283, 18);
            this.lnkProtocolsHelp1.TabIndex = 71;
            this.lnkProtocolsHelp1.TabStop = true;
            this.lnkProtocolsHelp1.Text = "What is the difference between protocols?";
            this.lnkProtocolsHelp1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkProtocolsHelp1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProtocolsHelp1_LinkClicked);
            // 
            // chkProtocolsAutomatic
            // 
            this.chkProtocolsAutomatic.AutoSize = true;
            this.chkProtocolsAutomatic.Location = new System.Drawing.Point(13, 14);
            this.chkProtocolsAutomatic.Name = "chkProtocolsAutomatic";
            this.chkProtocolsAutomatic.Size = new System.Drawing.Size(73, 17);
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
            this.colProtocolsCipher,
            this.colProtocolsDescription});
            this.lstProtocols.FullRowSelect = true;
            this.lstProtocols.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstProtocols.HideSelection = false;
            this.lstProtocols.Location = new System.Drawing.Point(13, 38);
            this.lstProtocols.MultiSelect = false;
            this.lstProtocols.Name = "lstProtocols";
            this.lstProtocols.OwnerDraw = true;
            this.lstProtocols.Size = new System.Drawing.Size(650, 204);
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
            this.colProtocolsEntry.Text = "Entry Index";
            // 
            // colProtocolsCipher
            // 
            this.colProtocolsCipher.Text = "Cipher";
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
            this.tabProxy.Size = new System.Drawing.Size(673, 286);
            this.tabProxy.TabIndex = 1;
            this.tabProxy.Text = "Proxy";
            // 
            // lnkProxyTorHelp
            // 
            this.lnkProxyTorHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkProxyTorHelp.Location = new System.Drawing.Point(383, 261);
            this.lnkProxyTorHelp.Name = "lnkProxyTorHelp";
            this.lnkProxyTorHelp.Size = new System.Drawing.Size(282, 22);
            this.lnkProxyTorHelp.TabIndex = 74;
            this.lnkProxyTorHelp.TabStop = true;
            this.lnkProxyTorHelp.Text = "More about Tor over VPN";
            this.lnkProxyTorHelp.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lnkProxyTorHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProxyTorHelp_LinkClicked);
            // 
            // txtProxyTorControlPassword
            // 
            this.txtProxyTorControlPassword.Location = new System.Drawing.Point(150, 236);
            this.txtProxyTorControlPassword.Name = "txtProxyTorControlPassword";
            this.txtProxyTorControlPassword.Size = new System.Drawing.Size(119, 20);
            this.txtProxyTorControlPassword.TabIndex = 73;
            // 
            // lblProxyTorControlPassword
            // 
            this.lblProxyTorControlPassword.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyTorControlPassword.ForeColor = System.Drawing.Color.Black;
            this.lblProxyTorControlPassword.Location = new System.Drawing.Point(16, 236);
            this.lblProxyTorControlPassword.Name = "lblProxyTorControlPassword";
            this.lblProxyTorControlPassword.Size = new System.Drawing.Size(128, 20);
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
            this.cmdProxyTorTest.Location = new System.Drawing.Point(275, 210);
            this.cmdProxyTorTest.Name = "cmdProxyTorTest";
            this.cmdProxyTorTest.Size = new System.Drawing.Size(79, 46);
            this.cmdProxyTorTest.TabIndex = 71;
            this.cmdProxyTorTest.Text = "Test";
            this.cmdProxyTorTest.UseVisualStyleBackColor = true;
            this.cmdProxyTorTest.Click += new System.EventHandler(this.cmdProxyTorTest_Click);
            // 
            // txtProxyTorControlPort
            // 
            this.txtProxyTorControlPort.Location = new System.Drawing.Point(150, 210);
            this.txtProxyTorControlPort.Name = "txtProxyTorControlPort";
            this.txtProxyTorControlPort.Size = new System.Drawing.Size(119, 20);
            this.txtProxyTorControlPort.TabIndex = 70;
            // 
            // lblProxyTorControlPort
            // 
            this.lblProxyTorControlPort.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyTorControlPort.ForeColor = System.Drawing.Color.Black;
            this.lblProxyTorControlPort.Location = new System.Drawing.Point(16, 210);
            this.lblProxyTorControlPort.Name = "lblProxyTorControlPort";
            this.lblProxyTorControlPort.Size = new System.Drawing.Size(128, 20);
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
            this.label17.Location = new System.Drawing.Point(380, 210);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(285, 42);
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
            this.label12.Location = new System.Drawing.Point(383, 15);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(282, 50);
            this.label12.TabIndex = 44;
            this.label12.Text = "       UDP, SSH and SSL connections will not be available if you use a proxy.";
            // 
            // lblProxyAuthentication
            // 
            this.lblProxyAuthentication.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyAuthentication.ForeColor = System.Drawing.Color.Black;
            this.lblProxyAuthentication.Location = new System.Drawing.Point(16, 108);
            this.lblProxyAuthentication.Name = "lblProxyAuthentication";
            this.lblProxyAuthentication.Size = new System.Drawing.Size(128, 21);
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
            this.cboProxyAuthentication.Location = new System.Drawing.Point(150, 108);
            this.cboProxyAuthentication.Name = "cboProxyAuthentication";
            this.cboProxyAuthentication.Size = new System.Drawing.Size(119, 21);
            this.cboProxyAuthentication.TabIndex = 42;
            this.cboProxyAuthentication.SelectedIndexChanged += new System.EventHandler(this.cboProxyAuthentication_SelectedIndexChanged);
            // 
            // txtProxyPassword
            // 
            this.txtProxyPassword.Location = new System.Drawing.Point(150, 161);
            this.txtProxyPassword.Name = "txtProxyPassword";
            this.txtProxyPassword.Size = new System.Drawing.Size(119, 20);
            this.txtProxyPassword.TabIndex = 41;
            // 
            // lblProxyPassword
            // 
            this.lblProxyPassword.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyPassword.ForeColor = System.Drawing.Color.Black;
            this.lblProxyPassword.Location = new System.Drawing.Point(16, 161);
            this.lblProxyPassword.Name = "lblProxyPassword";
            this.lblProxyPassword.Size = new System.Drawing.Size(128, 20);
            this.lblProxyPassword.TabIndex = 40;
            this.lblProxyPassword.Text = "Password:";
            this.lblProxyPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtProxyLogin
            // 
            this.txtProxyLogin.Location = new System.Drawing.Point(150, 135);
            this.txtProxyLogin.Name = "txtProxyLogin";
            this.txtProxyLogin.Size = new System.Drawing.Size(119, 20);
            this.txtProxyLogin.TabIndex = 39;
            // 
            // lblProxyLogin
            // 
            this.lblProxyLogin.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyLogin.ForeColor = System.Drawing.Color.Black;
            this.lblProxyLogin.Location = new System.Drawing.Point(16, 135);
            this.lblProxyLogin.Name = "lblProxyLogin";
            this.lblProxyLogin.Size = new System.Drawing.Size(128, 20);
            this.lblProxyLogin.TabIndex = 38;
            this.lblProxyLogin.Text = "Login:";
            this.lblProxyLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblProxyType
            // 
            this.lblProxyType.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyType.ForeColor = System.Drawing.Color.Black;
            this.lblProxyType.Location = new System.Drawing.Point(16, 18);
            this.lblProxyType.Name = "lblProxyType";
            this.lblProxyType.Size = new System.Drawing.Size(128, 21);
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
            this.cboProxyMode.Location = new System.Drawing.Point(150, 18);
            this.cboProxyMode.Name = "cboProxyMode";
            this.cboProxyMode.Size = new System.Drawing.Size(119, 21);
            this.cboProxyMode.TabIndex = 36;
            this.cboProxyMode.SelectedIndexChanged += new System.EventHandler(this.cboProxyMode_SelectedIndexChanged);
            // 
            // txtProxyPort
            // 
            this.txtProxyPort.Location = new System.Drawing.Point(150, 71);
            this.txtProxyPort.Name = "txtProxyPort";
            this.txtProxyPort.Size = new System.Drawing.Size(119, 20);
            this.txtProxyPort.TabIndex = 35;
            // 
            // lblProxyPort
            // 
            this.lblProxyPort.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyPort.ForeColor = System.Drawing.Color.Black;
            this.lblProxyPort.Location = new System.Drawing.Point(16, 71);
            this.lblProxyPort.Name = "lblProxyPort";
            this.lblProxyPort.Size = new System.Drawing.Size(128, 20);
            this.lblProxyPort.TabIndex = 34;
            this.lblProxyPort.Text = "Port:";
            this.lblProxyPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtProxyHost
            // 
            this.txtProxyHost.Location = new System.Drawing.Point(150, 45);
            this.txtProxyHost.Name = "txtProxyHost";
            this.txtProxyHost.Size = new System.Drawing.Size(119, 20);
            this.txtProxyHost.TabIndex = 33;
            // 
            // lblProxyHost
            // 
            this.lblProxyHost.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyHost.ForeColor = System.Drawing.Color.Black;
            this.lblProxyHost.Location = new System.Drawing.Point(16, 45);
            this.lblProxyHost.Name = "lblProxyHost";
            this.lblProxyHost.Size = new System.Drawing.Size(128, 20);
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
            this.tabRoutes.Size = new System.Drawing.Size(673, 286);
            this.tabRoutes.TabIndex = 5;
            this.tabRoutes.Text = "Routes";
            // 
            // lblRoutesNetworkLockWarning
            // 
            this.lblRoutesNetworkLockWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRoutesNetworkLockWarning.BackColor = System.Drawing.Color.Transparent;
            this.lblRoutesNetworkLockWarning.ForeColor = System.Drawing.Color.Black;
            this.lblRoutesNetworkLockWarning.Location = new System.Drawing.Point(9, 251);
            this.lblRoutesNetworkLockWarning.Name = "lblRoutesNetworkLockWarning";
            this.lblRoutesNetworkLockWarning.Size = new System.Drawing.Size(282, 29);
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
            this.cmdRouteEdit.Image = global::AirVPN.Lib.Forms.Properties.Resources.edit;
            this.cmdRouteEdit.Location = new System.Drawing.Point(633, 102);
            this.cmdRouteEdit.Name = "cmdRouteEdit";
            this.cmdRouteEdit.Size = new System.Drawing.Size(32, 30);
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
            this.cmdRouteRemove.Image = global::AirVPN.Lib.Forms.Properties.Resources.delete;
            this.cmdRouteRemove.Location = new System.Drawing.Point(633, 66);
            this.cmdRouteRemove.Name = "cmdRouteRemove";
            this.cmdRouteRemove.Size = new System.Drawing.Size(32, 30);
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
            this.cmdRouteAdd.Image = global::AirVPN.Lib.Forms.Properties.Resources.add;
            this.cmdRouteAdd.Location = new System.Drawing.Point(633, 30);
            this.cmdRouteAdd.Name = "cmdRouteAdd";
            this.cmdRouteAdd.Size = new System.Drawing.Size(32, 30);
            this.cmdRouteAdd.TabIndex = 39;
            this.cmdRouteAdd.UseVisualStyleBackColor = true;
            this.cmdRouteAdd.Click += new System.EventHandler(this.cmdRouteAdd_Click);
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(4, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(656, 18);
            this.label6.TabIndex = 3;
            this.label6.Text = "Network routing rules about what destination must be in the VPN tunnel or not.";
            // 
            // cboRoutesOtherwise
            // 
            this.cboRoutesOtherwise.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cboRoutesOtherwise.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRoutesOtherwise.FormattingEnabled = true;
            this.cboRoutesOtherwise.Location = new System.Drawing.Point(463, 259);
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
            this.lblRoutesOtherwise.Location = new System.Drawing.Point(284, 261);
            this.lblRoutesOtherwise.Name = "lblRoutesOtherwise";
            this.lblRoutesOtherwise.Size = new System.Drawing.Size(173, 19);
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
            this.lstRoutes.Location = new System.Drawing.Point(6, 30);
            this.lstRoutes.MultiSelect = false;
            this.lstRoutes.Name = "lstRoutes";
            this.lstRoutes.OwnerDraw = true;
            this.lstRoutes.Size = new System.Drawing.Size(621, 223);
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
            this.tabDNS.Controls.Add(this.lblDnsServers);
            this.tabDNS.Controls.Add(this.cmdDnsEdit);
            this.tabDNS.Controls.Add(this.cmdDnsRemove);
            this.tabDNS.Controls.Add(this.cmdDnsAdd);
            this.tabDNS.Controls.Add(this.lstDnsServers);
            this.tabDNS.Controls.Add(this.label7);
            this.tabDNS.Controls.Add(this.cboDnsSwitchMode);
            this.tabDNS.Controls.Add(this.chkDnsCheck);
            this.tabDNS.Location = new System.Drawing.Point(4, 24);
            this.tabDNS.Name = "tabDNS";
            this.tabDNS.Padding = new System.Windows.Forms.Padding(3);
            this.tabDNS.Size = new System.Drawing.Size(673, 286);
            this.tabDNS.TabIndex = 5;
            this.tabDNS.Text = "DNS";
            this.tabDNS.UseVisualStyleBackColor = true;
            // 
            // lblDnsServers
            // 
            this.lblDnsServers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDnsServers.BackColor = System.Drawing.Color.Transparent;
            this.lblDnsServers.ForeColor = System.Drawing.Color.Black;
            this.lblDnsServers.Location = new System.Drawing.Point(330, 18);
            this.lblDnsServers.Name = "lblDnsServers";
            this.lblDnsServers.Size = new System.Drawing.Size(328, 34);
            this.lblDnsServers.TabIndex = 80;
            this.lblDnsServers.Text = "DNS Servers:";
            // 
            // cmdDnsEdit
            // 
            this.cmdDnsEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDnsEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdDnsEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdDnsEdit.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdDnsEdit.FlatAppearance.BorderSize = 0;
            this.cmdDnsEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdDnsEdit.Image = global::AirVPN.Lib.Forms.Properties.Resources.edit;
            this.cmdDnsEdit.Location = new System.Drawing.Point(633, 129);
            this.cmdDnsEdit.Name = "cmdDnsEdit";
            this.cmdDnsEdit.Size = new System.Drawing.Size(32, 30);
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
            this.cmdDnsRemove.Image = global::AirVPN.Lib.Forms.Properties.Resources.delete;
            this.cmdDnsRemove.Location = new System.Drawing.Point(633, 93);
            this.cmdDnsRemove.Name = "cmdDnsRemove";
            this.cmdDnsRemove.Size = new System.Drawing.Size(32, 30);
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
            this.cmdDnsAdd.Image = global::AirVPN.Lib.Forms.Properties.Resources.add;
            this.cmdDnsAdd.Location = new System.Drawing.Point(633, 57);
            this.cmdDnsAdd.Name = "cmdDnsAdd";
            this.cmdDnsAdd.Size = new System.Drawing.Size(32, 30);
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
            this.lstDnsServers.Location = new System.Drawing.Point(334, 57);
            this.lstDnsServers.MultiSelect = false;
            this.lstDnsServers.Name = "lstDnsServers";
            this.lstDnsServers.OwnerDraw = true;
            this.lstDnsServers.Size = new System.Drawing.Size(293, 223);
            this.lstDnsServers.SmallImageList = this.imgRoutes;
            this.lstDnsServers.TabIndex = 76;
            this.lstDnsServers.UseCompatibleStateImageBehavior = false;
            this.lstDnsServers.View = System.Windows.Forms.View.Details;
            this.lstDnsServers.SelectedIndexChanged += new System.EventHandler(this.lstDnsServers_SelectedIndexChanged);
            this.lstDnsServers.DoubleClick += new System.EventHandler(this.lstDnsServers_DoubleClick);
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(17, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(151, 21);
            this.label7.TabIndex = 75;
            this.label7.Text = "DNS Switch mode:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.chkDnsCheck.Location = new System.Drawing.Point(20, 76);
            this.chkDnsCheck.Name = "chkDnsCheck";
            this.chkDnsCheck.Size = new System.Drawing.Size(287, 39);
            this.chkDnsCheck.TabIndex = 73;
            this.chkDnsCheck.Text = "Check if the tunnel use AirVPN DNS";
            this.chkDnsCheck.UseVisualStyleBackColor = false;
            // 
            // tabNetworkLock
            // 
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
            this.tabNetworkLock.Size = new System.Drawing.Size(673, 286);
            this.tabNetworkLock.TabIndex = 4;
            this.tabNetworkLock.Text = "Network lock";
            this.tabNetworkLock.UseVisualStyleBackColor = true;
            // 
            // lnkLockHelp
            // 
            this.lnkLockHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkLockHelp.Location = new System.Drawing.Point(412, 10);
            this.lnkLockHelp.Name = "lnkLockHelp";
            this.lnkLockHelp.Size = new System.Drawing.Size(253, 45);
            this.lnkLockHelp.TabIndex = 82;
            this.lnkLockHelp.TabStop = true;
            this.lnkLockHelp.Text = "More about Network Lock";
            this.lnkLockHelp.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lnkLockHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkLockHelp_LinkClicked);
            // 
            // chkLockAllowPing
            // 
            this.chkLockAllowPing.BackColor = System.Drawing.Color.Transparent;
            this.chkLockAllowPing.ForeColor = System.Drawing.Color.Black;
            this.chkLockAllowPing.Location = new System.Drawing.Point(17, 87);
            this.chkLockAllowPing.Name = "chkLockAllowPing";
            this.chkLockAllowPing.Size = new System.Drawing.Size(163, 25);
            this.chkLockAllowPing.TabIndex = 81;
            this.chkLockAllowPing.Text = "Allow ping";
            this.chkLockAllowPing.UseVisualStyleBackColor = false;
            // 
            // chkLockAllowPrivate
            // 
            this.chkLockAllowPrivate.BackColor = System.Drawing.Color.Transparent;
            this.chkLockAllowPrivate.ForeColor = System.Drawing.Color.Black;
            this.chkLockAllowPrivate.Location = new System.Drawing.Point(17, 59);
            this.chkLockAllowPrivate.Name = "chkLockAllowPrivate";
            this.chkLockAllowPrivate.Size = new System.Drawing.Size(163, 25);
            this.chkLockAllowPrivate.TabIndex = 80;
            this.chkLockAllowPrivate.Text = "Allow lan/private";
            this.chkLockAllowPrivate.UseVisualStyleBackColor = false;
            // 
            // lblLockRoutingOutWarning
            // 
            this.lblLockRoutingOutWarning.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.lblLockRoutingOutWarning.ForeColor = System.Drawing.Color.White;
            this.lblLockRoutingOutWarning.Location = new System.Drawing.Point(14, 124);
            this.lblLockRoutingOutWarning.Name = "lblLockRoutingOutWarning";
            this.lblLockRoutingOutWarning.Size = new System.Drawing.Size(169, 120);
            this.lblLockRoutingOutWarning.TabIndex = 78;
            this.lblLockRoutingOutWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLockAllowedIPS
            // 
            this.lblLockAllowedIPS.BackColor = System.Drawing.Color.Transparent;
            this.lblLockAllowedIPS.ForeColor = System.Drawing.Color.Black;
            this.lblLockAllowedIPS.Location = new System.Drawing.Point(189, 42);
            this.lblLockAllowedIPS.Name = "lblLockAllowedIPS";
            this.lblLockAllowedIPS.Size = new System.Drawing.Size(476, 20);
            this.lblLockAllowedIPS.TabIndex = 76;
            this.lblLockAllowedIPS.Text = "Addresses allowed:";
            this.lblLockAllowedIPS.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // txtLockAllowedIPS
            // 
            this.txtLockAllowedIPS.AcceptsReturn = true;
            this.txtLockAllowedIPS.AcceptsTab = true;
            this.txtLockAllowedIPS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLockAllowedIPS.Location = new System.Drawing.Point(189, 65);
            this.txtLockAllowedIPS.Multiline = true;
            this.txtLockAllowedIPS.Name = "txtLockAllowedIPS";
            this.txtLockAllowedIPS.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLockAllowedIPS.Size = new System.Drawing.Size(476, 205);
            this.txtLockAllowedIPS.TabIndex = 75;
            // 
            // lblLockMode
            // 
            this.lblLockMode.BackColor = System.Drawing.Color.Transparent;
            this.lblLockMode.ForeColor = System.Drawing.Color.Black;
            this.lblLockMode.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLockMode.Location = new System.Drawing.Point(14, 10);
            this.lblLockMode.Name = "lblLockMode";
            this.lblLockMode.Size = new System.Drawing.Size(88, 21);
            this.lblLockMode.TabIndex = 74;
            this.lblLockMode.Text = "Mode:";
            // 
            // cboLockMode
            // 
            this.cboLockMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLockMode.FormattingEnabled = true;
            this.cboLockMode.Location = new System.Drawing.Point(108, 10);
            this.cboLockMode.Name = "cboLockMode";
            this.cboLockMode.Size = new System.Drawing.Size(259, 21);
            this.cboLockMode.TabIndex = 73;
            // 
            // tabLogging
            // 
            this.tabLogging.Controls.Add(this.chkLogLevelDebug);
            this.tabLogging.Controls.Add(this.TxtLoggingPathComputed);
            this.tabLogging.Controls.Add(this.lblLoggingHelp);
            this.tabLogging.Controls.Add(this.TxtLoggingPath);
            this.tabLogging.Controls.Add(this.label8);
            this.tabLogging.Controls.Add(this.chkLoggingEnabled);
            this.tabLogging.Location = new System.Drawing.Point(4, 24);
            this.tabLogging.Name = "tabLogging";
            this.tabLogging.Size = new System.Drawing.Size(673, 286);
            this.tabLogging.TabIndex = 3;
            this.tabLogging.Text = "Logging";
            this.tabLogging.UseVisualStyleBackColor = true;
            // 
            // chkLogLevelDebug
            // 
            this.chkLogLevelDebug.AutoSize = true;
            this.chkLogLevelDebug.BackColor = System.Drawing.Color.Transparent;
            this.chkLogLevelDebug.ForeColor = System.Drawing.Color.Black;
            this.chkLogLevelDebug.Location = new System.Drawing.Point(465, 20);
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
            this.TxtLoggingPathComputed.Location = new System.Drawing.Point(74, 82);
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
            this.lblLoggingHelp.Location = new System.Drawing.Point(14, 179);
            this.lblLoggingHelp.Name = "lblLoggingHelp";
            this.lblLoggingHelp.Size = new System.Drawing.Size(637, 96);
            this.lblLoggingHelp.TabIndex = 61;
            this.lblLoggingHelp.Text = resources.GetString("lblLoggingHelp.Text");
            this.lblLoggingHelp.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // TxtLoggingPath
            // 
            this.TxtLoggingPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtLoggingPath.Location = new System.Drawing.Point(74, 59);
            this.TxtLoggingPath.Name = "TxtLoggingPath";
            this.TxtLoggingPath.Size = new System.Drawing.Size(591, 20);
            this.TxtLoggingPath.TabIndex = 61;
            this.TxtLoggingPath.TextChanged += new System.EventHandler(this.TxtLoggingPath_TextChanged);
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.ForeColor = System.Drawing.Color.Black;
            this.label8.Location = new System.Drawing.Point(14, 62);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 18);
            this.label8.TabIndex = 60;
            this.label8.Text = "Path:";
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
            this.ClientSize = new System.Drawing.Size(861, 360);
            this.Controls.Add(this.pnlCommands);
            this.Controls.Add(this.tabSettings);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(692, 393);
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
            this.tabGeneral.PerformLayout();
            this.pnlGeneralWindowsOnly.ResumeLayout(false);
            this.pnlGeneralWindowsOnly.PerformLayout();
            this.tabProtocols.ResumeLayout(false);
            this.tabProtocols.PerformLayout();
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
		private Skin.ToolTip myToolTip;
        private Skin.CheckBox chkSystemStart;
        private Skin.CheckBox chkConnect;
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
        private Skin.CheckBox chkGeneralStartLast;
        private Skin.TabControl tabSettings;
        private Skin.TabPage tabGeneral;
        private Skin.TabPage tabProtocols;
        private Skin.TabPage tabProxy;
        private Skin.TabPage tabRoutes;
        private Skin.Button cmdExeBrowse;
        private Skin.TextBox txtExePath;
        private Skin.Label label4;
		private Skin.CheckBox chkAdvancedCheckRoute;
        private Skin.CheckBox chkAdvancedWindowsTapUp;
        private Skin.CheckBox chkExpert;
        private Skin.CheckBox chkAdvancedOpenVpnDirectivesDefaultSkip;
        private Skin.TextBox txtAdvancedOpenVpnDirectivesDefault;
        private Skin.TextBox txtAdvancedOpenVpnDirectivesCustom;
        private Skin.Button cmdAdvancedEventsEdit;
        private Skin.Button cmdAdvancedEventsClear;
        private Skin.ListView lstAdvancedEvents;
        private Skin.Label label1;
        private Skin.Label label3;
        private Skin.Label label2;
        private Skin.ColumnHeader columnHeader3;
        private Skin.ColumnHeader columnHeader4;
        private Skin.Label lblGeneralTheme;
		private Skin.ComboBox cboGeneralTheme;
		private Skin.CheckBox chkAdvancedWindowsDhcpSwitch;
		private Skin.Button cmdAdvancedUninstallDriver;
		private Skin.CheckBox chkAdvancedPingerEnabled;
		private System.Windows.Forms.GroupBox pnlGeneralWindowsOnly;
		private System.Windows.Forms.GroupBox pnlAdvancedGeneralWindowsOnly;
		private Skin.TabPage tabLogging;
		private Skin.Label TxtLoggingPathComputed;
		private Skin.Label lblLoggingHelp;
		private Skin.TextBox TxtLoggingPath;
		private Skin.Label label8;
		private Skin.CheckBox chkLoggingEnabled;
		private Skin.CheckBox chkExitConfirm;
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
		private Skin.Label label7;
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
		private Skin.CheckBox chkNetLock;
		private Skin.Label label9;
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
        private System.Windows.Forms.ColumnHeader colProtocolsCipher;
        private Skin.LinkLabel lnkProtocolsHelp2;
        private Skin.LinkLabel lnkProtocolsHelp1;
        private System.Windows.Forms.CheckBox chkProtocolsAutomatic;
        private Skin.CheckBox chkWindowsWfp;
        private Skin.CheckBox chkUiFontGeneral;
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
    }
}