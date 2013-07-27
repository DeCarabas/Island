namespace Volcano
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.IO;
    using System.Windows.Threading;
    using Volcano.Model;

    using Frame = Volcano.Model.Frame;
    using Shape = Volcano.Model.Shape;
    using Point = System.Drawing.Point;
    using Rectangle = System.Drawing.Rectangle;
    using System.ComponentModel;

    public class ProjectCache
    {
        Trie<ChunkTemplate> chunkTrie;
        Dictionary<Frame, BitmapSource> frameCache = new Dictionary<Frame, BitmapSource>();
        UltimaProject project;
        Dictionary<ChunkTemplate, BitmapSource> terrainCache = new Dictionary<ChunkTemplate, BitmapSource>();

        public UltimaProject Project 
        {
            get { return this.project; }
            set
            {
                if (this.project != null) { this.project.PropertyChanged -= OnProjectPropertyChanged; }
                this.project = value;
                InvalidateCache();
                if (this.project != null) { this.project.PropertyChanged += OnProjectPropertyChanged; }
            }
        }

        public ICollection<ChunkTemplate> LookupChunks(string description)
        {
            if (this.chunkTrie == null)
            {
                this.chunkTrie = new Trie<ChunkTemplate>();
                foreach (ChunkTemplate template in Project.Map.ChunkTemplates)
                {
                    for (int y = 0; y < template.Height; y++)
                    {
                        for (int x = 0; x < template.Width; x++)
                        {
                            Frame frame = template[x, y];
                            string tileDesc = Project.Text.Contents[(int)frame.Shape.Id];
                            if (tileDesc != null)
                            {
                                string[] keywords = tileDesc.Split();
                                for (int i = 0; i < keywords.Length; i++)
                                {
                                    this.chunkTrie.Add(keywords[i], template);
                                }
                            }
                        }
                    }
                }
            }

            return this.chunkTrie.Lookup(description);
        }

        public BitmapSource RenderTerrain(ChunkTemplate template)
        {
            BitmapSource bitmap;
            if (this.terrainCache.TryGetValue(template, out bitmap)) { return bitmap; }

            // The GDI+ version of this routine just blits pixels straight into the bitmap.
            // This might be slower?
            //
            var terrain = new RenderTargetBitmap(
                MapUnits.PixelsPerChunk,
                MapUnits.PixelsPerChunk,
                0,
                0,
                PixelFormats.Default);

            var visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();
            for (int y = 0; y < template.Height; y++)
            {
                for (int x = 0; x < template.Width; x++)
                {
                    Frame frame = template[x, y];
                    if (frame.FrameType == FrameType.Shape) { continue; }

                    context.DrawImage(
                        RenderFrame(frame),
                        new Rect(
                            x * MapUnits.PixelsPerTile,
                            y * MapUnits.PixelsPerTile,
                            MapUnits.PixelsPerTile,
                            MapUnits.PixelsPerTile));
                }
            }
            context.Close();
            terrain.Render(visual);
            if (terrain.CanFreeze) { terrain.Freeze(); }
            this.terrainCache.Add(template, terrain);

            return terrain;
        }

        public BitmapSource RenderFrame(Frame frame)
        {
            BitmapSource bitmap;
            if (this.frameCache.TryGetValue(frame, out bitmap)) { return bitmap; }

            uint[] pixels = frame.GetPixels(Project.Palettes.Contents[0]);
            bitmap = BitmapSource.Create(
                frame.Width,
                frame.Height,
                0,
                0,
                PixelFormats.Bgra32,
                null,
                pixels,
                frame.Width * 4);
            this.frameCache.Add(frame, bitmap);
            return bitmap;
        }

        void InvalidateCache()
        {
            InvalidateTextCache();
            InvalidateVisualCache();
        }

        void InvalidateTextCache()
        {
            this.chunkTrie = null;
        }

        void InvalidateVisualCache()
        {
            this.frameCache.Clear();
            this.terrainCache.Clear();
        }

        void OnProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Shapes":
                case "Palettes":
                    InvalidateVisualCache();
                    break;

                case "Text":
                    InvalidateTextCache();
                    break;
            }
        }
    }
}