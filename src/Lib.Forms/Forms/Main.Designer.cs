namespace Eddie.Forms.Forms
{
    partial class Main
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
			System.Windows.Forms.ToolStripMenuItem mnuContextCopyAll;
			System.Windows.Forms.ToolStripMenuItem mnuContextSaveAll;
			System.Windows.Forms.ToolStripMenuItem mnuContextCopySelected;
			System.Windows.Forms.ToolStripMenuItem mnuContextSaveSelected;
			System.Windows.Forms.ColumnHeader colStatsKey;
			System.Windows.Forms.ColumnHeader colStatsValue;
			System.Windows.Forms.ColumnHeader colIcon;
			System.Windows.Forms.ColumnHeader colDate;
			System.Windows.Forms.ColumnHeader colMessage;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
			this.mnuMain = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mnuStatus = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuConnect = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuHomePage = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuUser = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPorts = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuSettings = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuUpdater = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuRestore = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuRestoreSep = new System.Windows.Forms.ToolStripSeparator();
			this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
			this.imgCountries = new System.Windows.Forms.ImageList(this.components);
			this.cmdServersRefresh = new Eddie.Forms.Skin.Button();
			this.cboScoreType = new Eddie.Forms.Skin.ComboBox();
			this.chkLockLast = new Eddie.Forms.Skin.CheckBox();
			this.cmdServersUndefined = new Eddie.Forms.Skin.Button();
			this.cmdServersBlackList = new Eddie.Forms.Skin.Button();
			this.cmdServersWhiteList = new Eddie.Forms.Skin.Button();
			this.cmdServersConnect = new Eddie.Forms.Skin.Button();
			this.cmdLogsSupport = new Eddie.Forms.Skin.Button();
			this.cmdLogsClean = new Eddie.Forms.Skin.Button();
			this.cmdLogsCopy = new Eddie.Forms.Skin.Button();
			this.cmdLogsSave = new Eddie.Forms.Skin.Button();
			this.cmdAreasUndefined = new Eddie.Forms.Skin.Button();
			this.cmdAreasBlackList = new Eddie.Forms.Skin.Button();
			this.cmdAreasWhiteList = new Eddie.Forms.Skin.Button();
			this.mnuLogsContext = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuServers = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mnuServersConnect = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuServersSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuServersWhiteList = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuServersBlackList = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuServersUndefined = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuServersRename = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuServersMore = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuServersRefresh = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuAreas = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mnuAreasWhiteList = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuAreasBlackList = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuAreasUndefined = new System.Windows.Forms.ToolStripMenuItem();
			this.tabMain = new Eddie.Forms.Skin.TabControl();
			this.tabOverview = new Eddie.Forms.Skin.TabPage();
			this.pnlConnected = new Eddie.Forms.Skin.Panel();
			this.txtConnectedExitIp = new Eddie.Forms.Skin.Label();
			this.lblConnectedExitIp = new Eddie.Forms.Skin.Label();
			this.lblConnectedUpload = new Eddie.Forms.Skin.Label();
			this.txtConnectedUpload = new System.Windows.Forms.Label();
			this.txtConnectedDownload = new System.Windows.Forms.Label();
			this.lblConnectedDownload = new Eddie.Forms.Skin.Label();
			this.txtConnectedSince = new Eddie.Forms.Skin.Label();
			this.lblConnectedSince = new Eddie.Forms.Skin.Label();
			this.cmdDisconnect = new Eddie.Forms.Skin.Button();
			this.lblConnectedLocation = new Eddie.Forms.Skin.Label();
			this.lblConnectedTo = new Eddie.Forms.Skin.Label();
			this.lblConnectedServerName = new Eddie.Forms.Skin.Label();
			this.lblConnectedCountry = new Eddie.Forms.Skin.Label();
			this.pnlWelcome = new Eddie.Forms.Skin.Panel();
			this.cboKey = new System.Windows.Forms.ComboBox();
			this.lblKey = new Eddie.Forms.Skin.Label();
			this.lblConnectSubtitle = new Eddie.Forms.Skin.Label();
			this.cmdLockedNetwork = new Eddie.Forms.Skin.Button();
			this.lblLoginIcon = new Eddie.Forms.Skin.Label();
			this.imgLockedNetwork = new Eddie.Forms.Skin.Label();
			this.cmdLogin = new Eddie.Forms.Skin.Button();
			this.lblPassword = new Eddie.Forms.Skin.Label();
			this.lblLogin = new Eddie.Forms.Skin.Label();
			this.cmdConnect = new Eddie.Forms.Skin.Button();
			this.chkRemember = new Eddie.Forms.Skin.CheckBox();
			this.txtPassword = new Eddie.Forms.Skin.TextBox();
			this.txtLogin = new Eddie.Forms.Skin.TextBox();
			this.pnlWaiting = new Eddie.Forms.Skin.Panel();
			this.lblWait2 = new Eddie.Forms.Skin.Label();
			this.lblWait1 = new Eddie.Forms.Skin.Label();
			this.cmdCancel = new Eddie.Forms.Skin.Button();
			this.tabProviders = new System.Windows.Forms.TabPage();
			this.cmdProviderEdit = new Eddie.Forms.Skin.Button();
			this.cmdProviderRemove = new Eddie.Forms.Skin.Button();
			this.cmdProviderAdd = new Eddie.Forms.Skin.Button();
			this.lstProviders = new Eddie.Forms.Skin.ListView();
			this.colProviderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colProviderProvider = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colProviderDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colProviderWebsite = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colProviderPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imgProviders = new System.Windows.Forms.ImageList(this.components);
			this.tabServers = new Eddie.Forms.Skin.TabPage();
			this.cmdServersMore = new Eddie.Forms.Skin.Button();
			this.cmdServersRename = new Eddie.Forms.Skin.Button();
			this.chkShowAll = new Eddie.Forms.Skin.CheckBox();
			this.lblScoreType = new Eddie.Forms.Skin.Label();
			this.pnlServers = new Eddie.Forms.Skin.Panel();
			this.tabCountries = new Eddie.Forms.Skin.TabPage();
			this.pnlAreas = new Eddie.Forms.Skin.Panel();
			this.tabSpeed = new Eddie.Forms.Skin.TabPage();
			this.lblSpeedResolution = new Eddie.Forms.Skin.Label();
			this.holSpeedChart = new System.Windows.Forms.Label();
			this.cboSpeedResolution = new Eddie.Forms.Skin.ComboBox();
			this.tabStats = new Eddie.Forms.Skin.TabPage();
			this.lstStats = new Eddie.Forms.Skin.ListView();
			this.tabLogs = new Eddie.Forms.Skin.TabPage();
			this.cmdLogsCommand = new Eddie.Forms.Skin.Button();
			this.lstLogs = new Eddie.Forms.Skin.ListView();
			mnuContextCopyAll = new System.Windows.Forms.ToolStripMenuItem();
			mnuContextSaveAll = new System.Windows.Forms.ToolStripMenuItem();
			mnuContextCopySelected = new System.Windows.Forms.ToolStripMenuItem();
			mnuContextSaveSelected = new System.Windows.Forms.ToolStripMenuItem();
			colStatsKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			colStatsValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			colIcon = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			colMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.mnuMain.SuspendLayout();
			this.mnuLogsContext.SuspendLayout();
			this.mnuServers.SuspendLayout();
			this.mnuAreas.SuspendLayout();
			this.tabMain.SuspendLayout();
			this.tabOverview.SuspendLayout();
			this.pnlConnected.SuspendLayout();
			this.pnlWelcome.SuspendLayout();
			this.pnlWaiting.SuspendLayout();
			this.tabProviders.SuspendLayout();
			this.tabServers.SuspendLayout();
			this.tabCountries.SuspendLayout();
			this.tabSpeed.SuspendLayout();
			this.tabStats.SuspendLayout();
			this.tabLogs.SuspendLayout();
			this.SuspendLayout();
			// 
			// mnuContextCopyAll
			// 
			mnuContextCopyAll.Image = global::Eddie.Forms.Properties.Resources.copy;
			mnuContextCopyAll.Name = "mnuContextCopyAll";
			mnuContextCopyAll.Size = new System.Drawing.Size(341, 30);
			mnuContextCopyAll.Text = "Copy all lines to clipboard";
			mnuContextCopyAll.Click += new System.EventHandler(this.mnuContextCopyAll_Click);
			// 
			// mnuContextSaveAll
			// 
			mnuContextSaveAll.Image = global::Eddie.Forms.Properties.Resources.save;
			mnuContextSaveAll.Name = "mnuContextSaveAll";
			mnuContextSaveAll.Size = new System.Drawing.Size(341, 30);
			mnuContextSaveAll.Text = "Save all lines to file";
			mnuContextSaveAll.Click += new System.EventHandler(this.mnuContextSaveAll_Click);
			// 
			// mnuContextCopySelected
			// 
			mnuContextCopySelected.Image = global::Eddie.Forms.Properties.Resources.copy;
			mnuContextCopySelected.Name = "mnuContextCopySelected";
			mnuContextCopySelected.Size = new System.Drawing.Size(341, 30);
			mnuContextCopySelected.Text = "Copy selected lines to clipboard";
			mnuContextCopySelected.Click += new System.EventHandler(this.mnuContextCopySelected_Click);
			// 
			// mnuContextSaveSelected
			// 
			mnuContextSaveSelected.Image = global::Eddie.Forms.Properties.Resources.save;
			mnuContextSaveSelected.Name = "mnuContextSaveSelected";
			mnuContextSaveSelected.Size = new System.Drawing.Size(341, 30);
			mnuContextSaveSelected.Text = "Save selected lines to file";
			mnuContextSaveSelected.Click += new System.EventHandler(this.mnuContextSaveSelected_Click);
			// 
			// colStatsKey
			// 
			colStatsKey.Text = "Name";
			colStatsKey.Width = 180;
			// 
			// colStatsValue
			// 
			colStatsValue.Text = "Value";
			colStatsValue.Width = 4000;
			// 
			// colIcon
			// 
			colIcon.Text = "";
			colIcon.Width = 22;
			// 
			// colDate
			// 
			colDate.Text = "Date";
			colDate.Width = 150;
			// 
			// colMessage
			// 
			colMessage.Text = "Message";
			colMessage.Width = 20000;
			// 
			// mnuMain
			// 
			this.mnuMain.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuStatus,
            this.mnuConnect,
            this.mnuSeparator3,
            this.mnuHomePage,
            this.mnuUser,
            this.mnuPorts,
            this.mnuSeparator1,
            this.mnuSettings,
            this.mnuUpdater,
            this.mnuAbout,
            this.toolStripSeparator2,
            this.mnuRestore,
            this.mnuRestoreSep,
            this.mnuExit});
			this.mnuMain.Name = "trayMenu";
			this.mnuMain.Size = new System.Drawing.Size(338, 328);
			// 
			// mnuStatus
			// 
			this.mnuStatus.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.mnuStatus.Image = global::Eddie.Forms.Properties.Resources.status_yellow;
			this.mnuStatus.Name = "mnuStatus";
			this.mnuStatus.Size = new System.Drawing.Size(337, 30);
			this.mnuStatus.Click += new System.EventHandler(this.mnuStatus_Click);
			// 
			// mnuConnect
			// 
			this.mnuConnect.ForeColor = System.Drawing.Color.Black;
			this.mnuConnect.Image = global::Eddie.Forms.Properties.Resources.connect;
			this.mnuConnect.Name = "mnuConnect";
			this.mnuConnect.Size = new System.Drawing.Size(337, 30);
			this.mnuConnect.Click += new System.EventHandler(this.mnuConnect_Click);
			// 
			// mnuSeparator3
			// 
			this.mnuSeparator3.Name = "mnuSeparator3";
			this.mnuSeparator3.Size = new System.Drawing.Size(334, 6);
			// 
			// mnuHomePage
			// 
			this.mnuHomePage.ForeColor = System.Drawing.SystemColors.ControlText;
			this.mnuHomePage.Image = global::Eddie.Forms.Properties.Resources.home;
			this.mnuHomePage.Name = "mnuHomePage";
			this.mnuHomePage.Size = new System.Drawing.Size(337, 30);
			this.mnuHomePage.Text = "Website";
			this.mnuHomePage.Click += new System.EventHandler(this.mnuHomePage_Click);
			// 
			// mnuUser
			// 
			this.mnuUser.ForeColor = System.Drawing.SystemColors.ControlText;
			this.mnuUser.Image = global::Eddie.Forms.Properties.Resources.stats;
			this.mnuUser.Name = "mnuUser";
			this.mnuUser.Size = new System.Drawing.Size(337, 30);
			this.mnuUser.Text = "Your &details and statistics (Web)";
			this.mnuUser.Click += new System.EventHandler(this.mnuUser_Click);
			// 
			// mnuPorts
			// 
			this.mnuPorts.ForeColor = System.Drawing.SystemColors.ControlText;
			this.mnuPorts.Image = global::Eddie.Forms.Properties.Resources.ports;
			this.mnuPorts.Name = "mnuPorts";
			this.mnuPorts.Size = new System.Drawing.Size(337, 30);
			this.mnuPorts.Text = "Forwarding &Ports (Web)";
			this.mnuPorts.Click += new System.EventHandler(this.mnuPorts_Click);
			// 
			// mnuSeparator1
			// 
			this.mnuSeparator1.Name = "mnuSeparator1";
			this.mnuSeparator1.Size = new System.Drawing.Size(334, 6);
			// 
			// mnuSettings
			// 
			this.mnuSettings.ForeColor = System.Drawing.SystemColors.ControlText;
			this.mnuSettings.Image = global::Eddie.Forms.Properties.Resources.settings;
			this.mnuSettings.Name = "mnuSettings";
			this.mnuSettings.Size = new System.Drawing.Size(337, 30);
			this.mnuSettings.Text = "&Preferences";
			this.mnuSettings.Click += new System.EventHandler(this.mnuSettings_Click);
			// 
			// mnuUpdater
			// 
			this.mnuUpdater.Name = "mnuUpdater";
			this.mnuUpdater.Size = new System.Drawing.Size(337, 30);
			this.mnuUpdater.Text = "&Update";
			this.mnuUpdater.Click += new System.EventHandler(this.mnuUpdater_Click);
			// 
			// mnuAbout
			// 
			this.mnuAbout.ForeColor = System.Drawing.SystemColors.ControlText;
			this.mnuAbout.Image = global::Eddie.Forms.Properties.Resources.about;
			this.mnuAbout.Name = "mnuAbout";
			this.mnuAbout.Size = new System.Drawing.Size(337, 30);
			this.mnuAbout.Text = "&About";
			this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(334, 6);
			// 
			// mnuRestore
			// 
			this.mnuRestore.Image = global::Eddie.Forms.Properties.Resources.restore;
			this.mnuRestore.Name = "mnuRestore";
			this.mnuRestore.Size = new System.Drawing.Size(337, 30);
			this.mnuRestore.Text = "&Restore";
			this.mnuRestore.Click += new System.EventHandler(this.mnuRestore_Click);
			// 
			// mnuRestoreSep
			// 
			this.mnuRestoreSep.Name = "mnuRestoreSep";
			this.mnuRestoreSep.Size = new System.Drawing.Size(334, 6);
			// 
			// mnuExit
			// 
			this.mnuExit.Image = global::Eddie.Forms.Properties.Resources.exit;
			this.mnuExit.Name = "mnuExit";
			this.mnuExit.Size = new System.Drawing.Size(337, 30);
			this.mnuExit.Text = "E&xit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// imgCountries
			// 
			this.imgCountries.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgCountries.ImageStream")));
			this.imgCountries.TransparentColor = System.Drawing.Color.Magenta;
			this.imgCountries.Images.SetKeyName(0, "ad");
			this.imgCountries.Images.SetKeyName(1, "ae");
			this.imgCountries.Images.SetKeyName(2, "af");
			this.imgCountries.Images.SetKeyName(3, "ag");
			this.imgCountries.Images.SetKeyName(4, "ai");
			this.imgCountries.Images.SetKeyName(5, "al");
			this.imgCountries.Images.SetKeyName(6, "am");
			this.imgCountries.Images.SetKeyName(7, "an");
			this.imgCountries.Images.SetKeyName(8, "ao");
			this.imgCountries.Images.SetKeyName(9, "aq");
			this.imgCountries.Images.SetKeyName(10, "ar");
			this.imgCountries.Images.SetKeyName(11, "as");
			this.imgCountries.Images.SetKeyName(12, "at");
			this.imgCountries.Images.SetKeyName(13, "au");
			this.imgCountries.Images.SetKeyName(14, "aw");
			this.imgCountries.Images.SetKeyName(15, "ax");
			this.imgCountries.Images.SetKeyName(16, "az");
			this.imgCountries.Images.SetKeyName(17, "ba");
			this.imgCountries.Images.SetKeyName(18, "bb");
			this.imgCountries.Images.SetKeyName(19, "bd");
			this.imgCountries.Images.SetKeyName(20, "be");
			this.imgCountries.Images.SetKeyName(21, "bf");
			this.imgCountries.Images.SetKeyName(22, "bg");
			this.imgCountries.Images.SetKeyName(23, "bh");
			this.imgCountries.Images.SetKeyName(24, "bi");
			this.imgCountries.Images.SetKeyName(25, "bj");
			this.imgCountries.Images.SetKeyName(26, "bl");
			this.imgCountries.Images.SetKeyName(27, "bm");
			this.imgCountries.Images.SetKeyName(28, "bn");
			this.imgCountries.Images.SetKeyName(29, "bo");
			this.imgCountries.Images.SetKeyName(30, "br");
			this.imgCountries.Images.SetKeyName(31, "bs");
			this.imgCountries.Images.SetKeyName(32, "bt");
			this.imgCountries.Images.SetKeyName(33, "bw");
			this.imgCountries.Images.SetKeyName(34, "by");
			this.imgCountries.Images.SetKeyName(35, "bz");
			this.imgCountries.Images.SetKeyName(36, "ca");
			this.imgCountries.Images.SetKeyName(37, "cc");
			this.imgCountries.Images.SetKeyName(38, "cd");
			this.imgCountries.Images.SetKeyName(39, "cf");
			this.imgCountries.Images.SetKeyName(40, "cg");
			this.imgCountries.Images.SetKeyName(41, "ch");
			this.imgCountries.Images.SetKeyName(42, "ci");
			this.imgCountries.Images.SetKeyName(43, "ck");
			this.imgCountries.Images.SetKeyName(44, "cl");
			this.imgCountries.Images.SetKeyName(45, "cm");
			this.imgCountries.Images.SetKeyName(46, "cn");
			this.imgCountries.Images.SetKeyName(47, "co");
			this.imgCountries.Images.SetKeyName(48, "cr");
			this.imgCountries.Images.SetKeyName(49, "cu");
			this.imgCountries.Images.SetKeyName(50, "cv");
			this.imgCountries.Images.SetKeyName(51, "cw");
			this.imgCountries.Images.SetKeyName(52, "cx");
			this.imgCountries.Images.SetKeyName(53, "cy");
			this.imgCountries.Images.SetKeyName(54, "cz");
			this.imgCountries.Images.SetKeyName(55, "de");
			this.imgCountries.Images.SetKeyName(56, "dj");
			this.imgCountries.Images.SetKeyName(57, "dk");
			this.imgCountries.Images.SetKeyName(58, "dm");
			this.imgCountries.Images.SetKeyName(59, "do");
			this.imgCountries.Images.SetKeyName(60, "dz");
			this.imgCountries.Images.SetKeyName(61, "ec");
			this.imgCountries.Images.SetKeyName(62, "ee");
			this.imgCountries.Images.SetKeyName(63, "eg");
			this.imgCountries.Images.SetKeyName(64, "eh");
			this.imgCountries.Images.SetKeyName(65, "er");
			this.imgCountries.Images.SetKeyName(66, "es");
			this.imgCountries.Images.SetKeyName(67, "et");
			this.imgCountries.Images.SetKeyName(68, "eu");
			this.imgCountries.Images.SetKeyName(69, "fi");
			this.imgCountries.Images.SetKeyName(70, "fj");
			this.imgCountries.Images.SetKeyName(71, "fk");
			this.imgCountries.Images.SetKeyName(72, "fm");
			this.imgCountries.Images.SetKeyName(73, "fo");
			this.imgCountries.Images.SetKeyName(74, "fr");
			this.imgCountries.Images.SetKeyName(75, "ga");
			this.imgCountries.Images.SetKeyName(76, "gb");
			this.imgCountries.Images.SetKeyName(77, "gd");
			this.imgCountries.Images.SetKeyName(78, "ge");
			this.imgCountries.Images.SetKeyName(79, "gg");
			this.imgCountries.Images.SetKeyName(80, "gh");
			this.imgCountries.Images.SetKeyName(81, "gi");
			this.imgCountries.Images.SetKeyName(82, "gl");
			this.imgCountries.Images.SetKeyName(83, "gm");
			this.imgCountries.Images.SetKeyName(84, "gn");
			this.imgCountries.Images.SetKeyName(85, "gq");
			this.imgCountries.Images.SetKeyName(86, "gr");
			this.imgCountries.Images.SetKeyName(87, "gs");
			this.imgCountries.Images.SetKeyName(88, "gt");
			this.imgCountries.Images.SetKeyName(89, "gu");
			this.imgCountries.Images.SetKeyName(90, "gw");
			this.imgCountries.Images.SetKeyName(91, "gy");
			this.imgCountries.Images.SetKeyName(92, "hk");
			this.imgCountries.Images.SetKeyName(93, "hn");
			this.imgCountries.Images.SetKeyName(94, "hr");
			this.imgCountries.Images.SetKeyName(95, "ht");
			this.imgCountries.Images.SetKeyName(96, "hu");
			this.imgCountries.Images.SetKeyName(97, "ic");
			this.imgCountries.Images.SetKeyName(98, "id");
			this.imgCountries.Images.SetKeyName(99, "ie");
			this.imgCountries.Images.SetKeyName(100, "il");
			this.imgCountries.Images.SetKeyName(101, "im");
			this.imgCountries.Images.SetKeyName(102, "in");
			this.imgCountries.Images.SetKeyName(103, "iq");
			this.imgCountries.Images.SetKeyName(104, "ir");
			this.imgCountries.Images.SetKeyName(105, "is");
			this.imgCountries.Images.SetKeyName(106, "it");
			this.imgCountries.Images.SetKeyName(107, "je");
			this.imgCountries.Images.SetKeyName(108, "jm");
			this.imgCountries.Images.SetKeyName(109, "jo");
			this.imgCountries.Images.SetKeyName(110, "jp");
			this.imgCountries.Images.SetKeyName(111, "ke");
			this.imgCountries.Images.SetKeyName(112, "kg");
			this.imgCountries.Images.SetKeyName(113, "kh");
			this.imgCountries.Images.SetKeyName(114, "ki");
			this.imgCountries.Images.SetKeyName(115, "km");
			this.imgCountries.Images.SetKeyName(116, "kn");
			this.imgCountries.Images.SetKeyName(117, "kp");
			this.imgCountries.Images.SetKeyName(118, "kr");
			this.imgCountries.Images.SetKeyName(119, "kv");
			this.imgCountries.Images.SetKeyName(120, "kw");
			this.imgCountries.Images.SetKeyName(121, "ky");
			this.imgCountries.Images.SetKeyName(122, "kz");
			this.imgCountries.Images.SetKeyName(123, "la");
			this.imgCountries.Images.SetKeyName(124, "lb");
			this.imgCountries.Images.SetKeyName(125, "lc");
			this.imgCountries.Images.SetKeyName(126, "li");
			this.imgCountries.Images.SetKeyName(127, "lk");
			this.imgCountries.Images.SetKeyName(128, "lr");
			this.imgCountries.Images.SetKeyName(129, "ls");
			this.imgCountries.Images.SetKeyName(130, "lt");
			this.imgCountries.Images.SetKeyName(131, "lu");
			this.imgCountries.Images.SetKeyName(132, "lv");
			this.imgCountries.Images.SetKeyName(133, "ly");
			this.imgCountries.Images.SetKeyName(134, "ma");
			this.imgCountries.Images.SetKeyName(135, "mc");
			this.imgCountries.Images.SetKeyName(136, "md");
			this.imgCountries.Images.SetKeyName(137, "me");
			this.imgCountries.Images.SetKeyName(138, "mf");
			this.imgCountries.Images.SetKeyName(139, "mg");
			this.imgCountries.Images.SetKeyName(140, "mh");
			this.imgCountries.Images.SetKeyName(141, "mk");
			this.imgCountries.Images.SetKeyName(142, "ml");
			this.imgCountries.Images.SetKeyName(143, "mm");
			this.imgCountries.Images.SetKeyName(144, "mn");
			this.imgCountries.Images.SetKeyName(145, "mo");
			this.imgCountries.Images.SetKeyName(146, "mp");
			this.imgCountries.Images.SetKeyName(147, "mq");
			this.imgCountries.Images.SetKeyName(148, "mr");
			this.imgCountries.Images.SetKeyName(149, "ms");
			this.imgCountries.Images.SetKeyName(150, "mt");
			this.imgCountries.Images.SetKeyName(151, "mu");
			this.imgCountries.Images.SetKeyName(152, "mv");
			this.imgCountries.Images.SetKeyName(153, "mw");
			this.imgCountries.Images.SetKeyName(154, "mx");
			this.imgCountries.Images.SetKeyName(155, "my");
			this.imgCountries.Images.SetKeyName(156, "mz");
			this.imgCountries.Images.SetKeyName(157, "na");
			this.imgCountries.Images.SetKeyName(158, "nc");
			this.imgCountries.Images.SetKeyName(159, "ne");
			this.imgCountries.Images.SetKeyName(160, "nf");
			this.imgCountries.Images.SetKeyName(161, "ng");
			this.imgCountries.Images.SetKeyName(162, "ni");
			this.imgCountries.Images.SetKeyName(163, "nl");
			this.imgCountries.Images.SetKeyName(164, "no");
			this.imgCountries.Images.SetKeyName(165, "np");
			this.imgCountries.Images.SetKeyName(166, "nr");
			this.imgCountries.Images.SetKeyName(167, "nu");
			this.imgCountries.Images.SetKeyName(168, "nz");
			this.imgCountries.Images.SetKeyName(169, "om");
			this.imgCountries.Images.SetKeyName(170, "pa");
			this.imgCountries.Images.SetKeyName(171, "pe");
			this.imgCountries.Images.SetKeyName(172, "pf");
			this.imgCountries.Images.SetKeyName(173, "pg");
			this.imgCountries.Images.SetKeyName(174, "ph");
			this.imgCountries.Images.SetKeyName(175, "pk");
			this.imgCountries.Images.SetKeyName(176, "pl");
			this.imgCountries.Images.SetKeyName(177, "pn");
			this.imgCountries.Images.SetKeyName(178, "pr");
			this.imgCountries.Images.SetKeyName(179, "ps");
			this.imgCountries.Images.SetKeyName(180, "pt");
			this.imgCountries.Images.SetKeyName(181, "pw");
			this.imgCountries.Images.SetKeyName(182, "py");
			this.imgCountries.Images.SetKeyName(183, "qa");
			this.imgCountries.Images.SetKeyName(184, "ro");
			this.imgCountries.Images.SetKeyName(185, "rs");
			this.imgCountries.Images.SetKeyName(186, "ru");
			this.imgCountries.Images.SetKeyName(187, "rw");
			this.imgCountries.Images.SetKeyName(188, "sa");
			this.imgCountries.Images.SetKeyName(189, "sb");
			this.imgCountries.Images.SetKeyName(190, "sc");
			this.imgCountries.Images.SetKeyName(191, "sd");
			this.imgCountries.Images.SetKeyName(192, "se");
			this.imgCountries.Images.SetKeyName(193, "sg");
			this.imgCountries.Images.SetKeyName(194, "sh");
			this.imgCountries.Images.SetKeyName(195, "si");
			this.imgCountries.Images.SetKeyName(196, "sk");
			this.imgCountries.Images.SetKeyName(197, "sl");
			this.imgCountries.Images.SetKeyName(198, "sm");
			this.imgCountries.Images.SetKeyName(199, "sn");
			this.imgCountries.Images.SetKeyName(200, "so");
			this.imgCountries.Images.SetKeyName(201, "sr");
			this.imgCountries.Images.SetKeyName(202, "ss");
			this.imgCountries.Images.SetKeyName(203, "st");
			this.imgCountries.Images.SetKeyName(204, "sv");
			this.imgCountries.Images.SetKeyName(205, "sy");
			this.imgCountries.Images.SetKeyName(206, "sz");
			this.imgCountries.Images.SetKeyName(207, "tc");
			this.imgCountries.Images.SetKeyName(208, "td");
			this.imgCountries.Images.SetKeyName(209, "tf");
			this.imgCountries.Images.SetKeyName(210, "tg");
			this.imgCountries.Images.SetKeyName(211, "th");
			this.imgCountries.Images.SetKeyName(212, "tj");
			this.imgCountries.Images.SetKeyName(213, "tk");
			this.imgCountries.Images.SetKeyName(214, "tl");
			this.imgCountries.Images.SetKeyName(215, "tm");
			this.imgCountries.Images.SetKeyName(216, "tn");
			this.imgCountries.Images.SetKeyName(217, "to");
			this.imgCountries.Images.SetKeyName(218, "tp");
			this.imgCountries.Images.SetKeyName(219, "tr");
			this.imgCountries.Images.SetKeyName(220, "tt");
			this.imgCountries.Images.SetKeyName(221, "tv");
			this.imgCountries.Images.SetKeyName(222, "tw");
			this.imgCountries.Images.SetKeyName(223, "tz");
			this.imgCountries.Images.SetKeyName(224, "ua");
			this.imgCountries.Images.SetKeyName(225, "ug");
			this.imgCountries.Images.SetKeyName(226, "us");
			this.imgCountries.Images.SetKeyName(227, "uy");
			this.imgCountries.Images.SetKeyName(228, "uz");
			this.imgCountries.Images.SetKeyName(229, "va");
			this.imgCountries.Images.SetKeyName(230, "vc");
			this.imgCountries.Images.SetKeyName(231, "ve");
			this.imgCountries.Images.SetKeyName(232, "vg");
			this.imgCountries.Images.SetKeyName(233, "vi");
			this.imgCountries.Images.SetKeyName(234, "vn");
			this.imgCountries.Images.SetKeyName(235, "vu");
			this.imgCountries.Images.SetKeyName(236, "wf");
			this.imgCountries.Images.SetKeyName(237, "ws");
			this.imgCountries.Images.SetKeyName(238, "ye");
			this.imgCountries.Images.SetKeyName(239, "yt");
			this.imgCountries.Images.SetKeyName(240, "za");
			this.imgCountries.Images.SetKeyName(241, "zm");
			this.imgCountries.Images.SetKeyName(242, "zw");
			this.imgCountries.Images.SetKeyName(243, "a1");
			this.imgCountries.Images.SetKeyName(244, "a2");
			this.imgCountries.Images.SetKeyName(245, "o1");
			// 
			// cmdServersRefresh
			// 
			this.cmdServersRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdServersRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdServersRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdServersRefresh.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdServersRefresh.FlatAppearance.BorderSize = 0;
			this.cmdServersRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdServersRefresh.Image = global::Eddie.Forms.Properties.Resources.reload;
			this.cmdServersRefresh.Location = new System.Drawing.Point(1042, 442);
			this.cmdServersRefresh.Name = "cmdServersRefresh";
			this.cmdServersRefresh.Size = new System.Drawing.Size(42, 42);
			this.cmdServersRefresh.TabIndex = 66;
			this.cmdServersRefresh.UseVisualStyleBackColor = true;
			this.cmdServersRefresh.Click += new System.EventHandler(this.cmdServersRefresh_Click);
			// 
			// cboScoreType
			// 
			this.cboScoreType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cboScoreType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboScoreType.FormattingEnabled = true;
			this.cboScoreType.Items.AddRange(new object[] {
            "Speed",
            "Latency"});
			this.cboScoreType.Location = new System.Drawing.Point(640, 454);
			this.cboScoreType.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
			this.cboScoreType.Name = "cboScoreType";
			this.cboScoreType.Size = new System.Drawing.Size(148, 28);
			this.cboScoreType.TabIndex = 49;
			this.cboScoreType.SelectionChangeCommitted += new System.EventHandler(this.cboScoreType_SelectionChangeCommitted);
			// 
			// chkLockLast
			// 
			this.chkLockLast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkLockLast.Location = new System.Drawing.Point(816, 454);
			this.chkLockLast.Name = "chkLockLast";
			this.chkLockLast.Size = new System.Drawing.Size(208, 32);
			this.chkLockLast.TabIndex = 46;
			this.chkLockLast.Text = "Lock Current";
			this.chkLockLast.UseVisualStyleBackColor = true;
			this.chkLockLast.CheckedChanged += new System.EventHandler(this.chkLockCurrent_CheckedChanged);
			// 
			// cmdServersUndefined
			// 
			this.cmdServersUndefined.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdServersUndefined.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdServersUndefined.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdServersUndefined.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdServersUndefined.FlatAppearance.BorderSize = 0;
			this.cmdServersUndefined.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdServersUndefined.Image = global::Eddie.Forms.Properties.Resources.blacklist_2;
			this.cmdServersUndefined.Location = new System.Drawing.Point(1042, 165);
			this.cmdServersUndefined.Name = "cmdServersUndefined";
			this.cmdServersUndefined.Size = new System.Drawing.Size(42, 42);
			this.cmdServersUndefined.TabIndex = 45;
			this.cmdServersUndefined.UseVisualStyleBackColor = true;
			this.cmdServersUndefined.Click += new System.EventHandler(this.cmdServersUndefined_Click);
			// 
			// cmdServersBlackList
			// 
			this.cmdServersBlackList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdServersBlackList.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdServersBlackList.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdServersBlackList.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdServersBlackList.FlatAppearance.BorderSize = 0;
			this.cmdServersBlackList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdServersBlackList.Image = global::Eddie.Forms.Properties.Resources.blacklist_1;
			this.cmdServersBlackList.Location = new System.Drawing.Point(1042, 117);
			this.cmdServersBlackList.Name = "cmdServersBlackList";
			this.cmdServersBlackList.Size = new System.Drawing.Size(42, 42);
			this.cmdServersBlackList.TabIndex = 44;
			this.cmdServersBlackList.UseVisualStyleBackColor = true;
			this.cmdServersBlackList.Click += new System.EventHandler(this.cmdServersBlackList_Click);
			// 
			// cmdServersWhiteList
			// 
			this.cmdServersWhiteList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdServersWhiteList.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdServersWhiteList.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdServersWhiteList.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdServersWhiteList.FlatAppearance.BorderSize = 0;
			this.cmdServersWhiteList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdServersWhiteList.Image = global::Eddie.Forms.Properties.Resources.blacklist_0;
			this.cmdServersWhiteList.Location = new System.Drawing.Point(1042, 69);
			this.cmdServersWhiteList.Name = "cmdServersWhiteList";
			this.cmdServersWhiteList.Size = new System.Drawing.Size(42, 42);
			this.cmdServersWhiteList.TabIndex = 43;
			this.cmdServersWhiteList.UseVisualStyleBackColor = true;
			this.cmdServersWhiteList.Click += new System.EventHandler(this.cmdServersWhiteList_Click);
			// 
			// cmdServersConnect
			// 
			this.cmdServersConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdServersConnect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdServersConnect.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdServersConnect.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdServersConnect.FlatAppearance.BorderSize = 0;
			this.cmdServersConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdServersConnect.Image = global::Eddie.Forms.Properties.Resources.connect;
			this.cmdServersConnect.Location = new System.Drawing.Point(1042, 6);
			this.cmdServersConnect.Name = "cmdServersConnect";
			this.cmdServersConnect.Size = new System.Drawing.Size(42, 42);
			this.cmdServersConnect.TabIndex = 42;
			this.cmdServersConnect.UseVisualStyleBackColor = true;
			this.cmdServersConnect.Click += new System.EventHandler(this.cmdServersConnect_Click);
			// 
			// cmdLogsSupport
			// 
			this.cmdLogsSupport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdLogsSupport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdLogsSupport.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdLogsSupport.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdLogsSupport.FlatAppearance.BorderSize = 0;
			this.cmdLogsSupport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdLogsSupport.Image = global::Eddie.Forms.Properties.Resources.support;
			this.cmdLogsSupport.Location = new System.Drawing.Point(1042, 178);
			this.cmdLogsSupport.Name = "cmdLogsSupport";
			this.cmdLogsSupport.Size = new System.Drawing.Size(42, 42);
			this.cmdLogsSupport.TabIndex = 51;
			this.cmdLogsSupport.UseVisualStyleBackColor = true;
			this.cmdLogsSupport.Click += new System.EventHandler(this.cmdLogsSupport_Click);
			// 
			// cmdLogsClean
			// 
			this.cmdLogsClean.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdLogsClean.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdLogsClean.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdLogsClean.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdLogsClean.FlatAppearance.BorderSize = 0;
			this.cmdLogsClean.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdLogsClean.Image = global::Eddie.Forms.Properties.Resources.clear;
			this.cmdLogsClean.Location = new System.Drawing.Point(1042, 6);
			this.cmdLogsClean.Name = "cmdLogsClean";
			this.cmdLogsClean.Size = new System.Drawing.Size(42, 42);
			this.cmdLogsClean.TabIndex = 48;
			this.cmdLogsClean.UseVisualStyleBackColor = true;
			this.cmdLogsClean.Click += new System.EventHandler(this.cmdLogsClean_Click);
			// 
			// cmdLogsCopy
			// 
			this.cmdLogsCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdLogsCopy.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdLogsCopy.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdLogsCopy.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdLogsCopy.FlatAppearance.BorderSize = 0;
			this.cmdLogsCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdLogsCopy.Image = global::Eddie.Forms.Properties.Resources.copy;
			this.cmdLogsCopy.Location = new System.Drawing.Point(1042, 54);
			this.cmdLogsCopy.Name = "cmdLogsCopy";
			this.cmdLogsCopy.Size = new System.Drawing.Size(42, 42);
			this.cmdLogsCopy.TabIndex = 47;
			this.cmdLogsCopy.UseVisualStyleBackColor = true;
			this.cmdLogsCopy.Click += new System.EventHandler(this.cmdLogsCopy_Click);
			// 
			// cmdLogsSave
			// 
			this.cmdLogsSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdLogsSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdLogsSave.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdLogsSave.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdLogsSave.FlatAppearance.BorderSize = 0;
			this.cmdLogsSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdLogsSave.Image = global::Eddie.Forms.Properties.Resources.save;
			this.cmdLogsSave.Location = new System.Drawing.Point(1042, 102);
			this.cmdLogsSave.Name = "cmdLogsSave";
			this.cmdLogsSave.Size = new System.Drawing.Size(42, 42);
			this.cmdLogsSave.TabIndex = 46;
			this.cmdLogsSave.UseVisualStyleBackColor = true;
			this.cmdLogsSave.Click += new System.EventHandler(this.cmdLogsSave_Click);
			// 
			// cmdAreasUndefined
			// 
			this.cmdAreasUndefined.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdAreasUndefined.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdAreasUndefined.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdAreasUndefined.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdAreasUndefined.FlatAppearance.BorderSize = 0;
			this.cmdAreasUndefined.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAreasUndefined.Image = global::Eddie.Forms.Properties.Resources.blacklist_2;
			this.cmdAreasUndefined.Location = new System.Drawing.Point(1042, 102);
			this.cmdAreasUndefined.Name = "cmdAreasUndefined";
			this.cmdAreasUndefined.Size = new System.Drawing.Size(42, 42);
			this.cmdAreasUndefined.TabIndex = 52;
			this.cmdAreasUndefined.UseVisualStyleBackColor = true;
			this.cmdAreasUndefined.Click += new System.EventHandler(this.cmdAreasUndefined_Click);
			// 
			// cmdAreasBlackList
			// 
			this.cmdAreasBlackList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdAreasBlackList.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdAreasBlackList.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdAreasBlackList.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdAreasBlackList.FlatAppearance.BorderSize = 0;
			this.cmdAreasBlackList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAreasBlackList.Image = global::Eddie.Forms.Properties.Resources.blacklist_1;
			this.cmdAreasBlackList.Location = new System.Drawing.Point(1042, 54);
			this.cmdAreasBlackList.Name = "cmdAreasBlackList";
			this.cmdAreasBlackList.Size = new System.Drawing.Size(42, 42);
			this.cmdAreasBlackList.TabIndex = 51;
			this.cmdAreasBlackList.UseVisualStyleBackColor = true;
			this.cmdAreasBlackList.Click += new System.EventHandler(this.cmdAreasBlackList_Click);
			// 
			// cmdAreasWhiteList
			// 
			this.cmdAreasWhiteList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdAreasWhiteList.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdAreasWhiteList.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdAreasWhiteList.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdAreasWhiteList.FlatAppearance.BorderSize = 0;
			this.cmdAreasWhiteList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAreasWhiteList.Image = global::Eddie.Forms.Properties.Resources.blacklist_0;
			this.cmdAreasWhiteList.Location = new System.Drawing.Point(1042, 6);
			this.cmdAreasWhiteList.Name = "cmdAreasWhiteList";
			this.cmdAreasWhiteList.Size = new System.Drawing.Size(42, 42);
			this.cmdAreasWhiteList.TabIndex = 61;
			this.cmdAreasWhiteList.UseVisualStyleBackColor = true;
			this.cmdAreasWhiteList.Click += new System.EventHandler(this.cmdAreasWhiteList_Click);
			// 
			// mnuLogsContext
			// 
			this.mnuLogsContext.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.mnuLogsContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuContextCopyAll,
            mnuContextSaveAll,
            this.toolStripSeparator1,
            mnuContextCopySelected,
            mnuContextSaveSelected});
			this.mnuLogsContext.Name = "mnuContext";
			this.mnuLogsContext.Size = new System.Drawing.Size(342, 130);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(338, 6);
			// 
			// mnuServers
			// 
			this.mnuServers.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.mnuServers.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuServersConnect,
            this.mnuServersSeparator1,
            this.mnuServersWhiteList,
            this.mnuServersBlackList,
            this.mnuServersUndefined,
            this.toolStripSeparator4,
            this.mnuServersRename,
            this.mnuServersMore,
            this.toolStripSeparator3,
            this.mnuServersRefresh});
			this.mnuServers.Name = "mnuServers";
			this.mnuServers.Size = new System.Drawing.Size(193, 232);
			// 
			// mnuServersConnect
			// 
			this.mnuServersConnect.Image = global::Eddie.Forms.Properties.Resources.connect;
			this.mnuServersConnect.Name = "mnuServersConnect";
			this.mnuServersConnect.Size = new System.Drawing.Size(192, 30);
			this.mnuServersConnect.Text = "Connect now";
			this.mnuServersConnect.Click += new System.EventHandler(this.mnuServersConnect_Click);
			// 
			// mnuServersSeparator1
			// 
			this.mnuServersSeparator1.Name = "mnuServersSeparator1";
			this.mnuServersSeparator1.Size = new System.Drawing.Size(189, 6);
			// 
			// mnuServersWhiteList
			// 
			this.mnuServersWhiteList.Image = global::Eddie.Forms.Properties.Resources.blacklist_0;
			this.mnuServersWhiteList.Name = "mnuServersWhiteList";
			this.mnuServersWhiteList.Size = new System.Drawing.Size(192, 30);
			this.mnuServersWhiteList.Text = "Whitelist";
			this.mnuServersWhiteList.Click += new System.EventHandler(this.mnuServersWhitelist_Click);
			// 
			// mnuServersBlackList
			// 
			this.mnuServersBlackList.Image = global::Eddie.Forms.Properties.Resources.blacklist_1;
			this.mnuServersBlackList.Name = "mnuServersBlackList";
			this.mnuServersBlackList.Size = new System.Drawing.Size(192, 30);
			this.mnuServersBlackList.Text = "Blacklist";
			this.mnuServersBlackList.Click += new System.EventHandler(this.mnuServersBlacklist_Click);
			// 
			// mnuServersUndefined
			// 
			this.mnuServersUndefined.Image = global::Eddie.Forms.Properties.Resources.blacklist_2;
			this.mnuServersUndefined.Name = "mnuServersUndefined";
			this.mnuServersUndefined.Size = new System.Drawing.Size(192, 30);
			this.mnuServersUndefined.Text = "Undefined";
			this.mnuServersUndefined.Click += new System.EventHandler(this.mnuServersUndefined_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(189, 6);
			// 
			// mnuServersRename
			// 
			this.mnuServersRename.Image = global::Eddie.Forms.Properties.Resources.rename;
			this.mnuServersRename.Name = "mnuServersRename";
			this.mnuServersRename.Size = new System.Drawing.Size(192, 30);
			this.mnuServersRename.Text = "Rename";
			this.mnuServersRename.Click += new System.EventHandler(this.mnuServersRename_Click);
			// 
			// mnuServersMore
			// 
			this.mnuServersMore.Image = global::Eddie.Forms.Properties.Resources.more;
			this.mnuServersMore.Name = "mnuServersMore";
			this.mnuServersMore.Size = new System.Drawing.Size(192, 30);
			this.mnuServersMore.Text = "More";
			this.mnuServersMore.Click += new System.EventHandler(this.mnuServersMore_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(189, 6);
			// 
			// mnuServersRefresh
			// 
			this.mnuServersRefresh.Image = global::Eddie.Forms.Properties.Resources.reload;
			this.mnuServersRefresh.Name = "mnuServersRefresh";
			this.mnuServersRefresh.Size = new System.Drawing.Size(192, 30);
			this.mnuServersRefresh.Text = "Refresh list";
			this.mnuServersRefresh.Click += new System.EventHandler(this.mnuServersRefresh_Click);
			// 
			// mnuAreas
			// 
			this.mnuAreas.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.mnuAreas.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAreasWhiteList,
            this.mnuAreasBlackList,
            this.mnuAreasUndefined});
			this.mnuAreas.Name = "mnuAreas";
			this.mnuAreas.Size = new System.Drawing.Size(171, 94);
			// 
			// mnuAreasWhiteList
			// 
			this.mnuAreasWhiteList.Image = global::Eddie.Forms.Properties.Resources.blacklist_0;
			this.mnuAreasWhiteList.Name = "mnuAreasWhiteList";
			this.mnuAreasWhiteList.Size = new System.Drawing.Size(170, 30);
			this.mnuAreasWhiteList.Text = "Whitelist";
			this.mnuAreasWhiteList.Click += new System.EventHandler(this.mnuAreasWhiteList_Click);
			// 
			// mnuAreasBlackList
			// 
			this.mnuAreasBlackList.Image = global::Eddie.Forms.Properties.Resources.blacklist_1;
			this.mnuAreasBlackList.Name = "mnuAreasBlackList";
			this.mnuAreasBlackList.Size = new System.Drawing.Size(170, 30);
			this.mnuAreasBlackList.Text = "Blacklist";
			this.mnuAreasBlackList.Click += new System.EventHandler(this.cmdAreasBlackList_Click);
			// 
			// mnuAreasUndefined
			// 
			this.mnuAreasUndefined.Image = global::Eddie.Forms.Properties.Resources.blacklist_2;
			this.mnuAreasUndefined.Name = "mnuAreasUndefined";
			this.mnuAreasUndefined.Size = new System.Drawing.Size(170, 30);
			this.mnuAreasUndefined.Text = "Undefined";
			this.mnuAreasUndefined.Click += new System.EventHandler(this.mnuAreasUndefined_Click);
			// 
			// tabMain
			// 
			this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabMain.Controls.Add(this.tabOverview);
			this.tabMain.Controls.Add(this.tabProviders);
			this.tabMain.Controls.Add(this.tabServers);
			this.tabMain.Controls.Add(this.tabCountries);
			this.tabMain.Controls.Add(this.tabSpeed);
			this.tabMain.Controls.Add(this.tabStats);
			this.tabMain.Controls.Add(this.tabLogs);
			this.tabMain.Location = new System.Drawing.Point(0, 0);
			this.tabMain.Margin = new System.Windows.Forms.Padding(0);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(1107, 538);
			this.tabMain.TabIndex = 21;
			this.tabMain.Visible = false;
			// 
			// tabOverview
			// 
			this.tabOverview.Controls.Add(this.pnlConnected);
			this.tabOverview.Controls.Add(this.pnlWelcome);
			this.tabOverview.Controls.Add(this.pnlWaiting);
			this.tabOverview.Location = new System.Drawing.Point(4, 29);
			this.tabOverview.Margin = new System.Windows.Forms.Padding(4);
			this.tabOverview.Name = "tabOverview";
			this.tabOverview.Size = new System.Drawing.Size(1099, 505);
			this.tabOverview.TabIndex = 4;
			this.tabOverview.Text = "Overview";
			this.tabOverview.UseVisualStyleBackColor = true;
			// 
			// pnlConnected
			// 
			this.pnlConnected.BackColor = System.Drawing.Color.Transparent;
			this.pnlConnected.Controls.Add(this.txtConnectedExitIp);
			this.pnlConnected.Controls.Add(this.lblConnectedExitIp);
			this.pnlConnected.Controls.Add(this.lblConnectedUpload);
			this.pnlConnected.Controls.Add(this.txtConnectedUpload);
			this.pnlConnected.Controls.Add(this.txtConnectedDownload);
			this.pnlConnected.Controls.Add(this.lblConnectedDownload);
			this.pnlConnected.Controls.Add(this.txtConnectedSince);
			this.pnlConnected.Controls.Add(this.lblConnectedSince);
			this.pnlConnected.Controls.Add(this.cmdDisconnect);
			this.pnlConnected.Controls.Add(this.lblConnectedLocation);
			this.pnlConnected.Controls.Add(this.lblConnectedTo);
			this.pnlConnected.Controls.Add(this.lblConnectedServerName);
			this.pnlConnected.Controls.Add(this.lblConnectedCountry);
			this.pnlConnected.Location = new System.Drawing.Point(347, 170);
			this.pnlConnected.Margin = new System.Windows.Forms.Padding(4);
			this.pnlConnected.Name = "pnlConnected";
			this.pnlConnected.Size = new System.Drawing.Size(745, 327);
			this.pnlConnected.TabIndex = 66;
			this.pnlConnected.Visible = false;
			// 
			// txtConnectedExitIp
			// 
			this.txtConnectedExitIp.Location = new System.Drawing.Point(235, 170);
			this.txtConnectedExitIp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.txtConnectedExitIp.Name = "txtConnectedExitIp";
			this.txtConnectedExitIp.Size = new System.Drawing.Size(495, 46);
			this.txtConnectedExitIp.TabIndex = 69;
			this.txtConnectedExitIp.Text = "1.2.3.4";
			this.txtConnectedExitIp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblConnectedExitIp
			// 
			this.lblConnectedExitIp.Location = new System.Drawing.Point(15, 170);
			this.lblConnectedExitIp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectedExitIp.Name = "lblConnectedExitIp";
			this.lblConnectedExitIp.Size = new System.Drawing.Size(212, 46);
			this.lblConnectedExitIp.TabIndex = 68;
			this.lblConnectedExitIp.Text = "Public Exit IP:";
			this.lblConnectedExitIp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblConnectedUpload
			// 
			this.lblConnectedUpload.Location = new System.Drawing.Point(390, 110);
			this.lblConnectedUpload.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectedUpload.Name = "lblConnectedUpload";
			this.lblConnectedUpload.Size = new System.Drawing.Size(98, 30);
			this.lblConnectedUpload.TabIndex = 65;
			this.lblConnectedUpload.Text = "Upload:";
			this.lblConnectedUpload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtConnectedUpload
			// 
			this.txtConnectedUpload.BackColor = System.Drawing.Color.White;
			this.txtConnectedUpload.Font = new System.Drawing.Font("Lucida Console", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtConnectedUpload.ForeColor = System.Drawing.Color.ForestGreen;
			this.txtConnectedUpload.Location = new System.Drawing.Point(492, 100);
			this.txtConnectedUpload.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.txtConnectedUpload.Name = "txtConnectedUpload";
			this.txtConnectedUpload.Size = new System.Drawing.Size(236, 60);
			this.txtConnectedUpload.TabIndex = 67;
			this.txtConnectedUpload.Text = "14332 kb/s";
			this.txtConnectedUpload.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// txtConnectedDownload
			// 
			this.txtConnectedDownload.BackColor = System.Drawing.Color.White;
			this.txtConnectedDownload.Font = new System.Drawing.Font("Lucida Console", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtConnectedDownload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(157)))), ((int)(((byte)(255)))));
			this.txtConnectedDownload.Location = new System.Drawing.Point(135, 100);
			this.txtConnectedDownload.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.txtConnectedDownload.Name = "txtConnectedDownload";
			this.txtConnectedDownload.Size = new System.Drawing.Size(236, 60);
			this.txtConnectedDownload.TabIndex = 65;
			this.txtConnectedDownload.Text = "14332 kb/s";
			this.txtConnectedDownload.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblConnectedDownload
			// 
			this.lblConnectedDownload.Location = new System.Drawing.Point(15, 112);
			this.lblConnectedDownload.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectedDownload.Name = "lblConnectedDownload";
			this.lblConnectedDownload.Size = new System.Drawing.Size(112, 30);
			this.lblConnectedDownload.TabIndex = 64;
			this.lblConnectedDownload.Text = "Download:";
			this.lblConnectedDownload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtConnectedSince
			// 
			this.txtConnectedSince.Location = new System.Drawing.Point(235, 222);
			this.txtConnectedSince.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.txtConnectedSince.Name = "txtConnectedSince";
			this.txtConnectedSince.Size = new System.Drawing.Size(495, 36);
			this.txtConnectedSince.TabIndex = 63;
			this.txtConnectedSince.Text = "VPN Time";
			this.txtConnectedSince.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblConnectedSince
			// 
			this.lblConnectedSince.Location = new System.Drawing.Point(15, 222);
			this.lblConnectedSince.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectedSince.Name = "lblConnectedSince";
			this.lblConnectedSince.Size = new System.Drawing.Size(212, 36);
			this.lblConnectedSince.TabIndex = 62;
			this.lblConnectedSince.Text = "Connection time:";
			this.lblConnectedSince.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmdDisconnect
			// 
			this.cmdDisconnect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdDisconnect.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdDisconnect.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdDisconnect.FlatAppearance.BorderSize = 0;
			this.cmdDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdDisconnect.ImageKey = "enter";
			this.cmdDisconnect.Location = new System.Drawing.Point(24, 268);
			this.cmdDisconnect.Margin = new System.Windows.Forms.Padding(0);
			this.cmdDisconnect.Name = "cmdDisconnect";
			this.cmdDisconnect.Size = new System.Drawing.Size(657, 51);
			this.cmdDisconnect.TabIndex = 61;
			this.cmdDisconnect.Text = "Disconnect";
			this.cmdDisconnect.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.cmdDisconnect.UseVisualStyleBackColor = true;
			this.cmdDisconnect.Click += new System.EventHandler(this.cmdDisconnect_Click);
			// 
			// lblConnectedLocation
			// 
			this.lblConnectedLocation.Location = new System.Drawing.Point(249, 63);
			this.lblConnectedLocation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectedLocation.Name = "lblConnectedLocation";
			this.lblConnectedLocation.Size = new System.Drawing.Size(481, 39);
			this.lblConnectedLocation.TabIndex = 3;
			this.lblConnectedLocation.Text = "Location";
			this.lblConnectedLocation.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblConnectedTo
			// 
			this.lblConnectedTo.Location = new System.Drawing.Point(15, 14);
			this.lblConnectedTo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectedTo.Name = "lblConnectedTo";
			this.lblConnectedTo.Size = new System.Drawing.Size(159, 45);
			this.lblConnectedTo.TabIndex = 2;
			this.lblConnectedTo.Text = "Connected to:";
			this.lblConnectedTo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblConnectedServerName
			// 
			this.lblConnectedServerName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.lblConnectedServerName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblConnectedServerName.Location = new System.Drawing.Point(246, 14);
			this.lblConnectedServerName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectedServerName.Name = "lblConnectedServerName";
			this.lblConnectedServerName.Size = new System.Drawing.Size(484, 45);
			this.lblConnectedServerName.TabIndex = 1;
			this.lblConnectedServerName.Text = "Server Name";
			this.lblConnectedServerName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblConnectedCountry
			// 
			this.lblConnectedCountry.BackColor = System.Drawing.Color.Transparent;
			this.lblConnectedCountry.Image = global::Eddie.Forms.Properties.Resources.blacklist_1;
			this.lblConnectedCountry.Location = new System.Drawing.Point(202, 20);
			this.lblConnectedCountry.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectedCountry.Name = "lblConnectedCountry";
			this.lblConnectedCountry.Size = new System.Drawing.Size(33, 30);
			this.lblConnectedCountry.TabIndex = 0;
			this.lblConnectedCountry.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// pnlWelcome
			// 
			this.pnlWelcome.BackColor = System.Drawing.Color.Transparent;
			this.pnlWelcome.Controls.Add(this.cboKey);
			this.pnlWelcome.Controls.Add(this.lblKey);
			this.pnlWelcome.Controls.Add(this.lblConnectSubtitle);
			this.pnlWelcome.Controls.Add(this.cmdLockedNetwork);
			this.pnlWelcome.Controls.Add(this.lblLoginIcon);
			this.pnlWelcome.Controls.Add(this.imgLockedNetwork);
			this.pnlWelcome.Controls.Add(this.cmdLogin);
			this.pnlWelcome.Controls.Add(this.lblPassword);
			this.pnlWelcome.Controls.Add(this.lblLogin);
			this.pnlWelcome.Controls.Add(this.cmdConnect);
			this.pnlWelcome.Controls.Add(this.chkRemember);
			this.pnlWelcome.Controls.Add(this.txtPassword);
			this.pnlWelcome.Controls.Add(this.txtLogin);
			this.pnlWelcome.Location = new System.Drawing.Point(24, 42);
			this.pnlWelcome.Name = "pnlWelcome";
			this.pnlWelcome.Size = new System.Drawing.Size(745, 326);
			this.pnlWelcome.TabIndex = 53;
			this.pnlWelcome.Visible = false;
			// 
			// cboKey
			// 
			this.cboKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboKey.Location = new System.Drawing.Point(254, 129);
			this.cboKey.Margin = new System.Windows.Forms.Padding(4);
			this.cboKey.Name = "cboKey";
			this.cboKey.Size = new System.Drawing.Size(467, 28);
			this.cboKey.TabIndex = 75;
			this.cboKey.SelectedIndexChanged += new System.EventHandler(this.cboKey_SelectedIndexChanged);
			// 
			// lblKey
			// 
			this.lblKey.BackColor = System.Drawing.Color.Transparent;
			this.lblKey.ForeColor = System.Drawing.Color.Black;
			this.lblKey.Location = new System.Drawing.Point(134, 132);
			this.lblKey.Name = "lblKey";
			this.lblKey.Size = new System.Drawing.Size(114, 30);
			this.lblKey.TabIndex = 74;
			this.lblKey.Text = "Device:";
			this.lblKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblConnectSubtitle
			// 
			this.lblConnectSubtitle.Location = new System.Drawing.Point(26, 230);
			this.lblConnectSubtitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectSubtitle.Name = "lblConnectSubtitle";
			this.lblConnectSubtitle.Size = new System.Drawing.Size(695, 27);
			this.lblConnectSubtitle.TabIndex = 73;
			this.lblConnectSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cmdLockedNetwork
			// 
			this.cmdLockedNetwork.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdLockedNetwork.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdLockedNetwork.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdLockedNetwork.FlatAppearance.BorderSize = 0;
			this.cmdLockedNetwork.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdLockedNetwork.ImageKey = "enter";
			this.cmdLockedNetwork.Location = new System.Drawing.Point(93, 268);
			this.cmdLockedNetwork.Margin = new System.Windows.Forms.Padding(0);
			this.cmdLockedNetwork.Name = "cmdLockedNetwork";
			this.cmdLockedNetwork.Size = new System.Drawing.Size(628, 51);
			this.cmdLockedNetwork.TabIndex = 71;
			this.cmdLockedNetwork.Text = "Enter";
			this.cmdLockedNetwork.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.cmdLockedNetwork.UseVisualStyleBackColor = true;
			this.cmdLockedNetwork.Click += new System.EventHandler(this.cmdLockedNetwork_Click);
			// 
			// lblLoginIcon
			// 
			this.lblLoginIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.lblLoginIcon.Image = global::Eddie.Forms.Properties.Resources.login;
			this.lblLoginIcon.Location = new System.Drawing.Point(49, 3);
			this.lblLoginIcon.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLoginIcon.Name = "lblLoginIcon";
			this.lblLoginIcon.Size = new System.Drawing.Size(72, 72);
			this.lblLoginIcon.TabIndex = 70;
			// 
			// imgLockedNetwork
			// 
			this.imgLockedNetwork.Location = new System.Drawing.Point(26, 264);
			this.imgLockedNetwork.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.imgLockedNetwork.Name = "imgLockedNetwork";
			this.imgLockedNetwork.Size = new System.Drawing.Size(62, 57);
			this.imgLockedNetwork.TabIndex = 63;
			this.imgLockedNetwork.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cmdLogin
			// 
			this.cmdLogin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdLogin.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdLogin.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdLogin.FlatAppearance.BorderSize = 0;
			this.cmdLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdLogin.ImageKey = "enter";
			this.cmdLogin.Location = new System.Drawing.Point(594, 3);
			this.cmdLogin.Margin = new System.Windows.Forms.Padding(0);
			this.cmdLogin.Name = "cmdLogin";
			this.cmdLogin.Size = new System.Drawing.Size(126, 72);
			this.cmdLogin.TabIndex = 61;
			this.cmdLogin.Text = "Login";
			this.cmdLogin.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.cmdLogin.UseVisualStyleBackColor = true;
			this.cmdLogin.Click += new System.EventHandler(this.cmdLogin_Click);
			// 
			// lblPassword
			// 
			this.lblPassword.BackColor = System.Drawing.Color.Transparent;
			this.lblPassword.ForeColor = System.Drawing.Color.Black;
			this.lblPassword.Location = new System.Drawing.Point(134, 45);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(196, 30);
			this.lblPassword.TabIndex = 56;
			this.lblPassword.Text = "Password:";
			this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblLogin
			// 
			this.lblLogin.BackColor = System.Drawing.Color.Transparent;
			this.lblLogin.ForeColor = System.Drawing.Color.Black;
			this.lblLogin.Location = new System.Drawing.Point(134, 3);
			this.lblLogin.Name = "lblLogin";
			this.lblLogin.Size = new System.Drawing.Size(196, 30);
			this.lblLogin.TabIndex = 55;
			this.lblLogin.Text = "Username/email:";
			this.lblLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmdConnect
			// 
			this.cmdConnect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdConnect.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdConnect.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdConnect.FlatAppearance.BorderSize = 0;
			this.cmdConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdConnect.ImageKey = "enter";
			this.cmdConnect.Location = new System.Drawing.Point(26, 174);
			this.cmdConnect.Margin = new System.Windows.Forms.Padding(0);
			this.cmdConnect.Name = "cmdConnect";
			this.cmdConnect.Size = new System.Drawing.Size(695, 51);
			this.cmdConnect.TabIndex = 60;
			this.cmdConnect.Text = "Enter";
			this.cmdConnect.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.cmdConnect.UseVisualStyleBackColor = true;
			this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
			// 
			// chkRemember
			// 
			this.chkRemember.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkRemember.AutoSize = true;
			this.chkRemember.Location = new System.Drawing.Point(510, 90);
			this.chkRemember.Name = "chkRemember";
			this.chkRemember.Size = new System.Drawing.Size(114, 24);
			this.chkRemember.TabIndex = 52;
			this.chkRemember.Text = "Remember";
			this.chkRemember.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.chkRemember.CheckedChanged += new System.EventHandler(this.chkRemember_CheckedChanged);
			// 
			// txtPassword
			// 
			this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPassword.Location = new System.Drawing.Point(342, 45);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(241, 26);
			this.txtPassword.TabIndex = 51;
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			// 
			// txtLogin
			// 
			this.txtLogin.Location = new System.Drawing.Point(342, 3);
			this.txtLogin.Name = "txtLogin";
			this.txtLogin.Size = new System.Drawing.Size(240, 26);
			this.txtLogin.TabIndex = 50;
			this.txtLogin.TextChanged += new System.EventHandler(this.txtLogin_TextChanged);
			// 
			// pnlWaiting
			// 
			this.pnlWaiting.BackColor = System.Drawing.Color.Transparent;
			this.pnlWaiting.Controls.Add(this.lblWait2);
			this.pnlWaiting.Controls.Add(this.lblWait1);
			this.pnlWaiting.Controls.Add(this.cmdCancel);
			this.pnlWaiting.Location = new System.Drawing.Point(111, 332);
			this.pnlWaiting.Margin = new System.Windows.Forms.Padding(4);
			this.pnlWaiting.Name = "pnlWaiting";
			this.pnlWaiting.Size = new System.Drawing.Size(369, 110);
			this.pnlWaiting.TabIndex = 65;
			this.pnlWaiting.Visible = false;
			// 
			// lblWait2
			// 
			this.lblWait2.BackColor = System.Drawing.Color.Transparent;
			this.lblWait2.Location = new System.Drawing.Point(22, 60);
			this.lblWait2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblWait2.Name = "lblWait2";
			this.lblWait2.Size = new System.Drawing.Size(254, 32);
			this.lblWait2.TabIndex = 64;
			this.lblWait2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblWait1
			// 
			this.lblWait1.Location = new System.Drawing.Point(4, 0);
			this.lblWait1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblWait1.Name = "lblWait1";
			this.lblWait1.Size = new System.Drawing.Size(118, 38);
			this.lblWait1.TabIndex = 63;
			this.lblWait1.Text = "label1";
			this.lblWait1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// cmdCancel
			// 
			this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdCancel.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdCancel.FlatAppearance.BorderSize = 0;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdCancel.ImageKey = "enter";
			this.cmdCancel.Location = new System.Drawing.Point(132, 42);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(111, 51);
			this.cmdCancel.TabIndex = 61;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// tabProviders
			// 
			this.tabProviders.Controls.Add(this.cmdProviderEdit);
			this.tabProviders.Controls.Add(this.cmdProviderRemove);
			this.tabProviders.Controls.Add(this.cmdProviderAdd);
			this.tabProviders.Controls.Add(this.lstProviders);
			this.tabProviders.Location = new System.Drawing.Point(4, 29);
			this.tabProviders.Margin = new System.Windows.Forms.Padding(4);
			this.tabProviders.Name = "tabProviders";
			this.tabProviders.Size = new System.Drawing.Size(1099, 505);
			this.tabProviders.TabIndex = 7;
			this.tabProviders.Text = "Providers";
			this.tabProviders.UseVisualStyleBackColor = true;
			// 
			// cmdProviderEdit
			// 
			this.cmdProviderEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdProviderEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdProviderEdit.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdProviderEdit.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdProviderEdit.FlatAppearance.BorderSize = 0;
			this.cmdProviderEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdProviderEdit.Image = global::Eddie.Forms.Properties.Resources.edit;
			this.cmdProviderEdit.Location = new System.Drawing.Point(1042, 108);
			this.cmdProviderEdit.Margin = new System.Windows.Forms.Padding(4);
			this.cmdProviderEdit.Name = "cmdProviderEdit";
			this.cmdProviderEdit.Size = new System.Drawing.Size(42, 42);
			this.cmdProviderEdit.TabIndex = 50;
			this.cmdProviderEdit.UseVisualStyleBackColor = true;
			this.cmdProviderEdit.Click += new System.EventHandler(this.cmdProviderEdit_Click);
			// 
			// cmdProviderRemove
			// 
			this.cmdProviderRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdProviderRemove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdProviderRemove.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdProviderRemove.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdProviderRemove.FlatAppearance.BorderSize = 0;
			this.cmdProviderRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdProviderRemove.Image = global::Eddie.Forms.Properties.Resources.delete;
			this.cmdProviderRemove.Location = new System.Drawing.Point(1042, 57);
			this.cmdProviderRemove.Margin = new System.Windows.Forms.Padding(4);
			this.cmdProviderRemove.Name = "cmdProviderRemove";
			this.cmdProviderRemove.Size = new System.Drawing.Size(42, 42);
			this.cmdProviderRemove.TabIndex = 49;
			this.cmdProviderRemove.UseVisualStyleBackColor = true;
			this.cmdProviderRemove.Click += new System.EventHandler(this.cmdProviderRemove_Click);
			// 
			// cmdProviderAdd
			// 
			this.cmdProviderAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdProviderAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdProviderAdd.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdProviderAdd.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdProviderAdd.FlatAppearance.BorderSize = 0;
			this.cmdProviderAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdProviderAdd.Image = global::Eddie.Forms.Properties.Resources.add;
			this.cmdProviderAdd.Location = new System.Drawing.Point(1042, 6);
			this.cmdProviderAdd.Margin = new System.Windows.Forms.Padding(4);
			this.cmdProviderAdd.Name = "cmdProviderAdd";
			this.cmdProviderAdd.Size = new System.Drawing.Size(42, 42);
			this.cmdProviderAdd.TabIndex = 48;
			this.cmdProviderAdd.UseVisualStyleBackColor = true;
			this.cmdProviderAdd.Click += new System.EventHandler(this.cmdProviderAdd_Click);
			// 
			// lstProviders
			// 
			this.lstProviders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstProviders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colProviderName,
            this.colProviderProvider,
            this.colProviderDescription,
            this.colProviderWebsite,
            this.colProviderPath});
			this.lstProviders.FullRowSelect = true;
			this.lstProviders.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstProviders.HideSelection = false;
			this.lstProviders.LargeImageList = this.imgProviders;
			this.lstProviders.Location = new System.Drawing.Point(6, 6);
			this.lstProviders.Margin = new System.Windows.Forms.Padding(4);
			this.lstProviders.MultiSelect = false;
			this.lstProviders.Name = "lstProviders";
			this.lstProviders.OwnerDraw = true;
			this.lstProviders.Size = new System.Drawing.Size(1028, 486);
			this.lstProviders.SmallImageList = this.imgProviders;
			this.lstProviders.TabIndex = 47;
			this.lstProviders.UseCompatibleStateImageBehavior = false;
			this.lstProviders.View = System.Windows.Forms.View.Details;
			this.lstProviders.SelectedIndexChanged += new System.EventHandler(this.lstProviders_SelectedIndexChanged);
			this.lstProviders.DoubleClick += new System.EventHandler(this.lstProviders_DoubleClick);
			// 
			// colProviderName
			// 
			this.colProviderName.Text = "Title";
			// 
			// colProviderProvider
			// 
			this.colProviderProvider.Text = "Provider";
			// 
			// colProviderDescription
			// 
			this.colProviderDescription.Text = "Provider Description";
			// 
			// colProviderWebsite
			// 
			this.colProviderWebsite.Text = "Website";
			// 
			// colProviderPath
			// 
			this.colProviderPath.Text = "Path";
			// 
			// imgProviders
			// 
			this.imgProviders.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgProviders.ImageStream")));
			this.imgProviders.TransparentColor = System.Drawing.Color.Transparent;
			this.imgProviders.Images.SetKeyName(0, "off");
			this.imgProviders.Images.SetKeyName(1, "on");
			// 
			// tabServers
			// 
			this.tabServers.BackColor = System.Drawing.Color.Transparent;
			this.tabServers.Controls.Add(this.cmdServersMore);
			this.tabServers.Controls.Add(this.cmdServersRename);
			this.tabServers.Controls.Add(this.chkShowAll);
			this.tabServers.Controls.Add(this.cmdServersRefresh);
			this.tabServers.Controls.Add(this.lblScoreType);
			this.tabServers.Controls.Add(this.cboScoreType);
			this.tabServers.Controls.Add(this.chkLockLast);
			this.tabServers.Controls.Add(this.pnlServers);
			this.tabServers.Controls.Add(this.cmdServersUndefined);
			this.tabServers.Controls.Add(this.cmdServersBlackList);
			this.tabServers.Controls.Add(this.cmdServersWhiteList);
			this.tabServers.Controls.Add(this.cmdServersConnect);
			this.tabServers.Location = new System.Drawing.Point(4, 29);
			this.tabServers.Name = "tabServers";
			this.tabServers.Padding = new System.Windows.Forms.Padding(3);
			this.tabServers.Size = new System.Drawing.Size(1099, 505);
			this.tabServers.TabIndex = 0;
			this.tabServers.Text = "Servers";
			// 
			// cmdServersMore
			// 
			this.cmdServersMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdServersMore.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdServersMore.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdServersMore.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdServersMore.FlatAppearance.BorderSize = 0;
			this.cmdServersMore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdServersMore.Image = global::Eddie.Forms.Properties.Resources.more;
			this.cmdServersMore.Location = new System.Drawing.Point(1042, 278);
			this.cmdServersMore.Name = "cmdServersMore";
			this.cmdServersMore.Size = new System.Drawing.Size(42, 42);
			this.cmdServersMore.TabIndex = 69;
			this.cmdServersMore.UseVisualStyleBackColor = true;
			this.cmdServersMore.Click += new System.EventHandler(this.cmdServersMore_Click);
			// 
			// cmdServersRename
			// 
			this.cmdServersRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdServersRename.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdServersRename.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdServersRename.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdServersRename.FlatAppearance.BorderSize = 0;
			this.cmdServersRename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdServersRename.Image = global::Eddie.Forms.Properties.Resources.rename;
			this.cmdServersRename.Location = new System.Drawing.Point(1042, 230);
			this.cmdServersRename.Name = "cmdServersRename";
			this.cmdServersRename.Size = new System.Drawing.Size(42, 42);
			this.cmdServersRename.TabIndex = 68;
			this.cmdServersRename.UseVisualStyleBackColor = true;
			this.cmdServersRename.Click += new System.EventHandler(this.cmdServersRename_Click);
			// 
			// chkShowAll
			// 
			this.chkShowAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkShowAll.Location = new System.Drawing.Point(6, 456);
			this.chkShowAll.Name = "chkShowAll";
			this.chkShowAll.Size = new System.Drawing.Size(198, 38);
			this.chkShowAll.TabIndex = 7;
			this.chkShowAll.Text = "Show All";
			this.chkShowAll.UseVisualStyleBackColor = true;
			this.chkShowAll.CheckedChanged += new System.EventHandler(this.chkShowAll_CheckedChanged);
			// 
			// lblScoreType
			// 
			this.lblScoreType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lblScoreType.Location = new System.Drawing.Point(378, 456);
			this.lblScoreType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblScoreType.Name = "lblScoreType";
			this.lblScoreType.Size = new System.Drawing.Size(254, 32);
			this.lblScoreType.TabIndex = 65;
			this.lblScoreType.Text = "Scoring Rule:";
			this.lblScoreType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// pnlServers
			// 
			this.pnlServers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlServers.BackColor = System.Drawing.Color.Maroon;
			this.pnlServers.Location = new System.Drawing.Point(6, 6);
			this.pnlServers.Margin = new System.Windows.Forms.Padding(0);
			this.pnlServers.Name = "pnlServers";
			this.pnlServers.Size = new System.Drawing.Size(1029, 440);
			this.pnlServers.TabIndex = 47;
			// 
			// tabCountries
			// 
			this.tabCountries.Controls.Add(this.cmdAreasWhiteList);
			this.tabCountries.Controls.Add(this.pnlAreas);
			this.tabCountries.Controls.Add(this.cmdAreasUndefined);
			this.tabCountries.Controls.Add(this.cmdAreasBlackList);
			this.tabCountries.Location = new System.Drawing.Point(4, 29);
			this.tabCountries.Margin = new System.Windows.Forms.Padding(4);
			this.tabCountries.Name = "tabCountries";
			this.tabCountries.Size = new System.Drawing.Size(1099, 505);
			this.tabCountries.TabIndex = 6;
			this.tabCountries.Text = "Countries";
			this.tabCountries.UseVisualStyleBackColor = true;
			// 
			// pnlAreas
			// 
			this.pnlAreas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlAreas.BackColor = System.Drawing.Color.Maroon;
			this.pnlAreas.Location = new System.Drawing.Point(6, 6);
			this.pnlAreas.Margin = new System.Windows.Forms.Padding(0);
			this.pnlAreas.Name = "pnlAreas";
			this.pnlAreas.Size = new System.Drawing.Size(1029, 488);
			this.pnlAreas.TabIndex = 60;
			// 
			// tabSpeed
			// 
			this.tabSpeed.Controls.Add(this.lblSpeedResolution);
			this.tabSpeed.Controls.Add(this.holSpeedChart);
			this.tabSpeed.Controls.Add(this.cboSpeedResolution);
			this.tabSpeed.Location = new System.Drawing.Point(4, 29);
			this.tabSpeed.Margin = new System.Windows.Forms.Padding(4);
			this.tabSpeed.Name = "tabSpeed";
			this.tabSpeed.Size = new System.Drawing.Size(1099, 505);
			this.tabSpeed.TabIndex = 5;
			this.tabSpeed.Text = "Speed";
			this.tabSpeed.UseVisualStyleBackColor = true;
			// 
			// lblSpeedResolution
			// 
			this.lblSpeedResolution.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblSpeedResolution.Location = new System.Drawing.Point(4, 456);
			this.lblSpeedResolution.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSpeedResolution.Name = "lblSpeedResolution";
			this.lblSpeedResolution.Size = new System.Drawing.Size(225, 32);
			this.lblSpeedResolution.TabIndex = 65;
			this.lblSpeedResolution.Text = "Resolution:";
			this.lblSpeedResolution.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// holSpeedChart
			// 
			this.holSpeedChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.holSpeedChart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.holSpeedChart.Location = new System.Drawing.Point(3, 3);
			this.holSpeedChart.Margin = new System.Windows.Forms.Padding(0);
			this.holSpeedChart.Name = "holSpeedChart";
			this.holSpeedChart.Size = new System.Drawing.Size(1086, 447);
			this.holSpeedChart.TabIndex = 51;
			this.holSpeedChart.Text = "Speed Chart";
			this.holSpeedChart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cboSpeedResolution
			// 
			this.cboSpeedResolution.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboSpeedResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboSpeedResolution.FormattingEnabled = true;
			this.cboSpeedResolution.Location = new System.Drawing.Point(238, 456);
			this.cboSpeedResolution.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
			this.cboSpeedResolution.Name = "cboSpeedResolution";
			this.cboSpeedResolution.Size = new System.Drawing.Size(848, 28);
			this.cboSpeedResolution.TabIndex = 50;
			this.cboSpeedResolution.SelectedIndexChanged += new System.EventHandler(this.cboSpeedResolution_SelectedIndexChanged);
			// 
			// tabStats
			// 
			this.tabStats.BackColor = System.Drawing.Color.Transparent;
			this.tabStats.Controls.Add(this.lstStats);
			this.tabStats.Location = new System.Drawing.Point(4, 29);
			this.tabStats.Name = "tabStats";
			this.tabStats.Size = new System.Drawing.Size(1099, 505);
			this.tabStats.TabIndex = 2;
			this.tabStats.Text = "Stats";
			// 
			// lstStats
			// 
			this.lstStats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstStats.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            colStatsKey,
            colStatsValue});
			this.lstStats.FullRowSelect = true;
			this.lstStats.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.lstStats.HideSelection = false;
			this.lstStats.Location = new System.Drawing.Point(6, 6);
			this.lstStats.Margin = new System.Windows.Forms.Padding(0);
			this.lstStats.MultiSelect = false;
			this.lstStats.Name = "lstStats";
			this.lstStats.OwnerDraw = true;
			this.lstStats.ShowItemToolTips = true;
			this.lstStats.Size = new System.Drawing.Size(1080, 487);
			this.lstStats.TabIndex = 1;
			this.lstStats.UseCompatibleStateImageBehavior = false;
			this.lstStats.View = System.Windows.Forms.View.Details;
			this.lstStats.DoubleClick += new System.EventHandler(this.lstStats_DoubleClick);
			// 
			// tabLogs
			// 
			this.tabLogs.BackColor = System.Drawing.Color.Transparent;
			this.tabLogs.Controls.Add(this.cmdLogsCommand);
			this.tabLogs.Controls.Add(this.cmdLogsSupport);
			this.tabLogs.Controls.Add(this.lstLogs);
			this.tabLogs.Controls.Add(this.cmdLogsClean);
			this.tabLogs.Controls.Add(this.cmdLogsCopy);
			this.tabLogs.Controls.Add(this.cmdLogsSave);
			this.tabLogs.Location = new System.Drawing.Point(4, 29);
			this.tabLogs.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
			this.tabLogs.Name = "tabLogs";
			this.tabLogs.Size = new System.Drawing.Size(1099, 505);
			this.tabLogs.TabIndex = 3;
			this.tabLogs.Text = "Logs";
			// 
			// cmdLogsCommand
			// 
			this.cmdLogsCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdLogsCommand.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdLogsCommand.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdLogsCommand.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdLogsCommand.FlatAppearance.BorderSize = 0;
			this.cmdLogsCommand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdLogsCommand.Image = global::Eddie.Forms.Properties.Resources.command;
			this.cmdLogsCommand.Location = new System.Drawing.Point(1042, 450);
			this.cmdLogsCommand.Name = "cmdLogsCommand";
			this.cmdLogsCommand.Size = new System.Drawing.Size(42, 42);
			this.cmdLogsCommand.TabIndex = 53;
			this.cmdLogsCommand.UseVisualStyleBackColor = true;
			this.cmdLogsCommand.Click += new System.EventHandler(this.cmdLogsCommand_Click);
			// 
			// lstLogs
			// 
			this.lstLogs.AllowColumnReorder = true;
			this.lstLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstLogs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            colIcon,
            colDate,
            colMessage});
			this.lstLogs.ContextMenuStrip = this.mnuLogsContext;
			this.lstLogs.FullRowSelect = true;
			this.lstLogs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstLogs.HideSelection = false;
			this.lstLogs.LabelWrap = false;
			this.lstLogs.Location = new System.Drawing.Point(6, 6);
			this.lstLogs.Margin = new System.Windows.Forms.Padding(0);
			this.lstLogs.Name = "lstLogs";
			this.lstLogs.OwnerDraw = true;
			this.lstLogs.ShowItemToolTips = true;
			this.lstLogs.Size = new System.Drawing.Size(1028, 484);
			this.lstLogs.TabIndex = 49;
			this.lstLogs.UseCompatibleStateImageBehavior = false;
			this.lstLogs.View = System.Windows.Forms.View.Details;
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.Fuchsia;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.ClientSize = new System.Drawing.Size(1107, 538);
			this.Controls.Add(this.tabMain);
			this.Name = "Main";
			this.Text = "AirVPN";
			this.mnuMain.ResumeLayout(false);
			this.mnuLogsContext.ResumeLayout(false);
			this.mnuServers.ResumeLayout(false);
			this.mnuAreas.ResumeLayout(false);
			this.tabMain.ResumeLayout(false);
			this.tabOverview.ResumeLayout(false);
			this.pnlConnected.ResumeLayout(false);
			this.pnlWelcome.ResumeLayout(false);
			this.pnlWelcome.PerformLayout();
			this.pnlWaiting.ResumeLayout(false);
			this.tabProviders.ResumeLayout(false);
			this.tabServers.ResumeLayout(false);
			this.tabCountries.ResumeLayout(false);
			this.tabSpeed.ResumeLayout(false);
			this.tabStats.ResumeLayout(false);
			this.tabLogs.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem mnuRestore;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
		private System.Windows.Forms.ToolStripMenuItem mnuSettings;
        private System.Windows.Forms.ToolStripMenuItem mnuHomePage;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuPorts;
        private System.Windows.Forms.ToolStripMenuItem mnuUser;
        public System.Windows.Forms.ImageList imgCountries;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
		private Skin.Panel pnlWelcome;
		private Skin.Label lblPassword;
		private Skin.Label lblLogin;
		private Skin.CheckBox chkRemember;
		private Skin.TextBox txtPassword;
		public Skin.TextBox txtLogin;
		private Skin.Button cmdCancel;
		private Skin.Button cmdConnect;
		private Skin.TabControl tabMain;
		private Skin.TabPage tabServers;
		private Skin.ComboBox cboScoreType;
		private Skin.CheckBox chkShowAll;
		private Skin.CheckBox chkLockLast;
		private Skin.Button cmdServersUndefined;
		private Skin.Button cmdServersBlackList;
		private Skin.Button cmdServersWhiteList;
		private Skin.Button cmdServersConnect;
		private Skin.TabPage tabStats;
		private Skin.ListView lstStats;
		private Skin.TabPage tabLogs;
		private Skin.ListView lstLogs;
		private Skin.Button cmdLogsClean;
		private Skin.Button cmdLogsCopy;
		private Skin.Button cmdLogsSave;
		private System.Windows.Forms.ContextMenuStrip mnuLogsContext;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ContextMenuStrip mnuServers;
		private System.Windows.Forms.ToolStripMenuItem mnuServersConnect;
		private System.Windows.Forms.ToolStripSeparator mnuServersSeparator1;
		private System.Windows.Forms.ToolStripMenuItem mnuServersWhiteList;
		private System.Windows.Forms.ToolStripMenuItem mnuServersBlackList;
		private System.Windows.Forms.ToolStripMenuItem mnuServersUndefined;
		private System.Windows.Forms.ContextMenuStrip mnuAreas;
		private System.Windows.Forms.ToolStripMenuItem mnuAreasWhiteList;
		private System.Windows.Forms.ToolStripMenuItem mnuAreasBlackList;
		private System.Windows.Forms.ToolStripMenuItem mnuAreasUndefined;
		private Skin.TabPage tabOverview;
		private Skin.TabPage tabSpeed;
		private Skin.ComboBox cboSpeedResolution;
		private System.Windows.Forms.Label holSpeedChart;
		private Skin.Button cmdLogin;
		private Skin.Panel pnlWaiting;
		private Skin.Label lblWait2;
		private Skin.Label lblWait1;
		private Skin.Button cmdLogsSupport;
		private Skin.Label lblSpeedResolution;
		private Skin.Label lblScoreType;
		private Skin.Panel pnlConnected;
		private Skin.Label txtConnectedExitIp;
		private Skin.Label lblConnectedExitIp;
		private Skin.Label lblConnectedUpload;
		private System.Windows.Forms.Label txtConnectedUpload;
		private System.Windows.Forms.Label txtConnectedDownload;
		private Skin.Label lblConnectedDownload;
		private Skin.Label txtConnectedSince;
		private Skin.Label lblConnectedSince;
		private Skin.Button cmdDisconnect;
		private Skin.Label lblConnectedLocation;
		private Skin.Label lblConnectedTo;
		private Skin.Label lblConnectedServerName;
		private Skin.Label lblConnectedCountry;
		private Skin.Label imgLockedNetwork;
		private Skin.Label lblLoginIcon;
		private System.Windows.Forms.ToolStripMenuItem mnuStatus;
		private System.Windows.Forms.ToolStripMenuItem mnuConnect;
		private System.Windows.Forms.ToolStripSeparator mnuRestoreSep;
		private System.Windows.Forms.ToolStripSeparator mnuSeparator3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private Skin.Button cmdLockedNetwork;
		private Skin.Label lblConnectSubtitle;
		private Skin.Button cmdServersRefresh;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem mnuServersRefresh;
        private System.Windows.Forms.ComboBox cboKey;
        private Skin.Label lblKey;
        private Skin.TabPage tabCountries;
        private Skin.Button cmdAreasUndefined;
        private Skin.Button cmdAreasBlackList;
        private Skin.Panel pnlAreas;
        private Skin.Panel pnlServers;
        private Skin.Button cmdAreasWhiteList;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem mnuServersRename;
		private Skin.Button cmdServersRename;
		private System.Windows.Forms.ToolStripMenuItem mnuServersMore;
		private Skin.Button cmdServersMore;
		private System.Windows.Forms.TabPage tabProviders;
		private Skin.Button cmdProviderEdit;
		private Skin.Button cmdProviderRemove;
		private Skin.Button cmdProviderAdd;
		private Skin.ListView lstProviders;
		private System.Windows.Forms.ColumnHeader colProviderName;
		private System.Windows.Forms.ColumnHeader colProviderDescription;
		private System.Windows.Forms.ColumnHeader colProviderWebsite;
		private System.Windows.Forms.ColumnHeader colProviderPath;
		private System.Windows.Forms.ImageList imgProviders;
		private System.Windows.Forms.ColumnHeader colProviderProvider;
		private Skin.Button cmdLogsCommand;
		private System.Windows.Forms.ToolStripMenuItem mnuUpdater;
	}
}

