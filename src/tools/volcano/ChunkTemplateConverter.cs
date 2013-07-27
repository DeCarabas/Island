namespace Volcano
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Volcano.Model;

    public class ChunkTemplateConverter : IValueConverter
    {
        public EditorContext Context { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const int maxZProjection = 16 * 4;
            const int imageSize = MapUnits.PixelsPerChunk + maxZProjection;

            if (value == null) { return null; }
            var chunk = (ChunkTemplate)value;
            var target = new RenderTargetBitmap(imageSize, imageSize, 0, 0, PixelFormats.Default);

            var visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();
            context.PushClip(new RectangleGeometry(new Rect(0, 0, imageSize, imageSize)));
            // Render the terrain...
            //
            context.DrawImage(
                Context.Cache.RenderTerrain(chunk),
                new Rect(maxZProjection, maxZProjection, MapUnits.PixelsPerChunk, MapUnits.PixelsPerChunk));

            // Construct a list of all the objects on the terrain...
            //
            List<MapObject> objects = new List<MapObject>();
            for (int y = 0; y < MapUnits.TilesPerChunk; y++)
            {
                for (int x = 0; x < MapUnits.TilesPerChunk; x++)
                {
                    Frame frame = chunk[x, y];
                    if (frame.FrameType != FrameType.Shape) { continue; }

                    objects.Add(new ChunkMapObject
                    {
                        Frame = frame,
                        Location = new MapPoint
                        {
                            X = maxZProjection + MapUnits.PixelsPerTile + (x * MapUnits.PixelsPerTile),
                            Y = maxZProjection + MapUnits.PixelsPerTile + (y * MapUnits.PixelsPerTile),
                        }
                    });
                }
            }
            //
            // Sort and render them.
            //
            foreach (MapObject mapObject in MapObject.SortObjects(objects))
            {
                var screenRect = mapObject.ScreenRect;
                context.DrawImage(
                    Context.Cache.RenderFrame(mapObject.Frame),
                    new Rect(screenRect.Left, screenRect.Top, screenRect.Width, screenRect.Height));
            }
            context.Close();

            target.Render(visual);
            return target;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}