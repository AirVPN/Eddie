namespace Eddie.Forms.Forms
{
    partial class WindowShellExternalPermission
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
			this.lblHostHelp = new Eddie.Forms.Skin.Label();
			this.cmdNo = new Eddie.Forms.Skin.Button();
			this.cmdYes = new Eddie.Forms.Skin.Button();
			this.cmdRuleSign = new Eddie.Forms.Skin.Button();
			this.cmdRuleHash = new Eddie.Forms.Skin.Button();
			this.cmdRulePath = new Eddie.Forms.Skin.Button();
			this.cmdRuleAll = new Eddie.Forms.Skin.Button();
			this.lblConnectedCountry = new Eddie.Forms.Skin.Label();
			this.label1 = new Eddie.Forms.Skin.Label();
			this.label2 = new Eddie.Forms.Skin.Label();
			this.label3 = new Eddie.Forms.Skin.Label();
			this.label4 = new Eddie.Forms.Skin.Label();
			this.label5 = new Eddie.Forms.Skin.Label();
			this.SuspendLayout();
			// 
			// lblHostHelp
			// 
			this.lblHostHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblHostHelp.BackColor = System.Drawing.Color.Transparent;
			this.lblHostHelp.ForeColor = System.Drawing.Color.Black;
			this.lblHostHelp.Location = new System.Drawing.Point(13, 9);
			this.lblHostHelp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHostHelp.Name = "lblHostHelp";
			this.lblHostHelp.Size = new System.Drawing.Size(752, 119);
			this.lblHostHelp.TabIndex = 46;
			this.lblHostHelp.Text = "Help";
			this.lblHostHelp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cmdNo
			// 
			this.cmdNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdNo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.cmdNo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdNo.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdNo.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdNo.FlatAppearance.BorderSize = 0;
			this.cmdNo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdNo.Location = new System.Drawing.Point(75, 151);
			this.cmdNo.Margin = new System.Windows.Forms.Padding(4);
			this.cmdNo.Name = "cmdNo";
			this.cmdNo.Size = new System.Drawing.Size(680, 120);
			this.cmdNo.TabIndex = 47;
			this.cmdNo.Text = "Not allowed.";
			this.cmdNo.UseVisualStyleBackColor = false;
			this.cmdNo.Click += new System.EventHandler(this.cmdNo_Click);
			// 
			// cmdYes
			// 
			this.cmdYes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdYes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdYes.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdYes.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdYes.FlatAppearance.BorderSize = 0;
			this.cmdYes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdYes.Location = new System.Drawing.Point(75, 291);
			this.cmdYes.Margin = new System.Windows.Forms.Padding(4);
			this.cmdYes.Name = "cmdYes";
			this.cmdYes.Size = new System.Drawing.Size(680, 120);
			this.cmdYes.TabIndex = 48;
			this.cmdYes.Text = "Allow only this time.";
			this.cmdYes.UseVisualStyleBackColor = true;
			this.cmdYes.Click += new System.EventHandler(this.cmdYes_Click);
			// 
			// cmdRuleSign
			// 
			this.cmdRuleSign.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdRuleSign.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdRuleSign.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdRuleSign.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdRuleSign.FlatAppearance.BorderSize = 0;
			this.cmdRuleSign.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdRuleSign.Location = new System.Drawing.Point(75, 430);
			this.cmdRuleSign.Margin = new System.Windows.Forms.Padding(4);
			this.cmdRuleSign.Name = "cmdRuleSign";
			this.cmdRuleSign.Size = new System.Drawing.Size(680, 120);
			this.cmdRuleSign.TabIndex = 49;
			this.cmdRuleSign.Text = "Always allow the signature\\nblabla";
			this.cmdRuleSign.UseVisualStyleBackColor = true;
			this.cmdRuleSign.Click += new System.EventHandler(this.cmdRuleSign_Click);
			// 
			// cmdRuleHash
			// 
			this.cmdRuleHash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdRuleHash.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdRuleHash.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdRuleHash.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdRuleHash.FlatAppearance.BorderSize = 0;
			this.cmdRuleHash.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdRuleHash.Location = new System.Drawing.Point(75, 570);
			this.cmdRuleHash.Margin = new System.Windows.Forms.Padding(4);
			this.cmdRuleHash.Name = "cmdRuleHash";
			this.cmdRuleHash.Size = new System.Drawing.Size(680, 120);
			this.cmdRuleHash.TabIndex = 50;
			this.cmdRuleHash.Text = "Always allow the hash\\nbbbb";
			this.cmdRuleHash.UseVisualStyleBackColor = true;
			this.cmdRuleHash.Click += new System.EventHandler(this.cmdRuleHash_Click);
			// 
			// cmdRulePath
			// 
			this.cmdRulePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdRulePath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdRulePath.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdRulePath.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdRulePath.FlatAppearance.BorderSize = 0;
			this.cmdRulePath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdRulePath.Location = new System.Drawing.Point(75, 710);
			this.cmdRulePath.Margin = new System.Windows.Forms.Padding(4);
			this.cmdRulePath.Name = "cmdRulePath";
			this.cmdRulePath.Size = new System.Drawing.Size(680, 120);
			this.cmdRulePath.TabIndex = 51;
			this.cmdRulePath.Text = "Always allow the path\\nbvbbb";
			this.cmdRulePath.UseVisualStyleBackColor = true;
			this.cmdRulePath.Click += new System.EventHandler(this.cmdRulePath_Click);
			// 
			// cmdRuleAll
			// 
			this.cmdRuleAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdRuleAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdRuleAll.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdRuleAll.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdRuleAll.FlatAppearance.BorderSize = 0;
			this.cmdRuleAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdRuleAll.Location = new System.Drawing.Point(75, 849);
			this.cmdRuleAll.Margin = new System.Windows.Forms.Padding(4);
			this.cmdRuleAll.Name = "cmdRuleAll";
			this.cmdRuleAll.Size = new System.Drawing.Size(680, 120);
			this.cmdRuleAll.TabIndex = 52;
			this.cmdRuleAll.Text = "Always allow all";
			this.cmdRuleAll.UseVisualStyleBackColor = true;
			this.cmdRuleAll.Click += new System.EventHandler(this.cmdRuleAll_Click);
			// 
			// lblConnectedCountry
			// 
			this.lblConnectedCountry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblConnectedCountry.BackColor = System.Drawing.Color.Transparent;
			this.lblConnectedCountry.Image = global::Eddie.Forms.Properties.Resources.netlock_off;
			this.lblConnectedCountry.Location = new System.Drawing.Point(19, 187);
			this.lblConnectedCountry.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectedCountry.Name = "lblConnectedCountry";
			this.lblConnectedCountry.Size = new System.Drawing.Size(48, 48);
			this.lblConnectedCountry.TabIndex = 53;
			this.lblConnectedCountry.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Image = global::Eddie.Forms.Properties.Resources.netlock_on;
			this.label1.Location = new System.Drawing.Point(19, 327);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 48);
			this.label1.TabIndex = 54;
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Image = global::Eddie.Forms.Properties.Resources.netlock_on;
			this.label2.Location = new System.Drawing.Point(19, 466);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 48);
			this.label2.TabIndex = 55;
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Image = global::Eddie.Forms.Properties.Resources.netlock_on;
			this.label3.Location = new System.Drawing.Point(19, 606);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(48, 48);
			this.label3.TabIndex = 56;
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Image = global::Eddie.Forms.Properties.Resources.netlock_on;
			this.label4.Location = new System.Drawing.Point(19, 746);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(48, 48);
			this.label4.TabIndex = 57;
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.BackColor = System.Drawing.Color.Transparent;
			this.label5.Image = global::Eddie.Forms.Properties.Resources.netlock_on;
			this.label5.Location = new System.Drawing.Point(19, 885);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(48, 48);
			this.label5.TabIndex = 58;
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// WindowShellExternalPermission
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(778, 995);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblConnectedCountry);
			this.Controls.Add(this.cmdRuleAll);
			this.Controls.Add(this.cmdRulePath);
			this.Controls.Add(this.cmdRuleHash);
			this.Controls.Add(this.cmdRuleSign);
			this.Controls.Add(this.cmdYes);
			this.Controls.Add(this.cmdNo);
			this.Controls.Add(this.lblHostHelp);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WindowShellExternalPermission";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Form1";
			this.ResumeLayout(false);

        }

        #endregion
		private Skin.Label lblHostHelp;
		private Skin.Button cmdNo;
		private Skin.Button cmdYes;
		private Skin.Button cmdRuleSign;
		private Skin.Button cmdRuleHash;
		private Skin.Button cmdRulePath;
		private Skin.Button cmdRuleAll;
		private Skin.Label lblConnectedCountry;
		private Skin.Label label1;
		private Skin.Label label2;
		private Skin.Label label3;
		private Skin.Label label4;
		private Skin.Label label5;
	}
}