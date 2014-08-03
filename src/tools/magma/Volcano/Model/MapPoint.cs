namespace Volcano.Model
{
    using System.ComponentModel;
    using System.Drawing;
    
    /// <summary>
    /// A point on the map.
    /// </summary>
    public class MapPoint : INotifyPropertyChanged
    {
        int x, y, z;

        // NOTE: X and Y are in pixels. Exult forces them to tile boundaries, but we leave them as pixels. For fun.
        //       (And also profit.)
        //
        //       Z is always in what Exult calls "lifts", which are a mythical vertical tile. There seems to be no 
        //       benefit to converting them into any kind of screen unit, so we don't.
        //
        /// <summary>
        /// The absolute X position of the object in the world, in pixels.
        /// </summary>
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
        /// <summary>
        /// The absolute Y position of the object, in pixels.
        /// </summary>
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
        /// <summary>
        /// The Z position of the object, in "lifts", which are an arbitrary unit representing a vertical tile.
        /// </summary>
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

        /// <summary>
        /// The X position, in absolute tiles.
        /// </summary>
        public int TileX { get { return X / MapUnits.PixelsPerTile; } }

        /// <summary>
        /// The Y position, in absolute tiles.
        /// </summary>
        public int TileY { get { return Y / MapUnits.PixelsPerTile; } }

        /// <summary>
        /// The X coordinate of this point, in regions.
        /// </summary>
        public int WorldX { get { return X / MapUnits.PixelsPerRegion; } }
        /// <summary>
        /// The Y coordinate of this point, in regions.
        /// </summary>
        public int WorldY { get { return Y / MapUnits.PixelsPerRegion; } }

        /// <summary>
        /// The X pixel offset of this point in its region.
        /// </summary>
        public int RegionOffsetX { get { return X - (WorldX * MapUnits.PixelsPerRegion); } }
        /// <summary>
        /// The Y pixel offset of this point in its region.
        /// </summary>
        public int RegionOffsetY { get { return Y - (WorldY * MapUnits.PixelsPerRegion); } }

        /// <summary>
        /// The X coordinate of this point, in chunks (relative to the region).
        /// </summary>
        public int RegionX { get { return RegionOffsetX / MapUnits.PixelsPerChunk; } }

        /// <summary>
        /// The Y coordinate of this point, in chunks (relative to the region).
        /// </summary>        
        public int RegionY { get { return RegionOffsetY / MapUnits.PixelsPerChunk; } }

        /// <summary>
        /// The location of this point in the screen, after projecting height, in pixels.
        /// </summary>
        public Point ScreenPoint { get { return new Point(X - (Z * 4), Y - (Z * 4)); } }

        public event PropertyChangedEventHandler PropertyChanged;

        void Notify(string property)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        }
    }
}
