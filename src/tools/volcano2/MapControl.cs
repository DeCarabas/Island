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

    using Volcano.Model;

    public partial class MapControl : Control
    {
        ProjectCache cache;
        int cameraX;
        int cameraY;
        bool itemsVisible;
        UltimaProject project;
        int zLimit = 16;

        public MapControl()
        {
            ItemsVisible = true;
            DoubleBuffered = true;
            InitializeComponent();
        }

        [Browsable(false)]
        public ProjectCache Cache 
        {
            get
            {
                if (this.cache == null) 
                {
                    this.cache = new ProjectCache { Project = Project }; 
                }
                return this.cache;
            }
            set
            {
                this.cache = value;
            }
        }
        
        [DefaultValue(0)]
        public int CameraX 
        {
            get { return this.cameraX; }
            set 
            {
                if (value != this.cameraX)
                {
                    this.cameraX = value;
                    Invalidate();
                }
            }
        }

        [DefaultValue(0)]
        public int CameraY 
        {
            get { return this.cameraY; }
            set
            {
                if (value != this.cameraY)
                {
                    this.cameraY = value;
                    Invalidate();
                }
            }
        }

        [DefaultValue(true)]
        public bool ItemsVisible 
        {
            get { return this.itemsVisible; }
            set
            {
                if (value != this.itemsVisible)
                {
                    this.itemsVisible = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public UltimaProject Project
        {
            get { return this.project; }
            set
            {
                this.project = value;
                if (this.cache != null) { this.cache.Project = value; }
                Invalidate();
            }
        }
        
        [DefaultValue(16)]
        public int ZLimit 
        {
            get { return this.zLimit; }
            set 
            { 
                this.zLimit = value; 
                Invalidate(); 
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (this.project != null)
            {
                Rectangle worldView = new Rectangle(cameraX, cameraY, Width, Height);

                RenderTerrain(pe.Graphics, worldView);
                RenderMapObjects(pe.Graphics, worldView);
            }
            base.OnPaint(pe);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (this.project == null) { base.OnPaintBackground(pevent); }
        }

        void RenderMapObjects(Graphics graphics, Rectangle worldView)
        {
            if (this.zLimit < 0) { return; }
            if (!this.itemsVisible) { return; }

            List<MapObject> objects = new List<MapObject>();

            // TODO: Remove this and work completely off the objects in the chunk.
            int left = worldView.Left / MapUnits.PixelsPerChunk;
            int top = worldView.Top / MapUnits.PixelsPerChunk;

            int right = (worldView.Right / MapUnits.PixelsPerChunk) + 1;
            int bottom = (worldView.Bottom / MapUnits.PixelsPerChunk) + 1;

            for (int y = Math.Max(top, 0); y < bottom; y++)
            {
                for (int x = Math.Max(left, 0); x < right; x++)
                {
                    int regionX = x / MapUnits.ChunksPerRegion;
                    int regionY = y / MapUnits.ChunksPerRegion;

                    if (regionX >= this.project.Map.Width) { continue; }
                    if (regionY >= this.project.Map.Height) { continue; }

                    int cx = x - (regionX * MapUnits.ChunksPerRegion);
                    int cy = y - (regionY * MapUnits.ChunksPerRegion);

                    List<MapObject> chunkObjects = this.project.Map[regionX, regionY][cx, cy].Objects;
                    foreach (MapObject obj in chunkObjects)
                    {
                        if (obj.Location.Z < ZLimit) { objects.Add(obj); }
                    }
                }
            }

            IList<MapObject> sortedObjects = MapObject.SortObjects(objects);
            for (int i = 0; i < sortedObjects.Count; i++)
            {
                MapObject mapObject = sortedObjects[i];

                Point screenPoint = mapObject.Location.ScreenPoint;

                // Correct for upper-left of canvas.
                //
                screenPoint.X -= worldView.Left;
                screenPoint.Y -= worldView.Top;

                // Correct for hotspot.
                //
                screenPoint.X -= mapObject.Frame.HotSpotX;
                screenPoint.Y -= mapObject.Frame.HotSpotY;

                Image objectImage = mapObject.Image;
                if (objectImage == null)
                {
                    mapObject.Image = this.cache.GetShapeImage(mapObject.Frame);
                    objectImage = mapObject.Image;
                }

                graphics.DrawImageUnscaled(objectImage, screenPoint);
            }
        }

        void RenderTerrain(Graphics graphics, Rectangle worldView)
        {
            int left = worldView.Left / MapUnits.PixelsPerChunk;
            int top = worldView.Top / MapUnits.PixelsPerChunk;

            int right = (worldView.Right / MapUnits.PixelsPerChunk) + 1;
            int bottom = (worldView.Bottom / MapUnits.PixelsPerChunk) + 1;

            for (int y = Math.Max(top, 0); y < bottom; y += 1)
            {
                for (int x = Math.Max(left, 0); x < right; x += 1)
                {
                    // What chunk are we on?
                    //
                    int regionX = x / MapUnits.ChunksPerRegion;
                    int regionY = y / MapUnits.ChunksPerRegion;

                    if (regionX >= this.project.Map.Width) { continue; }
                    if (regionY >= this.project.Map.Height) { continue; }

                    int chunkX = x - (regionX * MapUnits.ChunksPerRegion);
                    int chunkY = y - (regionY * MapUnits.ChunksPerRegion);

                    MapChunk chunk = this.project.Map[regionX, regionY][chunkX, chunkY];
                    if (chunk.Image == null)
                    {
                        chunk.Image = this.cache.GetTerrainDrawing(chunk.Template);
                    }

                    // The coordinates are *slightly* off here because, when assembling the drawing, the 
                    // relative offset of the land tiles was not taken into account. (The hotspot for a 
                    // tile is actually in the lower-right corner of the tile.)
                    //
                    // TODO: cameraX
                    //
                    int screenX = (x * MapUnits.PixelsPerChunk) - worldView.Left - MapUnits.PixelsPerTile;
                    int screenY = (y * MapUnits.PixelsPerChunk) - worldView.Top - MapUnits.PixelsPerTile;
                    graphics.DrawImageUnscaled(chunk.Image, screenX, screenY);
                }
            }
        }
    }
}
