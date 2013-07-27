namespace Volcano
{
    partial class MapPalette
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
            this.zLimit = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.zLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // zLimit
            // 
            this.zLimit.Location = new System.Drawing.Point(12, 12);
            this.zLimit.Maximum = 16;
            this.zLimit.Minimum = -1;
            this.zLimit.Name = "zLimit";
            this.zLimit.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.zLimit.Size = new System.Drawing.Size(45, 156);
            this.zLimit.TabIndex = 1;
            this.zLimit.TickFrequency = 0;
            this.zLimit.TickStyle = System.Windows.Forms.TickStyle.None;
            this.zLimit.Scroll += new System.EventHandler(this.zLimit_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 171);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Z Cutoff";
            // 
            // MapPalette
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(54, 201);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.zLimit);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MapPalette";
            this.Text = "Map";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.zLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar zLimit;
        private System.Windows.Forms.Label label1;

    }
}