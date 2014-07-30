namespace Volcano.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a region in the world map.
    /// </summary>
    /// <remarks>
    /// A region is a collection of chunks; a sort of meta-chunk.
    /// </remarks>
    public class MapRegion
    {
        MapChunk[] chunks = new MapChunk[MapUnits.ChunksPerRegion * MapUnits.ChunksPerRegion];

        /// <summary>
        /// Gets or sets a chunk in the region.
        /// </summary>
        /// <param name="x">The x-coordinate of the chunk to modify, in region space.</param>
        /// <param name="y">The y-coordinate of the chunk to modify, in region space.</param>
        /// <returns>The chunk.</returns>
        public MapChunk this[int x, int y]
        {
            get
            {
                if ((x < 0) || (x >= MapUnits.ChunksPerRegion)) { throw new ArgumentOutOfRangeException("x"); }
                if ((y < 0) || (y >= MapUnits.ChunksPerRegion)) { throw new ArgumentOutOfRangeException("y"); }
                return this.chunks[(y * MapUnits.ChunksPerRegion) + x];
            }
            set
            {
                if ((x < 0) || (x >= MapUnits.ChunksPerRegion)) { throw new ArgumentOutOfRangeException("x"); }
                if ((y < 0) || (y >= MapUnits.ChunksPerRegion)) { throw new ArgumentOutOfRangeException("y"); }
                this.chunks[(y * MapUnits.ChunksPerRegion) + x] = value;
            }
        }

        /// <summary>
        /// Gets the height of this region, in chunks.
        /// </summary>
        public int Height { get { return MapUnits.ChunksPerRegion; } }

        /// <summary>
        /// Gets the width of this region, in chunks.
        /// </summary>
        public int Width { get { return MapUnits.ChunksPerRegion; } }
    }
}
