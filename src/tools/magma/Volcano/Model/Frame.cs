namespace Volcano.Model
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    /// Represents a bitmap in an Ultima 7 style engine, with some additional metadata that goes along with it.
    /// </summary>
    /// <remarks>
    /// Bitmaps in U7 are RLE encoded, palettized in a palette of 256 colors (that can be changed depending on various
    /// conditions), and (furthermore) contain a 'hot spot' which indicates some mythical reference point for the 
    /// bitmap. (This hot spot is what allows the Avatar's feet to always be at the right place, for example.)
    /// </remarks>
    public class Frame
    {
        Rectangle bounds;
        byte[] data;
        int frameNumber;
        FrameType frameType;
        Shape shape;

        /// <summary>
        /// Constructs a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="shape">The shape that this frame is part of.</param>
        /// <param name="frameNumber">The frame number within the shape that this frame is.</param>
        /// <param name="bounds">The bounds of this frame; note that this describes a rectangle with the hot spot at 
        /// (0, 0).</param>
        /// <param name="data">The RLE encoded bitmap data for the frame.</param>
        /// <param name="frameType">The type of frame that this is.</param>
        public Frame(Shape shape, int frameNumber, Rectangle bounds, byte[] data, FrameType frameType)
        {
            this.bounds = bounds;
            this.data = data;
            this.frameType = frameType;
            this.shape = shape;
            this.frameNumber = frameNumber;
        }

        /// <summary>Gets the RLE encoded data for this frame.</summary>
        public byte[] Data { get { return this.data; } }

        /// <summary>Gets the number of this frame in its enclosing <see cref="Shape"/>.</summary>
        public int FrameNumber { get { return this.frameNumber; } }

        /// <summary>Gets the type of this frame.</summary>
        public FrameType FrameType { get { return this.frameType; } }

        /// <summary>Gets the height of this frame, in pixels.</summary>
        public int Height { get { return this.bounds.Height; } }
        
        /// <summary>Gets the X offset of the hotspot of the frame in the bitmap.</summary>
        public int HotSpotX
        {
            get
            {
                int value = this.bounds.X;

                // Some day I will understand this.
                if (FrameType == FrameType.Shape) { value += 1; }

                return value;
            }
        }
        
        /// <summary>Gets the Y offset of the hotspot of the frame in the bitmap.</summary>
        public int HotSpotY
        {
            get
            {
                int value = this.bounds.Y;

                // Some day I will understand this.
                if (FrameType == FrameType.Shape) { value += 1; }

                return value;
            }
        }

        /// <summary>Gets the <see cref="Shape"/> that this frame is a part of.</summary>
        public Shape Shape { get { return this.shape; } }

        /// <summary>Gets the 3d bounding box for the shape this is part of.</summary>
        public Size3D TileSize { get { return this.shape.GetFrameSize(this.frameNumber); } }

        /// <summary>Gets the width of this frame, in pixels.</summary>
        public int Width { get { return this.bounds.Width; } }

        /// <summary>
        /// Decodes the frame data into a Bitmap with the specified color palette.
        /// </summary>
        /// <param name="palette">The palette to use when converting the bitmap.</param>
        /// <returns>A decoded bitmap.</returns>
        public Bitmap GetBitmap(Color[] palette)
        {
            Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            DrawBitmap(bitmap, 0, 0, palette);
            return bitmap;
        }

        /// <summary>
        /// Draws the frame data onto the specified Bitmap, at the specified offset, given the specified color palette.
        /// </summary>
        /// <param name="target">The bitmap to draw onto.</param>
        /// <param name="x">The x-offset at which to draw the frame.</param>
        /// <param name="y">The y-offset at which to draw the frame.</param>
        /// <param name="palette">The palette to use when converting the bitmap.</param>
        public void DrawBitmap(Bitmap target, int x, int y, Color[] palette)
        {
            if (FrameType == FrameType.Shape)
            {
                DrawShape(target, x, y, palette);
            }
            else
            {
                DrawTile(target, x, y, palette);
            }
        }

        /// <summary>
        /// Draws a shape-based frame onto the specified bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to draw onto.</param>
        /// <param name="x">The x-offset at which to draw the frame.</param>
        /// <param name="y">The y-offset at which to draw the frame.</param>
        /// <param name="palette">The palette to use when converting the bitmap.</param>
        /// <remarks>This is the format that uses this RLE encoding.</remarks>
        void DrawShape(Bitmap bitmap, int x, int y, Color[] palette)
        {
            // Shapes are this funky funky RLE format.
            //
            BinaryReader reader = new BinaryReader(new MemoryStream(this.data));
            ushort sliceLength = reader.ReadUInt16();
            while (sliceLength != 0)
            {
                bool isCompressed = (sliceLength & 0x1) != 0;
                sliceLength = (ushort)(sliceLength >> 1);

                short offsetX = reader.ReadInt16();
                short offsetY = reader.ReadInt16();

                int actualX = this.bounds.X + offsetX + x;
                int actualY = this.bounds.Y + offsetY + y;

                if (isCompressed)
                {
                    ushort i = 0;
                    while (i < sliceLength)
                    {
                        byte blockLength = reader.ReadByte();
                        bool isRepeatedPixel = (blockLength & 0x1) != 0;
                        blockLength = (byte)(blockLength >> 1);

                        if (isRepeatedPixel)
                        {
                            byte pixel = reader.ReadByte();
                            for (byte j = 0; j < blockLength; j++)
                            {
                                bitmap.SetPixel(actualX + i + j, actualY, palette[pixel]);
                            }
                        }
                        else
                        {
                            for (byte j = 0; j < blockLength; j++)
                            {
                                byte pixel = reader.ReadByte();
                                bitmap.SetPixel(actualX + i + j, actualY, palette[pixel]);
                            }
                        }

                        i += blockLength;
                    }
                }
                else
                {
                    for (ushort i = 0; i < sliceLength; i++)
                    {
                        byte pixel = reader.ReadByte();
                        bitmap.SetPixel(actualX + i, actualY, palette[pixel]);
                    }
                }

                sliceLength = reader.ReadUInt16();
            }
        }

        /// <summary>
        /// Draws a tile-based frame onto the specified bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to draw onto.</param>
        /// <param name="x">The x-offset at which to draw the frame.</param>
        /// <param name="y">The y-offset at which to draw the frame.</param>
        /// <param name="palette">The palette to use when converting the bitmap.</param>
        /// <remarks>Tiles are just plain-old rectangular palettized bitmaps.</remarks>
        void DrawTile(Bitmap bitmap, int x, int y, Color[] palette)
        {
            // Tiles are just rectangular bitmaps.
            //
            for (int iy = 0; iy < Height; iy++)
            {
                for (int ix = 0; ix < Width; ix++)
                {
                    bitmap.SetPixel(ix + x, iy + y, palette[this.data[(iy * Height) + ix]]);
                }
            }
        }

        /// <summary>
        /// Gets raw pixel data for this frame.
        /// </summary>
        /// <param name="palette">The palette to use to get the pixel data.</param>
        /// <returns>An array of pixel data for the frame, in 32bit BGRA format. (That's the one with alpha in the
        /// least-significant byte.)</returns>
        public uint[] GetPixels(Color[] palette)
        {
            if (frameType == FrameType.Shape)
            {
                return GetPixelsForShape(palette);
            }
            else
            {
                return GetPixelsForTile(palette);
            }
        }

        /// <summary>
        /// Gets raw pixel data for this shape-based frame.
        /// </summary>
        /// <param name="palette">The palette to use to get the pixel data.</param>
        /// <returns>An array of pixel data for the frame, in 32bit BGRA format. (That's the one with alpha in the
        /// most-significant byte.)</returns>
        /// <remarks>Yeah, this is a copy of the RLE decoding logic.</remarks>
        uint[] GetPixelsForShape(Color[] palette)
        {
            // Shapes are this funky funky RLE format.
            //
            uint[] pixels = new uint[this.bounds.Width * this.bounds.Height];
            //for (int i = 0; i < pixels.Length; i++)
            //{
            //    pixels[i] = 0xFF; // Set the alpha on all pixels to FF, so that they are transparent by default.
            //}

            BinaryReader reader = new BinaryReader(new MemoryStream(this.data));
            ushort sliceLength = reader.ReadUInt16();
            while (sliceLength != 0)
            {
                bool isCompressed = (sliceLength & 0x1) != 0;
                sliceLength = (ushort)(sliceLength >> 1);

                short offsetX = reader.ReadInt16();
                short offsetY = reader.ReadInt16();

                int actualY = this.bounds.Y + offsetY;
                int actualX = this.bounds.X + offsetX;

                int startOffset = (actualY * this.bounds.Width) + actualX;
                if (isCompressed)
                {
                    ushort i = 0;
                    while (i < sliceLength)
                    {
                        byte blockLength = reader.ReadByte();
                        bool isRepeatedPixel = (blockLength & 0x1) != 0;
                        blockLength = (byte)(blockLength >> 1);

                        if (isRepeatedPixel)
                        {
                            byte pixel = reader.ReadByte();
                            uint fullPixel = PackPixelBGRA(palette[pixel]);
                            for (byte j = 0; j < blockLength; j++)
                            {
                                pixels[startOffset + i + j] = fullPixel;
                            }
                        }
                        else
                        {
                            for (byte j = 0; j < blockLength; j++)
                            {
                                byte pixel = reader.ReadByte();
                                pixels[startOffset + i + j] = PackPixelBGRA(palette[pixel]);
                            }
                        }

                        i += blockLength;
                    }
                }
                else
                {
                    for (ushort i = 0; i < sliceLength; i++)
                    {
                        byte pixel = reader.ReadByte();
                        pixels[startOffset + i] = PackPixelBGRA(palette[pixel]);
                    }
                }

                sliceLength = reader.ReadUInt16();
            }

            return pixels;
        }

        /// <summary>
        /// Gets raw pixel data for this tile-based frame.
        /// </summary>
        /// <param name="palette">The palette to use to get the pixel data.</param>
        /// <returns>An array of pixel data for the frame, in 32bit BGRA format. (That's the one with alpha in the
        /// most-significant byte.)</returns>
        /// <remarks>Tiles are just boring old rectangular bitmaps.</remarks>
        uint[] GetPixelsForTile(Color[] palette)
        {
            // Tiles are just rectangular bitmaps.
            //
            uint[] pixels = new uint[this.bounds.Width * this.bounds.Height];
            for (int i = 0; i < this.data.Length; i++)
            {
                pixels[i] = PackPixelBGRA(palette[this.data[i]]);
            }

            return pixels;
        }

        /// <summary>
        /// Packs a color into BGRA format.
        /// </summary>
        /// <param name="color">The color to pack.</param>
        /// <returns>A uint which represents the 32b BGRA packed value.</returns>
        public static uint PackPixelBGRA(Color color)
        {
            return (uint)((color.B << 0) | (color.G << 8) | (color.R << 16) | (color.A << 24)); // Hike!
        }
    }
}
