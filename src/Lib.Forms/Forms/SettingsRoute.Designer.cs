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
            this.lblHostHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHostHelp.BackColor = System.Drawing.Color.Transparent;
            this.lblHostHelp.ForeColor = System.Drawing.Color.Black;
            this.lblHostHelp.Location = new System.Drawing.Point(95, 41);
            this.lblHostHelp.Name = "lblHostHelp";
            this.lblHostHelp.Size = new System.Drawing.Size(307, 74);
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
            this.cboAction.Location = new System.Drawing.Point(95, 134);
            this.cboAction.Name = "cboAction";
            this.cboAction.Size = new System.Drawing.Size(307, 21);
            this.cboAction.TabIndex = 2;
            // 
            // cmdOk
            // 
            this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.FlatAppearance.BorderSize = 0;
            this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdOk.Location = new System.Drawing.Point(102, 204);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(106, 27);
            this.cmdOk.TabIndex = 4;
            this.cmdOk.Text = "Save";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // lblProxyType
            // 
            this.lblProxyType.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyType.ForeColor = System.Drawing.Color.Black;
            this.lblProxyType.Location = new System.Drawing.Point(12, 134);
            this.lblProxyType.Name = "lblProxyType";
            this.lblProxyType.Size = new System.Drawing.Size(78, 21);
            this.lblProxyType.TabIndex = 45;
            this.lblProxyType.Text = "Action:";
            this.lblProxyType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtNotes
            // 
            this.txtNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNotes.Location = new System.Drawing.Point(95, 166);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.Size = new System.Drawing.Size(307, 20);
            this.txtNotes.TabIndex = 3;
            // 
            // lblNotes
            // 
            this.lblNotes.BackColor = System.Drawing.Color.Transparent;
            this.lblNotes.ForeColor = System.Drawing.Color.Black;
            this.lblNotes.Location = new System.Drawing.Point(12, 166);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new System.Drawing.Size(78, 20);
            this.lblNotes.TabIndex = 42;
            this.lblNotes.Text = "Notes:";
            this.lblNotes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.FlatAppearance.BorderSize = 0;
            this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdCancel.Location = new System.Drawing.Point(215, 204);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(106, 27);
            this.cmdCancel.TabIndex = 5;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // txtHost
            // 
            this.txtHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHost.Location = new System.Drawing.Point(95, 18);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(307, 20);
            this.txtHost.TabIndex = 1;
            this.txtHost.TextChanged += new System.EventHandler(this.txtHost_TextChanged);
            // 
            // lblProxyHost
            // 
            this.lblProxyHost.BackColor = System.Drawing.Color.Transparent;
            this.lblProxyHost.ForeColor = System.Drawing.Color.Black;
            this.lblProxyHost.Location = new System.Drawing.Point(15, 18);
            this.lblProxyHost.Name = "lblProxyHost";
            this.lblProxyHost.Size = new System.Drawing.Size(75, 20);
            this.lblProxyHost.TabIndex = 40;
            this.lblProxyHost.Text = "IP / Host:";
            this.lblProxyHost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsRoute
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(426, 243);
            this.Controls.Add(this.lblHostHelp);
            this.Controls.Add(this.cboAction);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.lblProxyType);
            this.Controls.Add(this.txtNotes);
            this.Controls.Add(this.lblNotes);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.txtHost);
            this.Controls.Add(this.lblProxyHost);
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