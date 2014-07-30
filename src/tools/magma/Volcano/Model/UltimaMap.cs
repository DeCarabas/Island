namespace Volcano.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the world map.
    /// </summary>
    /// <remarks>
    /// The world map in an Ultima game is divided up into a 12x12 grid of "regions", which are further divided into 
    /// "chunks", which are further divided into tiles or shapes.
    /// </remarks>
    public class UltimaMap
    {
        const int width = 12;
        const int height = 12;

        List<ChunkTemplate> chunkTemplates = new List<ChunkTemplate>();
        MapRegion[] regions = new MapRegion[width * height];

        /// <summary>
        /// Gets or sets the region at a particular point.
        /// </summary>
        /// <param name="x">The x-coordinate (in map space) of the region.</param>
        /// <param name="y">The y-coordinate (in map space) of the region.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the list of chunk templates associated with this map.
        /// </summary>
        public List<ChunkTemplate> ChunkTemplates { get { return this.chunkTemplates; } }

        /// <summary>
        /// Gets the height of the map, in regions.
        /// </summary>
        public int Height { get { return height; } }

        /// <summary>
        /// Gets the width of the map, in regions.
        /// </summary>
        public int Width { get { return width; } }        
    }
}
