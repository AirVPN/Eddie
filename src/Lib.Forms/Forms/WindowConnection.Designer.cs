namespace Eddie.Forms.Forms
{
    partial class WindowConnection
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
			this.tabMain = new Eddie.Forms.Skin.TabControl();
			this.tabOvpnGenerated = new System.Windows.Forms.TabPage();
			this.txtOvpnGenerated = new Eddie.Forms.Skin.TextBox();
			this.tabOvpnOriginal = new System.Windows.Forms.TabPage();
			this.txtOvpnOriginal = new Eddie.Forms.Skin.TextBox();
			this.cmdOk = new Eddie.Forms.Skin.Button();
			this.tabMain.SuspendLayout();
			this.tabOvpnGenerated.SuspendLayout();
			this.tabOvpnOriginal.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabMain
			// 
			this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabMain.Controls.Add(this.tabOvpnGenerated);
			this.tabMain.Controls.Add(this.tabOvpnOriginal);
			this.tabMain.Location = new System.Drawing.Point(0, 0);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(567, 364);
			this.tabMain.TabIndex = 0;
			// 
			// tabOvpnGenerated
			// 
			this.tabOvpnGenerated.Controls.Add(this.txtOvpnGenerated);
			this.tabOvpnGenerated.Location = new System.Drawing.Point(4, 22);
			this.tabOvpnGenerated.Name = "tabOvpnGenerated";
			this.tabOvpnGenerated.Size = new System.Drawing.Size(559, 338);
			this.tabOvpnGenerated.TabIndex = 0;
			this.tabOvpnGenerated.Text = "OpenVPN Eddie";
			this.tabOvpnGenerated.UseVisualStyleBackColor = true;
			// 
			// txtOvpnGenerated
			// 
			this.txtOvpnGenerated.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOvpnGenerated.Location = new System.Drawing.Point(7, 7);
			this.txtOvpnGenerated.Multiline = true;
			this.txtOvpnGenerated.Name = "txtOvpnGenerated";
			this.txtOvpnGenerated.ReadOnly = true;
			this.txtOvpnGenerated.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtOvpnGenerated.Size = new System.Drawing.Size(544, 323);
			this.txtOvpnGenerated.TabIndex = 0;
			this.txtOvpnGenerated.WordWrap = false;
			// 
			// tabOvpnOriginal
			// 
			this.tabOvpnOriginal.Controls.Add(this.txtOvpnOriginal);
			this.tabOvpnOriginal.Location = new System.Drawing.Point(4, 22);
			this.tabOvpnOriginal.Name = "tabOvpnOriginal";
			this.tabOvpnOriginal.Size = new System.Drawing.Size(559, 388);
			this.tabOvpnOriginal.TabIndex = 1;
			this.tabOvpnOriginal.Text = "OpenVPN Original";
			this.tabOvpnOriginal.UseVisualStyleBackColor = true;
			// 
			// txtOvpnOriginal
			// 
			this.txtOvpnOriginal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOvpnOriginal.Location = new System.Drawing.Point(7, 7);
			this.txtOvpnOriginal.Multiline = true;
			this.txtOvpnOriginal.Name = "txtOvpnOriginal";
			this.txtOvpnOriginal.ReadOnly = true;
			this.txtOvpnOriginal.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtOvpnOriginal.Size = new System.Drawing.Size(544, 373);
			this.txtOvpnOriginal.TabIndex = 1;
			this.txtOvpnOriginal.WordWrap = false;
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
			this.cmdOk.Location = new System.Drawing.Point(204, 371);
			this.cmdOk.Margin = new System.Windows.Forms.Padding(4);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(160, 30);
			this.cmdOk.TabIndex = 3;
			this.cmdOk.Text = "Close";
			this.cmdOk.UseVisualStyleBackColor = true;
			// 
			// WindowConnection
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdOk;
			this.ClientSize = new System.Drawing.Size(567, 414);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.tabMain);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WindowConnection";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Form1";
			this.tabMain.ResumeLayout(false);
			this.tabOvpnGenerated.ResumeLayout(false);
			this.tabOvpnGenerated.PerformLayout();
			this.tabOvpnOriginal.ResumeLayout(false);
			this.tabOvpnOriginal.PerformLayout();
			this.ResumeLayout(false);

        }

		#endregion

		private Skin.TabControl tabMain;
		private System.Windows.Forms.TabPage tabOvpnGenerated;
		private System.Windows.Forms.TabPage tabOvpnOriginal;
		private Skin.TextBox txtOvpnGenerated;
		private Skin.TextBox txtOvpnOriginal;
		private Skin.Button cmdOk;
	}
}