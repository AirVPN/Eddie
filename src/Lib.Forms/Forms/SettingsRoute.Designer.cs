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
			this.cboAction = new AirVPN.Gui.Skin.ComboBox();
			this.cmdOk = new AirVPN.Gui.Skin.Button();
			this.lblProxyType = new AirVPN.Gui.Skin.Label();
			this.txtNetMask = new AirVPN.Gui.Skin.TextBox();
			this.lblProxyPort = new AirVPN.Gui.Skin.Label();
			this.cmdCancel = new AirVPN.Gui.Skin.Button();
			this.txtHost = new AirVPN.Gui.Skin.TextBox();
			this.lblProxyHost = new AirVPN.Gui.Skin.Label();
			this.SuspendLayout();
			// 
			// cboAction
			// 
			this.cboAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboAction.FormattingEnabled = true;
			this.cboAction.Location = new System.Drawing.Point(89, 70);
			this.cboAction.Name = "cboAction";
			this.cboAction.Size = new System.Drawing.Size(141, 21);
			this.cboAction.TabIndex = 3;
			// 
			// cmdOk
			// 
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.Location = new System.Drawing.Point(24, 103);
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
			this.lblProxyType.Location = new System.Drawing.Point(27, 74);
			this.lblProxyType.Name = "lblProxyType";
			this.lblProxyType.Size = new System.Drawing.Size(57, 20);
			this.lblProxyType.TabIndex = 45;
			this.lblProxyType.Text = "Action:";
			this.lblProxyType.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtNetMask
			// 
			this.txtNetMask.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtNetMask.Location = new System.Drawing.Point(89, 44);
			this.txtNetMask.Name = "txtNetMask";
			this.txtNetMask.Size = new System.Drawing.Size(141, 20);
			this.txtNetMask.TabIndex = 2;
			// 
			// lblProxyPort
			// 
			this.lblProxyPort.BackColor = System.Drawing.Color.Transparent;
			this.lblProxyPort.ForeColor = System.Drawing.Color.Black;
			this.lblProxyPort.Location = new System.Drawing.Point(27, 47);
			this.lblProxyPort.Name = "lblProxyPort";
			this.lblProxyPort.Size = new System.Drawing.Size(57, 20);
			this.lblProxyPort.TabIndex = 42;
			this.lblProxyPort.Text = "Netmask:";
			this.lblProxyPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cmdCancel
			// 
			this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatAppearance.BorderSize = 0;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdCancel.Location = new System.Drawing.Point(130, 103);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(100, 28);
			this.cmdCancel.TabIndex = 5;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// txtHost
			// 
			this.txtHost.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtHost.Location = new System.Drawing.Point(89, 18);
			this.txtHost.Name = "txtHost";
			this.txtHost.Size = new System.Drawing.Size(141, 20);
			this.txtHost.TabIndex = 1;
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
			this.ClientSize = new System.Drawing.Size(254, 145);
			this.Controls.Add(this.cboAction);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.lblProxyType);
			this.Controls.Add(this.txtNetMask);
			this.Controls.Add(this.lblProxyPort);
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
			this.Load += new System.EventHandler(this.SettingsRoute_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Skin.Button cmdOk;
        private Skin.Label lblProxyType;
        private Skin.TextBox txtNetMask;
        private Skin.Label lblProxyPort;
        private Skin.Button cmdCancel;
        private Skin.TextBox txtHost;
        private Skin.Label lblProxyHost;
        private Skin.ComboBox cboAction;
    }
}