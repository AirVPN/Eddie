namespace AirVPN.Gui
{
	partial class ChartTest
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
			this.numDown = new System.Windows.Forms.NumericUpDown();
			this.numUp = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.numDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numUp)).BeginInit();
			this.SuspendLayout();
			// 
			// numDown
			// 
			this.numDown.Location = new System.Drawing.Point(43, 22);
			this.numDown.Maximum = new decimal(new int[] {
            1215752192,
            23,
            0,
            0});
			this.numDown.Name = "numDown";
			this.numDown.Size = new System.Drawing.Size(120, 20);
			this.numDown.TabIndex = 0;
			// 
			// numUp
			// 
			this.numUp.Location = new System.Drawing.Point(43, 48);
			this.numUp.Maximum = new decimal(new int[] {
            1215752192,
            23,
            0,
            0});
			this.numUp.Name = "numUp";
			this.numUp.Size = new System.Drawing.Size(120, 20);
			this.numUp.TabIndex = 1;
			// 
			// ChartTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(338, 116);
			this.Controls.Add(this.numUp);
			this.Controls.Add(this.numDown);
			this.Name = "ChartTest";
			this.Text = "ChartTest";
			((System.ComponentModel.ISupportInitialize)(this.numDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numUp)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NumericUpDown numDown;
		private System.Windows.Forms.NumericUpDown numUp;

	}
}