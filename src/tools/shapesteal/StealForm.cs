using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Volcano.Model;

namespace shapesteal
{
    public partial class StealForm : Form
    {
        private PictureBox pictureBox;
        readonly UltimaProject project;
        int shapeIndex;
        int frameIndex;
        int paletteIndex;

        public StealForm(UltimaProject project)
        {
            InitializeComponent();
            this.project = project;

            SetImage();
        }

        void SetImage()
        {
            var shape = this.project.Shapes.Contents[shapeIndex];
            var frame = shape.Frames[frameIndex];
            Bitmap bitmap = frame.GetBitmap(this.project.Palettes.Contents[paletteIndex]);            
            pictureBox.Image = new Bitmap(bitmap, bitmap.Width * 8, bitmap.Height * 8);

            string tileDesc = this.project.Text.Contents[(int)frame.Shape.Id] ?? "(null)";

            this.Text = String.Format(
                "Shape {0} {1} - Frame {2} - Palette {3} - Size {4}", 
                shapeIndex, tileDesc, frameIndex, paletteIndex, shape.Size);
        }

        private void InitializeComponent()
        {
            this.pictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.Color.Black;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(1076, 820);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // StealForm
            // 
            this.ClientSize = new System.Drawing.Size(1076, 820);
            this.Controls.Add(this.pictureBox);
            this.Name = "StealForm";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.KeyReleased);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        private void KeyReleased(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    this.frameIndex = 0;
                    do
                    {
                        this.shapeIndex = (this.shapeIndex - 1);
                        if (this.shapeIndex < 0) { this.shapeIndex = this.project.Shapes.Count - 1; }
                    } while (this.project.Shapes.Contents[this.shapeIndex] == null);
                    SetImage();
                    e.Handled = true;
                    break;

                case Keys.Down:
                    this.frameIndex = 0;
                    do
                    {
                        this.shapeIndex = (this.shapeIndex + 1) % this.project.Shapes.Count;
                    } while (this.project.Shapes.Contents[this.shapeIndex] == null);
                    SetImage();
                    e.Handled = true;
                    break;

                case Keys.Left:
                    this.frameIndex = (this.frameIndex - 1);
                    if (this.frameIndex < 0)
                    {
                        var shape = this.project.Shapes.Contents[this.shapeIndex];
                        this.frameIndex = shape.Frames.Length - 1;
                    }
                    SetImage();
                    e.Handled = true;
                    break;

                case Keys.Right:
                    {
                        var shape = this.project.Shapes.Contents[this.shapeIndex];
                        this.frameIndex = (this.frameIndex + 1) % shape.Frames.Length;
                    }
                    SetImage();
                    e.Handled = true;
                    break;

                case Keys.PageUp:
                    do
                    {
                        this.paletteIndex = (this.paletteIndex - 1);
                        if (this.paletteIndex < 0) { this.paletteIndex = this.project.Palettes.Count - 1; }
                    } while (this.project.Palettes.Contents[this.paletteIndex] == null);
                    SetImage();
                    e.Handled = true;
                    break;

                case Keys.PageDown:
                    do
                    {
                        this.paletteIndex = (this.paletteIndex + 1) % this.project.Palettes.Count;
                    } while (this.project.Palettes.Contents[this.paletteIndex] == null);                    
                    SetImage();
                    e.Handled = true;
                    break;
            }
        }
    }
}
