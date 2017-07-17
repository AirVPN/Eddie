namespace Eddie.Gui.Forms
{
    partial class Login
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
			this.lblPassword = new Eddie.Gui.Skin.Label();
			this.lblUserName = new Eddie.Gui.Skin.Label();
			this.txtPassword = new Eddie.Gui.Skin.TextBox();
			this.txtUserName = new Eddie.Gui.Skin.TextBox();
			this.cmdOk = new Eddie.Gui.Skin.Button();
			this.cmdCancel = new Eddie.Gui.Skin.Button();
			this.cboRemember = new Eddie.Gui.Skin.ComboBox();
			this.lblRemember = new Eddie.Gui.Skin.Label();
			this.SuspendLayout();
			// 
			// lblPassword
			// 
			this.lblPassword.BackColor = System.Drawing.Color.Transparent;
			this.lblPassword.ForeColor = System.Drawing.Color.Black;
			this.lblPassword.Location = new System.Drawing.Point(11, 40);
			this.lblPassword.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(76, 20);
			this.lblPassword.TabIndex = 61;
			this.lblPassword.Text = "Password:";
			this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblUserName
			// 
			this.lblUserName.BackColor = System.Drawing.Color.Transparent;
			this.lblUserName.ForeColor = System.Drawing.Color.Black;
			this.lblUserName.Location = new System.Drawing.Point(11, 16);
			this.lblUserName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblUserName.Name = "lblUserName";
			this.lblUserName.Size = new System.Drawing.Size(76, 20);
			this.lblUserName.TabIndex = 60;
			this.lblUserName.Text = "Username:";
			this.lblUserName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtPassword
			// 
			this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPassword.Location = new System.Drawing.Point(91, 40);
			this.txtPassword.Margin = new System.Windows.Forms.Padding(2);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(222, 20);
			this.txtPassword.TabIndex = 58;
			// 
			// txtUserName
			// 
			this.txtUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtUserName.Location = new System.Drawing.Point(91, 16);
			this.txtUserName.Margin = new System.Windows.Forms.Padding(2);
			this.txtUserName.Name = "txtUserName";
			this.txtUserName.Size = new System.Drawing.Size(222, 20);
			this.txtUserName.TabIndex = 57;
			// 
			// cmdOk
			// 
			this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.Location = new System.Drawing.Point(53, 109);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(106, 27);
			this.cmdOk.TabIndex = 62;
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
			this.cmdCancel.Location = new System.Drawing.Point(166, 109);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(106, 27);
			this.cmdCancel.TabIndex = 63;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// cboRemember
			// 
			this.cboRemember.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboRemember.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboRemember.FormattingEnabled = true;
			this.cboRemember.Location = new System.Drawing.Point(91, 65);
			this.cboRemember.Name = "cboRemember";
			this.cboRemember.Size = new System.Drawing.Size(221, 21);
			this.cboRemember.TabIndex = 65;
			// 
			// lblRemember
			// 
			this.lblRemember.BackColor = System.Drawing.Color.Transparent;
			this.lblRemember.ForeColor = System.Drawing.Color.Black;
			this.lblRemember.Location = new System.Drawing.Point(12, 65);
			this.lblRemember.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblRemember.Name = "lblRemember";
			this.lblRemember.Size = new System.Drawing.Size(76, 21);
			this.lblRemember.TabIndex = 66;
			this.lblRemember.Text = "Remember:";
			this.lblRemember.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// Login
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(324, 155);
			this.Controls.Add(this.lblRemember);
			this.Controls.Add(this.cboRemember);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.lblPassword);
			this.Controls.Add(this.lblUserName);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.txtUserName);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimizeBox = false;
			this.Name = "Login";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		#endregion

		private Skin.Label lblPassword;
		private Skin.Label lblUserName;
		private Skin.TextBox txtPassword;
		public Skin.TextBox txtUserName;
		private Skin.Button cmdOk;
		private Skin.Button cmdCancel;
		private Skin.ComboBox cboRemember;
		private Skin.Label lblRemember;
	}
}