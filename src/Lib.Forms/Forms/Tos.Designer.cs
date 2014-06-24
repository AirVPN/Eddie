namespace AirVPN.Gui.Forms
{
    partial class Tos
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
			this.txtTos = new AirVPN.Gui.Skin.TextBox();
			this.panel1 = new AirVPN.Gui.Skin.Panel();
			this.chkAccept2 = new AirVPN.Gui.Skin.CheckBox();
			this.chkAccept1 = new AirVPN.Gui.Skin.CheckBox();
			this.cmdCancel = new AirVPN.Gui.Skin.Button();
			this.cmdOk = new AirVPN.Gui.Skin.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtTos
			// 
			this.txtTos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtTos.BackColor = System.Drawing.Color.White;
			this.txtTos.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtTos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.txtTos.Location = new System.Drawing.Point(5, 4);
			this.txtTos.Margin = new System.Windows.Forms.Padding(10);
			this.txtTos.Multiline = true;
			this.txtTos.Name = "txtTos";
			this.txtTos.ReadOnly = true;
			this.txtTos.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtTos.Size = new System.Drawing.Size(577, 321);
			this.txtTos.TabIndex = 5;
			// 
			// panel1
			// 
			this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.panel1.BackColor = System.Drawing.Color.Transparent;
			this.panel1.Controls.Add(this.chkAccept2);
			this.panel1.Controls.Add(this.chkAccept1);
			this.panel1.Controls.Add(this.cmdCancel);
			this.panel1.Controls.Add(this.cmdOk);
			this.panel1.Location = new System.Drawing.Point(136, 333);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(317, 106);
			this.panel1.TabIndex = 18;
			// 
			// chkAccept2
			// 
			this.chkAccept2.AutoSize = true;
			this.chkAccept2.BackColor = System.Drawing.Color.Transparent;
			this.chkAccept2.ForeColor = System.Drawing.Color.Black;
			this.chkAccept2.Location = new System.Drawing.Point(26, 36);
			this.chkAccept2.Name = "chkAccept2";
			this.chkAccept2.Size = new System.Drawing.Size(91, 17);
			this.chkAccept2.TabIndex = 2;
			this.chkAccept2.Text = "TOS Check 2";
			this.chkAccept2.UseVisualStyleBackColor = false;
			this.chkAccept2.CheckedChanged += new System.EventHandler(this.chkAccept2_CheckedChanged);
			// 
			// chkAccept1
			// 
			this.chkAccept1.AutoSize = true;
			this.chkAccept1.BackColor = System.Drawing.Color.Transparent;
			this.chkAccept1.ForeColor = System.Drawing.Color.Black;
			this.chkAccept1.Location = new System.Drawing.Point(27, 15);
			this.chkAccept1.Name = "chkAccept1";
			this.chkAccept1.Size = new System.Drawing.Size(91, 17);
			this.chkAccept1.TabIndex = 1;
			this.chkAccept1.Text = "TOS Check 1";
			this.chkAccept1.UseVisualStyleBackColor = false;
			this.chkAccept1.CheckedChanged += new System.EventHandler(this.chkAccept1_CheckedChanged);
			// 
			// cmdCancel
			// 
			this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdCancel.FlatAppearance.BorderSize = 0;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.cmdCancel.Location = new System.Drawing.Point(164, 64);
			this.cmdCancel.Margin = new System.Windows.Forms.Padding(4);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(150, 30);
			this.cmdCancel.TabIndex = 4;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// cmdOk
			// 
			this.cmdOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.cmdOk.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.cmdOk.FlatAppearance.BorderSize = 0;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdOk.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cmdOk.Location = new System.Drawing.Point(3, 64);
			this.cmdOk.Margin = new System.Windows.Forms.Padding(4);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(150, 30);
			this.cmdOk.TabIndex = 3;
			this.cmdOk.Text = "Ok";
			this.cmdOk.UseVisualStyleBackColor = true;			
			// 
			// Tos
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(588, 444);
			this.Controls.Add(this.txtTos);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinimizeBox = false;
			this.Name = "Tos";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Air VPN - Terms of Service";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Skin.Panel panel1;
        private Skin.CheckBox chkAccept2;
        private Skin.CheckBox chkAccept1;
        private Skin.TextBox txtTos;
        private Skin.Button cmdCancel;
        private Skin.Button cmdOk;

    }
}