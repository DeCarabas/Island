namespace Volcano.Model
{
    using System.ComponentModel;
    using System.Drawing;
    
    // This is a class because structs are hard to manipulate directly, and I didn't want an explosion of properties
    // in order to manipulate positions.
    //
    public class MapPoint : INotifyPropertyChanged
    {
        int x, y, z;

        // NOTE: X and Y are in pixels. Exult forces them to tile boundaries, but we leave them as pixels. For fun.
        //       (And also profit.)
        //
        //       Z is always in what Exult calls "lifts", which are a mythical vertical tile. There seems to be no 
        //       benefit to converting them into any kind of screen unit, so we don't.
        //
        public int X 
        {
            get { return this.x; }
            set
            {
                this.x = value;

                Notify("X");
                Notify("TileX");
                Notify("WorldX");
                Notify("RegionOffsetX");
                Notify("RegionX");
                Notify("ScreenPoint");
            }
        }
        public int Y 
        {
            get { return this.y; }
            set
            {
                this.y = value;

                Notify("Y");
                Notify("TileY");
                Notify("WorldY");
                Notify("RegionOffsetY");
                Notify("RegionY");
                Notify("ScreenPoint");
            }
        }
        public int Z 
        {
            get { return this.z; }
            set
            {
                this.z = value;

                Notify("Z");
                Notify("ScreenPoint");
            }
        }

        public int TileX { get { return X / MapUnits.PixelsPerTile; } }
        public int TileY { get { return Y / MapUnits.PixelsPerTile; } }

        public int WorldX { get { return X / MapUnits.PixelsPerRegion; } }
        public int WorldY { get { return Y / MapUnits.PixelsPerRegion; } }

        public int RegionOffsetX { get { return X - (WorldX * MapUnits.PixelsPerRegion); } }
        public int RegionOffsetY { get { return Y - (WorldY * MapUnits.PixelsPerRegion); } }

        public int RegionX { get { return RegionOffsetX / MapUnits.PixelsPerChunk; } }
        public int RegionY { get { return RegionOffsetY / MapUnits.PixelsPerChunk; } }

        public Point ScreenPoint { get { return new Point(X - (Z * 4), Y - (Z * 4)); } }

        public event PropertyChangedEventHandler PropertyChanged;

        void Notify(string property)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        }
    }
}
