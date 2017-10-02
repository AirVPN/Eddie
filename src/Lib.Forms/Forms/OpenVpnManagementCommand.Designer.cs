namespace Eddie.Forms.Forms
{
	partial class OpenVpnManagementCommand
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
            this.lnkHelp = new Eddie.Forms.Skin.LinkLabel();
            this.cmdOk = new Eddie.Forms.Skin.Button();
            this.cmdCancel = new Eddie.Forms.Skin.Button();
            this.txtCommand = new Eddie.Forms.Skin.TextBox();
            this.lblCommand = new Eddie.Forms.Skin.Label();
            this.SuspendLayout();
            // 
            // lnkHelp
            // 
            this.lnkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkHelp.BackColor = System.Drawing.Color.Transparent;
            this.lnkHelp.ForeColor = System.Drawing.Color.Black;
            this.lnkHelp.Location = new System.Drawing.Point(12, 60);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(410, 25);
            this.lnkHelp.TabIndex = 41;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "Click here for more informations about OpenVPN Management Interface";
            this.lnkHelp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkHelp.Click += new System.EventHandler(this.lnkHelp_LinkClicked);
            // 
            // cmdOk
            // 
            this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.FlatAppearance.BorderSize = 0;
            this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdOk.Location = new System.Drawing.Point(106, 102);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(106, 27);
            this.cmdOk.TabIndex = 5;
            this.cmdOk.Text = "Send";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.FlatAppearance.BorderSize = 0;
            this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdCancel.Location = new System.Drawing.Point(220, 102);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(106, 27);
            this.cmdCancel.TabIndex = 6;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // txtCommand
            // 
            this.txtCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommand.Location = new System.Drawing.Point(119, 18);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(303, 20);
            this.txtCommand.TabIndex = 1;
            this.txtCommand.Text = "help";
            // 
            // lblCommand
            // 
            this.lblCommand.BackColor = System.Drawing.Color.Transparent;
            this.lblCommand.ForeColor = System.Drawing.Color.Black;
            this.lblCommand.Location = new System.Drawing.Point(13, 18);
            this.lblCommand.Name = "lblCommand";
            this.lblCommand.Size = new System.Drawing.Size(100, 20);
            this.lblCommand.TabIndex = 40;
            this.lblCommand.Text = "Command :";
            this.lblCommand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // OpenVpnManagementCommand
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(434, 142);
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.txtCommand);
            this.Controls.Add(this.lblCommand);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenVpnManagementCommand";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.SettingsRoute_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

		private Skin.Button cmdOk;
        private Skin.Button cmdCancel;
        private Skin.TextBox txtCommand;
		private Skin.Label lblCommand;
		private Skin.LinkLabel lnkHelp;
    }
}