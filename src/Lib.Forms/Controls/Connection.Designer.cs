namespace Eddie.Forms.Controls
{
	partial class Connection
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
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
			this.lblConnectedServerName = new Eddie.Forms.Skin.Label();
			this.SuspendLayout();
			// 
			// txtConnectedExitIp
			// 
			this.txtConnectedExitIp.Location = new System.Drawing.Point(222, 117);
			this.txtConnectedExitIp.Name = "txtConnectedExitIp";
			this.txtConnectedExitIp.Size = new System.Drawing.Size(288, 24);
			this.txtConnectedExitIp.TabIndex = 80;
			this.txtConnectedExitIp.Text = "1.2.3.4";
			this.txtConnectedExitIp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblConnectedExitIp
			// 
			this.lblConnectedExitIp.Location = new System.Drawing.Point(75, 117);
			this.lblConnectedExitIp.Name = "lblConnectedExitIp";
			this.lblConnectedExitIp.Size = new System.Drawing.Size(138, 24);
			this.lblConnectedExitIp.TabIndex = 79;
			this.lblConnectedExitIp.Text = "Public Exit IP:";
			this.lblConnectedExitIp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblConnectedUpload
			// 
			this.lblConnectedUpload.Location = new System.Drawing.Point(296, 70);
			this.lblConnectedUpload.Name = "lblConnectedUpload";
			this.lblConnectedUpload.Size = new System.Drawing.Size(65, 20);
			this.lblConnectedUpload.TabIndex = 76;
			this.lblConnectedUpload.Text = "Upload:";
			this.lblConnectedUpload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtConnectedUpload
			// 
			this.txtConnectedUpload.BackColor = System.Drawing.Color.White;
			this.txtConnectedUpload.Font = new System.Drawing.Font("Lucida Console", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtConnectedUpload.ForeColor = System.Drawing.Color.ForestGreen;
			this.txtConnectedUpload.Location = new System.Drawing.Point(364, 64);
			this.txtConnectedUpload.Name = "txtConnectedUpload";
			this.txtConnectedUpload.Size = new System.Drawing.Size(144, 40);
			this.txtConnectedUpload.TabIndex = 78;
			this.txtConnectedUpload.Text = "14332 kb/s";
			this.txtConnectedUpload.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// txtConnectedDownload
			// 
			this.txtConnectedDownload.BackColor = System.Drawing.Color.White;
			this.txtConnectedDownload.Font = new System.Drawing.Font("Lucida Console", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtConnectedDownload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(157)))), ((int)(((byte)(255)))));
			this.txtConnectedDownload.Location = new System.Drawing.Point(146, 64);
			this.txtConnectedDownload.Name = "txtConnectedDownload";
			this.txtConnectedDownload.Size = new System.Drawing.Size(144, 40);
			this.txtConnectedDownload.TabIndex = 77;
			this.txtConnectedDownload.Text = "14332 kb/s";
			this.txtConnectedDownload.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblConnectedDownload
			// 
			this.lblConnectedDownload.Location = new System.Drawing.Point(69, 72);
			this.lblConnectedDownload.Name = "lblConnectedDownload";
			this.lblConnectedDownload.Size = new System.Drawing.Size(74, 20);
			this.lblConnectedDownload.TabIndex = 75;
			this.lblConnectedDownload.Text = "Download:";
			this.lblConnectedDownload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtConnectedSince
			// 
			this.txtConnectedSince.Location = new System.Drawing.Point(222, 145);
			this.txtConnectedSince.Name = "txtConnectedSince";
			this.txtConnectedSince.Size = new System.Drawing.Size(288, 24);
			this.txtConnectedSince.TabIndex = 74;
			this.txtConnectedSince.Text = "VPN Time";
			this.txtConnectedSince.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblConnectedSince
			// 
			this.lblConnectedSince.Location = new System.Drawing.Point(72, 145);
			this.lblConnectedSince.Name = "lblConnectedSince";
			this.lblConnectedSince.Size = new System.Drawing.Size(141, 24);
			this.lblConnectedSince.TabIndex = 73;
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
			this.cmdDisconnect.Location = new System.Drawing.Point(72, 176);
			this.cmdDisconnect.Margin = new System.Windows.Forms.Padding(0);
			this.cmdDisconnect.Name = "cmdDisconnect";
			this.cmdDisconnect.Size = new System.Drawing.Size(438, 34);
			this.cmdDisconnect.TabIndex = 72;
			this.cmdDisconnect.Text = "Disconnect";
			this.cmdDisconnect.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.cmdDisconnect.UseVisualStyleBackColor = true;
			// 
			// lblConnectedLocation
			// 
			this.lblConnectedLocation.Location = new System.Drawing.Point(222, 39);
			this.lblConnectedLocation.Name = "lblConnectedLocation";
			this.lblConnectedLocation.Size = new System.Drawing.Size(284, 26);
			this.lblConnectedLocation.TabIndex = 71;
			this.lblConnectedLocation.Text = "Location";
			this.lblConnectedLocation.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblConnectedServerName
			// 
			this.lblConnectedServerName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.lblConnectedServerName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblConnectedServerName.Location = new System.Drawing.Point(220, 6);
			this.lblConnectedServerName.Name = "lblConnectedServerName";
			this.lblConnectedServerName.Size = new System.Drawing.Size(289, 30);
			this.lblConnectedServerName.TabIndex = 70;
			this.lblConnectedServerName.Text = "Server Name";
			this.lblConnectedServerName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Connection
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.txtConnectedExitIp);
			this.Controls.Add(this.lblConnectedExitIp);
			this.Controls.Add(this.lblConnectedUpload);
			this.Controls.Add(this.txtConnectedUpload);
			this.Controls.Add(this.txtConnectedDownload);
			this.Controls.Add(this.lblConnectedDownload);
			this.Controls.Add(this.txtConnectedSince);
			this.Controls.Add(this.lblConnectedSince);
			this.Controls.Add(this.cmdDisconnect);
			this.Controls.Add(this.lblConnectedLocation);
			this.Controls.Add(this.lblConnectedServerName);
			this.Name = "Connection";
			this.Size = new System.Drawing.Size(578, 217);
			this.ResumeLayout(false);

		}

		#endregion

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
		private Skin.Label lblConnectedServerName;
	}
}
