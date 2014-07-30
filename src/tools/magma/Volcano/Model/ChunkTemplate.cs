namespace Volcano.Model
{
    using System;

    /// <summary>
    /// Represents a pre-defined chunk template in the world.
    /// </summary>
    /// <remarks>
    /// The map in the U7 engine is tile-based, but at two levels. The core "tile" is a small bit of graphics, 8px wide 
    /// and 8px tall. These are grouped into a "chunk" of tiles, 16x16. These "chunks" are then placed on the map. Just
    /// as a given tile may be in more than one place at a time, a given "chunk" may be re-used in multiple places 
    /// throughout the world.
    /// 
    /// <para>This class is the shared representation of the chunk. Contrast this with <see cref="MapChunk"/>, which, 
    /// like <see cref="MapRegion"/>, represents an actualized place in the world, with real, interactable objects.
    /// </para>
    /// </remarks>
    public class ChunkTemplate
    {
        const int width = 16;
        const int height = 16;

        Frame[] tiles = new Frame[width * height];

        /// <summary>
        /// Gets or sets the frame at a given position in the chunk.
        /// </summary>
        /// <param name="x">The x-coordinate of the frame.</param>
        /// <param name="y">The y-coordinate of the frame.</param>
        /// <returns>The <see cref="Frame"/> at the specified x and y coordinate.</returns>
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

        /// <summary>Gets the width of this chunk template, in frames.</summary>
        public int Height { get { return height; } }

        /// <summary>Gets or sets the ID of this chunk template, as it is referred to in the map.</summary>
        public int Id { get; set; }

        /// <summary>Gets the width of this chunk template, in frames.</summary>
        public int Width { get { return width; } }
    }
}