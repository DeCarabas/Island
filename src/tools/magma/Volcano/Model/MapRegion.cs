namespace Volcano.Model
{
    using System;
    using System.Collections.Generic;

    public class MapRegion
    {
        MapChunk[] chunks = new MapChunk[MapUnits.ChunksPerRegion * MapUnits.ChunksPerRegion];

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

        public int Height { get { return MapUnits.ChunksPerRegion; } }
        public int Width { get { return MapUnits.ChunksPerRegion; } }
    }
}
