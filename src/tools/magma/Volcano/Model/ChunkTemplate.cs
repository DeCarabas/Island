namespace Volcano.Model
{
    using System;

    // Chunks are shared across the map; they can be displayed in more than one place at once. ChunkTemplate is the
    // shared representation for the chunk. (Contrast with MapChunk, which, like MapRegion, represents a single definite
    // place in the world.)
    //
    public class ChunkTemplate
    {
        const int width = 16;
        const int height = 16;

        Frame[] tiles = new Frame[width * height];

        public Frame this[int x, int y]
        {
            get
            {
                if ((x < 0) || (x >= width)) { throw new ArgumentOutOfRangeException("x"); }
                if ((y < 0) || (y >= height)) { throw new ArgumentOutOfRangeException("y"); }
                return this.tiles[(y * width) + x];
            }
            set
            {
                if ((x < 0) || (x >= width)) { throw new ArgumentOutOfRangeException("x"); }
                if ((y < 0) || (y >= height)) { throw new ArgumentOutOfRangeException("y"); }
                this.tiles[(y * width) + x] = value;
            }
        }

        public int Height { get { return height; } }
        public int Id { get; set; }
        public int Width { get { return width; } }
    }
}