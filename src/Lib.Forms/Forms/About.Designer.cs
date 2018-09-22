namespace Eddie.Forms.Forms
{
    partial class About
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
			this.lblVersion = new Eddie.Forms.Skin.Label();
			this.cmdClose = new Eddie.Forms.Skin.Button();
			this.lblLibraries = new Eddie.Forms.Skin.Label();
			this.lnkLibraries = new Eddie.Forms.Skin.LinkLabel();
			this.lblLicense = new Eddie.Forms.Skin.Label();
			this.lnkLicense = new Eddie.Forms.Skin.LinkLabel();
			this.lblSources = new Eddie.Forms.Skin.Label();
			this.lnkSources = new Eddie.Forms.Skin.LinkLabel();
			this.lblManual = new Eddie.Forms.Skin.Label();
			this.lblWebsite = new Eddie.Forms.Skin.Label();
			this.lnkManual = new Eddie.Forms.Skin.LinkLabel();
			this.lnkWebsite = new Eddie.Forms.Skin.LinkLabel();
			this.lblDeveloped = new Eddie.Forms.Skin.Label();
			this.cmdSystemReport = new Eddie.Forms.Skin.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.lnkAirVPN = new Eddie.Forms.Skin.LinkLabel();
			this.lblThanks = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// lblVersion
			// 
			this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblVersion.BackColor = System.Drawing.Color.Transparent;
			this.lblVersion.ForeColor = System.Drawing.Color.White;
			this.lblVersion.Location = new System.Drawing.Point(756, 50);
			this.lblVersion.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(396, 39);
			this.lblVersion.TabIndex = 58;
			this.lblVersion.Text = "v2.0";
			this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmdClose
			// 
			this.cmdClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdClose.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdClose.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdClose.FlatAppearance.BorderSize = 0;
			this.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdClose.Location = new System.Drawing.Point(852, 585);
			this.cmdClose.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.cmdClose.Name = "cmdClose";
			this.cmdClose.Size = new System.Drawing.Size(300, 56);
			this.cmdClose.TabIndex = 0;
			this.cmdClose.Text = "Close";
			this.cmdClose.UseVisualStyleBackColor = true;
			this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
			// 
			// lblLibraries
			// 
			this.lblLibraries.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lblLibraries.BackColor = System.Drawing.Color.Transparent;
			this.lblLibraries.Location = new System.Drawing.Point(459, 394);
			this.lblLibraries.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lblLibraries.Name = "lblLibraries";
			this.lblLibraries.Size = new System.Drawing.Size(254, 42);
			this.lblLibraries.TabIndex = 69;
			this.lblLibraries.Text = "Libraries and tools:";
			this.lblLibraries.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lnkLibraries
			// 
			this.lnkLibraries.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lnkLibraries.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(188)))), ((int)(((byte)(253)))));
			this.lnkLibraries.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkLibraries.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkLibraries.Location = new System.Drawing.Point(724, 394);
			this.lnkLibraries.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lnkLibraries.Name = "lnkLibraries";
			this.lnkLibraries.Size = new System.Drawing.Size(432, 42);
			this.lnkLibraries.TabIndex = 68;
			this.lnkLibraries.TabStop = true;
			this.lnkLibraries.Text = "OpenVPN, stunnel, TunTap OS X, LZO";
			this.lnkLibraries.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lnkLibraries.Click += new System.EventHandler(this.lnkLibraries_LinkClicked);
			// 
			// lblLicense
			// 
			this.lblLicense.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lblLicense.BackColor = System.Drawing.Color.Transparent;
			this.lblLicense.Location = new System.Drawing.Point(459, 339);
			this.lblLicense.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lblLicense.Name = "lblLicense";
			this.lblLicense.Size = new System.Drawing.Size(254, 42);
			this.lblLicense.TabIndex = 67;
			this.lblLicense.Text = "License:";
			this.lblLicense.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lnkLicense
			// 
			this.lnkLicense.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lnkLicense.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(188)))), ((int)(((byte)(253)))));
			this.lnkLicense.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkLicense.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkLicense.Location = new System.Drawing.Point(724, 339);
			this.lnkLicense.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lnkLicense.Name = "lnkLicense";
			this.lnkLicense.Size = new System.Drawing.Size(432, 42);
			this.lnkLicense.TabIndex = 66;
			this.lnkLicense.TabStop = true;
			this.lnkLicense.Text = "GNU General Public License v3";
			this.lnkLicense.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lnkLicense.Click += new System.EventHandler(this.lnkLicense_LinkClicked);
			// 
			// lblSources
			// 
			this.lblSources.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lblSources.BackColor = System.Drawing.Color.Transparent;
			this.lblSources.Location = new System.Drawing.Point(459, 284);
			this.lblSources.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lblSources.Name = "lblSources";
			this.lblSources.Size = new System.Drawing.Size(254, 42);
			this.lblSources.TabIndex = 65;
			this.lblSources.Text = "Sources:";
			this.lblSources.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lnkSources
			// 
			this.lnkSources.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lnkSources.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(188)))), ((int)(((byte)(253)))));
			this.lnkSources.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkSources.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkSources.Location = new System.Drawing.Point(724, 284);
			this.lnkSources.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lnkSources.Name = "lnkSources";
			this.lnkSources.Size = new System.Drawing.Size(432, 42);
			this.lnkSources.TabIndex = 64;
			this.lnkSources.TabStop = true;
			this.lnkSources.Text = "https://github.com/AirVPN/Eddie";
			this.lnkSources.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lnkSources.Click += new System.EventHandler(this.lnkSources_LinkClicked);
			// 
			// lblManual
			// 
			this.lblManual.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lblManual.BackColor = System.Drawing.Color.Transparent;
			this.lblManual.Location = new System.Drawing.Point(459, 228);
			this.lblManual.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lblManual.Name = "lblManual";
			this.lblManual.Size = new System.Drawing.Size(254, 42);
			this.lblManual.TabIndex = 63;
			this.lblManual.Text = "Manual:";
			this.lblManual.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblWebsite
			// 
			this.lblWebsite.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lblWebsite.BackColor = System.Drawing.Color.Transparent;
			this.lblWebsite.Location = new System.Drawing.Point(459, 172);
			this.lblWebsite.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lblWebsite.Name = "lblWebsite";
			this.lblWebsite.Size = new System.Drawing.Size(254, 42);
			this.lblWebsite.TabIndex = 62;
			this.lblWebsite.Text = "Website: ";
			this.lblWebsite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lnkManual
			// 
			this.lnkManual.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lnkManual.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(188)))), ((int)(((byte)(253)))));
			this.lnkManual.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkManual.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkManual.Location = new System.Drawing.Point(724, 228);
			this.lnkManual.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lnkManual.Name = "lnkManual";
			this.lnkManual.Size = new System.Drawing.Size(432, 42);
			this.lnkManual.TabIndex = 61;
			this.lnkManual.TabStop = true;
			this.lnkManual.Text = "https://eddie.website/docs";
			this.lnkManual.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lnkManual.Click += new System.EventHandler(this.lnkManual_LinkClicked);
			// 
			// lnkWebsite
			// 
			this.lnkWebsite.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lnkWebsite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(188)))), ((int)(((byte)(253)))));
			this.lnkWebsite.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkWebsite.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkWebsite.Location = new System.Drawing.Point(724, 172);
			this.lnkWebsite.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lnkWebsite.Name = "lnkWebsite";
			this.lnkWebsite.Size = new System.Drawing.Size(432, 42);
			this.lnkWebsite.TabIndex = 60;
			this.lnkWebsite.TabStop = true;
			this.lnkWebsite.Text = "https://eddie.website";
			this.lnkWebsite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lnkWebsite.Click += new System.EventHandler(this.lnkWebsite_LinkClicked);
			// 
			// lblDeveloped
			// 
			this.lblDeveloped.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lblDeveloped.BackColor = System.Drawing.Color.Transparent;
			this.lblDeveloped.Location = new System.Drawing.Point(20, 129);
			this.lblDeveloped.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lblDeveloped.Name = "lblDeveloped";
			this.lblDeveloped.Size = new System.Drawing.Size(417, 46);
			this.lblDeveloped.TabIndex = 72;
			this.lblDeveloped.Text = "Developed by:";
			this.lblDeveloped.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cmdSystemReport
			// 
			this.cmdSystemReport.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.cmdSystemReport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdSystemReport.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdSystemReport.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdSystemReport.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdSystemReport.FlatAppearance.BorderSize = 0;
			this.cmdSystemReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdSystemReport.Location = new System.Drawing.Point(723, 448);
			this.cmdSystemReport.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.cmdSystemReport.Name = "cmdSystemReport";
			this.cmdSystemReport.Size = new System.Drawing.Size(434, 36);
			this.cmdSystemReport.TabIndex = 73;
			this.cmdSystemReport.Text = "System Report";
			this.cmdSystemReport.UseVisualStyleBackColor = true;
			this.cmdSystemReport.Click += new System.EventHandler(this.cmdSystemReport_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.BackgroundImage = global::Eddie.Forms.Properties.Resources.about_airvpn;
			this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.pictureBox1.Location = new System.Drawing.Point(20, 172);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(417, 220);
			this.pictureBox1.TabIndex = 74;
			this.pictureBox1.TabStop = false;
			// 
			// lnkAirVPN
			// 
			this.lnkAirVPN.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lnkAirVPN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(188)))), ((int)(((byte)(253)))));
			this.lnkAirVPN.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lnkAirVPN.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lnkAirVPN.Location = new System.Drawing.Point(20, 393);
			this.lnkAirVPN.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lnkAirVPN.Name = "lnkAirVPN";
			this.lnkAirVPN.Size = new System.Drawing.Size(417, 42);
			this.lnkAirVPN.TabIndex = 77;
			this.lnkAirVPN.TabStop = true;
			this.lnkAirVPN.Text = "https://airvpn.org";
			this.lnkAirVPN.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lnkAirVPN.Click += new System.EventHandler(this.lnkAirVPN_Click);
			// 
			// lblThanks
			// 
			this.lblThanks.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lblThanks.BackColor = System.Drawing.Color.Transparent;
			this.lblThanks.ForeColor = System.Drawing.Color.Black;
			this.lblThanks.Location = new System.Drawing.Point(24, 448);
			this.lblThanks.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblThanks.Name = "lblThanks";
			this.lblThanks.Size = new System.Drawing.Size(412, 192);
			this.lblThanks.TabIndex = 78;
			this.lblThanks.Text = resources.GetString("lblThanks.Text");
			this.lblThanks.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// About
			// 
			this.AcceptButton = this.cmdClose;
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdClose;
			this.ClientSize = new System.Drawing.Size(1176, 660);
			this.Controls.Add(this.lblThanks);
			this.Controls.Add(this.lnkAirVPN);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.cmdSystemReport);
			this.Controls.Add(this.lblDeveloped);
			this.Controls.Add(this.lblLibraries);
			this.Controls.Add(this.lnkLibraries);
			this.Controls.Add(this.lblLicense);
			this.Controls.Add(this.lnkLicense);
			this.Controls.Add(this.lblSources);
			this.Controls.Add(this.lnkSources);
			this.Controls.Add(this.lblManual);
			this.Controls.Add(this.lblWebsite);
			this.Controls.Add(this.lnkManual);
			this.Controls.Add(this.lnkWebsite);
			this.Controls.Add(this.lblVersion);
			this.Controls.Add(this.cmdClose);
			this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.MinimizeBox = false;
			this.Name = "About";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Skin.Button cmdClose;
        private Skin.Label lblVersion;
        private Skin.Label lblLibraries;
        private Skin.LinkLabel lnkLibraries;
        private Skin.Label lblLicense;
        private Skin.LinkLabel lnkLicense;
        private Skin.Label lblSources;
        private Skin.LinkLabel lnkSources;
        private Skin.Label lblManual;
        private Skin.Label lblWebsite;
        private Skin.LinkLabel lnkManual;
        private Skin.LinkLabel lnkWebsite;
        private Skin.Label lblDeveloped;
		private Skin.Button cmdSystemReport;
		private System.Windows.Forms.PictureBox pictureBox1;
		private Skin.LinkLabel lnkAirVPN;
		private System.Windows.Forms.Label lblThanks;
	}
}