namespace Volcano.Model
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    public class Frame
    {
        Rectangle bounds;
        byte[] data;
        int frameNumber;
        FrameType frameType;
        Shape shape;

        public Frame(Shape shape, int frameNumber, Rectangle bounds, byte[] data, FrameType frameType)
        {
            this.bounds = bounds;
            this.data = data;
            this.frameType = frameType;
            this.shape = shape;
            this.frameNumber = frameNumber;
        }

        public byte[] Data { get { return this.data; } }
        public int FrameNumber { get { return this.frameNumber; } }
        public FrameType FrameType { get { return this.frameType; } }
        public int Height { get { return this.bounds.Height; } }
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
        public Shape Shape { get { return this.shape; } }
        public Size3D TileSize { get { return this.shape.GetFrameSize(this.frameNumber); } }
        public int Width { get { return this.bounds.Width; } }

        public Bitmap GetBitmap(Color[] palette)
        {
            Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            DrawBitmap(bitmap, 0, 0, palette);
            return bitmap;
        }

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

        public static uint PackPixelBGRA(Color color)
        {
            return (uint)((color.B << 0) | (color.G << 8) | (color.R << 16) | (color.A << 24)); // Hike!
        }
    }
}
