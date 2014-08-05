namespace Volcano.Model
{
    using System;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Indicates what sort of shape this is.
    /// </summary>
    /// <remarks>
    /// This is used to determine what the quality field actually means, among other things.
    /// </remarks>
    public enum ShapeClass
    {
        /// <summary>
        /// Trees, scenery.
        /// </summary>
        Unusable = 0,

        /// <summary>
        /// Has a 'quality' field indicating which thing it is.
        /// </summary>
        Quality = 2,

        /// <summary>
        /// Can have more than one: coins, say, or arrows.
        /// </summary>
        Quantity = 3,

        /// <summary>
        /// Breakable items.
        /// </summary>
        Breakable = 4,

        /// <summary>
        /// The item quality is a set of flags.
        /// </summary>
        /// <remarks>If bit 3 is set, it's OK to take.</remarks>
        QualityFlags = 5,
        
        /// <summary>
        /// It's a container.
        /// </summary>
        Container = 6,

        /// <summary>
        /// It's an egg, or a trap, or a moongate; something that runs when you step in it.
        /// </summary>
        Hatchable = 7,

        /// <summary>
        /// A spellbook.
        /// </summary>
        Spellbook = 8,

        /// <summary>
        /// A barge.
        /// </summary>
        Barge = 9,

        /// <summary>
        /// A virtue stone.
        /// </summary>
        VirtueStone = 11,

        /// <summary>
        /// A non-human NPC.
        /// </summary>
        Monster = 12,		

        /// <summary>
        /// A human NPC.
        /// </summary>
        Human = 13,		

        /// <summary>
        /// Some part of a building, like a roof, a window, or a mountain.
        /// </summary>
        Building = 14
    }

    /// <summary>
    /// Represents a class of thing in the game.
    /// </summary>
    /// <remarks>
    /// Shape serves two different roles:
    ///   - It's the container for bitmaps, as a series of frames.
    ///   - It stores metadata about the object, much as a game object-class would. This includes information about its
    ///     behavior in the game world, and how to interpret its instance data.
    /// </remarks>
    public class Shape
    {
        Frame[] frames;

        public ShapeClass Class { get; set; }
        public Frame[] Frames { get { return this.frames; } }
        public bool Occludes { get; set; }
        public Size3D Size { get; set; }
        public long Id { get; set; }

        public Size3D GetFrameSize(int frameNumber)
        {
            // STRANGE BUT TRUE: Frames with bit 5 set swap X and Y sizes. (Now you know why there are empty frames in
            // the shapes files.)
            //
            bool swapXY = ((frameNumber >> 5) & 0x1) != 0;
            return new Size3D
            {
                X = swapXY ? this.Size.Y : this.Size.X,
                Y = swapXY ? this.Size.X : this.Size.Y,
                Z = this.Size.Z
            };
        }

        public static Shape Load(Stream stream)
        {
            Shape shape = new Shape();
            BinaryReader reader = new BinaryReader(stream);
            
            // OK kids, listen up. There are many many tricks here, and you must pay ATTENTION.
            //
            // The first thing is kinda a hack: how do we know if this is a SHAPE, or a SET OF TILES?
            // Here is the trick that Exult uses:
            //
            int shapeLength = reader.ReadInt32();
            bool isShape = (shapeLength == stream.Length);
            //
            // ...yeah. I know. You need to fudge the file in order to make these NOT EQUAL if you want
            // to store tilesets.
            //
            if (isShape)
            {
                // OK, it's a proper shape; following the size is a set of frame offsets. How do we know how many
                // frame offsets we have? Simple: we read the offset of the first frame, and that tells how much
                // space is in the header, which lets us figure out how many more frames. Watch.
                //
                int firstOffset = reader.ReadInt32();
                int frameCount = (firstOffset - 4) / 4;

                int[] frameOffsets = new int[frameCount + 1];
                frameOffsets[0] = firstOffset;
                for (int i = 1; i < frameCount; i++)
                {
                    frameOffsets[i] = reader.ReadInt32();
                }
                frameOffsets[frameCount] = (int)stream.Length;
                
                // Now we get to read all the data. Note that we postpone decoding it, however, because it's kinda 
                // painful to do so.
                //
                shape.frames = new Frame[frameCount];
                for (int i = 0; i < frameCount; i++)
                {
                    stream.Position = frameOffsets[i];

                    // These offsets are not what you might expect. There is a mythical pixel, the "hot spot". The image
                    // goes around this position. (Why they didn't just describe it as a normal bitmap and then the 
                    // location of the hot-spot, I don't know.) 
                    //
                    // So, if we take the standard assumption that a pixel coordinate (x,y) denotes the top-left corner
                    // of the pixel x-over and y-down, then the top-left corner of the bottom-right-most pixel is 
                    // (right, below) and the top-right corner of the top-left-most pixel is (-left, -above).
                    //
                    int right = reader.ReadInt16();
                    int left = reader.ReadInt16();
                    int above = reader.ReadInt16();
                    int below = reader.ReadInt16();
                    //
                    // We want to deal with linear bitmaps, and we're not directly rendering to a screen, so we change
                    // the coordinates to be what we want. The "bounds" tell us how to transform from image space to
                    // screen space: bounds.X is the X coordinate of the hotspot, bounds.Y is the Y coordinate of the 
                    // hotspot. Width and Height are the bounds of the image.
                    //
                    var bounds = new Rectangle(left, above, left + right + 1, above + below + 1);

                    int nextOffset = frameOffsets[i + 1];
                    int frameLength = nextOffset - (int)stream.Position;
                    byte[] data = new byte[frameLength];

                    stream.Read(data, 0, frameLength);

                    shape.frames[i] = new Frame(shape, i, bounds, data, FrameType.Shape);
                }
            }
            else
            {
                // It was't a shape, it must be a tileset. This is a bunch of 8x8 bitmaps. BUT HOW MANY?
                // GOD KNOWS. Most people don't bother to fetch the number of tiles.
                //
                const int tilesize = 8 * 8;
                long frameCount = stream.Length / tilesize;
                shape.frames = new Frame[frameCount];
                
                stream.Position = 0;

                Rectangle bounds = new Rectangle(8, 8, 8, 8); 
                for (long i = 0; i < frameCount; i++)
                {
                    byte[] data = new byte[tilesize];
                    stream.Read(data, 0, tilesize);

                    shape.Frames[i] = new Frame(shape, (int)i, bounds, data, FrameType.Tile);
                }
            }

            return shape;
        }
    }

    public struct Size3D
    {
        public int X;
        public int Y;
        public int Z;
    }
}
