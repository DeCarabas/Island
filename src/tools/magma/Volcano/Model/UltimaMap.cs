namespace Volcano.Model
{
    using System.Collections.Generic;
    using System;

    public class UltimaMap
    {
        const int width = 12;
        const int height = 12;

        List<ChunkTemplate> chunkTemplates = new List<ChunkTemplate>();
        MapRegion[] regions = new MapRegion[width * height];

        public MapRegion this[int x, int y]
        {
            get
            {
                if ((x < 0) || (x >= width)) { throw new ArgumentOutOfRangeException("x"); }
                if ((y < 0) || (y >= height)) { throw new ArgumentOutOfRangeException("y"); }
                return this.regions[(y * width) + x];
            }
            set
            {
                if ((x < 0) || (x >= width)) { throw new ArgumentOutOfRangeException("x"); }
                if ((y < 0) || (y >= height)) { throw new ArgumentOutOfRangeException("y"); }
                this.regions[(y * width) + x] = value;
            }
        }

        public int Height { get { return height; } }
        public int Width { get { return width; } }

        public List<ChunkTemplate> ChunkTemplates { get { return this.chunkTemplates; } }
    }
}
