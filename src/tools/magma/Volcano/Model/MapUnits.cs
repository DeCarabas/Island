namespace Volcano.Model
{
    using System;
    using System.IO;

    /// <summary>
    /// A collection of useful dimensions for the map.
    /// </summary>
    public static class MapUnits
    {
        /// <summary>
        /// The number of pixels in a tile.
        /// </summary>
        public const int PixelsPerTile = 8;

        /// <summary>
        /// The number of tiles on an edge in a chunk.
        /// </summary>
        public const int TilesPerChunk = 16;

        /// <summary>
        /// The number of chunks on an edge in a region.
        /// </summary>
        public const int ChunksPerRegion = 16;

        /// <summary>
        /// The number of regions on an edge in a map.
        /// </summary>
        public const int RegionsPerMap = 12;

        /// <summary>
        /// The number of pixels on an edge in a chunk.
        /// </summary>
        public const int PixelsPerChunk = PixelsPerTile * TilesPerChunk;

        /// <summary>
        /// The number of pixels on an edge in a region.
        /// </summary>
        public const int PixelsPerRegion = PixelsPerChunk * ChunksPerRegion;
        
        /// <summary>
        /// The number of pixels on an edge in a map.
        /// </summary>
        public const int PixelsPerMap = PixelsPerRegion * RegionsPerMap;

        /// <summary>
        /// The number of chunks on an edge in a map.
        /// </summary>
        public const int ChunksPerMap = ChunksPerRegion * RegionsPerMap;

        /// <summary>
        /// Takes a coordinate in pixels and truncates it to the nearest chunk boundary.
        /// </summary>
        public static int TruncatePixelsToChunkBoundary(int pixelCoord)
        {
            // Pixels per chunk is 8 * 16 = 128 = 7F
            return pixelCoord & ~0x7F;
        }
    }
}
