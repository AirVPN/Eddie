namespace AirVPN.Gui.Forms
{
    partial class About
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
			AirVPN.Gui.Skin.TabPage tabPage1;
			AirVPN.Gui.Skin.TabPage tabPage2;
			AirVPN.Gui.Skin.TabPage tabPage3;
			this.lblVersion = new AirVPN.Gui.Skin.Label();
			this.tabMain = new AirVPN.Gui.Skin.TabControl();
			this.label3 = new AirVPN.Gui.Skin.Label();
			this.label2 = new AirVPN.Gui.Skin.Label();
			this.lnkManual = new System.Windows.Forms.LinkLabel();
			this.lnkWebsite = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.lnkGPL = new AirVPN.Gui.Skin.LinkLabel();
			this.txtLicense = new System.Windows.Forms.TextBox();
			this.txtThirdParty = new System.Windows.Forms.TextBox();
			this.cmdClose = new AirVPN.Gui.Skin.Button();
			tabPage1 = new AirVPN.Gui.Skin.TabPage();
			tabPage2 = new AirVPN.Gui.Skin.TabPage();
			tabPage3 = new AirVPN.Gui.Skin.TabPage();
			this.tabMain.SuspendLayout();
			tabPage1.SuspendLayout();
			tabPage2.SuspendLayout();
			tabPage3.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblVersion
			// 
			this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblVersion.BackColor = System.Drawing.Color.Transparent;
			this.lblVersion.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblVersion.ForeColor = System.Drawing.Color.White;
			this.lblVersion.Location = new System.Drawing.Point(380, 59);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(198, 21);
			this.lblVersion.TabIndex = 58;
			this.lblVersion.Text = "v2.0";
			this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabMain
			// 
			this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabMain.Controls.Add(tabPage1);
			this.tabMain.Controls.Add(tabPage2);
			this.tabMain.Controls.Add(tabPage3);
			this.tabMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
			this.tabMain.ItemSize = new System.Drawing.Size(150, 20);
			this.tabMain.Location = new System.Drawing.Point(0, 88);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(591, 336);
			this.tabMain.TabIndex = 57;
			// 
			// tabPage1
			// 
			tabPage1.BackColor = System.Drawing.Color.White;
			tabPage1.Controls.Add(this.label3);
			tabPage1.Controls.Add(this.label2);
			tabPage1.Controls.Add(this.lnkManual);
			tabPage1.Controls.Add(this.lnkWebsite);
			tabPage1.Controls.Add(this.label1);
			tabPage1.Location = new System.Drawing.Point(4, 24);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(3);
			tabPage1.Size = new System.Drawing.Size(583, 308);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "About";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(156, 130);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 23);
			this.label3.TabIndex = 5;
			this.label3.Text = "Manual:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(156, 99);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "Website: ";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lnkManual
			// 
			this.lnkManual.AutoSize = true;
			this.lnkManual.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lnkManual.Location = new System.Drawing.Point(262, 131);
			this.lnkManual.Name = "lnkManual";
			this.lnkManual.Size = new System.Drawing.Size(191, 20);
			this.lnkManual.TabIndex = 3;
			this.lnkManual.TabStop = true;
			this.lnkManual.Text = "https://airvpn.org/software";
			this.lnkManual.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkManual_LinkClicked);
			// 
			// lnkWebsite
			// 
			this.lnkWebsite.AutoSize = true;
			this.lnkWebsite.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lnkWebsite.Location = new System.Drawing.Point(262, 100);
			this.lnkWebsite.Name = "lnkWebsite";
			this.lnkWebsite.Size = new System.Drawing.Size(126, 20);
			this.lnkWebsite.TabIndex = 2;
			this.lnkWebsite.TabStop = true;
			this.lnkWebsite.Text = "https://airvpn.org";
			this.lnkWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkWebsite_LinkClicked);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(72, 37);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(432, 25);
			this.label1.TabIndex = 0;
			this.label1.Text = "AirVPN - The air to breathe the real Internet.";
			// 
			// tabPage2
			// 
			tabPage2.BackColor = System.Drawing.Color.White;
			tabPage2.Controls.Add(this.lnkGPL);
			tabPage2.Controls.Add(this.txtLicense);
			tabPage2.Location = new System.Drawing.Point(4, 24);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(3);
			tabPage2.Size = new System.Drawing.Size(583, 308);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "License";
			// 
			// lnkGPL
			// 
			this.lnkGPL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkGPL.AutoSize = true;
			this.lnkGPL.BackColor = System.Drawing.Color.Transparent;
			this.lnkGPL.ForeColor = System.Drawing.Color.Black;
			this.lnkGPL.Location = new System.Drawing.Point(338, 292);
			this.lnkGPL.Name = "lnkGPL";
			this.lnkGPL.Size = new System.Drawing.Size(228, 13);
			this.lnkGPL.TabIndex = 8;
			this.lnkGPL.TabStop = true;
			this.lnkGPL.Text = "On web: http://www.gnu.org/licenses/gpl.html";
			this.lnkGPL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGPL_LinkClicked);
			// 
			// txtLicense
			// 
			this.txtLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLicense.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtLicense.Location = new System.Drawing.Point(2, 2);
			this.txtLicense.Multiline = true;
			this.txtLicense.Name = "txtLicense";
			this.txtLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtLicense.Size = new System.Drawing.Size(566, 289);
			this.txtLicense.TabIndex = 0;
			// 
			// tabPage3
			// 
			tabPage3.BackColor = System.Drawing.Color.White;
			tabPage3.Controls.Add(this.txtThirdParty);
			tabPage3.Location = new System.Drawing.Point(4, 24);
			tabPage3.Name = "tabPage3";
			tabPage3.Size = new System.Drawing.Size(583, 308);
			tabPage3.TabIndex = 2;
			tabPage3.Text = "Libraries and tools";
			// 
			// txtThirdParty
			// 
			this.txtThirdParty.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtThirdParty.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtThirdParty.Location = new System.Drawing.Point(3, 10);
			this.txtThirdParty.Multiline = true;
			this.txtThirdParty.Name = "txtThirdParty";
			this.txtThirdParty.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtThirdParty.Size = new System.Drawing.Size(566, 289);
			this.txtThirdParty.TabIndex = 1;
			// 
			// cmdClose
			// 
			this.cmdClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdClose.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdClose.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdClose.FlatAppearance.BorderSize = 0;
			this.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdClose.Location = new System.Drawing.Point(220, 430);
			this.cmdClose.Name = "cmdClose";
			this.cmdClose.Size = new System.Drawing.Size(150, 30);
			this.cmdClose.TabIndex = 0;
			this.cmdClose.Text = "Ok";
			this.cmdClose.UseVisualStyleBackColor = true;
			this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
			// 
			// About
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(590, 468);
			this.Controls.Add(this.lblVersion);
			this.Controls.Add(this.tabMain);
			this.Controls.Add(this.cmdClose);
			this.MinimizeBox = false;
			this.Name = "About";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.tabMain.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage1.PerformLayout();
			tabPage2.ResumeLayout(false);
			tabPage2.PerformLayout();
			tabPage3.ResumeLayout(false);
			tabPage3.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Skin.Button cmdClose;
        private Skin.TabControl tabMain;
        private Skin.Label lblVersion;
        private System.Windows.Forms.TextBox txtLicense;
        private Skin.LinkLabel lnkGPL;
		private System.Windows.Forms.LinkLabel lnkWebsite;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtThirdParty;
		private Skin.Label label3;
		private Skin.Label label2;
		private System.Windows.Forms.LinkLabel lnkManual;
    }
}