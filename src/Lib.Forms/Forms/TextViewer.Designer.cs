namespace Eddie.Forms.Forms
{
    partial class TextViewer
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
			this.txtData = new Eddie.Forms.Skin.TextBox();
			this.cmdOk = new Eddie.Forms.Skin.Button();
			this.SuspendLayout();
			// 
			// txtData
			// 
			this.txtData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtData.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtData.Location = new System.Drawing.Point(13, 14);
			this.txtData.Margin = new System.Windows.Forms.Padding(4);
			this.txtData.Multiline = true;
			this.txtData.Name = "txtData";
			this.txtData.ReadOnly = true;
			this.txtData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtData.Size = new System.Drawing.Size(912, 401);
			this.txtData.TabIndex = 0;
			this.txtData.WordWrap = false;
			// 
			// cmdOk
			// 
			this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cmdOk.Location = new System.Drawing.Point(389, 423);
			this.cmdOk.Margin = new System.Windows.Forms.Padding(4);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(160, 30);
			this.cmdOk.TabIndex = 2;
			this.cmdOk.Text = "Close";
			this.cmdOk.UseVisualStyleBackColor = true;
			// 
			// TextViewer
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cmdOk;
			this.ClientSize = new System.Drawing.Size(939, 466);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.txtData);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "TextViewer";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Skin.TextBox txtData;
		private Skin.Button cmdOk;
	}
}