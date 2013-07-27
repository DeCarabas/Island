namespace Volcano.Model
{
    using System;
    using System.IO;

    public static class MapUnits
    {
        public const int PixelsPerTile = 8;
        public const int TilesPerChunk = 16;
        public const int ChunksPerRegion = 16;
        public const int RegionsPerMap = 12;

        public const int PixelsPerChunk = PixelsPerTile * TilesPerChunk;
        public const int PixelsPerRegion = PixelsPerChunk * ChunksPerRegion;
        public const int PixelsPerMap = PixelsPerRegion * RegionsPerMap;

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
