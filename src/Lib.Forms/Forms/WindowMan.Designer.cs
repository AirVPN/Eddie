namespace Eddie.Forms.Forms
{
    partial class WindowMan
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
			this.lstOptions = new Eddie.Forms.Skin.ListView();
			this.colOptionName = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.colOptionDesc = ((Eddie.Forms.Skin.ColumnHeader)(new Eddie.Forms.Skin.ColumnHeader()));
			this.colOptionDefault = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colOptionValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.pnlProviders = new Eddie.Forms.Skin.Panel();
			this.SuspendLayout();
			// 
			// txtBody
			// 
			this.txtBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtBody.BackColor = System.Drawing.Color.White;
			this.txtBody.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.txtBody.Location = new System.Drawing.Point(6, 4);
			this.txtBody.Margin = new System.Windows.Forms.Padding(10);
			this.txtBody.Multiline = true;
			this.txtBody.Name = "txtBody";
			this.txtBody.ReadOnly = true;
			this.txtBody.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtBody.Size = new System.Drawing.Size(737, 172);
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
			this.cmdCopyClipboard.Location = new System.Drawing.Point(748, 11);
			this.cmdCopyClipboard.Margin = new System.Windows.Forms.Padding(2);
			this.cmdCopyClipboard.Name = "cmdCopyClipboard";
			this.cmdCopyClipboard.Size = new System.Drawing.Size(28, 28);
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
			this.cmdSave.Location = new System.Drawing.Point(748, 43);
			this.cmdSave.Margin = new System.Windows.Forms.Padding(2);
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.Size = new System.Drawing.Size(28, 28);
			this.cmdSave.TabIndex = 3;
			this.cmdSave.UseVisualStyleBackColor = true;
			this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
			// 
			// cmdOk
			// 
			this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cmdOk.Location = new System.Drawing.Point(312, 418);
			this.cmdOk.Margin = new System.Windows.Forms.Padding(4);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(160, 30);
			this.cmdOk.TabIndex = 1;
			this.cmdOk.Text = "Close";
			this.cmdOk.UseVisualStyleBackColor = true;
			this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
			// 
			// pgrStep
			// 
			this.pgrStep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.pgrStep.Location = new System.Drawing.Point(557, 392);
			this.pgrStep.Name = "pgrStep";
			this.pgrStep.Size = new System.Drawing.Size(186, 23);
			this.pgrStep.TabIndex = 51;
			// 
			// lblStep
			// 
			this.lblStep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblStep.BackColor = System.Drawing.Color.Transparent;
			this.lblStep.ForeColor = System.Drawing.Color.Black;
			this.lblStep.Location = new System.Drawing.Point(6, 392);
			this.lblStep.Name = "lblStep";
			this.lblStep.Size = new System.Drawing.Size(545, 23);
			this.lblStep.TabIndex = 52;
			this.lblStep.Text = "Starting...";
			this.lblStep.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lstOptions
			// 
			this.lstOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstOptions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colOptionName,
            this.colOptionDesc,
            this.colOptionDefault,
            this.colOptionValue});
			this.lstOptions.FullRowSelect = true;
			this.lstOptions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstOptions.HideSelection = false;
			this.lstOptions.Location = new System.Drawing.Point(583, 201);
			this.lstOptions.MultiSelect = false;
			this.lstOptions.Name = "lstOptions";
			this.lstOptions.OwnerDraw = true;
			this.lstOptions.Size = new System.Drawing.Size(134, 175);
			this.lstOptions.TabIndex = 70;
			this.lstOptions.UseCompatibleStateImageBehavior = false;
			this.lstOptions.View = System.Windows.Forms.View.Details;
			// 
			// colOptionName
			// 
			this.colOptionName.Text = "Name";
			// 
			// colOptionDesc
			// 
			this.colOptionDesc.Text = "Description";
			// 
			// colOptionDefault
			// 
			this.colOptionDefault.Text = "Default";
			// 
			// colOptionValue
			// 
			this.colOptionValue.Text = "Value";
			// 
			// pnlProviders
			// 
			this.pnlProviders.AutoScroll = true;
			this.pnlProviders.Location = new System.Drawing.Point(120, 216);
			this.pnlProviders.Name = "pnlProviders";
			this.pnlProviders.Size = new System.Drawing.Size(365, 173);
			this.pnlProviders.TabIndex = 71;
			// 
			// WindowMan
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdOk;
			this.ClientSize = new System.Drawing.Size(784, 461);
			this.Controls.Add(this.pnlProviders);
			this.Controls.Add(this.lstOptions);
			this.Controls.Add(this.lblStep);
			this.Controls.Add(this.pgrStep);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.cmdCopyClipboard);
			this.Controls.Add(this.cmdSave);
			this.Controls.Add(this.txtBody);
			this.MinimizeBox = false;
			this.Name = "WindowMan";
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
		private Skin.ListView lstOptions;
		private Skin.ColumnHeader colOptionName;
		private Skin.ColumnHeader colOptionDesc;
		private System.Windows.Forms.ColumnHeader colOptionDefault;
		private System.Windows.Forms.ColumnHeader colOptionValue;
		private Skin.Panel pnlProviders;
	}
}