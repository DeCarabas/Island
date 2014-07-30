namespace Volcano
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing.Imaging;
    using Volcano.Model;

    public partial class MapWindow : Form
    {
        const bool itemsWhileDragging = false;

        ProjectCache cache = new ProjectCache();
        Point oldLocation;
        MapPalette palette;
        UltimaProject project = new UltimaProject { GameDirectory = @"c:\src\island\games\u7" };

        public MapWindow()
        {
            InitializeComponent();
            this.project.Load();
            this.cache.Project = this.project;

            this.mapView.Project = this.project;
            this.mapView.Cache = this.cache;
            this.mapView.CameraX = 589 * 8;
            this.mapView.CameraY = 2551 * 8;

            this.palette = new MapPalette 
            { 
                Control = this.mapView, 
            };
            this.palette.Show();
        }

        void OnMapKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '<')
            {
                this.mapView.ZLimit--;
            }
            else if (e.KeyChar == '>')
            {
                this.mapView.ZLimit++;
            }
            e.Handled = true;
        }

        void OnMapMouseDown(object sender, MouseEventArgs e)
        {
            this.oldLocation = e.Location;
            this.mapView.Capture = true;
        }

        void OnMapMouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                this.mapView.ItemsVisible = itemsWhileDragging;
                this.mapView.CameraX -= e.Location.X - this.oldLocation.X;
                this.mapView.CameraY -= e.Location.Y - this.oldLocation.Y;
                this.oldLocation = e.Location;
            }
        }

        void OnMapMouseUp(object sender, MouseEventArgs e)
        {
            this.mapView.ItemsVisible = true;
            this.mapView.Capture = false;
        }

        void OnMapPaint(object sender, PaintEventArgs e)
        {
            Text = String.Format(
                "Volcano: {0} tcache - {1} icache - {2} zlimit",
                this.cache.TerrainCacheSize,
                this.cache.ImageCacheSize,
                this.mapView.ZLimit);
        }
    }
}
