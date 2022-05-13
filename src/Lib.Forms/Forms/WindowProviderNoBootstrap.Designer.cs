﻿namespace Eddie.Forms.Forms
{
    partial class WindowProviderNoBootstrap
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
			this.cmdCancel = new Eddie.Forms.Skin.Button();
			this.txtManualUrls = new Eddie.Forms.Skin.TextBox();
			this.lblBody = new Eddie.Forms.Skin.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.chkDontShowAgain = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
			this.cmdOk.Location = new System.Drawing.Point(302, 306);
			this.cmdOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(159, 40);
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
			this.cmdCancel.Location = new System.Drawing.Point(471, 306);
			this.cmdCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(159, 40);
			this.cmdCancel.TabIndex = 5;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// txtManualUrls
			// 
			this.txtManualUrls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtManualUrls.Location = new System.Drawing.Point(165, 225);
			this.txtManualUrls.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtManualUrls.Name = "txtManualUrls";
			this.txtManualUrls.Size = new System.Drawing.Size(751, 26);
			this.txtManualUrls.TabIndex = 6;
			// 
			// lblBody
			// 
			this.lblBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblBody.BackColor = System.Drawing.Color.Transparent;
			this.lblBody.ForeColor = System.Drawing.Color.Black;
			this.lblBody.Location = new System.Drawing.Point(160, 14);
			this.lblBody.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblBody.Name = "lblBody";
			this.lblBody.Size = new System.Drawing.Size(758, 205);
			this.lblBody.TabIndex = 65;
			this.lblBody.Text = "Title:";
			this.lblBody.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.BackgroundImage = global::Eddie.Forms.Properties.Resources.log_error;
			this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pictureBox1.Location = new System.Drawing.Point(18, 14);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(134, 154);
			this.pictureBox1.TabIndex = 66;
			this.pictureBox1.TabStop = false;
			// 
			// chkDontShowAgain
			// 
			this.chkDontShowAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkDontShowAgain.BackColor = System.Drawing.Color.Transparent;
			this.chkDontShowAgain.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.chkDontShowAgain.Location = new System.Drawing.Point(524, 263);
			this.chkDontShowAgain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.chkDontShowAgain.Name = "chkDontShowAgain";
			this.chkDontShowAgain.Size = new System.Drawing.Size(394, 33);
			this.chkDontShowAgain.TabIndex = 67;
			this.chkDontShowAgain.Text = "Don\'t show this message again";
			this.chkDontShowAgain.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.chkDontShowAgain.UseVisualStyleBackColor = false;
			// 
			// WindowProviderNoBootstrap
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(936, 364);
			this.Controls.Add(this.chkDontShowAgain);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.lblBody);
			this.Controls.Add(this.txtManualUrls);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.cmdCancel);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WindowProviderNoBootstrap";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		private void LblTitle_Click(object sender, System.EventArgs e)
		{
			throw new System.NotImplementedException();
		}

		#endregion

		private Skin.Button cmdOk;
        private Skin.Button cmdCancel;
		private Skin.TextBox txtManualUrls;
		private Skin.Label lblBody;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.CheckBox chkDontShowAgain;
	}
}