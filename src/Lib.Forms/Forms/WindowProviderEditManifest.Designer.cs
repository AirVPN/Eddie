namespace Eddie.Gui.Forms
{
    partial class WindowProviderEditManifest
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
			this.cmdOk = new Eddie.Gui.Skin.Button();
			this.cmdCancel = new Eddie.Gui.Skin.Button();
			this.chkEnabled = new Eddie.Gui.Skin.CheckBox();
			this.lblTitle = new Eddie.Gui.Skin.LinkLabel();
			this.lblSubtitle = new Eddie.Gui.Skin.Label();
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
			this.cmdOk.Location = new System.Drawing.Point(102, 76);
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
			this.cmdCancel.Location = new System.Drawing.Point(215, 76);
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
			this.chkEnabled.TabIndex = 6;
			this.chkEnabled.Text = "Enabled";
			this.chkEnabled.UseVisualStyleBackColor = false;
			// 
			// lblTitle
			// 
			this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTitle.BackColor = System.Drawing.Color.Transparent;
			this.lblTitle.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(141)))));
			this.lblTitle.Location = new System.Drawing.Point(116, 13);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(298, 29);
			this.lblTitle.TabIndex = 7;
			this.lblTitle.TabStop = true;
			this.lblTitle.Text = "Title";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblTitle.Click += new System.EventHandler(this.lblTitle_Click_1);
			// 
			// lblSubtitle
			// 
			this.lblSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSubtitle.BackColor = System.Drawing.Color.Transparent;
			this.lblSubtitle.Location = new System.Drawing.Point(12, 42);
			this.lblSubtitle.Name = "lblSubtitle";
			this.lblSubtitle.Size = new System.Drawing.Size(402, 20);
			this.lblSubtitle.TabIndex = 8;
			this.lblSubtitle.Text = "Subtitle";
			this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WindowProviderEditManifest
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(426, 115);
			this.Controls.Add(this.lblSubtitle);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.chkEnabled);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.cmdCancel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WindowProviderEditManifest";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		private void LblTitle_Click(object sender, System.EventArgs e)
		{
			throw new System.NotImplementedException();
		}

		#endregion

		private Skin.Button cmdOk;
        private Skin.Button cmdCancel;
		private Skin.CheckBox chkEnabled;
		private Skin.LinkLabel lblTitle;
		private Skin.Label lblSubtitle;
	}
}