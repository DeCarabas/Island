namespace Volcano
{
    partial class MapWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapWindow));
            this.mapView = new Volcano.MapControl();
            this.SuspendLayout();
            // 
            // mapView
            // 
            this.mapView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapView.Location = new System.Drawing.Point(0, 0);
            this.mapView.Name = "mapView";
            this.mapView.Project = null;
            this.mapView.Size = new System.Drawing.Size(1125, 609);
            this.mapView.TabIndex = 1;
            this.mapView.Text = "mapControl1";
            this.mapView.Paint += new System.Windows.Forms.PaintEventHandler(this.OnMapPaint);
            this.mapView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMapMouseMove);
            this.mapView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMapMouseDown);
            this.mapView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnMapKeyPress);
            this.mapView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMapMouseUp);
            // 
            // MapWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1125, 609);
            this.Controls.Add(this.mapView);
            this.DoubleBuffered = true;
            this.Name = "MapWindow";
            this.Text = "Volcano";
            this.ResumeLayout(false);
        }

        #endregion

        private MapControl mapViewer;
        private MapControl mapView;
    }
}

