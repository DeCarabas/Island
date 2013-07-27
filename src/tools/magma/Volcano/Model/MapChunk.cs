namespace Volcano.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    // Represents an actual physical chunk.
    //
    public class MapChunk
    {
        MapPoint location;
        List<MapObject> objects = new List<MapObject>();
        bool templateObjectsCreated = false;
        ChunkTemplate template = null;

        public MapPoint Location
        {
            get
            {
                if (this.location == null) { this.location = new MapPoint(); }
                return this.location;
            }
            set
            {
                this.location = value;
            }
        }
        public Image Image { get; set; }
        public List<MapObject> Objects 
        { 
            get 
            {
                if (!this.templateObjectsCreated)
                {
                    ChunkTemplate chunk = Template;
                    for (int ty = 0; ty < chunk.Height; ty++)
                    {
                        for (int tx = 0; tx < chunk.Width; tx++)
                        {
                            Frame frame = chunk[tx, ty];
                            if (frame.FrameType != FrameType.Shape) { continue; }

                            this.objects.Add(new ChunkMapObject
                            {
                                Frame = frame,
                                Location = new MapPoint
                                {
                                    X = this.location.X + (tx * MapUnits.PixelsPerTile),
                                    Y = this.location.Y + (ty * MapUnits.PixelsPerTile),
                                }
                            });
                        }
                    }
                    
                    this.templateObjectsCreated = true;
                }

                return this.objects; 
            } 
        }
        public ChunkTemplate Template 
        {
            get { return this.template; }
            set
            {
                this.template = value;
                ClearChunkObjects();
            }
        }

        public void ClearChunkObjects()
        {
            this.objects.RemoveAll(obj => obj is ChunkMapObject);
            this.templateObjectsCreated = false;
        }
    }
}
