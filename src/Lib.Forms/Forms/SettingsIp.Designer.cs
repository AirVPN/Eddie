namespace Eddie.Gui.Forms
{
	partial class SettingsIp
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
            this.txtIP = new Eddie.Gui.Skin.TextBox();
            this.lblIP = new Eddie.Gui.Skin.Label();
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
            this.cmdOk.Location = new System.Drawing.Point(102, 60);
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
            this.cmdCancel.Location = new System.Drawing.Point(215, 60);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(106, 27);
            this.cmdCancel.TabIndex = 5;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // txtIP
            // 
            this.txtIP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIP.Location = new System.Drawing.Point(102, 18);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(300, 20);
            this.txtIP.TabIndex = 1;
            this.txtIP.TextChanged += new System.EventHandler(this.txtIp_TextChanged);
            // 
            // lblIP
            // 
            this.lblIP.BackColor = System.Drawing.Color.Transparent;
            this.lblIP.ForeColor = System.Drawing.Color.Black;
            this.lblIP.Location = new System.Drawing.Point(12, 18);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(84, 20);
            this.lblIP.TabIndex = 40;
            this.lblIP.Text = "IP / Host:";
            this.lblIP.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsIp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(426, 99);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.lblIP);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsIp";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

		private Skin.Button cmdOk;
        private Skin.Button cmdCancel;
        private Skin.TextBox txtIP;
		private Skin.Label lblIP;
    }
}