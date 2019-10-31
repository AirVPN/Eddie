namespace Eddie.Forms.Forms
{
	partial class FrontMessage
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
			this.cmdClose = new Eddie.Forms.Skin.Button();
			this.lnkWebsite = new Eddie.Forms.Skin.LinkLabel();
			this.lblMessage = new System.Windows.Forms.Label();
			this.SuspendLayout();
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
			this.cmdClose.Location = new System.Drawing.Point(378, 363);
			this.cmdClose.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.cmdClose.Name = "cmdClose";
			this.cmdClose.Size = new System.Drawing.Size(300, 56);
			this.cmdClose.TabIndex = 1;
			this.cmdClose.Text = "Ok";
			this.cmdClose.UseVisualStyleBackColor = true;
			this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
			// 
			// lnkWebsite
			// 
			this.lnkWebsite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkWebsite.BackColor = System.Drawing.Color.Transparent;
			this.lnkWebsite.Location = new System.Drawing.Point(32, 318);
			this.lnkWebsite.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lnkWebsite.Name = "lnkWebsite";
			this.lnkWebsite.Size = new System.Drawing.Size(1002, 39);
			this.lnkWebsite.TabIndex = 3;
			this.lnkWebsite.TabStop = true;
			this.lnkWebsite.Text = "More";
			this.lnkWebsite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lnkWebsite.Click += new System.EventHandler(this.lnkWebsite_LinkClicked);
			// 
			// lblMessage
			// 
			this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblMessage.BackColor = System.Drawing.Color.Transparent;
			this.lblMessage.ForeColor = System.Drawing.Color.Black;
			this.lblMessage.Location = new System.Drawing.Point(24, 16);
			this.lblMessage.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(1010, 291);
			this.lblMessage.TabIndex = 4;
			this.lblMessage.Text = "Important Message";
			this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FrontMessage
			// 
			this.AcceptButton = this.cmdClose;
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdClose;
			this.ClientSize = new System.Drawing.Size(1058, 441);
			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.lnkWebsite);
			this.Controls.Add(this.cmdClose);
			this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.Name = "FrontMessage";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "FrontMessage";
			this.ResumeLayout(false);

		}

		#endregion

		private Skin.Button cmdClose;
		private Skin.LinkLabel lnkWebsite;
		private System.Windows.Forms.Label lblMessage;
	}
}