namespace Eddie.Forms.Forms
{
    partial class WindowMessage
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
			this.cmdOk = new Eddie.Forms.Skin.Button();
			this.lblIcon = new System.Windows.Forms.Label();
			this.lblMessage = new System.Windows.Forms.Label();
			this.cmdYes = new Eddie.Forms.Skin.Button();
			this.cmdNo = new Eddie.Forms.Skin.Button();
			this.SuspendLayout();
			// 
			// cmdOk
			// 
			this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.Location = new System.Drawing.Point(334, 145);
			this.cmdOk.Margin = new System.Windows.Forms.Padding(4);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(159, 40);
			this.cmdOk.TabIndex = 4;
			this.cmdOk.Text = "Ok";
			this.cmdOk.UseVisualStyleBackColor = true;
			this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
			// 
			// lblIcon
			// 
			this.lblIcon.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblIcon.BackColor = System.Drawing.Color.Transparent;
			this.lblIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.lblIcon.Image = global::Eddie.Forms.Properties.Resources.log_error;
			this.lblIcon.Location = new System.Drawing.Point(13, 43);
			this.lblIcon.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblIcon.Name = "lblIcon";
			this.lblIcon.Size = new System.Drawing.Size(64, 64);
			this.lblIcon.TabIndex = 71;
			// 
			// lblMessage
			// 
			this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblMessage.BackColor = System.Drawing.Color.Transparent;
			this.lblMessage.ForeColor = System.Drawing.Color.Black;
			this.lblMessage.Location = new System.Drawing.Point(84, 9);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(409, 128);
			this.lblMessage.TabIndex = 72;
			this.lblMessage.Text = "fkds ls klòdfòl dsòlkf òdslkaf òdlsafòld sòlafk dsòlkafòkldskòflsdòk lfkòdslkf sd" +
    "òlf òlksdfòl kdsòlf òldsfòkl dsfa dsfds af sdf";
			this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cmdYes
			// 
			this.cmdYes.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdYes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdYes.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.cmdYes.FlatAppearance.BorderSize = 0;
			this.cmdYes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdYes.Location = new System.Drawing.Point(88, 145);
			this.cmdYes.Margin = new System.Windows.Forms.Padding(4);
			this.cmdYes.Name = "cmdYes";
			this.cmdYes.Size = new System.Drawing.Size(159, 40);
			this.cmdYes.TabIndex = 73;
			this.cmdYes.Text = "Y";
			this.cmdYes.UseVisualStyleBackColor = true;
			this.cmdYes.Click += new System.EventHandler(this.cmdYes_Click);
			// 
			// cmdNo
			// 
			this.cmdNo.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdNo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdNo.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdNo.DialogResult = System.Windows.Forms.DialogResult.No;
			this.cmdNo.FlatAppearance.BorderSize = 0;
			this.cmdNo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdNo.Location = new System.Drawing.Point(255, 145);
			this.cmdNo.Margin = new System.Windows.Forms.Padding(4);
			this.cmdNo.Name = "cmdNo";
			this.cmdNo.Size = new System.Drawing.Size(159, 40);
			this.cmdNo.TabIndex = 74;
			this.cmdNo.Text = "N";
			this.cmdNo.UseVisualStyleBackColor = true;
			this.cmdNo.Click += new System.EventHandler(this.cmdNo_Click);
			// 
			// WindowMessage
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdOk;
			this.ClientSize = new System.Drawing.Size(505, 198);
			this.Controls.Add(this.cmdNo);
			this.Controls.Add(this.cmdYes);
			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.lblIcon);
			this.Controls.Add(this.cmdOk);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WindowMessage";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

        }

        #endregion

        private Skin.Button cmdOk;
		private System.Windows.Forms.Label lblIcon;
		private System.Windows.Forms.Label lblMessage;
		private Skin.Button cmdYes;
		private Skin.Button cmdNo;
	}
}