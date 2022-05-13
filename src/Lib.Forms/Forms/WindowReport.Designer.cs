namespace Eddie.Forms.Forms
{
	partial class WindowReport
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
			this.txtBody = new Eddie.Forms.Skin.TextBox();
			this.cmdCopyClipboard = new Eddie.Forms.Skin.Button();
			this.cmdSave = new Eddie.Forms.Skin.Button();
			this.cmdOk = new Eddie.Forms.Skin.Button();
			this.pgrStep = new System.Windows.Forms.ProgressBar();
			this.lblStep = new Eddie.Forms.Skin.Label();
			this.cmdUpload = new Eddie.Forms.Skin.Button();
			this.txtUploadUrl = new Eddie.Forms.Skin.TextBox();
			this.SuspendLayout();
			// 
			// txtBody
			// 
			this.txtBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtBody.BackColor = System.Drawing.Color.White;
			this.txtBody.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.txtBody.Location = new System.Drawing.Point(9, 6);
			this.txtBody.Margin = new System.Windows.Forms.Padding(15);
			this.txtBody.Multiline = true;
			this.txtBody.Name = "txtBody";
			this.txtBody.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtBody.Size = new System.Drawing.Size(1104, 577);
			this.txtBody.TabIndex = 4;
			// 
			// cmdCopyClipboard
			// 
			this.cmdCopyClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCopyClipboard.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdCopyClipboard.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdCopyClipboard.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdCopyClipboard.FlatAppearance.BorderSize = 0;
			this.cmdCopyClipboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdCopyClipboard.Image = global::Eddie.Forms.Properties.Resources.copy;
			this.cmdCopyClipboard.Location = new System.Drawing.Point(1122, 16);
			this.cmdCopyClipboard.Name = "cmdCopyClipboard";
			this.cmdCopyClipboard.Size = new System.Drawing.Size(42, 42);
			this.cmdCopyClipboard.TabIndex = 2;
			this.cmdCopyClipboard.UseVisualStyleBackColor = true;
			this.cmdCopyClipboard.Click += new System.EventHandler(this.cmdCopyClipboard_Click);
			// 
			// cmdSave
			// 
			this.cmdSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdSave.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdSave.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdSave.FlatAppearance.BorderSize = 0;
			this.cmdSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdSave.Image = global::Eddie.Forms.Properties.Resources.save;
			this.cmdSave.Location = new System.Drawing.Point(1122, 64);
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.Size = new System.Drawing.Size(42, 42);
			this.cmdSave.TabIndex = 3;
			this.cmdSave.UseVisualStyleBackColor = true;
			this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
			// 
			// cmdOk
			// 
			this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cmdOk.Location = new System.Drawing.Point(875, 628);
			this.cmdOk.Margin = new System.Windows.Forms.Padding(6);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(240, 45);
			this.cmdOk.TabIndex = 1;
			this.cmdOk.Text = "Close";
			this.cmdOk.UseVisualStyleBackColor = true;
			this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
			// 
			// pgrStep
			// 
			this.pgrStep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.pgrStep.Location = new System.Drawing.Point(836, 588);
			this.pgrStep.Margin = new System.Windows.Forms.Padding(4);
			this.pgrStep.Name = "pgrStep";
			this.pgrStep.Size = new System.Drawing.Size(279, 34);
			this.pgrStep.TabIndex = 51;
			// 
			// lblStep
			// 
			this.lblStep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblStep.BackColor = System.Drawing.Color.Transparent;
			this.lblStep.ForeColor = System.Drawing.Color.Black;
			this.lblStep.Location = new System.Drawing.Point(478, 588);
			this.lblStep.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStep.Name = "lblStep";
			this.lblStep.Size = new System.Drawing.Size(349, 34);
			this.lblStep.TabIndex = 52;
			this.lblStep.Text = "Starting...";
			this.lblStep.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmdUpload
			// 
			this.cmdUpload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cmdUpload.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdUpload.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdUpload.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdUpload.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdUpload.FlatAppearance.BorderSize = 0;
			this.cmdUpload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdUpload.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cmdUpload.Location = new System.Drawing.Point(9, 628);
			this.cmdUpload.Margin = new System.Windows.Forms.Padding(6);
			this.cmdUpload.Name = "cmdUpload";
			this.cmdUpload.Size = new System.Drawing.Size(459, 45);
			this.cmdUpload.TabIndex = 53;
			this.cmdUpload.Text = "Upload to paste URL in support ticket";
			this.cmdUpload.UseVisualStyleBackColor = true;
			this.cmdUpload.Click += new System.EventHandler(this.cmdUpload_Click);
			// 
			// txtUploadUrl
			// 
			this.txtUploadUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtUploadUrl.Location = new System.Drawing.Point(9, 593);
			this.txtUploadUrl.Name = "txtUploadUrl";
			this.txtUploadUrl.ReadOnly = true;
			this.txtUploadUrl.Size = new System.Drawing.Size(459, 26);
			this.txtUploadUrl.TabIndex = 54;
			// 
			// WindowReport
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdOk;
			this.ClientSize = new System.Drawing.Size(1176, 692);
			this.Controls.Add(this.txtUploadUrl);
			this.Controls.Add(this.cmdUpload);
			this.Controls.Add(this.lblStep);
			this.Controls.Add(this.pgrStep);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.cmdCopyClipboard);
			this.Controls.Add(this.cmdSave);
			this.Controls.Add(this.txtBody);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimizeBox = false;
			this.Name = "WindowReport";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "System Report";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Skin.TextBox txtBody;
		private Skin.Button cmdCopyClipboard;
		private Skin.Button cmdSave;
		private Skin.Button cmdOk;
		private System.Windows.Forms.ProgressBar pgrStep;
		private Skin.Label lblStep;
		private Skin.Button cmdUpload;
		private Skin.TextBox txtUploadUrl;
	}
}