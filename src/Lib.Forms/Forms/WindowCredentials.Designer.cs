namespace Eddie.Forms.Forms
{
    partial class WindowCredentials
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
			this.lblPassword = new Eddie.Forms.Skin.Label();
			this.lblUsername = new Eddie.Forms.Skin.Label();
			this.txtPassword = new Eddie.Forms.Skin.TextBox();
			this.txtUsername = new Eddie.Forms.Skin.TextBox();
			this.cmdOk = new Eddie.Forms.Skin.Button();
			this.cmdCancel = new Eddie.Forms.Skin.Button();
			this.cboRemember = new Eddie.Forms.Skin.ComboBox();
			this.lblRemember = new Eddie.Forms.Skin.Label();
			this.SuspendLayout();
			// 
			// lblPassword
			// 
			this.lblPassword.BackColor = System.Drawing.Color.Transparent;
			this.lblPassword.ForeColor = System.Drawing.Color.Black;
			this.lblPassword.Location = new System.Drawing.Point(11, 46);
			this.lblPassword.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(76, 20);
			this.lblPassword.TabIndex = 61;
			this.lblPassword.Text = "Password:";
			this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblUsername
			// 
			this.lblUsername.BackColor = System.Drawing.Color.Transparent;
			this.lblUsername.ForeColor = System.Drawing.Color.Black;
			this.lblUsername.Location = new System.Drawing.Point(11, 16);
			this.lblUsername.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size(76, 20);
			this.lblUsername.TabIndex = 60;
			this.lblUsername.Text = "Username:";
			this.lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtPassword
			// 
			this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPassword.Location = new System.Drawing.Point(91, 46);
			this.txtPassword.Margin = new System.Windows.Forms.Padding(2);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(222, 20);
			this.txtPassword.TabIndex = 58;
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			// 
			// txtUsername
			// 
			this.txtUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtUsername.Location = new System.Drawing.Point(91, 16);
			this.txtUsername.Margin = new System.Windows.Forms.Padding(2);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(222, 20);
			this.txtUsername.TabIndex = 57;
			this.txtUsername.TextChanged += new System.EventHandler(this.txtUsername_TextChanged);
			// 
			// cmdOk
			// 
			this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.Location = new System.Drawing.Point(53, 128);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(106, 27);
			this.cmdOk.TabIndex = 62;
			this.cmdOk.Text = "Login";
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
			this.cmdCancel.Location = new System.Drawing.Point(166, 128);
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
			this.cboRemember.Location = new System.Drawing.Point(91, 76);
			this.cboRemember.Name = "cboRemember";
			this.cboRemember.Size = new System.Drawing.Size(221, 21);
			this.cboRemember.TabIndex = 65;
			// 
			// lblRemember
			// 
			this.lblRemember.BackColor = System.Drawing.Color.Transparent;
			this.lblRemember.ForeColor = System.Drawing.Color.Black;
			this.lblRemember.Location = new System.Drawing.Point(12, 76);
			this.lblRemember.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblRemember.Name = "lblRemember";
			this.lblRemember.Size = new System.Drawing.Size(76, 21);
			this.lblRemember.TabIndex = 66;
			this.lblRemember.Text = "Remember:";
			this.lblRemember.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WindowCredentials
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(324, 174);
			this.Controls.Add(this.lblRemember);
			this.Controls.Add(this.cboRemember);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.lblPassword);
			this.Controls.Add(this.lblUsername);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.txtUsername);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WindowCredentials";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		#endregion

		private Skin.Label lblPassword;
		private Skin.Label lblUsername;
		private Skin.TextBox txtPassword;
		public Skin.TextBox txtUsername;
		private Skin.Button cmdOk;
		private Skin.Button cmdCancel;
		private Skin.ComboBox cboRemember;
		private Skin.Label lblRemember;
	}
}