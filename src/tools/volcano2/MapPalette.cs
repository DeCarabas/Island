using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Volcano
{
    public partial class MapPalette : Form
    {
        MapControl control;

        public MapPalette()
        {
            InitializeComponent();
        }

        public MapControl Control 
        {
            get { return this.control; }
            set
            {
                this.control = value;
                SetupControls();
            }
        }

        void SetupControls()
        {
            zLimit.Value = this.control.ZLimit;
        }

        void zLimit_Scroll(object sender, EventArgs e)
        {
            this.control.ZLimit = zLimit.Value;
        }
    }
}
