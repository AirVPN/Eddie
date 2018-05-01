namespace Eddie.Forms.Controls
{
	partial class Dashboard
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pnlProviders = new Eddie.Forms.Skin.Panel();
			this.pnlConnections = new Eddie.Forms.Skin.Panel();
			this.pnlSpeed = new Eddie.Forms.Skin.Panel();
			this.SuspendLayout();
			// 
			// pnlProviders
			// 
			this.pnlProviders.Location = new System.Drawing.Point(3, 371);
			this.pnlProviders.Name = "pnlProviders";
			this.pnlProviders.Size = new System.Drawing.Size(794, 126);
			this.pnlProviders.TabIndex = 0;
			// 
			// pnlConnections
			// 
			this.pnlConnections.Location = new System.Drawing.Point(399, 95);
			this.pnlConnections.Name = "pnlConnections";
			this.pnlConnections.Size = new System.Drawing.Size(398, 127);
			this.pnlConnections.TabIndex = 1;
			// 
			// pnlSpeed
			// 
			this.pnlSpeed.Location = new System.Drawing.Point(3, 95);
			this.pnlSpeed.Name = "pnlSpeed";
			this.pnlSpeed.Size = new System.Drawing.Size(390, 127);
			this.pnlSpeed.TabIndex = 2;
			// 
			// Dashboard
			// 
			this.Controls.Add(this.pnlSpeed);
			this.Controls.Add(this.pnlConnections);
			this.Controls.Add(this.pnlProviders);
			this.Name = "Dashboard";
			this.Size = new System.Drawing.Size(800, 500);
			this.ResumeLayout(false);

		}

		#endregion

		private Skin.Panel pnlProviders;
		private Skin.Panel pnlConnections;
		private Skin.Panel pnlSpeed;
	}
}
