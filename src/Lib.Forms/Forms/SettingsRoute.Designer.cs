namespace AirVPN.Gui.Forms
{
    partial class SettingsRoute
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
			this.lblHostHelp = new AirVPN.Gui.Skin.Label();
			this.cboAction = new AirVPN.Gui.Skin.ComboBox();
			this.cmdOk = new AirVPN.Gui.Skin.Button();
			this.lblProxyType = new AirVPN.Gui.Skin.Label();
			this.txtNotes = new AirVPN.Gui.Skin.TextBox();
			this.lblNotes = new AirVPN.Gui.Skin.Label();
			this.cmdCancel = new AirVPN.Gui.Skin.Button();
			this.txtHost = new AirVPN.Gui.Skin.TextBox();
			this.lblProxyHost = new AirVPN.Gui.Skin.Label();
			this.SuspendLayout();
			// 
			// lblHostHelp
			// 
			this.lblHostHelp.BackColor = System.Drawing.Color.Transparent;
			this.lblHostHelp.ForeColor = System.Drawing.Color.Black;
			this.lblHostHelp.Location = new System.Drawing.Point(89, 41);
			this.lblHostHelp.Name = "lblHostHelp";
			this.lblHostHelp.Size = new System.Drawing.Size(288, 68);
			this.lblHostHelp.TabIndex = 46;
			this.lblHostHelp.Text = "Help";
			this.lblHostHelp.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// cboAction
			// 
			this.cboAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboAction.FormattingEnabled = true;
			this.cboAction.Location = new System.Drawing.Point(89, 136);
			this.cboAction.Name = "cboAction";
			this.cboAction.Size = new System.Drawing.Size(288, 21);
			this.cboAction.TabIndex = 3;
			// 
			// cmdOk
			// 
			this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.Location = new System.Drawing.Point(96, 207);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(100, 28);
			this.cmdOk.TabIndex = 4;
			this.cmdOk.Text = "Save";
			this.cmdOk.UseVisualStyleBackColor = true;
			this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
			// 
			// lblProxyType
			// 
			this.lblProxyType.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyType.ForeColor = System.Drawing.Color.Black;
			this.lblProxyType.Location = new System.Drawing.Point(27, 140);
			this.lblProxyType.Name = "lblProxyType";
			this.lblProxyType.Size = new System.Drawing.Size(57, 20);
			this.lblProxyType.TabIndex = 45;
			this.lblProxyType.Text = "Action:";
			this.lblProxyType.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtNotes
			// 
			this.txtNotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtNotes.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtNotes.Location = new System.Drawing.Point(89, 169);
			this.txtNotes.Name = "txtNotes";
			this.txtNotes.Size = new System.Drawing.Size(288, 20);
			this.txtNotes.TabIndex = 2;
			// 
			// lblNotes
			// 
			this.lblNotes.BackColor = System.Drawing.Color.Transparent;
			this.lblNotes.ForeColor = System.Drawing.Color.Black;
			this.lblNotes.Location = new System.Drawing.Point(27, 172);
			this.lblNotes.Name = "lblNotes";
			this.lblNotes.Size = new System.Drawing.Size(57, 20);
			this.lblNotes.TabIndex = 42;
			this.lblNotes.Text = "Notes:";
			this.lblNotes.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cmdCancel
			// 
			this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatAppearance.BorderSize = 0;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdCancel.Location = new System.Drawing.Point(202, 207);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(100, 28);
			this.cmdCancel.TabIndex = 5;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// txtHost
			// 
			this.txtHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtHost.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtHost.Location = new System.Drawing.Point(89, 18);
			this.txtHost.Name = "txtHost";
			this.txtHost.Size = new System.Drawing.Size(288, 20);
			this.txtHost.TabIndex = 1;
			this.txtHost.TextChanged += new System.EventHandler(this.txtHost_TextChanged);
			// 
			// lblProxyHost
			// 
			this.lblProxyHost.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyHost.ForeColor = System.Drawing.Color.Black;
			this.lblProxyHost.Location = new System.Drawing.Point(27, 21);
			this.lblProxyHost.Name = "lblProxyHost";
			this.lblProxyHost.Size = new System.Drawing.Size(57, 20);
			this.lblProxyHost.TabIndex = 40;
			this.lblProxyHost.Text = "IP / Host:";
			this.lblProxyHost.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// SettingsRoute
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(399, 247);
			this.Controls.Add(this.lblHostHelp);
			this.Controls.Add(this.cboAction);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.lblProxyType);
			this.Controls.Add(this.txtNotes);
			this.Controls.Add(this.lblNotes);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.txtHost);
			this.Controls.Add(this.lblProxyHost);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsRoute";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Skin.Button cmdOk;
        private Skin.Label lblProxyType;
        private Skin.TextBox txtNotes;
        private Skin.Label lblNotes;
        private Skin.Button cmdCancel;
        private Skin.TextBox txtHost;
        private Skin.Label lblProxyHost;
        private Skin.ComboBox cboAction;
		private Skin.Label lblHostHelp;
    }
}