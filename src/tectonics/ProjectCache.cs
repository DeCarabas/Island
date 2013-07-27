namespace Volcano.Tectonics
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using Volcano.Model;
    
    public class ProjectCache
    {
        Dictionary<Frame, Image> imageCache = new Dictionary<Frame, Image>();
        UltimaProject project;
        Dictionary<ChunkTemplate, Image> terrainCache = new Dictionary<ChunkTemplate, Image>();

        public int ImageCacheSize { get { return this.imageCache.Count; } }
        public UltimaProject Project
        {
            get { return this.project; }
            set
            {
                if (value != this.project) { Clear(); }
                this.project = value;
            }
        }
        public int TerrainCacheSize { get { return this.terrainCache.Count; } }

        public void Clear()
        {
            this.imageCache.Clear();
            this.terrainCache.Clear();
        }

        public Image GetShapeImage(Frame frame)
        {
            Image bitmap;
            if (!imageCache.TryGetValue(frame, out bitmap))
            {
                bitmap = frame.GetBitmap(this.project.Palettes.Contents[0]);
                imageCache[frame] = bitmap;
            }
            return bitmap;
        }

        public Image GetTerrainDrawing(ChunkTemplate chunk)
        {
            Image drawing;
            if (this.terrainCache.TryGetValue(chunk, out drawing)) { return drawing; }

            Bitmap bitmap = new Bitmap(
                chunk.Width * MapUnits.PixelsPerTile,
                chunk.Height * MapUnits.PixelsPerTile,
                PixelFormat.Format32bppArgb);

            for (int y = 0; y < chunk.Height; y++)
            {
                for (int x = 0; x < chunk.Width; x++)
                {
                    Frame frame = chunk[x, y];

                    if (frame.FrameType != FrameType.Tile) { continue; }

                    int originX = (x * MapUnits.PixelsPerTile);
                    int originY = (y * MapUnits.PixelsPerTile);

                    frame.DrawBitmap(bitmap, originX, originY, this.project.Palettes.Contents[0]);
                }
            }

            this.terrainCache.Add(chunk, bitmap);
            return bitmap;
        }
    }
}
