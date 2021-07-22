﻿namespace Eddie.Forms.Forms
{
    partial class WindowProviderEditWireGuard
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
			this.cmdOk = new Eddie.Forms.Skin.Button();
			this.cmdCancel = new Eddie.Forms.Skin.Button();
			this.chkEnabled = new Eddie.Forms.Skin.CheckBox();
			this.cmdPathBrowse = new Eddie.Forms.Skin.Button();
			this.txtPath = new Eddie.Forms.Skin.TextBox();
			this.lblPath = new Eddie.Forms.Skin.Label();
			this.txtTitle2 = new Eddie.Forms.Skin.TextBox();
			this.lblTitle2 = new Eddie.Forms.Skin.Label();
			this.lblSubtitle = new Eddie.Forms.Skin.Label();
			this.lblTitle = new Eddie.Forms.Skin.LinkLabel();
			this.lblAuthPassPassword = new Eddie.Forms.Skin.Label();
			this.lblAuthPassUsername = new Eddie.Forms.Skin.Label();
			this.txtAuthPassPassword = new Eddie.Forms.Skin.TextBox();
			this.txtAuthPassUsername = new Eddie.Forms.Skin.TextBox();
			this.label2 = new Eddie.Forms.Skin.Label();
			this.chkSupportIPv6 = new Eddie.Forms.Skin.CheckBox();
			this.lblSupportIPv6 = new Eddie.Forms.Skin.Label();
			this.SuspendLayout();
			// 
			// cmdOk
			// 
			this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.Location = new System.Drawing.Point(134, 337);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(106, 27);
			this.cmdOk.TabIndex = 4;
			this.cmdOk.Text = "Save";
			this.cmdOk.UseVisualStyleBackColor = true;
			this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatAppearance.BorderSize = 0;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdCancel.Location = new System.Drawing.Point(247, 337);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(106, 27);
			this.cmdCancel.TabIndex = 5;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// chkEnabled
			// 
			this.chkEnabled.AutoSize = true;
			this.chkEnabled.BackColor = System.Drawing.Color.Transparent;
			this.chkEnabled.Location = new System.Drawing.Point(12, 12);
			this.chkEnabled.Name = "chkEnabled";
			this.chkEnabled.Size = new System.Drawing.Size(65, 17);
			this.chkEnabled.TabIndex = 7;
			this.chkEnabled.Text = "Enabled";
			this.chkEnabled.UseVisualStyleBackColor = false;
			// 
			// cmdPathBrowse
			// 
			this.cmdPathBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdPathBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdPathBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdPathBrowse.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdPathBrowse.FlatAppearance.BorderSize = 0;
			this.cmdPathBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdPathBrowse.Image = global::Eddie.Forms.Properties.Resources.browse;
			this.cmdPathBrowse.Location = new System.Drawing.Point(438, 102);
			this.cmdPathBrowse.Name = "cmdPathBrowse";
			this.cmdPathBrowse.Size = new System.Drawing.Size(33, 20);
			this.cmdPathBrowse.TabIndex = 63;
			this.cmdPathBrowse.UseVisualStyleBackColor = true;
			this.cmdPathBrowse.Click += new System.EventHandler(this.cmdPathBrowse_Click);
			// 
			// txtPath
			// 
			this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPath.Location = new System.Drawing.Point(153, 102);
			this.txtPath.Name = "txtPath";
			this.txtPath.Size = new System.Drawing.Size(279, 20);
			this.txtPath.TabIndex = 62;
			// 
			// lblPath
			// 
			this.lblPath.BackColor = System.Drawing.Color.Transparent;
			this.lblPath.ForeColor = System.Drawing.Color.Black;
			this.lblPath.Location = new System.Drawing.Point(12, 102);
			this.lblPath.Name = "lblPath";
			this.lblPath.Size = new System.Drawing.Size(137, 20);
			this.lblPath.TabIndex = 61;
			this.lblPath.Text = "Path (*.ovpn):";
			this.lblPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtTitle2
			// 
			this.txtTitle2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtTitle2.Location = new System.Drawing.Point(153, 72);
			this.txtTitle2.Name = "txtTitle2";
			this.txtTitle2.Size = new System.Drawing.Size(279, 20);
			this.txtTitle2.TabIndex = 65;
			// 
			// lblTitle2
			// 
			this.lblTitle2.BackColor = System.Drawing.Color.Transparent;
			this.lblTitle2.ForeColor = System.Drawing.Color.Black;
			this.lblTitle2.Location = new System.Drawing.Point(12, 72);
			this.lblTitle2.Name = "lblTitle2";
			this.lblTitle2.Size = new System.Drawing.Size(137, 20);
			this.lblTitle2.TabIndex = 64;
			this.lblTitle2.Text = "Title:";
			this.lblTitle2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblSubtitle
			// 
			this.lblSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSubtitle.BackColor = System.Drawing.Color.Transparent;
			this.lblSubtitle.Location = new System.Drawing.Point(76, 38);
			this.lblSubtitle.Name = "lblSubtitle";
			this.lblSubtitle.Size = new System.Drawing.Size(402, 20);
			this.lblSubtitle.TabIndex = 67;
			this.lblSubtitle.Text = "Subtitle";
			this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblTitle
			// 
			this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTitle.BackColor = System.Drawing.Color.Transparent;
			this.lblTitle.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lblTitle.Location = new System.Drawing.Point(180, 9);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(298, 29);
			this.lblTitle.TabIndex = 66;
			this.lblTitle.TabStop = true;
			this.lblTitle.Text = "Title";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblTitle.Click += new System.EventHandler(this.lblTitle_Click);
			// 
			// lblAuthPassPassword
			// 
			this.lblAuthPassPassword.BackColor = System.Drawing.Color.Transparent;
			this.lblAuthPassPassword.ForeColor = System.Drawing.Color.Black;
			this.lblAuthPassPassword.Location = new System.Drawing.Point(11, 170);
			this.lblAuthPassPassword.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblAuthPassPassword.Name = "lblAuthPassPassword";
			this.lblAuthPassPassword.Size = new System.Drawing.Size(138, 20);
			this.lblAuthPassPassword.TabIndex = 71;
			this.lblAuthPassPassword.Text = "Password:";
			this.lblAuthPassPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblAuthPassUsername
			// 
			this.lblAuthPassUsername.BackColor = System.Drawing.Color.Transparent;
			this.lblAuthPassUsername.ForeColor = System.Drawing.Color.Black;
			this.lblAuthPassUsername.Location = new System.Drawing.Point(11, 140);
			this.lblAuthPassUsername.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblAuthPassUsername.Name = "lblAuthPassUsername";
			this.lblAuthPassUsername.Size = new System.Drawing.Size(138, 20);
			this.lblAuthPassUsername.TabIndex = 70;
			this.lblAuthPassUsername.Text = "Username:";
			this.lblAuthPassUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtAuthPassPassword
			// 
			this.txtAuthPassPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtAuthPassPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtAuthPassPassword.Location = new System.Drawing.Point(153, 170);
			this.txtAuthPassPassword.Margin = new System.Windows.Forms.Padding(2);
			this.txtAuthPassPassword.Name = "txtAuthPassPassword";
			this.txtAuthPassPassword.PasswordChar = '*';
			this.txtAuthPassPassword.Size = new System.Drawing.Size(279, 20);
			this.txtAuthPassPassword.TabIndex = 69;
			// 
			// txtAuthPassUsername
			// 
			this.txtAuthPassUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtAuthPassUsername.Location = new System.Drawing.Point(153, 140);
			this.txtAuthPassUsername.Margin = new System.Windows.Forms.Padding(2);
			this.txtAuthPassUsername.Name = "txtAuthPassUsername";
			this.txtAuthPassUsername.Size = new System.Drawing.Size(279, 20);
			this.txtAuthPassUsername.TabIndex = 68;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Location = new System.Drawing.Point(153, 199);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(279, 36);
			this.label2.TabIndex = 72;
			this.label2.Text = "Credentials are checked during connection.";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkSupportIPv6
			// 
			this.chkSupportIPv6.Location = new System.Drawing.Point(153, 263);
			this.chkSupportIPv6.Name = "chkSupportIPv6";
			this.chkSupportIPv6.Size = new System.Drawing.Size(318, 24);
			this.chkSupportIPv6.TabIndex = 73;
			this.chkSupportIPv6.UseVisualStyleBackColor = true;
			// 
			// lblSupportIPv6
			// 
			this.lblSupportIPv6.BackColor = System.Drawing.Color.Transparent;
			this.lblSupportIPv6.ForeColor = System.Drawing.Color.Black;
			this.lblSupportIPv6.Location = new System.Drawing.Point(12, 264);
			this.lblSupportIPv6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblSupportIPv6.Name = "lblSupportIPv6";
			this.lblSupportIPv6.Size = new System.Drawing.Size(138, 20);
			this.lblSupportIPv6.TabIndex = 74;
			this.lblSupportIPv6.Text = "Support IPv6:";
			this.lblSupportIPv6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WindowProviderEditWireGuard
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(490, 376);
			this.Controls.Add(this.lblSupportIPv6);
			this.Controls.Add(this.chkSupportIPv6);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.lblAuthPassPassword);
			this.Controls.Add(this.lblAuthPassUsername);
			this.Controls.Add(this.txtAuthPassPassword);
			this.Controls.Add(this.txtAuthPassUsername);
			this.Controls.Add(this.lblSubtitle);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.txtTitle2);
			this.Controls.Add(this.lblTitle2);
			this.Controls.Add(this.cmdPathBrowse);
			this.Controls.Add(this.txtPath);
			this.Controls.Add(this.lblPath);
			this.Controls.Add(this.chkEnabled);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.cmdCancel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WindowProviderEditWireGuard";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Skin.Button cmdOk;
        private Skin.Button cmdCancel;
		private Skin.CheckBox chkEnabled;
		private Skin.Button cmdPathBrowse;
		private Skin.TextBox txtPath;
		private Skin.Label lblPath;
		private Skin.TextBox txtTitle2;
		private Skin.Label lblTitle2;
		private Skin.Label lblSubtitle;
		private Skin.LinkLabel lblTitle;
		private Skin.Label lblAuthPassPassword;
		private Skin.Label lblAuthPassUsername;
		private Skin.TextBox txtAuthPassPassword;
		private Skin.TextBox txtAuthPassUsername;
		private Skin.Label label2;
		private Skin.CheckBox chkSupportIPv6;
		private Skin.Label lblSupportIPv6;
	}
}