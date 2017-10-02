namespace Eddie.Forms.Forms
{
    partial class WindowProviderAdd
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
			this.cboProvider = new Eddie.Forms.Skin.ComboBox();
			this.cmdOk = new Eddie.Forms.Skin.Button();
			this.lblProvider = new Eddie.Forms.Skin.Label();
			this.cmdCancel = new Eddie.Forms.Skin.Button();
			this.SuspendLayout();
			// 
			// cboProvider
			// 
			this.cboProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboProvider.FormattingEnabled = true;
			this.cboProvider.Location = new System.Drawing.Point(95, 14);
			this.cboProvider.Name = "cboProvider";
			this.cboProvider.Size = new System.Drawing.Size(533, 21);
			this.cboProvider.TabIndex = 2;
			// 
			// cmdOk
			// 
			this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.Location = new System.Drawing.Point(215, 55);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(106, 27);
			this.cmdOk.TabIndex = 4;
			this.cmdOk.Text = "Save";
			this.cmdOk.UseVisualStyleBackColor = true;
			this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
			// 
			// lblProvider
			// 
			this.lblProvider.BackColor = System.Drawing.Color.Transparent;
			this.lblProvider.ForeColor = System.Drawing.Color.Black;
			this.lblProvider.Location = new System.Drawing.Point(12, 14);
			this.lblProvider.Name = "lblProvider";
			this.lblProvider.Size = new System.Drawing.Size(78, 21);
			this.lblProvider.TabIndex = 45;
			this.lblProvider.Text = "Provider:";
			this.lblProvider.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmdCancel
			// 
			this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatAppearance.BorderSize = 0;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdCancel.Location = new System.Drawing.Point(328, 55);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(106, 27);
			this.cmdCancel.TabIndex = 5;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// WindowProviderAdd
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(652, 94);
			this.Controls.Add(this.cboProvider);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.lblProvider);
			this.Controls.Add(this.cmdCancel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WindowProviderAdd";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Form1";
			this.ResumeLayout(false);

        }

        #endregion

        private Skin.Button cmdOk;
        private Skin.Label lblProvider;
        private Skin.Button cmdCancel;
        private Skin.ComboBox cboProvider;
    }
}