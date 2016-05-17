namespace Eddie.Gui.Forms
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
            this.lblVersion = new Eddie.Gui.Skin.Label();
            this.cmdClose = new Eddie.Gui.Skin.Button();
            this.lblLibraries = new Eddie.Gui.Skin.Label();
            this.lnkLibraries = new Eddie.Gui.Skin.LinkLabel();
            this.lblLicense = new Eddie.Gui.Skin.Label();
            this.lnkLicense = new Eddie.Gui.Skin.LinkLabel();
            this.lblSources = new Eddie.Gui.Skin.Label();
            this.lnkSources = new Eddie.Gui.Skin.LinkLabel();
            this.lblManual = new Eddie.Gui.Skin.Label();
            this.lblWebsite = new Eddie.Gui.Skin.Label();
            this.lnkManual = new Eddie.Gui.Skin.LinkLabel();
            this.lnkWebsite = new Eddie.Gui.Skin.LinkLabel();
            this.lblTitle = new Eddie.Gui.Skin.Label();
            this.lblDeveloped = new Eddie.Gui.Skin.Label();
            this.SuspendLayout();
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblVersion.ForeColor = System.Drawing.Color.White;
            this.lblVersion.Location = new System.Drawing.Point(507, 52);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(264, 26);
            this.lblVersion.TabIndex = 58;
            this.lblVersion.Text = "v2.0";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmdClose
            // 
            this.cmdClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdClose.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdClose.FlatAppearance.BorderSize = 0;
            this.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdClose.Location = new System.Drawing.Point(293, 415);
            this.cmdClose.Margin = new System.Windows.Forms.Padding(4);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(200, 37);
            this.cmdClose.TabIndex = 0;
            this.cmdClose.Text = "Ok";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // lblLibraries
            // 
            this.lblLibraries.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblLibraries.BackColor = System.Drawing.Color.Transparent;
            this.lblLibraries.Location = new System.Drawing.Point(65, 346);
            this.lblLibraries.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLibraries.Name = "lblLibraries";
            this.lblLibraries.Size = new System.Drawing.Size(281, 28);
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
            this.lnkLibraries.Location = new System.Drawing.Point(355, 346);
            this.lnkLibraries.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkLibraries.Name = "lnkLibraries";
            this.lnkLibraries.Size = new System.Drawing.Size(371, 28);
            this.lnkLibraries.TabIndex = 68;
            this.lnkLibraries.TabStop = true;
            this.lnkLibraries.Text = "OpenVPN, Stunnel, TunTap OS X, LZO";
            this.lnkLibraries.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lnkLibraries.Click += new System.EventHandler(this.lnkLibraries_LinkClicked);
            // 
            // lblLicense
            // 
            this.lblLicense.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblLicense.BackColor = System.Drawing.Color.Transparent;
            this.lblLicense.Location = new System.Drawing.Point(65, 309);
            this.lblLicense.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLicense.Name = "lblLicense";
            this.lblLicense.Size = new System.Drawing.Size(281, 28);
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
            this.lnkLicense.Location = new System.Drawing.Point(355, 309);
            this.lnkLicense.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkLicense.Name = "lnkLicense";
            this.lnkLicense.Size = new System.Drawing.Size(371, 28);
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
            this.lblSources.Location = new System.Drawing.Point(65, 272);
            this.lblSources.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSources.Name = "lblSources";
            this.lblSources.Size = new System.Drawing.Size(281, 28);
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
            this.lnkSources.Location = new System.Drawing.Point(355, 272);
            this.lnkSources.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkSources.Name = "lnkSources";
            this.lnkSources.Size = new System.Drawing.Size(371, 28);
            this.lnkSources.TabIndex = 64;
            this.lnkSources.TabStop = true;
            this.lnkSources.Text = "https://github.com/AirVPN/airvpn-client";
            this.lnkSources.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lnkSources.Click += new System.EventHandler(this.lnkSources_LinkClicked);
            // 
            // lblManual
            // 
            this.lblManual.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblManual.BackColor = System.Drawing.Color.Transparent;
            this.lblManual.Location = new System.Drawing.Point(65, 235);
            this.lblManual.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblManual.Name = "lblManual";
            this.lblManual.Size = new System.Drawing.Size(281, 28);
            this.lblManual.TabIndex = 63;
            this.lblManual.Text = "Manual:";
            this.lblManual.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblWebsite
            // 
            this.lblWebsite.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblWebsite.BackColor = System.Drawing.Color.Transparent;
            this.lblWebsite.Location = new System.Drawing.Point(65, 198);
            this.lblWebsite.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWebsite.Name = "lblWebsite";
            this.lblWebsite.Size = new System.Drawing.Size(281, 28);
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
            this.lnkManual.Location = new System.Drawing.Point(355, 235);
            this.lnkManual.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkManual.Name = "lnkManual";
            this.lnkManual.Size = new System.Drawing.Size(371, 28);
            this.lnkManual.TabIndex = 61;
            this.lnkManual.TabStop = true;
            this.lnkManual.Text = "https://airvpn.org/software";
            this.lnkManual.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lnkManual.Click += new System.EventHandler(this.lnkManual_LinkClicked);
            // 
            // lnkWebsite
            // 
            this.lnkWebsite.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lnkWebsite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(188)))), ((int)(((byte)(253)))));
            this.lnkWebsite.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkWebsite.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
            this.lnkWebsite.Location = new System.Drawing.Point(355, 198);
            this.lnkWebsite.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkWebsite.Name = "lnkWebsite";
            this.lnkWebsite.Size = new System.Drawing.Size(371, 28);
            this.lnkWebsite.TabIndex = 60;
            this.lnkWebsite.TabStop = true;
            this.lnkWebsite.Text = "https://airvpn.org";
            this.lnkWebsite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lnkWebsite.Click += new System.EventHandler(this.lnkWebsite_LinkClicked);
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.BackColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 139);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(755, 31);
            this.lblTitle.TabIndex = 59;
            this.lblTitle.Text = "AirVPN - The air to breathe the real Internet.";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDeveloped
            // 
            this.lblDeveloped.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDeveloped.BackColor = System.Drawing.Color.Transparent;
            this.lblDeveloped.Location = new System.Drawing.Point(16, 98);
            this.lblDeveloped.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDeveloped.Name = "lblDeveloped";
            this.lblDeveloped.Size = new System.Drawing.Size(755, 31);
            this.lblDeveloped.TabIndex = 72;
            this.lblDeveloped.Text = "Developed by:";
            this.lblDeveloped.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(787, 468);
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
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.cmdClose);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimizeBox = false;
            this.Name = "About";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
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
        private Skin.Label lblTitle;
        private Skin.Label lblDeveloped;
    }
}