namespace AirVPN.Gui.Forms
{
	partial class SettingsEvent
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
            this.cmdOk = new AirVPN.Gui.Skin.Button();
            this.txtArguments = new AirVPN.Gui.Skin.TextBox();
            this.lblArguments = new AirVPN.Gui.Skin.Label();
            this.cmdCancel = new AirVPN.Gui.Skin.Button();
            this.txtFileName = new AirVPN.Gui.Skin.TextBox();
            this.lblFileName = new AirVPN.Gui.Skin.Label();
            this.chkWaitEnd = new AirVPN.Gui.Skin.CheckBox();
            this.cmdExeBrowse = new AirVPN.Gui.Skin.Button();
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
            this.cmdOk.Location = new System.Drawing.Point(106, 102);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(106, 27);
            this.cmdOk.TabIndex = 5;
            this.cmdOk.Text = "Save";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // txtArguments
            // 
            this.txtArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtArguments.Location = new System.Drawing.Point(129, 43);
            this.txtArguments.Name = "txtArguments";
            this.txtArguments.Size = new System.Drawing.Size(252, 20);
            this.txtArguments.TabIndex = 3;
            // 
            // lblArguments
            // 
            this.lblArguments.BackColor = System.Drawing.Color.Transparent;
            this.lblArguments.ForeColor = System.Drawing.Color.Black;
            this.lblArguments.Location = new System.Drawing.Point(16, 43);
            this.lblArguments.Name = "lblArguments";
            this.lblArguments.Size = new System.Drawing.Size(107, 20);
            this.lblArguments.TabIndex = 42;
            this.lblArguments.Text = "Arguments:";
            this.lblArguments.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.FlatAppearance.BorderSize = 0;
            this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdCancel.Location = new System.Drawing.Point(220, 102);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(106, 27);
            this.cmdCancel.TabIndex = 6;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // txtFileName
            // 
            this.txtFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFileName.Location = new System.Drawing.Point(129, 18);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(252, 20);
            this.txtFileName.TabIndex = 1;
            // 
            // lblFileName
            // 
            this.lblFileName.BackColor = System.Drawing.Color.Transparent;
            this.lblFileName.ForeColor = System.Drawing.Color.Black;
            this.lblFileName.Location = new System.Drawing.Point(13, 18);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(110, 20);
            this.lblFileName.TabIndex = 40;
            this.lblFileName.Text = "File name:";
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkWaitEnd
            // 
            this.chkWaitEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkWaitEnd.BackColor = System.Drawing.Color.Transparent;
            this.chkWaitEnd.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkWaitEnd.ForeColor = System.Drawing.Color.Black;
            this.chkWaitEnd.Location = new System.Drawing.Point(95, 69);
            this.chkWaitEnd.Name = "chkWaitEnd";
            this.chkWaitEnd.Size = new System.Drawing.Size(286, 20);
            this.chkWaitEnd.TabIndex = 4;
            this.chkWaitEnd.Text = "Wait end of process";
            this.chkWaitEnd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkWaitEnd.UseVisualStyleBackColor = false;
            // 
            // cmdExeBrowse
            // 
            this.cmdExeBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdExeBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdExeBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdExeBrowse.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.cmdExeBrowse.FlatAppearance.BorderSize = 0;
            this.cmdExeBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdExeBrowse.Image = global::AirVPN.Lib.Forms.Properties.Resources.browse;
            this.cmdExeBrowse.Location = new System.Drawing.Point(387, 18);
            this.cmdExeBrowse.Name = "cmdExeBrowse";
            this.cmdExeBrowse.Size = new System.Drawing.Size(35, 20);
            this.cmdExeBrowse.TabIndex = 2;
            this.cmdExeBrowse.UseVisualStyleBackColor = true;
            this.cmdExeBrowse.Click += new System.EventHandler(this.cmdExeBrowse_Click);
            // 
            // SettingsEvent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(434, 142);
            this.Controls.Add(this.cmdExeBrowse);
            this.Controls.Add(this.chkWaitEnd);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.txtArguments);
            this.Controls.Add(this.lblArguments);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.txtFileName);
            this.Controls.Add(this.lblFileName);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsEvent";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Skin.Button cmdOk;
        private Skin.TextBox txtArguments;
        private Skin.Label lblArguments;
        private Skin.Button cmdCancel;
        private Skin.TextBox txtFileName;
        private Skin.Label lblFileName;
        private Skin.CheckBox chkWaitEnd;
        private Skin.Button cmdExeBrowse;
    }
}