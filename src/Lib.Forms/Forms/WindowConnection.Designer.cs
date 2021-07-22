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
			this.tabConfigGenerated = new System.Windows.Forms.TabPage();
			this.txtConfigGenerated = new Eddie.Forms.Skin.TextBox();
			this.tabConfigOriginal = new System.Windows.Forms.TabPage();
			this.txtConfigOriginal = new Eddie.Forms.Skin.TextBox();
			this.cmdOk = new Eddie.Forms.Skin.Button();
			this.tabMain.SuspendLayout();
			this.tabConfigGenerated.SuspendLayout();
			this.tabConfigOriginal.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabMain
			// 
			this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabMain.Controls.Add(this.tabConfigGenerated);
			this.tabMain.Controls.Add(this.tabConfigOriginal);
			this.tabMain.Location = new System.Drawing.Point(0, 0);
			this.tabMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(850, 546);
			this.tabMain.TabIndex = 0;
			// 
			// tabConfigGenerated
			// 
			this.tabConfigGenerated.Controls.Add(this.txtConfigGenerated);
			this.tabConfigGenerated.Location = new System.Drawing.Point(4, 29);
			this.tabConfigGenerated.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.tabConfigGenerated.Name = "tabConfigGenerated";
			this.tabConfigGenerated.Size = new System.Drawing.Size(842, 513);
			this.tabConfigGenerated.TabIndex = 0;
			this.tabConfigGenerated.Text = "Config Generated";
			this.tabConfigGenerated.UseVisualStyleBackColor = true;
			// 
			// txtConfigGenerated
			// 
			this.txtConfigGenerated.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtConfigGenerated.Location = new System.Drawing.Point(10, 10);
			this.txtConfigGenerated.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtConfigGenerated.Multiline = true;
			this.txtConfigGenerated.Name = "txtConfigGenerated";
			this.txtConfigGenerated.ReadOnly = true;
			this.txtConfigGenerated.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtConfigGenerated.Size = new System.Drawing.Size(814, 482);
			this.txtConfigGenerated.TabIndex = 0;
			this.txtConfigGenerated.WordWrap = false;
			// 
			// tabConfigOriginal
			// 
			this.tabConfigOriginal.Controls.Add(this.txtConfigOriginal);
			this.tabConfigOriginal.Location = new System.Drawing.Point(4, 29);
			this.tabConfigOriginal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.tabConfigOriginal.Name = "tabConfigOriginal";
			this.tabConfigOriginal.Size = new System.Drawing.Size(842, 513);
			this.tabConfigOriginal.TabIndex = 1;
			this.tabConfigOriginal.Text = "Config Original";
			this.tabConfigOriginal.UseVisualStyleBackColor = true;
			// 
			// txtConfigOriginal
			// 
			this.txtConfigOriginal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtConfigOriginal.Location = new System.Drawing.Point(10, 10);
			this.txtConfigOriginal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtConfigOriginal.Multiline = true;
			this.txtConfigOriginal.Name = "txtConfigOriginal";
			this.txtConfigOriginal.ReadOnly = true;
			this.txtConfigOriginal.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtConfigOriginal.Size = new System.Drawing.Size(814, 558);
			this.txtConfigOriginal.TabIndex = 1;
			this.txtConfigOriginal.WordWrap = false;
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
			this.cmdOk.Location = new System.Drawing.Point(306, 556);
			this.cmdOk.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(240, 45);
			this.cmdOk.TabIndex = 3;
			this.cmdOk.Text = "Close";
			this.cmdOk.UseVisualStyleBackColor = true;
			// 
			// WindowConnection
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdOk;
			this.ClientSize = new System.Drawing.Size(850, 621);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.tabMain);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WindowConnection";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Form1";
			this.tabMain.ResumeLayout(false);
			this.tabConfigGenerated.ResumeLayout(false);
			this.tabConfigGenerated.PerformLayout();
			this.tabConfigOriginal.ResumeLayout(false);
			this.tabConfigOriginal.PerformLayout();
			this.ResumeLayout(false);

        }

		#endregion

		private Skin.TabControl tabMain;
		private System.Windows.Forms.TabPage tabConfigGenerated;
		private System.Windows.Forms.TabPage tabConfigOriginal;
		private Skin.TextBox txtConfigGenerated;
		private Skin.TextBox txtConfigOriginal;
		private Skin.Button cmdOk;
	}
}