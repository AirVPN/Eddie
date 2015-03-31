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
			AirVPN.Gui.Skin.TabPage tabPage1;
			AirVPN.Gui.Skin.TabPage tabPage2;
			AirVPN.Gui.Skin.TabPage tabPage3;
			AirVPN.Gui.Skin.ColumnHeader columnHeader1;
			AirVPN.Gui.Skin.ColumnHeader columnHeader2;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
			AirVPN.Gui.Skin.ColumnHeader columnHeader5;
			this.label13 = new AirVPN.Gui.Skin.Label();
			this.label10 = new AirVPN.Gui.Skin.Label();
			this.label9 = new AirVPN.Gui.Skin.Label();
			this.lblIpV6 = new AirVPN.Gui.Skin.Label();
			this.cboIpV6 = new AirVPN.Gui.Skin.ComboBox();
			this.lblAdvancedManifestRefresh = new AirVPN.Gui.Skin.Label();
			this.cboAdvancedManifestRefresh = new AirVPN.Gui.Skin.ComboBox();
			this.cmdAdvancedGeneralDocs = new AirVPN.Gui.Skin.Button();
			this.pnlAdvancedGeneralWindowsOnly = new System.Windows.Forms.GroupBox();
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
			this.chkNetLock = new AirVPN.Gui.Skin.CheckBox();
			this.chkExitConfirm = new AirVPN.Gui.Skin.CheckBox();
			this.pnlGeneralWindowsOnly = new System.Windows.Forms.GroupBox();
			this.chkSystemStart = new AirVPN.Gui.Skin.CheckBox();
			this.chkMinimizeTray = new AirVPN.Gui.Skin.CheckBox();
			this.lblGeneralTheme = new AirVPN.Gui.Skin.Label();
			this.cboGeneralTheme = new AirVPN.Gui.Skin.ComboBox();
			this.chkGeneralStartLast = new AirVPN.Gui.Skin.CheckBox();
			this.cmdTos = new AirVPN.Gui.Skin.Button();
			this.chkConnect = new AirVPN.Gui.Skin.CheckBox();
			this.tabMode = new AirVPN.Gui.Skin.TabPage();
			this.txtModeTorControlPassword = new AirVPN.Gui.Skin.TextBox();
			this.label16 = new AirVPN.Gui.Skin.Label();
			this.cmdModeTorTest = new AirVPN.Gui.Skin.Button();
			this.txtModeTorHost = new AirVPN.Gui.Skin.TextBox();
			this.lblModeTorHost = new AirVPN.Gui.Skin.Label();
			this.txtModeTorControlPort = new AirVPN.Gui.Skin.TextBox();
			this.lblModeTorControlPort = new AirVPN.Gui.Skin.Label();
			this.txtModeTorPort = new AirVPN.Gui.Skin.TextBox();
			this.lblModeTorPort = new AirVPN.Gui.Skin.Label();
			this.optModeAutomatic = new AirVPN.Gui.Skin.RadioButton();
			this.optModeTOR = new AirVPN.Gui.Skin.RadioButton();
			this.cmdModeDocs = new AirVPN.Gui.Skin.Button();
			this.optModeTCP443 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeUDP443 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeTCP2018 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeUDP2018 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeTCP53 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeUDP53 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeTCP80 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeUDP80 = new AirVPN.Gui.Skin.RadioButton();
			this.label11 = new AirVPN.Gui.Skin.Label();
			this.optModeSSH53 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeSSH80 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeSSH22Alt = new AirVPN.Gui.Skin.RadioButton();
			this.optModeSSH22 = new AirVPN.Gui.Skin.RadioButton();
			this.optModeSSL443 = new AirVPN.Gui.Skin.RadioButton();
			this.lblModeSSH = new AirVPN.Gui.Skin.Label();
			this.lblModeSSL = new AirVPN.Gui.Skin.Label();
			this.optModeTCP2018Alt = new AirVPN.Gui.Skin.RadioButton();
			this.optModeUDP2018Alt = new AirVPN.Gui.Skin.RadioButton();
			this.optModeUDP53Alt = new AirVPN.Gui.Skin.RadioButton();
			this.optModeUDP80Alt = new AirVPN.Gui.Skin.RadioButton();
			this.optModeUDP443Alt = new AirVPN.Gui.Skin.RadioButton();
			this.label5 = new AirVPN.Gui.Skin.Label();
			this.lblModeGroup5 = new AirVPN.Gui.Skin.Label();
			this.lblModeGroup1 = new AirVPN.Gui.Skin.Label();
			this.lblModeGroup2 = new AirVPN.Gui.Skin.Label();
			this.lblModeGroup3 = new AirVPN.Gui.Skin.Label();
			this.lblModeGroup4 = new AirVPN.Gui.Skin.Label();
			this.tabProxy = new AirVPN.Gui.Skin.TabPage();
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
			this.tabAdvanced = new AirVPN.Gui.Skin.TabPage();
			this.tabAdvancedMain = new AirVPN.Gui.Skin.TabControl();
			this.tabPage6 = new AirVPN.Gui.Skin.TabPage();
			this.lblDnsServers = new AirVPN.Gui.Skin.Label();
			this.cmdDnsEdit = new AirVPN.Gui.Skin.Button();
			this.cmdDnsRemove = new AirVPN.Gui.Skin.Button();
			this.cmdDnsAdd = new AirVPN.Gui.Skin.Button();
			this.lstDnsServers = new AirVPN.Gui.Skin.ListView();
			this.label7 = new AirVPN.Gui.Skin.Label();
			this.cboDnsSwitchMode = new AirVPN.Gui.Skin.ComboBox();
			this.chkDnsCheck = new AirVPN.Gui.Skin.CheckBox();
			this.tabPage5 = new AirVPN.Gui.Skin.TabPage();
			this.chkLockAllowPing = new AirVPN.Gui.Skin.CheckBox();
			this.chkLockAllowPrivate = new AirVPN.Gui.Skin.CheckBox();
			this.cmdLockHelp = new AirVPN.Gui.Skin.Button();
			this.lblLockRoutingOutWarning = new AirVPN.Gui.Skin.Label();
			this.lblLockAllowedIPS = new AirVPN.Gui.Skin.Label();
			this.txtLockAllowedIPS = new AirVPN.Gui.Skin.TextBox();
			this.lblLockMode = new AirVPN.Gui.Skin.Label();
			this.cboLockMode = new AirVPN.Gui.Skin.ComboBox();
			this.tabPage4 = new AirVPN.Gui.Skin.TabPage();
			this.TxtLoggingPathComputed = new AirVPN.Gui.Skin.Label();
			this.lblLoggingHelp = new AirVPN.Gui.Skin.Label();
			this.TxtLoggingPath = new AirVPN.Gui.Skin.TextBox();
			this.label8 = new AirVPN.Gui.Skin.Label();
			this.chkLoggingEnabled = new AirVPN.Gui.Skin.CheckBox();
			tabPage1 = new AirVPN.Gui.Skin.TabPage();
			tabPage2 = new AirVPN.Gui.Skin.TabPage();
			tabPage3 = new AirVPN.Gui.Skin.TabPage();
			columnHeader1 = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
			columnHeader2 = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
			columnHeader5 = ((AirVPN.Gui.Skin.ColumnHeader)(new AirVPN.Gui.Skin.ColumnHeader()));
			tabPage1.SuspendLayout();
			this.pnlAdvancedGeneralWindowsOnly.SuspendLayout();
			tabPage2.SuspendLayout();
			tabPage3.SuspendLayout();
			this.mnuRoutes.SuspendLayout();
			this.pnlCommands.SuspendLayout();
			this.tabSettings.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.pnlGeneralWindowsOnly.SuspendLayout();
			this.tabMode.SuspendLayout();
			this.tabProxy.SuspendLayout();
			this.tabRoutes.SuspendLayout();
			this.tabAdvanced.SuspendLayout();
			this.tabAdvancedMain.SuspendLayout();
			this.tabPage6.SuspendLayout();
			this.tabPage5.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabPage1
			// 
			tabPage1.BackColor = System.Drawing.Color.White;
			tabPage1.Controls.Add(this.label13);
			tabPage1.Controls.Add(this.label10);
			tabPage1.Controls.Add(this.label9);
			tabPage1.Controls.Add(this.lblIpV6);
			tabPage1.Controls.Add(this.cboIpV6);
			tabPage1.Controls.Add(this.lblAdvancedManifestRefresh);
			tabPage1.Controls.Add(this.cboAdvancedManifestRefresh);
			tabPage1.Controls.Add(this.cmdAdvancedGeneralDocs);
			tabPage1.Controls.Add(this.pnlAdvancedGeneralWindowsOnly);
			tabPage1.Controls.Add(this.chkAdvancedPingerEnabled);
			tabPage1.Controls.Add(this.cmdExeBrowse);
			tabPage1.Controls.Add(this.txtExePath);
			tabPage1.Controls.Add(this.label4);
			tabPage1.Controls.Add(this.chkAdvancedCheckRoute);
			tabPage1.Controls.Add(this.chkExpert);
			tabPage1.Location = new System.Drawing.Point(4, 24);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(3);
			tabPage1.Size = new System.Drawing.Size(622, 263);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "General";
			// 
			// label13
			// 
			this.label13.BackColor = System.Drawing.Color.Transparent;
			this.label13.ForeColor = System.Drawing.Color.Black;
			this.label13.Location = new System.Drawing.Point(10, 18);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(209, 22);
			this.label13.TabIndex = 79;
			this.label13.Text = "Expert Mode:";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label10
			// 
			this.label10.BackColor = System.Drawing.Color.Transparent;
			this.label10.ForeColor = System.Drawing.Color.Black;
			this.label10.Location = new System.Drawing.Point(13, 141);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(206, 22);
			this.label10.TabIndex = 78;
			this.label10.Text = "Enable Pinger / Latency Tests:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.BackColor = System.Drawing.Color.Transparent;
			this.label9.ForeColor = System.Drawing.Color.Black;
			this.label9.Location = new System.Drawing.Point(13, 60);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(209, 22);
			this.label9.TabIndex = 77;
			this.label9.Text = "Check if the tunnel effectively works:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblIpV6
			// 
			this.lblIpV6.BackColor = System.Drawing.Color.Transparent;
			this.lblIpV6.ForeColor = System.Drawing.Color.Black;
			this.lblIpV6.Location = new System.Drawing.Point(16, 87);
			this.lblIpV6.Name = "lblIpV6";
			this.lblIpV6.Size = new System.Drawing.Size(206, 21);
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
			this.cboIpV6.Location = new System.Drawing.Point(231, 87);
			this.cboIpV6.Name = "cboIpV6";
			this.cboIpV6.Size = new System.Drawing.Size(125, 21);
			this.cboIpV6.TabIndex = 75;
			// 
			// lblAdvancedManifestRefresh
			// 
			this.lblAdvancedManifestRefresh.BackColor = System.Drawing.Color.Transparent;
			this.lblAdvancedManifestRefresh.ForeColor = System.Drawing.Color.Black;
			this.lblAdvancedManifestRefresh.Location = new System.Drawing.Point(13, 114);
			this.lblAdvancedManifestRefresh.Name = "lblAdvancedManifestRefresh";
			this.lblAdvancedManifestRefresh.Size = new System.Drawing.Size(206, 21);
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
			this.cboAdvancedManifestRefresh.Location = new System.Drawing.Point(231, 114);
			this.cboAdvancedManifestRefresh.Name = "cboAdvancedManifestRefresh";
			this.cboAdvancedManifestRefresh.Size = new System.Drawing.Size(125, 21);
			this.cboAdvancedManifestRefresh.TabIndex = 73;
			// 
			// cmdAdvancedGeneralDocs
			// 
			this.cmdAdvancedGeneralDocs.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdAdvancedGeneralDocs.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdAdvancedGeneralDocs.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdAdvancedGeneralDocs.FlatAppearance.BorderSize = 0;
			this.cmdAdvancedGeneralDocs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAdvancedGeneralDocs.Image = global::AirVPN.Lib.Forms.Properties.Resources.help;
			this.cmdAdvancedGeneralDocs.Location = new System.Drawing.Point(502, 217);
			this.cmdAdvancedGeneralDocs.Name = "cmdAdvancedGeneralDocs";
			this.cmdAdvancedGeneralDocs.Size = new System.Drawing.Size(100, 30);
			this.cmdAdvancedGeneralDocs.TabIndex = 73;
			this.cmdAdvancedGeneralDocs.UseVisualStyleBackColor = true;
			this.cmdAdvancedGeneralDocs.Click += new System.EventHandler(this.cmdAdvancedGeneralDocs_Click);
			// 
			// pnlAdvancedGeneralWindowsOnly
			// 
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkAdvancedWindowsTapUp);
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.chkAdvancedWindowsDhcpSwitch);
			this.pnlAdvancedGeneralWindowsOnly.Controls.Add(this.cmdAdvancedUninstallDriver);
			this.pnlAdvancedGeneralWindowsOnly.Location = new System.Drawing.Point(388, 18);
			this.pnlAdvancedGeneralWindowsOnly.Name = "pnlAdvancedGeneralWindowsOnly";
			this.pnlAdvancedGeneralWindowsOnly.Size = new System.Drawing.Size(221, 141);
			this.pnlAdvancedGeneralWindowsOnly.TabIndex = 69;
			this.pnlAdvancedGeneralWindowsOnly.TabStop = false;
			this.pnlAdvancedGeneralWindowsOnly.Text = "Microsoft Windows Only";
			// 
			// chkAdvancedWindowsTapUp
			// 
			this.chkAdvancedWindowsTapUp.BackColor = System.Drawing.Color.Transparent;
			this.chkAdvancedWindowsTapUp.ForeColor = System.Drawing.Color.Black;
			this.chkAdvancedWindowsTapUp.Location = new System.Drawing.Point(15, 23);
			this.chkAdvancedWindowsTapUp.Name = "chkAdvancedWindowsTapUp";
			this.chkAdvancedWindowsTapUp.Size = new System.Drawing.Size(194, 26);
			this.chkAdvancedWindowsTapUp.TabIndex = 55;
			this.chkAdvancedWindowsTapUp.Text = "Force TAP interface UP";
			this.chkAdvancedWindowsTapUp.UseVisualStyleBackColor = false;
			// 
			// chkAdvancedWindowsDhcpSwitch
			// 
			this.chkAdvancedWindowsDhcpSwitch.BackColor = System.Drawing.Color.Transparent;
			this.chkAdvancedWindowsDhcpSwitch.ForeColor = System.Drawing.Color.Black;
			this.chkAdvancedWindowsDhcpSwitch.Location = new System.Drawing.Point(15, 46);
			this.chkAdvancedWindowsDhcpSwitch.Name = "chkAdvancedWindowsDhcpSwitch";
			this.chkAdvancedWindowsDhcpSwitch.Size = new System.Drawing.Size(194, 26);
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
			this.cmdAdvancedUninstallDriver.Location = new System.Drawing.Point(9, 99);
			this.cmdAdvancedUninstallDriver.Name = "cmdAdvancedUninstallDriver";
			this.cmdAdvancedUninstallDriver.Size = new System.Drawing.Size(203, 30);
			this.cmdAdvancedUninstallDriver.TabIndex = 65;
			this.cmdAdvancedUninstallDriver.Text = "Uninstall Driver";
			this.cmdAdvancedUninstallDriver.UseVisualStyleBackColor = true;
			this.cmdAdvancedUninstallDriver.Click += new System.EventHandler(this.cmdAdvancedUninstallDriver_Click);
			// 
			// chkAdvancedPingerEnabled
			// 
			this.chkAdvancedPingerEnabled.BackColor = System.Drawing.Color.Transparent;
			this.chkAdvancedPingerEnabled.ForeColor = System.Drawing.Color.Black;
			this.chkAdvancedPingerEnabled.Location = new System.Drawing.Point(231, 141);
			this.chkAdvancedPingerEnabled.Name = "chkAdvancedPingerEnabled";
			this.chkAdvancedPingerEnabled.Size = new System.Drawing.Size(125, 25);
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
			this.cmdExeBrowse.Location = new System.Drawing.Point(308, 224);
			this.cmdExeBrowse.Name = "cmdExeBrowse";
			this.cmdExeBrowse.Size = new System.Drawing.Size(31, 25);
			this.cmdExeBrowse.TabIndex = 60;
			this.cmdExeBrowse.UseVisualStyleBackColor = true;
			this.cmdExeBrowse.Click += new System.EventHandler(this.cmdExeBrowse_Click);
			// 
			// txtExePath
			// 
			this.txtExePath.Location = new System.Drawing.Point(16, 227);
			this.txtExePath.Name = "txtExePath";
			this.txtExePath.Size = new System.Drawing.Size(286, 20);
			this.txtExePath.TabIndex = 59;
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.ForeColor = System.Drawing.Color.Black;
			this.label4.Location = new System.Drawing.Point(15, 211);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(126, 13);
			this.label4.TabIndex = 58;
			this.label4.Text = "OpenVPN Custom Path:";
			// 
			// chkAdvancedCheckRoute
			// 
			this.chkAdvancedCheckRoute.BackColor = System.Drawing.Color.Transparent;
			this.chkAdvancedCheckRoute.ForeColor = System.Drawing.Color.Black;
			this.chkAdvancedCheckRoute.Location = new System.Drawing.Point(231, 60);
			this.chkAdvancedCheckRoute.Name = "chkAdvancedCheckRoute";
			this.chkAdvancedCheckRoute.Size = new System.Drawing.Size(125, 25);
			this.chkAdvancedCheckRoute.TabIndex = 57;
			this.chkAdvancedCheckRoute.UseVisualStyleBackColor = false;
			// 
			// chkExpert
			// 
			this.chkExpert.BackColor = System.Drawing.Color.Transparent;
			this.chkExpert.ForeColor = System.Drawing.Color.Black;
			this.chkExpert.Location = new System.Drawing.Point(231, 18);
			this.chkExpert.Name = "chkExpert";
			this.chkExpert.Size = new System.Drawing.Size(125, 22);
			this.chkExpert.TabIndex = 54;
			this.chkExpert.UseVisualStyleBackColor = false;
			// 
			// tabPage2
			// 
			tabPage2.BackColor = System.Drawing.Color.White;
			tabPage2.Controls.Add(this.label3);
			tabPage2.Controls.Add(this.label2);
			tabPage2.Controls.Add(this.chkAdvancedOpenVpnDirectivesDefaultSkip);
			tabPage2.Controls.Add(this.txtAdvancedOpenVpnDirectivesDefault);
			tabPage2.Controls.Add(this.txtAdvancedOpenVpnDirectivesCustom);
			tabPage2.Location = new System.Drawing.Point(4, 24);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(3);
			tabPage2.Size = new System.Drawing.Size(622, 263);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "OVPN directives";
			// 
			// label3
			// 
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(310, 7);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(144, 13);
			this.label3.TabIndex = 61;
			this.label3.Text = "Default:";
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.ForeColor = System.Drawing.Color.Black;
			this.label2.Location = new System.Drawing.Point(6, 7);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(145, 13);
			this.label2.TabIndex = 60;
			this.label2.Text = "Custom:";
			// 
			// chkAdvancedOpenVpnDirectivesDefaultSkip
			// 
			this.chkAdvancedOpenVpnDirectivesDefaultSkip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkAdvancedOpenVpnDirectivesDefaultSkip.BackColor = System.Drawing.Color.Transparent;
			this.chkAdvancedOpenVpnDirectivesDefaultSkip.ForeColor = System.Drawing.Color.Black;
			this.chkAdvancedOpenVpnDirectivesDefaultSkip.Location = new System.Drawing.Point(313, 225);
			this.chkAdvancedOpenVpnDirectivesDefaultSkip.Name = "chkAdvancedOpenVpnDirectivesDefaultSkip";
			this.chkAdvancedOpenVpnDirectivesDefaultSkip.Size = new System.Drawing.Size(306, 32);
			this.chkAdvancedOpenVpnDirectivesDefaultSkip.TabIndex = 59;
			this.chkAdvancedOpenVpnDirectivesDefaultSkip.Text = "Skip the above default directives. If unchecked, your custom directives are appen" +
    "ded.";
			this.chkAdvancedOpenVpnDirectivesDefaultSkip.UseVisualStyleBackColor = false;
			// 
			// txtAdvancedOpenVpnDirectivesDefault
			// 
			this.txtAdvancedOpenVpnDirectivesDefault.AcceptsReturn = true;
			this.txtAdvancedOpenVpnDirectivesDefault.AcceptsTab = true;
			this.txtAdvancedOpenVpnDirectivesDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtAdvancedOpenVpnDirectivesDefault.Location = new System.Drawing.Point(313, 23);
			this.txtAdvancedOpenVpnDirectivesDefault.Multiline = true;
			this.txtAdvancedOpenVpnDirectivesDefault.Name = "txtAdvancedOpenVpnDirectivesDefault";
			this.txtAdvancedOpenVpnDirectivesDefault.ReadOnly = true;
			this.txtAdvancedOpenVpnDirectivesDefault.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtAdvancedOpenVpnDirectivesDefault.Size = new System.Drawing.Size(306, 196);
			this.txtAdvancedOpenVpnDirectivesDefault.TabIndex = 58;
			// 
			// txtAdvancedOpenVpnDirectivesCustom
			// 
			this.txtAdvancedOpenVpnDirectivesCustom.AcceptsReturn = true;
			this.txtAdvancedOpenVpnDirectivesCustom.AcceptsTab = true;
			this.txtAdvancedOpenVpnDirectivesCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtAdvancedOpenVpnDirectivesCustom.Location = new System.Drawing.Point(9, 23);
			this.txtAdvancedOpenVpnDirectivesCustom.Multiline = true;
			this.txtAdvancedOpenVpnDirectivesCustom.Name = "txtAdvancedOpenVpnDirectivesCustom";
			this.txtAdvancedOpenVpnDirectivesCustom.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtAdvancedOpenVpnDirectivesCustom.Size = new System.Drawing.Size(304, 196);
			this.txtAdvancedOpenVpnDirectivesCustom.TabIndex = 57;
			// 
			// tabPage3
			// 
			tabPage3.BackColor = System.Drawing.Color.White;
			tabPage3.Controls.Add(this.cmdAdvancedEventsEdit);
			tabPage3.Controls.Add(this.cmdAdvancedEventsClear);
			tabPage3.Controls.Add(this.lstAdvancedEvents);
			tabPage3.Controls.Add(this.label1);
			tabPage3.Location = new System.Drawing.Point(4, 24);
			tabPage3.Name = "tabPage3";
			tabPage3.Size = new System.Drawing.Size(622, 263);
			tabPage3.TabIndex = 2;
			tabPage3.Text = "Events";
			// 
			// cmdAdvancedEventsEdit
			// 
			this.cmdAdvancedEventsEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdAdvancedEventsEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdAdvancedEventsEdit.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdAdvancedEventsEdit.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdAdvancedEventsEdit.FlatAppearance.BorderSize = 0;
			this.cmdAdvancedEventsEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAdvancedEventsEdit.Image = global::AirVPN.Lib.Forms.Properties.Resources.edit_16;
			this.cmdAdvancedEventsEdit.Location = new System.Drawing.Point(589, 57);
			this.cmdAdvancedEventsEdit.Name = "cmdAdvancedEventsEdit";
			this.cmdAdvancedEventsEdit.Size = new System.Drawing.Size(30, 30);
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
			this.cmdAdvancedEventsClear.Image = global::AirVPN.Lib.Forms.Properties.Resources.delete_16;
			this.cmdAdvancedEventsClear.Location = new System.Drawing.Point(589, 23);
			this.cmdAdvancedEventsClear.Name = "cmdAdvancedEventsClear";
			this.cmdAdvancedEventsClear.Size = new System.Drawing.Size(30, 30);
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
			this.lstAdvancedEvents.Location = new System.Drawing.Point(9, 23);
			this.lstAdvancedEvents.MultiSelect = false;
			this.lstAdvancedEvents.Name = "lstAdvancedEvents";
			this.lstAdvancedEvents.OwnerDraw = true;
			this.lstAdvancedEvents.Size = new System.Drawing.Size(574, 228);
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
			this.label1.Size = new System.Drawing.Size(324, 13);
			this.label1.TabIndex = 56;
			this.label1.Text = "External shell (double click to browse):";
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "IP Address";
			columnHeader5.Width = 150;
			// 
			// mnuRoutes
			// 
			this.mnuRoutes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRoutesAdd,
            this.mnuRoutesRemove,
            this.mnuRoutesEdit});
			this.mnuRoutes.Name = "mnuServers";
			this.mnuRoutes.Size = new System.Drawing.Size(118, 70);
			// 
			// mnuRoutesAdd
			// 
			this.mnuRoutesAdd.Image = global::AirVPN.Lib.Forms.Properties.Resources.add_16;
			this.mnuRoutesAdd.Name = "mnuRoutesAdd";
			this.mnuRoutesAdd.Size = new System.Drawing.Size(117, 22);
			this.mnuRoutesAdd.Text = "Add";
			this.mnuRoutesAdd.Click += new System.EventHandler(this.mnuRoutesAdd_Click);
			// 
			// mnuRoutesRemove
			// 
			this.mnuRoutesRemove.Image = global::AirVPN.Lib.Forms.Properties.Resources.delete_16;
			this.mnuRoutesRemove.Name = "mnuRoutesRemove";
			this.mnuRoutesRemove.Size = new System.Drawing.Size(117, 22);
			this.mnuRoutesRemove.Text = "Remove";
			this.mnuRoutesRemove.Click += new System.EventHandler(this.mnuRoutesRemove_Click);
			// 
			// mnuRoutesEdit
			// 
			this.mnuRoutesEdit.Image = global::AirVPN.Lib.Forms.Properties.Resources.edit_16;
			this.mnuRoutesEdit.Name = "mnuRoutesEdit";
			this.mnuRoutesEdit.Size = new System.Drawing.Size(117, 22);
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
			this.pnlCommands.Location = new System.Drawing.Point(165, 325);
			this.pnlCommands.Name = "pnlCommands";
			this.pnlCommands.Size = new System.Drawing.Size(309, 36);
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
			this.cmdCancel.Location = new System.Drawing.Point(156, 3);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(150, 30);
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
			this.cmdOk.Size = new System.Drawing.Size(150, 30);
			this.cmdOk.TabIndex = 40;
			this.cmdOk.Text = "Save";
			this.cmdOk.UseVisualStyleBackColor = true;
			this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
			// 
			// tabSettings
			// 
			this.tabSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabSettings.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabSettings.Controls.Add(this.tabGeneral);
			this.tabSettings.Controls.Add(this.tabMode);
			this.tabSettings.Controls.Add(this.tabProxy);
			this.tabSettings.Controls.Add(this.tabRoutes);
			this.tabSettings.Controls.Add(this.tabAdvanced);
			this.tabSettings.ItemSize = new System.Drawing.Size(80, 20);
			this.tabSettings.Location = new System.Drawing.Point(0, 0);
			this.tabSettings.Name = "tabSettings";
			this.tabSettings.SelectedIndex = 0;
			this.tabSettings.Size = new System.Drawing.Size(638, 319);
			this.tabSettings.TabIndex = 41;
			// 
			// tabGeneral
			// 
			this.tabGeneral.BackColor = System.Drawing.Color.White;
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
			this.tabGeneral.Size = new System.Drawing.Size(630, 291);
			this.tabGeneral.TabIndex = 0;
			this.tabGeneral.Text = "General";
			// 
			// chkNetLock
			// 
			this.chkNetLock.AutoSize = true;
			this.chkNetLock.BackColor = System.Drawing.Color.Transparent;
			this.chkNetLock.ForeColor = System.Drawing.Color.Black;
			this.chkNetLock.Location = new System.Drawing.Point(16, 40);
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
			this.chkExitConfirm.Location = new System.Drawing.Point(16, 127);
			this.chkExitConfirm.Name = "chkExitConfirm";
			this.chkExitConfirm.Size = new System.Drawing.Size(263, 17);
			this.chkExitConfirm.TabIndex = 66;
			this.chkExitConfirm.Text = "Exit confirmation prompt";
			this.chkExitConfirm.UseVisualStyleBackColor = false;
			// 
			// pnlGeneralWindowsOnly
			// 
			this.pnlGeneralWindowsOnly.Controls.Add(this.chkSystemStart);
			this.pnlGeneralWindowsOnly.Controls.Add(this.chkMinimizeTray);
			this.pnlGeneralWindowsOnly.Location = new System.Drawing.Point(409, 15);
			this.pnlGeneralWindowsOnly.Name = "pnlGeneralWindowsOnly";
			this.pnlGeneralWindowsOnly.Size = new System.Drawing.Size(200, 100);
			this.pnlGeneralWindowsOnly.TabIndex = 65;
			this.pnlGeneralWindowsOnly.TabStop = false;
			this.pnlGeneralWindowsOnly.Text = "Microsoft Windows Only";
			// 
			// chkSystemStart
			// 
			this.chkSystemStart.AutoSize = true;
			this.chkSystemStart.BackColor = System.Drawing.Color.Transparent;
			this.chkSystemStart.ForeColor = System.Drawing.Color.Black;
			this.chkSystemStart.Location = new System.Drawing.Point(20, 23);
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
			this.chkMinimizeTray.Location = new System.Drawing.Point(20, 46);
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
			this.lblGeneralTheme.Location = new System.Drawing.Point(17, 258);
			this.lblGeneralTheme.Name = "lblGeneralTheme";
			this.lblGeneralTheme.Size = new System.Drawing.Size(45, 20);
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
			this.cboGeneralTheme.Location = new System.Drawing.Point(66, 254);
			this.cboGeneralTheme.Name = "cboGeneralTheme";
			this.cboGeneralTheme.Size = new System.Drawing.Size(63, 21);
			this.cboGeneralTheme.TabIndex = 42;
			this.cboGeneralTheme.SelectedIndexChanged += new System.EventHandler(this.cboGeneralTheme_SelectedIndexChanged);
			// 
			// chkGeneralStartLast
			// 
			this.chkGeneralStartLast.BackColor = System.Drawing.Color.Transparent;
			this.chkGeneralStartLast.ForeColor = System.Drawing.Color.Black;
			this.chkGeneralStartLast.Location = new System.Drawing.Point(16, 81);
			this.chkGeneralStartLast.Name = "chkGeneralStartLast";
			this.chkGeneralStartLast.Size = new System.Drawing.Size(263, 40);
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
			this.cmdTos.Location = new System.Drawing.Point(409, 245);
			this.cmdTos.Name = "cmdTos";
			this.cmdTos.Size = new System.Drawing.Size(200, 30);
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
			this.chkConnect.Location = new System.Drawing.Point(16, 15);
			this.chkConnect.Name = "chkConnect";
			this.chkConnect.Size = new System.Drawing.Size(146, 17);
			this.chkConnect.TabIndex = 30;
			this.chkConnect.Text = "*Connect when launched";
			this.chkConnect.UseVisualStyleBackColor = false;
			// 
			// tabMode
			// 
			this.tabMode.BackColor = System.Drawing.Color.White;
			this.tabMode.Controls.Add(this.txtModeTorControlPassword);
			this.tabMode.Controls.Add(this.label16);
			this.tabMode.Controls.Add(this.cmdModeTorTest);
			this.tabMode.Controls.Add(this.txtModeTorHost);
			this.tabMode.Controls.Add(this.lblModeTorHost);
			this.tabMode.Controls.Add(this.txtModeTorControlPort);
			this.tabMode.Controls.Add(this.lblModeTorControlPort);
			this.tabMode.Controls.Add(this.txtModeTorPort);
			this.tabMode.Controls.Add(this.lblModeTorPort);
			this.tabMode.Controls.Add(this.optModeAutomatic);
			this.tabMode.Controls.Add(this.optModeTOR);
			this.tabMode.Controls.Add(this.cmdModeDocs);
			this.tabMode.Controls.Add(this.optModeTCP443);
			this.tabMode.Controls.Add(this.optModeUDP443);
			this.tabMode.Controls.Add(this.optModeTCP2018);
			this.tabMode.Controls.Add(this.optModeUDP2018);
			this.tabMode.Controls.Add(this.optModeTCP53);
			this.tabMode.Controls.Add(this.optModeUDP53);
			this.tabMode.Controls.Add(this.optModeTCP80);
			this.tabMode.Controls.Add(this.optModeUDP80);
			this.tabMode.Controls.Add(this.label11);
			this.tabMode.Controls.Add(this.optModeSSH53);
			this.tabMode.Controls.Add(this.optModeSSH80);
			this.tabMode.Controls.Add(this.optModeSSH22Alt);
			this.tabMode.Controls.Add(this.optModeSSH22);
			this.tabMode.Controls.Add(this.optModeSSL443);
			this.tabMode.Controls.Add(this.lblModeSSH);
			this.tabMode.Controls.Add(this.lblModeSSL);
			this.tabMode.Controls.Add(this.optModeTCP2018Alt);
			this.tabMode.Controls.Add(this.optModeUDP2018Alt);
			this.tabMode.Controls.Add(this.optModeUDP53Alt);
			this.tabMode.Controls.Add(this.optModeUDP80Alt);
			this.tabMode.Controls.Add(this.optModeUDP443Alt);
			this.tabMode.Controls.Add(this.label5);
			this.tabMode.Controls.Add(this.lblModeGroup5);
			this.tabMode.Controls.Add(this.lblModeGroup1);
			this.tabMode.Controls.Add(this.lblModeGroup2);
			this.tabMode.Controls.Add(this.lblModeGroup3);
			this.tabMode.Controls.Add(this.lblModeGroup4);
			this.tabMode.Location = new System.Drawing.Point(4, 24);
			this.tabMode.Name = "tabMode";
			this.tabMode.Size = new System.Drawing.Size(630, 291);
			this.tabMode.TabIndex = 3;
			this.tabMode.Text = "Protocols";
			// 
			// txtModeTorControlPassword
			// 
			this.txtModeTorControlPassword.Location = new System.Drawing.Point(499, 254);
			this.txtModeTorControlPassword.Name = "txtModeTorControlPassword";
			this.txtModeTorControlPassword.Size = new System.Drawing.Size(57, 20);
			this.txtModeTorControlPassword.TabIndex = 68;
			// 
			// label16
			// 
			this.label16.BackColor = System.Drawing.Color.Transparent;
			this.label16.ForeColor = System.Drawing.Color.Black;
			this.label16.Location = new System.Drawing.Point(433, 257);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(64, 20);
			this.label16.TabIndex = 67;
			this.label16.Text = "Password:";
			this.label16.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cmdModeTorTest
			// 
			this.cmdModeTorTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdModeTorTest.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdModeTorTest.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdModeTorTest.FlatAppearance.BorderSize = 0;
			this.cmdModeTorTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdModeTorTest.Location = new System.Drawing.Point(565, 254);
			this.cmdModeTorTest.Name = "cmdModeTorTest";
			this.cmdModeTorTest.Size = new System.Drawing.Size(49, 20);
			this.cmdModeTorTest.TabIndex = 66;
			this.cmdModeTorTest.Text = "Test";
			this.cmdModeTorTest.UseVisualStyleBackColor = true;
			this.cmdModeTorTest.Click += new System.EventHandler(this.cmdModeTorTest_Click);
			// 
			// txtModeTorHost
			// 
			this.txtModeTorHost.Location = new System.Drawing.Point(119, 254);
			this.txtModeTorHost.Name = "txtModeTorHost";
			this.txtModeTorHost.Size = new System.Drawing.Size(85, 20);
			this.txtModeTorHost.TabIndex = 45;
			// 
			// lblModeTorHost
			// 
			this.lblModeTorHost.BackColor = System.Drawing.Color.Transparent;
			this.lblModeTorHost.ForeColor = System.Drawing.Color.Black;
			this.lblModeTorHost.Location = new System.Drawing.Point(72, 257);
			this.lblModeTorHost.Name = "lblModeTorHost";
			this.lblModeTorHost.Size = new System.Drawing.Size(45, 20);
			this.lblModeTorHost.TabIndex = 44;
			this.lblModeTorHost.Text = "Host:";
			this.lblModeTorHost.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtModeTorControlPort
			// 
			this.txtModeTorControlPort.Location = new System.Drawing.Point(384, 254);
			this.txtModeTorControlPort.Name = "txtModeTorControlPort";
			this.txtModeTorControlPort.Size = new System.Drawing.Size(38, 20);
			this.txtModeTorControlPort.TabIndex = 45;
			// 
			// lblModeTorControlPort
			// 
			this.lblModeTorControlPort.BackColor = System.Drawing.Color.Transparent;
			this.lblModeTorControlPort.ForeColor = System.Drawing.Color.Black;
			this.lblModeTorControlPort.Location = new System.Drawing.Point(296, 257);
			this.lblModeTorControlPort.Name = "lblModeTorControlPort";
			this.lblModeTorControlPort.Size = new System.Drawing.Size(84, 20);
			this.lblModeTorControlPort.TabIndex = 44;
			this.lblModeTorControlPort.Text = "Control Port:";
			this.lblModeTorControlPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtModeTorPort
			// 
			this.txtModeTorPort.Location = new System.Drawing.Point(249, 254);
			this.txtModeTorPort.Name = "txtModeTorPort";
			this.txtModeTorPort.Size = new System.Drawing.Size(38, 20);
			this.txtModeTorPort.TabIndex = 43;
			// 
			// lblModeTorPort
			// 
			this.lblModeTorPort.BackColor = System.Drawing.Color.Transparent;
			this.lblModeTorPort.ForeColor = System.Drawing.Color.Black;
			this.lblModeTorPort.Location = new System.Drawing.Point(202, 257);
			this.lblModeTorPort.Name = "lblModeTorPort";
			this.lblModeTorPort.Size = new System.Drawing.Size(45, 20);
			this.lblModeTorPort.TabIndex = 42;
			this.lblModeTorPort.Text = "Port:";
			this.lblModeTorPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// optModeAutomatic
			// 
			this.optModeAutomatic.AutoSize = true;
			this.optModeAutomatic.BackColor = System.Drawing.Color.Transparent;
			this.optModeAutomatic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.optModeAutomatic.ForeColor = System.Drawing.Color.Black;
			this.optModeAutomatic.Location = new System.Drawing.Point(8, 11);
			this.optModeAutomatic.Name = "optModeAutomatic";
			this.optModeAutomatic.Size = new System.Drawing.Size(94, 20);
			this.optModeAutomatic.TabIndex = 41;
			this.optModeAutomatic.Text = "Automatic";
			this.optModeAutomatic.UseVisualStyleBackColor = false;
			this.optModeAutomatic.CheckedChanged += new System.EventHandler(this.optModeAutomatic_CheckedChanged);
			// 
			// optModeTOR
			// 
			this.optModeTOR.AutoSize = true;
			this.optModeTOR.BackColor = System.Drawing.Color.Transparent;
			this.optModeTOR.ForeColor = System.Drawing.Color.Black;
			this.optModeTOR.Location = new System.Drawing.Point(18, 255);
			this.optModeTOR.Name = "optModeTOR";
			this.optModeTOR.Size = new System.Drawing.Size(48, 17);
			this.optModeTOR.TabIndex = 40;
			this.optModeTOR.Text = "TOR";
			this.optModeTOR.UseVisualStyleBackColor = false;
			this.optModeTOR.CheckedChanged += new System.EventHandler(this.optModeTOR_CheckedChanged);
			// 
			// cmdModeDocs
			// 
			this.cmdModeDocs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdModeDocs.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdModeDocs.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdModeDocs.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdModeDocs.FlatAppearance.BorderSize = 0;
			this.cmdModeDocs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdModeDocs.Image = global::AirVPN.Lib.Forms.Properties.Resources.help;
			this.cmdModeDocs.Location = new System.Drawing.Point(523, 7);
			this.cmdModeDocs.Name = "cmdModeDocs";
			this.cmdModeDocs.Size = new System.Drawing.Size(100, 30);
			this.cmdModeDocs.TabIndex = 39;
			this.cmdModeDocs.UseVisualStyleBackColor = true;
			this.cmdModeDocs.Click += new System.EventHandler(this.cmdModeDocs_Click);
			// 
			// optModeTCP443
			// 
			this.optModeTCP443.AutoSize = true;
			this.optModeTCP443.BackColor = System.Drawing.Color.Transparent;
			this.optModeTCP443.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.optModeTCP443.ForeColor = System.Drawing.Color.Black;
			this.optModeTCP443.Location = new System.Drawing.Point(317, 11);
			this.optModeTCP443.Name = "optModeTCP443";
			this.optModeTCP443.Size = new System.Drawing.Size(147, 19);
			this.optModeTCP443.TabIndex = 31;
			this.optModeTCP443.Text = "Protocol TCP, port 443";
			this.optModeTCP443.UseVisualStyleBackColor = false;
			// 
			// optModeUDP443
			// 
			this.optModeUDP443.AutoSize = true;
			this.optModeUDP443.BackColor = System.Drawing.Color.Transparent;
			this.optModeUDP443.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.optModeUDP443.ForeColor = System.Drawing.Color.Black;
			this.optModeUDP443.Location = new System.Drawing.Point(137, 11);
			this.optModeUDP443.Name = "optModeUDP443";
			this.optModeUDP443.Size = new System.Drawing.Size(150, 19);
			this.optModeUDP443.TabIndex = 30;
			this.optModeUDP443.Text = "Protocol UDP, port 443";
			this.optModeUDP443.UseVisualStyleBackColor = false;
			// 
			// optModeTCP2018
			// 
			this.optModeTCP2018.AutoSize = true;
			this.optModeTCP2018.BackColor = System.Drawing.Color.Transparent;
			this.optModeTCP2018.ForeColor = System.Drawing.Color.Black;
			this.optModeTCP2018.Location = new System.Drawing.Point(22, 210);
			this.optModeTCP2018.Name = "optModeTCP2018";
			this.optModeTCP2018.Size = new System.Drawing.Size(139, 17);
			this.optModeTCP2018.TabIndex = 29;
			this.optModeTCP2018.Text = "Protocol TCP, port 2018";
			this.optModeTCP2018.UseVisualStyleBackColor = false;
			this.optModeTCP2018.CheckedChanged += new System.EventHandler(this.optModeTCP2018_CheckedChanged);
			// 
			// optModeUDP2018
			// 
			this.optModeUDP2018.AutoSize = true;
			this.optModeUDP2018.BackColor = System.Drawing.Color.Transparent;
			this.optModeUDP2018.ForeColor = System.Drawing.Color.Black;
			this.optModeUDP2018.Location = new System.Drawing.Point(22, 187);
			this.optModeUDP2018.Name = "optModeUDP2018";
			this.optModeUDP2018.Size = new System.Drawing.Size(141, 17);
			this.optModeUDP2018.TabIndex = 28;
			this.optModeUDP2018.Text = "Protocol UDP, port 2018";
			this.optModeUDP2018.UseVisualStyleBackColor = false;
			this.optModeUDP2018.CheckedChanged += new System.EventHandler(this.optModeUDP2018_CheckedChanged);
			// 
			// optModeTCP53
			// 
			this.optModeTCP53.AutoSize = true;
			this.optModeTCP53.BackColor = System.Drawing.Color.Transparent;
			this.optModeTCP53.ForeColor = System.Drawing.Color.Black;
			this.optModeTCP53.Location = new System.Drawing.Point(22, 164);
			this.optModeTCP53.Name = "optModeTCP53";
			this.optModeTCP53.Size = new System.Drawing.Size(127, 17);
			this.optModeTCP53.TabIndex = 27;
			this.optModeTCP53.Text = "Protocol TCP, port 53";
			this.optModeTCP53.UseVisualStyleBackColor = false;
			this.optModeTCP53.CheckedChanged += new System.EventHandler(this.optModeTCP53_CheckedChanged);
			// 
			// optModeUDP53
			// 
			this.optModeUDP53.AutoSize = true;
			this.optModeUDP53.BackColor = System.Drawing.Color.Transparent;
			this.optModeUDP53.ForeColor = System.Drawing.Color.Black;
			this.optModeUDP53.Location = new System.Drawing.Point(22, 141);
			this.optModeUDP53.Name = "optModeUDP53";
			this.optModeUDP53.Size = new System.Drawing.Size(129, 17);
			this.optModeUDP53.TabIndex = 26;
			this.optModeUDP53.Text = "Protocol UDP, port 53";
			this.optModeUDP53.UseVisualStyleBackColor = false;
			this.optModeUDP53.CheckedChanged += new System.EventHandler(this.optModeUDP53_CheckedChanged);
			// 
			// optModeTCP80
			// 
			this.optModeTCP80.AutoSize = true;
			this.optModeTCP80.BackColor = System.Drawing.Color.Transparent;
			this.optModeTCP80.ForeColor = System.Drawing.Color.Black;
			this.optModeTCP80.Location = new System.Drawing.Point(22, 118);
			this.optModeTCP80.Name = "optModeTCP80";
			this.optModeTCP80.Size = new System.Drawing.Size(127, 17);
			this.optModeTCP80.TabIndex = 25;
			this.optModeTCP80.Text = "Protocol TCP, port 80";
			this.optModeTCP80.UseVisualStyleBackColor = false;
			this.optModeTCP80.CheckedChanged += new System.EventHandler(this.optModeTCP80_CheckedChanged);
			// 
			// optModeUDP80
			// 
			this.optModeUDP80.AutoSize = true;
			this.optModeUDP80.BackColor = System.Drawing.Color.Transparent;
			this.optModeUDP80.ForeColor = System.Drawing.Color.Black;
			this.optModeUDP80.Location = new System.Drawing.Point(22, 95);
			this.optModeUDP80.Name = "optModeUDP80";
			this.optModeUDP80.Size = new System.Drawing.Size(129, 17);
			this.optModeUDP80.TabIndex = 24;
			this.optModeUDP80.Text = "Protocol UDP, port 80";
			this.optModeUDP80.UseVisualStyleBackColor = false;
			this.optModeUDP80.CheckedChanged += new System.EventHandler(this.optModeUDP80_CheckedChanged);
			// 
			// label11
			// 
			this.label11.BackColor = System.Drawing.Color.Transparent;
			this.label11.ForeColor = System.Drawing.Color.Black;
			this.label11.Location = new System.Drawing.Point(13, 48);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(161, 39);
			this.label11.TabIndex = 23;
			this.label11.Text = "Alternative ports, \r\nif your ISP applies caps or blocks";
			// 
			// optModeSSH53
			// 
			this.optModeSSH53.AutoSize = true;
			this.optModeSSH53.BackColor = System.Drawing.Color.Transparent;
			this.optModeSSH53.ForeColor = System.Drawing.Color.Black;
			this.optModeSSH53.Location = new System.Drawing.Point(450, 147);
			this.optModeSSH53.Name = "optModeSSH53";
			this.optModeSSH53.Size = new System.Drawing.Size(59, 17);
			this.optModeSSH53.TabIndex = 22;
			this.optModeSSH53.Text = "Port 53";
			this.optModeSSH53.UseVisualStyleBackColor = false;
			this.optModeSSH53.CheckedChanged += new System.EventHandler(this.optModeSSH53_CheckedChanged);
			// 
			// optModeSSH80
			// 
			this.optModeSSH80.AutoSize = true;
			this.optModeSSH80.BackColor = System.Drawing.Color.Transparent;
			this.optModeSSH80.ForeColor = System.Drawing.Color.Black;
			this.optModeSSH80.Location = new System.Drawing.Point(450, 124);
			this.optModeSSH80.Name = "optModeSSH80";
			this.optModeSSH80.Size = new System.Drawing.Size(59, 17);
			this.optModeSSH80.TabIndex = 21;
			this.optModeSSH80.Text = "Port 80";
			this.optModeSSH80.UseVisualStyleBackColor = false;
			this.optModeSSH80.CheckedChanged += new System.EventHandler(this.optModeSSH80_CheckedChanged);
			// 
			// optModeSSH22Alt
			// 
			this.optModeSSH22Alt.AutoSize = true;
			this.optModeSSH22Alt.BackColor = System.Drawing.Color.Transparent;
			this.optModeSSH22Alt.ForeColor = System.Drawing.Color.Black;
			this.optModeSSH22Alt.Location = new System.Drawing.Point(450, 101);
			this.optModeSSH22Alt.Name = "optModeSSH22Alt";
			this.optModeSSH22Alt.Size = new System.Drawing.Size(158, 17);
			this.optModeSSH22Alt.TabIndex = 20;
			this.optModeSSH22Alt.Text = "Port 22 (Alternative Entry-IP)";
			this.optModeSSH22Alt.UseVisualStyleBackColor = false;
			this.optModeSSH22Alt.CheckedChanged += new System.EventHandler(this.optModeSSH22Alt_CheckedChanged);
			// 
			// optModeSSH22
			// 
			this.optModeSSH22.AutoSize = true;
			this.optModeSSH22.BackColor = System.Drawing.Color.Transparent;
			this.optModeSSH22.ForeColor = System.Drawing.Color.Black;
			this.optModeSSH22.Location = new System.Drawing.Point(450, 78);
			this.optModeSSH22.Name = "optModeSSH22";
			this.optModeSSH22.Size = new System.Drawing.Size(59, 17);
			this.optModeSSH22.TabIndex = 19;
			this.optModeSSH22.Text = "Port 22";
			this.optModeSSH22.UseVisualStyleBackColor = false;
			this.optModeSSH22.CheckedChanged += new System.EventHandler(this.optModeSSH22_CheckedChanged);
			// 
			// optModeSSL443
			// 
			this.optModeSSL443.AutoSize = true;
			this.optModeSSL443.BackColor = System.Drawing.Color.Transparent;
			this.optModeSSL443.ForeColor = System.Drawing.Color.Black;
			this.optModeSSL443.Location = new System.Drawing.Point(450, 210);
			this.optModeSSL443.Name = "optModeSSL443";
			this.optModeSSL443.Size = new System.Drawing.Size(65, 17);
			this.optModeSSL443.TabIndex = 18;
			this.optModeSSL443.Text = "Port 443";
			this.optModeSSL443.UseVisualStyleBackColor = false;
			this.optModeSSL443.CheckedChanged += new System.EventHandler(this.optModeSSL443_CheckedChanged);
			// 
			// lblModeSSH
			// 
			this.lblModeSSH.BackColor = System.Drawing.Color.Transparent;
			this.lblModeSSH.ForeColor = System.Drawing.Color.Black;
			this.lblModeSSH.Location = new System.Drawing.Point(436, 57);
			this.lblModeSSH.Name = "lblModeSSH";
			this.lblModeSSH.Size = new System.Drawing.Size(221, 18);
			this.lblModeSSH.TabIndex = 17;
			this.lblModeSSH.Text = "SSH Tunnel";
			// 
			// lblModeSSL
			// 
			this.lblModeSSL.BackColor = System.Drawing.Color.Transparent;
			this.lblModeSSL.ForeColor = System.Drawing.Color.Black;
			this.lblModeSSL.Location = new System.Drawing.Point(436, 192);
			this.lblModeSSL.Name = "lblModeSSL";
			this.lblModeSSL.Size = new System.Drawing.Size(221, 15);
			this.lblModeSSL.TabIndex = 16;
			this.lblModeSSL.Text = "SSL Tunnel";
			// 
			// optModeTCP2018Alt
			// 
			this.optModeTCP2018Alt.AutoSize = true;
			this.optModeTCP2018Alt.BackColor = System.Drawing.Color.Transparent;
			this.optModeTCP2018Alt.ForeColor = System.Drawing.Color.Black;
			this.optModeTCP2018Alt.Location = new System.Drawing.Point(214, 187);
			this.optModeTCP2018Alt.Name = "optModeTCP2018Alt";
			this.optModeTCP2018Alt.Size = new System.Drawing.Size(139, 17);
			this.optModeTCP2018Alt.TabIndex = 15;
			this.optModeTCP2018Alt.Text = "Protocol TCP, port 2018";
			this.optModeTCP2018Alt.UseVisualStyleBackColor = false;
			this.optModeTCP2018Alt.CheckedChanged += new System.EventHandler(this.optModeTCP2018Alt_CheckedChanged);
			// 
			// optModeUDP2018Alt
			// 
			this.optModeUDP2018Alt.AutoSize = true;
			this.optModeUDP2018Alt.BackColor = System.Drawing.Color.Transparent;
			this.optModeUDP2018Alt.ForeColor = System.Drawing.Color.Black;
			this.optModeUDP2018Alt.Location = new System.Drawing.Point(214, 164);
			this.optModeUDP2018Alt.Name = "optModeUDP2018Alt";
			this.optModeUDP2018Alt.Size = new System.Drawing.Size(141, 17);
			this.optModeUDP2018Alt.TabIndex = 14;
			this.optModeUDP2018Alt.Text = "Protocol UDP, port 2018";
			this.optModeUDP2018Alt.UseVisualStyleBackColor = false;
			this.optModeUDP2018Alt.CheckedChanged += new System.EventHandler(this.optModeUDP2018Alt_CheckedChanged);
			// 
			// optModeUDP53Alt
			// 
			this.optModeUDP53Alt.AutoSize = true;
			this.optModeUDP53Alt.BackColor = System.Drawing.Color.Transparent;
			this.optModeUDP53Alt.ForeColor = System.Drawing.Color.Black;
			this.optModeUDP53Alt.Location = new System.Drawing.Point(214, 141);
			this.optModeUDP53Alt.Name = "optModeUDP53Alt";
			this.optModeUDP53Alt.Size = new System.Drawing.Size(129, 17);
			this.optModeUDP53Alt.TabIndex = 13;
			this.optModeUDP53Alt.Text = "Protocol UDP, port 53";
			this.optModeUDP53Alt.UseVisualStyleBackColor = false;
			this.optModeUDP53Alt.CheckedChanged += new System.EventHandler(this.optModeUDP53Alt_CheckedChanged);
			// 
			// optModeUDP80Alt
			// 
			this.optModeUDP80Alt.AutoSize = true;
			this.optModeUDP80Alt.BackColor = System.Drawing.Color.Transparent;
			this.optModeUDP80Alt.ForeColor = System.Drawing.Color.Black;
			this.optModeUDP80Alt.Location = new System.Drawing.Point(214, 118);
			this.optModeUDP80Alt.Name = "optModeUDP80Alt";
			this.optModeUDP80Alt.Size = new System.Drawing.Size(129, 17);
			this.optModeUDP80Alt.TabIndex = 12;
			this.optModeUDP80Alt.Text = "Protocol UDP, port 80";
			this.optModeUDP80Alt.UseVisualStyleBackColor = false;
			this.optModeUDP80Alt.CheckedChanged += new System.EventHandler(this.optModeUDP80Alt_CheckedChanged);
			// 
			// optModeUDP443Alt
			// 
			this.optModeUDP443Alt.AutoSize = true;
			this.optModeUDP443Alt.BackColor = System.Drawing.Color.Transparent;
			this.optModeUDP443Alt.Checked = true;
			this.optModeUDP443Alt.ForeColor = System.Drawing.Color.Black;
			this.optModeUDP443Alt.Location = new System.Drawing.Point(214, 95);
			this.optModeUDP443Alt.Name = "optModeUDP443Alt";
			this.optModeUDP443Alt.Size = new System.Drawing.Size(135, 17);
			this.optModeUDP443Alt.TabIndex = 11;
			this.optModeUDP443Alt.TabStop = true;
			this.optModeUDP443Alt.Text = "Protocol UDP, port 443";
			this.optModeUDP443Alt.UseVisualStyleBackColor = false;
			this.optModeUDP443Alt.CheckedChanged += new System.EventHandler(this.optModeUDP443Alt_CheckedChanged);
			// 
			// label5
			// 
			this.label5.BackColor = System.Drawing.Color.Transparent;
			this.label5.ForeColor = System.Drawing.Color.Black;
			this.label5.Location = new System.Drawing.Point(206, 58);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(221, 29);
			this.label5.TabIndex = 8;
			this.label5.Text = "Alternative Entry IP, \r\nif your ISP blocks the standard Entry IP";
			// 
			// lblModeGroup5
			// 
			this.lblModeGroup5.BackColor = System.Drawing.Color.Transparent;
			this.lblModeGroup5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblModeGroup5.ForeColor = System.Drawing.Color.Black;
			this.lblModeGroup5.Location = new System.Drawing.Point(8, 246);
			this.lblModeGroup5.Name = "lblModeGroup5";
			this.lblModeGroup5.Size = new System.Drawing.Size(614, 35);
			this.lblModeGroup5.TabIndex = 46;
			this.lblModeGroup5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblModeGroup1
			// 
			this.lblModeGroup1.BackColor = System.Drawing.Color.Transparent;
			this.lblModeGroup1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblModeGroup1.ForeColor = System.Drawing.Color.Black;
			this.lblModeGroup1.Location = new System.Drawing.Point(8, 41);
			this.lblModeGroup1.Name = "lblModeGroup1";
			this.lblModeGroup1.Size = new System.Drawing.Size(171, 195);
			this.lblModeGroup1.TabIndex = 47;
			this.lblModeGroup1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblModeGroup2
			// 
			this.lblModeGroup2.BackColor = System.Drawing.Color.Transparent;
			this.lblModeGroup2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblModeGroup2.ForeColor = System.Drawing.Color.Black;
			this.lblModeGroup2.Location = new System.Drawing.Point(201, 41);
			this.lblModeGroup2.Name = "lblModeGroup2";
			this.lblModeGroup2.Size = new System.Drawing.Size(216, 195);
			this.lblModeGroup2.TabIndex = 48;
			this.lblModeGroup2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblModeGroup3
			// 
			this.lblModeGroup3.BackColor = System.Drawing.Color.Transparent;
			this.lblModeGroup3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblModeGroup3.ForeColor = System.Drawing.Color.Black;
			this.lblModeGroup3.Location = new System.Drawing.Point(429, 41);
			this.lblModeGroup3.Name = "lblModeGroup3";
			this.lblModeGroup3.Size = new System.Drawing.Size(191, 135);
			this.lblModeGroup3.TabIndex = 49;
			this.lblModeGroup3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblModeGroup4
			// 
			this.lblModeGroup4.BackColor = System.Drawing.Color.Transparent;
			this.lblModeGroup4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblModeGroup4.ForeColor = System.Drawing.Color.Black;
			this.lblModeGroup4.Location = new System.Drawing.Point(429, 186);
			this.lblModeGroup4.Name = "lblModeGroup4";
			this.lblModeGroup4.Size = new System.Drawing.Size(191, 48);
			this.lblModeGroup4.TabIndex = 50;
			this.lblModeGroup4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tabProxy
			// 
			this.tabProxy.BackColor = System.Drawing.Color.White;
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
			this.tabProxy.Size = new System.Drawing.Size(630, 291);
			this.tabProxy.TabIndex = 1;
			this.tabProxy.Text = "Proxy";
			// 
			// label17
			// 
			this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label17.BackColor = System.Drawing.Color.Transparent;
			this.label17.ForeColor = System.Drawing.Color.Black;
			this.label17.Image = ((System.Drawing.Image)(resources.GetObject("label17.Image")));
			this.label17.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.label17.Location = new System.Drawing.Point(395, 71);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(223, 56);
			this.label17.TabIndex = 45;
			this.label17.Text = "       If you use TOR, you need to set up your proxy inside TOR configuration";
			// 
			// label12
			// 
			this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label12.BackColor = System.Drawing.Color.Transparent;
			this.label12.ForeColor = System.Drawing.Color.Black;
			this.label12.Image = ((System.Drawing.Image)(resources.GetObject("label12.Image")));
			this.label12.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.label12.Location = new System.Drawing.Point(395, 15);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(223, 50);
			this.label12.TabIndex = 44;
			this.label12.Text = "       UDP, SSH and SSL connections will not be available if you use a proxy.";
			// 
			// lblProxyAuthentication
			// 
			this.lblProxyAuthentication.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyAuthentication.ForeColor = System.Drawing.Color.Black;
			this.lblProxyAuthentication.Location = new System.Drawing.Point(161, 20);
			this.lblProxyAuthentication.Name = "lblProxyAuthentication";
			this.lblProxyAuthentication.Size = new System.Drawing.Size(100, 20);
			this.lblProxyAuthentication.TabIndex = 43;
			this.lblProxyAuthentication.Text = "Authentication:";
			this.lblProxyAuthentication.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cboProxyAuthentication
			// 
			this.cboProxyAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboProxyAuthentication.FormattingEnabled = true;
			this.cboProxyAuthentication.Items.AddRange(new object[] {
            "None",
            "Basic",
            "NTLM"});
			this.cboProxyAuthentication.Location = new System.Drawing.Point(268, 16);
			this.cboProxyAuthentication.Name = "cboProxyAuthentication";
			this.cboProxyAuthentication.Size = new System.Drawing.Size(63, 21);
			this.cboProxyAuthentication.TabIndex = 42;
			this.cboProxyAuthentication.SelectedIndexChanged += new System.EventHandler(this.cboProxyAuthentication_SelectedIndexChanged);
			// 
			// txtProxyPassword
			// 
			this.txtProxyPassword.Location = new System.Drawing.Point(268, 69);
			this.txtProxyPassword.Name = "txtProxyPassword";
			this.txtProxyPassword.Size = new System.Drawing.Size(93, 20);
			this.txtProxyPassword.TabIndex = 41;
			// 
			// lblProxyPassword
			// 
			this.lblProxyPassword.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyPassword.ForeColor = System.Drawing.Color.Black;
			this.lblProxyPassword.Location = new System.Drawing.Point(161, 72);
			this.lblProxyPassword.Name = "lblProxyPassword";
			this.lblProxyPassword.Size = new System.Drawing.Size(100, 20);
			this.lblProxyPassword.TabIndex = 40;
			this.lblProxyPassword.Text = "Password:";
			this.lblProxyPassword.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtProxyLogin
			// 
			this.txtProxyLogin.Location = new System.Drawing.Point(268, 43);
			this.txtProxyLogin.Name = "txtProxyLogin";
			this.txtProxyLogin.Size = new System.Drawing.Size(93, 20);
			this.txtProxyLogin.TabIndex = 39;
			// 
			// lblProxyLogin
			// 
			this.lblProxyLogin.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyLogin.ForeColor = System.Drawing.Color.Black;
			this.lblProxyLogin.Location = new System.Drawing.Point(161, 46);
			this.lblProxyLogin.Name = "lblProxyLogin";
			this.lblProxyLogin.Size = new System.Drawing.Size(100, 20);
			this.lblProxyLogin.TabIndex = 38;
			this.lblProxyLogin.Text = "Login:";
			this.lblProxyLogin.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblProxyType
			// 
			this.lblProxyType.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyType.ForeColor = System.Drawing.Color.Black;
			this.lblProxyType.Location = new System.Drawing.Point(11, 20);
			this.lblProxyType.Name = "lblProxyType";
			this.lblProxyType.Size = new System.Drawing.Size(45, 20);
			this.lblProxyType.TabIndex = 37;
			this.lblProxyType.Text = "Type:";
			this.lblProxyType.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
            "Socks"});
			this.cboProxyMode.Location = new System.Drawing.Point(60, 16);
			this.cboProxyMode.Name = "cboProxyMode";
			this.cboProxyMode.Size = new System.Drawing.Size(63, 21);
			this.cboProxyMode.TabIndex = 36;
			this.cboProxyMode.SelectedIndexChanged += new System.EventHandler(this.cboProxyMode_SelectedIndexChanged);
			// 
			// txtProxyPort
			// 
			this.txtProxyPort.Location = new System.Drawing.Point(60, 69);
			this.txtProxyPort.Name = "txtProxyPort";
			this.txtProxyPort.Size = new System.Drawing.Size(38, 20);
			this.txtProxyPort.TabIndex = 35;
			// 
			// lblProxyPort
			// 
			this.lblProxyPort.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyPort.ForeColor = System.Drawing.Color.Black;
			this.lblProxyPort.Location = new System.Drawing.Point(11, 72);
			this.lblProxyPort.Name = "lblProxyPort";
			this.lblProxyPort.Size = new System.Drawing.Size(45, 20);
			this.lblProxyPort.TabIndex = 34;
			this.lblProxyPort.Text = "Port:";
			this.lblProxyPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtProxyHost
			// 
			this.txtProxyHost.Location = new System.Drawing.Point(60, 43);
			this.txtProxyHost.Name = "txtProxyHost";
			this.txtProxyHost.Size = new System.Drawing.Size(93, 20);
			this.txtProxyHost.TabIndex = 33;
			// 
			// lblProxyHost
			// 
			this.lblProxyHost.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyHost.ForeColor = System.Drawing.Color.Black;
			this.lblProxyHost.Location = new System.Drawing.Point(11, 46);
			this.lblProxyHost.Name = "lblProxyHost";
			this.lblProxyHost.Size = new System.Drawing.Size(45, 20);
			this.lblProxyHost.TabIndex = 32;
			this.lblProxyHost.Text = "Host:";
			this.lblProxyHost.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
			this.tabRoutes.Size = new System.Drawing.Size(630, 291);
			this.tabRoutes.TabIndex = 5;
			this.tabRoutes.Text = "Routes";
			// 
			// lblRoutesNetworkLockWarning
			// 
			this.lblRoutesNetworkLockWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblRoutesNetworkLockWarning.BackColor = System.Drawing.Color.Transparent;
			this.lblRoutesNetworkLockWarning.ForeColor = System.Drawing.Color.Black;
			this.lblRoutesNetworkLockWarning.Location = new System.Drawing.Point(8, 260);
			this.lblRoutesNetworkLockWarning.Name = "lblRoutesNetworkLockWarning";
			this.lblRoutesNetworkLockWarning.Size = new System.Drawing.Size(264, 29);
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
			this.cmdRouteEdit.Image = global::AirVPN.Lib.Forms.Properties.Resources.edit_16;
			this.cmdRouteEdit.Location = new System.Drawing.Point(589, 99);
			this.cmdRouteEdit.Name = "cmdRouteEdit";
			this.cmdRouteEdit.Size = new System.Drawing.Size(30, 30);
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
			this.cmdRouteRemove.Image = global::AirVPN.Lib.Forms.Properties.Resources.delete_16;
			this.cmdRouteRemove.Location = new System.Drawing.Point(589, 65);
			this.cmdRouteRemove.Name = "cmdRouteRemove";
			this.cmdRouteRemove.Size = new System.Drawing.Size(30, 30);
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
			this.cmdRouteAdd.Image = global::AirVPN.Lib.Forms.Properties.Resources.add_16;
			this.cmdRouteAdd.Location = new System.Drawing.Point(589, 31);
			this.cmdRouteAdd.Name = "cmdRouteAdd";
			this.cmdRouteAdd.Size = new System.Drawing.Size(30, 30);
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
			this.label6.Size = new System.Drawing.Size(615, 18);
			this.label6.TabIndex = 3;
			this.label6.Text = "Network routing rules about what destination must be in the VPN tunnel or not.";
			// 
			// cboRoutesOtherwise
			// 
			this.cboRoutesOtherwise.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cboRoutesOtherwise.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboRoutesOtherwise.FormattingEnabled = true;
			this.cboRoutesOtherwise.Location = new System.Drawing.Point(429, 260);
			this.cboRoutesOtherwise.Name = "cboRoutesOtherwise";
			this.cboRoutesOtherwise.Size = new System.Drawing.Size(154, 21);
			this.cboRoutesOtherwise.TabIndex = 2;
			this.cboRoutesOtherwise.SelectedIndexChanged += new System.EventHandler(this.cboRoutesOtherwise_SelectedIndexChanged);
			// 
			// lblRoutesOtherwise
			// 
			this.lblRoutesOtherwise.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lblRoutesOtherwise.BackColor = System.Drawing.Color.Transparent;
			this.lblRoutesOtherwise.ForeColor = System.Drawing.Color.Black;
			this.lblRoutesOtherwise.Location = new System.Drawing.Point(264, 263);
			this.lblRoutesOtherwise.Name = "lblRoutesOtherwise";
			this.lblRoutesOtherwise.Size = new System.Drawing.Size(162, 15);
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
			this.lstRoutes.Location = new System.Drawing.Point(6, 31);
			this.lstRoutes.MultiSelect = false;
			this.lstRoutes.Name = "lstRoutes";
			this.lstRoutes.OwnerDraw = true;
			this.lstRoutes.Size = new System.Drawing.Size(577, 223);
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
			// tabAdvanced
			// 
			this.tabAdvanced.BackColor = System.Drawing.Color.White;
			this.tabAdvanced.Controls.Add(this.tabAdvancedMain);
			this.tabAdvanced.Location = new System.Drawing.Point(4, 24);
			this.tabAdvanced.Name = "tabAdvanced";
			this.tabAdvanced.Size = new System.Drawing.Size(630, 291);
			this.tabAdvanced.TabIndex = 2;
			this.tabAdvanced.Text = "Advanced";
			// 
			// tabAdvancedMain
			// 
			this.tabAdvancedMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabAdvancedMain.Controls.Add(tabPage1);
			this.tabAdvancedMain.Controls.Add(this.tabPage6);
			this.tabAdvancedMain.Controls.Add(this.tabPage5);
			this.tabAdvancedMain.Controls.Add(this.tabPage4);
			this.tabAdvancedMain.Controls.Add(tabPage2);
			this.tabAdvancedMain.Controls.Add(tabPage3);
			this.tabAdvancedMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabAdvancedMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
			this.tabAdvancedMain.ItemSize = new System.Drawing.Size(150, 20);
			this.tabAdvancedMain.Location = new System.Drawing.Point(0, 0);
			this.tabAdvancedMain.Name = "tabAdvancedMain";
			this.tabAdvancedMain.SelectedIndex = 0;
			this.tabAdvancedMain.Size = new System.Drawing.Size(630, 291);
			this.tabAdvancedMain.TabIndex = 56;
			// 
			// tabPage6
			// 
			this.tabPage6.Controls.Add(this.lblDnsServers);
			this.tabPage6.Controls.Add(this.cmdDnsEdit);
			this.tabPage6.Controls.Add(this.cmdDnsRemove);
			this.tabPage6.Controls.Add(this.cmdDnsAdd);
			this.tabPage6.Controls.Add(this.lstDnsServers);
			this.tabPage6.Controls.Add(this.label7);
			this.tabPage6.Controls.Add(this.cboDnsSwitchMode);
			this.tabPage6.Controls.Add(this.chkDnsCheck);
			this.tabPage6.Location = new System.Drawing.Point(4, 24);
			this.tabPage6.Name = "tabPage6";
			this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage6.Size = new System.Drawing.Size(622, 263);
			this.tabPage6.TabIndex = 5;
			this.tabPage6.Text = "DNS";
			this.tabPage6.UseVisualStyleBackColor = true;
			// 
			// lblDnsServers
			// 
			this.lblDnsServers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDnsServers.BackColor = System.Drawing.Color.Transparent;
			this.lblDnsServers.ForeColor = System.Drawing.Color.Black;
			this.lblDnsServers.Location = new System.Drawing.Point(310, 18);
			this.lblDnsServers.Name = "lblDnsServers";
			this.lblDnsServers.Size = new System.Drawing.Size(306, 34);
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
			this.cmdDnsEdit.Image = global::AirVPN.Lib.Forms.Properties.Resources.edit_16;
			this.cmdDnsEdit.Location = new System.Drawing.Point(586, 126);
			this.cmdDnsEdit.Name = "cmdDnsEdit";
			this.cmdDnsEdit.Size = new System.Drawing.Size(30, 30);
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
			this.cmdDnsRemove.Image = global::AirVPN.Lib.Forms.Properties.Resources.delete_16;
			this.cmdDnsRemove.Location = new System.Drawing.Point(586, 92);
			this.cmdDnsRemove.Name = "cmdDnsRemove";
			this.cmdDnsRemove.Size = new System.Drawing.Size(30, 30);
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
			this.cmdDnsAdd.Image = global::AirVPN.Lib.Forms.Properties.Resources.add_16;
			this.cmdDnsAdd.Location = new System.Drawing.Point(586, 58);
			this.cmdDnsAdd.Name = "cmdDnsAdd";
			this.cmdDnsAdd.Size = new System.Drawing.Size(30, 30);
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
			this.lstDnsServers.Location = new System.Drawing.Point(313, 58);
			this.lstDnsServers.MultiSelect = false;
			this.lstDnsServers.Name = "lstDnsServers";
			this.lstDnsServers.OwnerDraw = true;
			this.lstDnsServers.Size = new System.Drawing.Size(267, 185);
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
			this.label7.Location = new System.Drawing.Point(16, 21);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(112, 13);
			this.label7.TabIndex = 75;
			this.label7.Text = "DNS Switch mode:";
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
			this.cboDnsSwitchMode.Location = new System.Drawing.Point(134, 18);
			this.cboDnsSwitchMode.Name = "cboDnsSwitchMode";
			this.cboDnsSwitchMode.Size = new System.Drawing.Size(125, 21);
			this.cboDnsSwitchMode.TabIndex = 74;
			// 
			// chkDnsCheck
			// 
			this.chkDnsCheck.AutoSize = true;
			this.chkDnsCheck.BackColor = System.Drawing.Color.Transparent;
			this.chkDnsCheck.ForeColor = System.Drawing.Color.Black;
			this.chkDnsCheck.Location = new System.Drawing.Point(19, 77);
			this.chkDnsCheck.Name = "chkDnsCheck";
			this.chkDnsCheck.Size = new System.Drawing.Size(198, 17);
			this.chkDnsCheck.TabIndex = 73;
			this.chkDnsCheck.Text = "Check if the tunnel use AirVPN DNS";
			this.chkDnsCheck.UseVisualStyleBackColor = false;
			// 
			// tabPage5
			// 
			this.tabPage5.Controls.Add(this.chkLockAllowPing);
			this.tabPage5.Controls.Add(this.chkLockAllowPrivate);
			this.tabPage5.Controls.Add(this.cmdLockHelp);
			this.tabPage5.Controls.Add(this.lblLockRoutingOutWarning);
			this.tabPage5.Controls.Add(this.lblLockAllowedIPS);
			this.tabPage5.Controls.Add(this.txtLockAllowedIPS);
			this.tabPage5.Controls.Add(this.lblLockMode);
			this.tabPage5.Controls.Add(this.cboLockMode);
			this.tabPage5.Location = new System.Drawing.Point(4, 24);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Size = new System.Drawing.Size(622, 263);
			this.tabPage5.TabIndex = 4;
			this.tabPage5.Text = "Network lock";
			this.tabPage5.UseVisualStyleBackColor = true;
			// 
			// chkLockAllowPing
			// 
			this.chkLockAllowPing.AutoSize = true;
			this.chkLockAllowPing.BackColor = System.Drawing.Color.Transparent;
			this.chkLockAllowPing.ForeColor = System.Drawing.Color.Black;
			this.chkLockAllowPing.Location = new System.Drawing.Point(16, 72);
			this.chkLockAllowPing.Name = "chkLockAllowPing";
			this.chkLockAllowPing.Size = new System.Drawing.Size(74, 17);
			this.chkLockAllowPing.TabIndex = 81;
			this.chkLockAllowPing.Text = "Allow ping";
			this.chkLockAllowPing.UseVisualStyleBackColor = false;
			// 
			// chkLockAllowPrivate
			// 
			this.chkLockAllowPrivate.AutoSize = true;
			this.chkLockAllowPrivate.BackColor = System.Drawing.Color.Transparent;
			this.chkLockAllowPrivate.ForeColor = System.Drawing.Color.Black;
			this.chkLockAllowPrivate.Location = new System.Drawing.Point(16, 49);
			this.chkLockAllowPrivate.Name = "chkLockAllowPrivate";
			this.chkLockAllowPrivate.Size = new System.Drawing.Size(105, 17);
			this.chkLockAllowPrivate.TabIndex = 80;
			this.chkLockAllowPrivate.Text = "Allow lan/private";
			this.chkLockAllowPrivate.UseVisualStyleBackColor = false;
			// 
			// cmdLockHelp
			// 
			this.cmdLockHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdLockHelp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdLockHelp.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdLockHelp.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdLockHelp.FlatAppearance.BorderSize = 0;
			this.cmdLockHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdLockHelp.Image = global::AirVPN.Lib.Forms.Properties.Resources.help;
			this.cmdLockHelp.Location = new System.Drawing.Point(506, 9);
			this.cmdLockHelp.Name = "cmdLockHelp";
			this.cmdLockHelp.Size = new System.Drawing.Size(100, 30);
			this.cmdLockHelp.TabIndex = 79;
			this.cmdLockHelp.UseVisualStyleBackColor = true;
			this.cmdLockHelp.Click += new System.EventHandler(this.cmdLockHelp_Click);
			// 
			// lblLockRoutingOutWarning
			// 
			this.lblLockRoutingOutWarning.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.lblLockRoutingOutWarning.ForeColor = System.Drawing.Color.White;
			this.lblLockRoutingOutWarning.Location = new System.Drawing.Point(13, 126);
			this.lblLockRoutingOutWarning.Name = "lblLockRoutingOutWarning";
			this.lblLockRoutingOutWarning.Size = new System.Drawing.Size(158, 122);
			this.lblLockRoutingOutWarning.TabIndex = 78;
			this.lblLockRoutingOutWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblLockAllowedIPS
			// 
			this.lblLockAllowedIPS.BackColor = System.Drawing.Color.Transparent;
			this.lblLockAllowedIPS.ForeColor = System.Drawing.Color.Black;
			this.lblLockAllowedIPS.Location = new System.Drawing.Point(174, 50);
			this.lblLockAllowedIPS.Name = "lblLockAllowedIPS";
			this.lblLockAllowedIPS.Size = new System.Drawing.Size(145, 13);
			this.lblLockAllowedIPS.TabIndex = 76;
			this.lblLockAllowedIPS.Text = "Addresses allowed:";
			// 
			// txtLockAllowedIPS
			// 
			this.txtLockAllowedIPS.AcceptsReturn = true;
			this.txtLockAllowedIPS.AcceptsTab = true;
			this.txtLockAllowedIPS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLockAllowedIPS.Location = new System.Drawing.Point(177, 66);
			this.txtLockAllowedIPS.Multiline = true;
			this.txtLockAllowedIPS.Name = "txtLockAllowedIPS";
			this.txtLockAllowedIPS.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLockAllowedIPS.Size = new System.Drawing.Size(429, 182);
			this.txtLockAllowedIPS.TabIndex = 75;
			// 
			// lblLockMode
			// 
			this.lblLockMode.BackColor = System.Drawing.Color.Transparent;
			this.lblLockMode.ForeColor = System.Drawing.Color.Black;
			this.lblLockMode.Location = new System.Drawing.Point(13, 13);
			this.lblLockMode.Name = "lblLockMode";
			this.lblLockMode.Size = new System.Drawing.Size(49, 13);
			this.lblLockMode.TabIndex = 74;
			this.lblLockMode.Text = "Mode:";
			// 
			// cboLockMode
			// 
			this.cboLockMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLockMode.FormattingEnabled = true;
			this.cboLockMode.Location = new System.Drawing.Point(67, 9);
			this.cboLockMode.Name = "cboLockMode";
			this.cboLockMode.Size = new System.Drawing.Size(243, 21);
			this.cboLockMode.TabIndex = 73;
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.TxtLoggingPathComputed);
			this.tabPage4.Controls.Add(this.lblLoggingHelp);
			this.tabPage4.Controls.Add(this.TxtLoggingPath);
			this.tabPage4.Controls.Add(this.label8);
			this.tabPage4.Controls.Add(this.chkLoggingEnabled);
			this.tabPage4.Location = new System.Drawing.Point(4, 24);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(622, 263);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Logging";
			this.tabPage4.UseVisualStyleBackColor = true;
			// 
			// TxtLoggingPathComputed
			// 
			this.TxtLoggingPathComputed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TxtLoggingPathComputed.BackColor = System.Drawing.Color.Transparent;
			this.TxtLoggingPathComputed.ForeColor = System.Drawing.Color.Black;
			this.TxtLoggingPathComputed.Location = new System.Drawing.Point(70, 83);
			this.TxtLoggingPathComputed.Name = "TxtLoggingPathComputed";
			this.TxtLoggingPathComputed.Size = new System.Drawing.Size(548, 98);
			this.TxtLoggingPathComputed.TabIndex = 62;
			this.TxtLoggingPathComputed.Text = "Current Computed Path";
			this.TxtLoggingPathComputed.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblLoggingHelp
			// 
			this.lblLoggingHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblLoggingHelp.BackColor = System.Drawing.Color.Transparent;
			this.lblLoggingHelp.ForeColor = System.Drawing.Color.Black;
			this.lblLoggingHelp.Location = new System.Drawing.Point(13, 181);
			this.lblLoggingHelp.Name = "lblLoggingHelp";
			this.lblLoggingHelp.Size = new System.Drawing.Size(596, 67);
			this.lblLoggingHelp.TabIndex = 61;
			this.lblLoggingHelp.Text = resources.GetString("lblLoggingHelp.Text");
			this.lblLoggingHelp.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// TxtLoggingPath
			// 
			this.TxtLoggingPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TxtLoggingPath.Location = new System.Drawing.Point(70, 60);
			this.TxtLoggingPath.Name = "TxtLoggingPath";
			this.TxtLoggingPath.Size = new System.Drawing.Size(539, 20);
			this.TxtLoggingPath.TabIndex = 61;
			this.TxtLoggingPath.TextChanged += new System.EventHandler(this.TxtLoggingPath_TextChanged);
			// 
			// label8
			// 
			this.label8.BackColor = System.Drawing.Color.Transparent;
			this.label8.ForeColor = System.Drawing.Color.Black;
			this.label8.Location = new System.Drawing.Point(13, 63);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(51, 18);
			this.label8.TabIndex = 60;
			this.label8.Text = "Path:";
			// 
			// chkLoggingEnabled
			// 
			this.chkLoggingEnabled.AutoSize = true;
			this.chkLoggingEnabled.BackColor = System.Drawing.Color.Transparent;
			this.chkLoggingEnabled.ForeColor = System.Drawing.Color.Black;
			this.chkLoggingEnabled.Location = new System.Drawing.Point(16, 20);
			this.chkLoggingEnabled.Name = "chkLoggingEnabled";
			this.chkLoggingEnabled.Size = new System.Drawing.Size(136, 17);
			this.chkLoggingEnabled.TabIndex = 55;
			this.chkLoggingEnabled.Text = "Logging on file enabled";
			this.chkLoggingEnabled.UseVisualStyleBackColor = false;
			// 
			// Settings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(638, 366);
			this.Controls.Add(this.pnlCommands);
			this.Controls.Add(this.tabSettings);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(650, 400);
			this.Name = "Settings";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			tabPage1.ResumeLayout(false);
			tabPage1.PerformLayout();
			this.pnlAdvancedGeneralWindowsOnly.ResumeLayout(false);
			tabPage2.ResumeLayout(false);
			tabPage2.PerformLayout();
			tabPage3.ResumeLayout(false);
			this.mnuRoutes.ResumeLayout(false);
			this.pnlCommands.ResumeLayout(false);
			this.tabSettings.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.tabGeneral.PerformLayout();
			this.pnlGeneralWindowsOnly.ResumeLayout(false);
			this.pnlGeneralWindowsOnly.PerformLayout();
			this.tabMode.ResumeLayout(false);
			this.tabMode.PerformLayout();
			this.tabProxy.ResumeLayout(false);
			this.tabProxy.PerformLayout();
			this.tabRoutes.ResumeLayout(false);
			this.tabAdvanced.ResumeLayout(false);
			this.tabAdvancedMain.ResumeLayout(false);
			this.tabPage6.ResumeLayout(false);
			this.tabPage6.PerformLayout();
			this.tabPage5.ResumeLayout(false);
			this.tabPage5.PerformLayout();
			this.tabPage4.ResumeLayout(false);
			this.tabPage4.PerformLayout();
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
        private Skin.RadioButton optModeSSH53;
        private Skin.RadioButton optModeSSH80;
        private Skin.RadioButton optModeSSH22Alt;
        private Skin.RadioButton optModeSSH22;
        private Skin.RadioButton optModeSSL443;
        private Skin.Label lblModeSSH;
        private Skin.Label lblModeSSL;
        private Skin.RadioButton optModeTCP2018Alt;
        private Skin.RadioButton optModeUDP2018Alt;
        private Skin.RadioButton optModeUDP53Alt;
        private Skin.RadioButton optModeUDP80Alt;
        private Skin.RadioButton optModeUDP443Alt;
        private Skin.Label label5;
        private Skin.RadioButton optModeTCP443;
        private Skin.RadioButton optModeUDP443;
        private Skin.RadioButton optModeTCP2018;
        private Skin.RadioButton optModeUDP2018;
        private Skin.RadioButton optModeTCP53;
        private Skin.RadioButton optModeUDP53;
        private Skin.RadioButton optModeTCP80;
        private Skin.RadioButton optModeUDP80;
        private Skin.Label label11;
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
        private Skin.TabPage tabMode;
        private Skin.TabPage tabProxy;
        private Skin.TabPage tabAdvanced;
        private Skin.TabPage tabRoutes;
		private Skin.TabControl tabAdvancedMain;
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
		private Skin.TabPage tabPage4;
		private Skin.Label TxtLoggingPathComputed;
		private Skin.Label lblLoggingHelp;
		private Skin.TextBox TxtLoggingPath;
		private Skin.Label label8;
		private Skin.CheckBox chkLoggingEnabled;
		private Skin.CheckBox chkExitConfirm;
		private Skin.TabPage tabPage5;
		private Skin.Label lblLockAllowedIPS;
		private Skin.TextBox txtLockAllowedIPS;
		private Skin.Label lblLockMode;
		private Skin.ComboBox cboLockMode;
		private Skin.Label lblLockRoutingOutWarning;
		private Skin.Label lblRoutesNetworkLockWarning;
		private Skin.Button cmdModeDocs;
		private Skin.Button cmdAdvancedGeneralDocs;
		private Skin.Button cmdLockHelp;
		private Skin.RadioButton optModeAutomatic;
		private Skin.RadioButton optModeTOR;
		private Skin.Label lblModeGroup5;
		private Skin.TextBox txtModeTorControlPort;
		private Skin.Label lblModeTorControlPort;
		private Skin.TextBox txtModeTorPort;
		private Skin.Label lblModeTorPort;
		private Skin.TextBox txtModeTorHost;
		private Skin.Label lblModeTorHost;
		private Skin.Label lblModeGroup1;
		private Skin.Label lblModeGroup2;
		private Skin.Label lblModeGroup3;
		private Skin.Label lblModeGroup4;
		private Skin.Button cmdModeTorTest;
		private Skin.TextBox txtModeTorControlPassword;
		private Skin.Label label16;
		private Skin.Label label17;
		private Skin.Label lblAdvancedManifestRefresh;
		private Skin.ComboBox cboAdvancedManifestRefresh;
		private Skin.TabPage tabPage6;
		private Skin.Label lblIpV6;
		private Skin.ComboBox cboIpV6;
		private Skin.Label label7;
		private Skin.ComboBox cboDnsSwitchMode;
		private Skin.CheckBox chkDnsCheck;
		private Skin.CheckBox chkLockAllowPing;
		private Skin.CheckBox chkLockAllowPrivate;
		private Skin.Label label9;
		private Skin.Label label13;
		private Skin.Label label10;
		private Skin.Label lblDnsServers;
		private Skin.Button cmdDnsEdit;
		private Skin.Button cmdDnsRemove;
		private Skin.Button cmdDnsAdd;
		private Skin.ListView lstDnsServers;
		private Skin.CheckBox chkNetLock;

    }
}