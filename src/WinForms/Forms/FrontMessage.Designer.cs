namespace AirVPN.Gui.Forms
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
			this.cmdClose = new AirVPN.Gui.Skin.Button();
			this.lnkWebsite = new System.Windows.Forms.LinkLabel();
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
			this.cmdClose.Location = new System.Drawing.Point(189, 197);
			this.cmdClose.Name = "cmdClose";
			this.cmdClose.Size = new System.Drawing.Size(150, 30);
			this.cmdClose.TabIndex = 1;
			this.cmdClose.Text = "Ok";
			this.cmdClose.UseVisualStyleBackColor = true;
			this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
			// 
			// lnkWebsite
			// 
			this.lnkWebsite.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.lnkWebsite.AutoSize = true;
			this.lnkWebsite.BackColor = System.Drawing.Color.Transparent;
			this.lnkWebsite.Location = new System.Drawing.Point(161, 172);
			this.lnkWebsite.Name = "lnkWebsite";
			this.lnkWebsite.Size = new System.Drawing.Size(217, 13);
			this.lnkWebsite.TabIndex = 3;
			this.lnkWebsite.TabStop = true;
			this.lnkWebsite.Text = "Look https://airvpn.org for more informations";
			this.lnkWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkWebsite_LinkClicked);
			// 
			// lblMessage
			// 
			this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblMessage.BackColor = System.Drawing.Color.Transparent;
			this.lblMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblMessage.Location = new System.Drawing.Point(12, 9);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(505, 158);
			this.lblMessage.TabIndex = 4;
			this.lblMessage.Text = "Important Message";
			this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FrontMessage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(529, 239);
			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.lnkWebsite);
			this.Controls.Add(this.cmdClose);
			this.Name = "FrontMessage";
			this.Text = "FrontMessage";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Skin.Button cmdClose;
		private System.Windows.Forms.LinkLabel lnkWebsite;
		private System.Windows.Forms.Label lblMessage;
	}
}