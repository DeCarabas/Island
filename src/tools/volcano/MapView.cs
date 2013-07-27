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

    public class MapView : Panel
    {
        public static readonly DependencyProperty CacheProperty =
            DependencyProperty.Register(
                "Cache",
                typeof(ProjectCache),
                typeof(MapView),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty CameraXProperty =
            DependencyProperty.Register(
                "CameraX",
                typeof(int), // TODO: Make this a double
                typeof(MapView), 
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty CameraYProperty =
            DependencyProperty.Register(
                "CameraY",
                typeof(int), // TODO: Make this a double
                typeof(MapView), 
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ItemsVisibleProperty =
            DependencyProperty.Register(
                "ItemsVisible",
                typeof(bool),
                typeof(MapView), 
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.RegisterAttached("Left", typeof(double), typeof(MapView));

        public static readonly DependencyProperty ProjectProperty =
            DependencyProperty.Register(
                "Project",
                typeof(UltimaProject),
                typeof(MapView),
                new FrameworkPropertyMetadata(
                    null, 
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnProjectChanged));

        public static readonly DependencyProperty TopProperty =
            DependencyProperty.RegisterAttached("Top", typeof(double), typeof(MapView));

        public static readonly DependencyProperty ZLimitProperty =
            DependencyProperty.Register(
                "ZLimit",
                typeof(int),
                typeof(MapView),
                new FrameworkPropertyMetadata(16, FrameworkPropertyMetadataOptions.AffectsRender));

        [Browsable(false)]
        [DefaultValue(null)]
        public ProjectCache Cache
        {
            get { return (ProjectCache)GetValue(CacheProperty); }
            set { SetValue(CacheProperty, value); }
        }

        [DefaultValue(0)]
        public int CameraX
        {
            get { return (int)GetValue(CameraXProperty);  }
            set { SetValue(CameraXProperty, value); }
        }

        [DefaultValue(0)]
        public int CameraY
        {
            get { return (int)GetValue(CameraYProperty); }
            set { SetValue(CameraYProperty, value); }
        }

        [DefaultValue(true)]
        public bool ItemsVisible
        {
            get { return (bool)GetValue(ItemsVisibleProperty); }
            set { SetValue(ItemsVisibleProperty, value); }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        public UltimaProject Project
        {
            get { return (UltimaProject)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        [DefaultValue(16)]
        public int ZLimit
        {
            get { return (int)GetValue(ZLimitProperty); }
            set { SetValue(ZLimitProperty, value); }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                var client = new System.Windows.Point(GetLeft(child) - CameraX, GetTop(child) - CameraY);
                child.Arrange(new Rect(client, child.DesiredSize));
            }
            return finalSize;
        }

        public static double GetLeft(UIElement element)
        {
            return (double)element.GetValue(LeftProperty);
        }

        public static double GetTop(UIElement element)
        {
            return (double)element.GetValue(TopProperty);
        }

        public MapPoint ScreenToWorld(int screenX, int screenY)
        {
            return new MapPoint
            {
                X = screenX + CameraX,
                Y = screenY + CameraY
            };
        }

        public IList<MapObject> GetObjectsAroundPoint(MapPoint worldPoint)
        {
            // Because of the limits on height and the way Z transforms, we know that any object 
            // this point might interesct is within a chunk from the point.
            //
            var view = new Rectangle(
                worldPoint.X - MapUnits.PixelsPerChunk,
                worldPoint.Y - MapUnits.PixelsPerChunk,
                3 * MapUnits.PixelsPerChunk,
                3 * MapUnits.PixelsPerChunk);

            return GetObjectsInView(view);
        }

        public IList<MapObject> GetObjectsInView(Rectangle worldView)
        {
            List<MapObject> objects = new List<MapObject>();

            // TODO: Remove this and work completely off the objects in the chunk.
            int left = worldView.Left / MapUnits.PixelsPerChunk;
            int top = worldView.Top / MapUnits.PixelsPerChunk;

            int right = (worldView.Right / MapUnits.PixelsPerChunk) + 1;
            int bottom = (worldView.Bottom / MapUnits.PixelsPerChunk) + 1;

            for (int y = Math.Max(top, 0); y < bottom; y++)
            {
                for (int x = Math.Max(left, 0); x < right; x++)
                {
                    int regionX = x / MapUnits.ChunksPerRegion;
                    int regionY = y / MapUnits.ChunksPerRegion;

                    if (regionX >= Project.Map.Width) { continue; }
                    if (regionY >= Project.Map.Height) { continue; }

                    int cx = x - (regionX * MapUnits.ChunksPerRegion);
                    int cy = y - (regionY * MapUnits.ChunksPerRegion);

                    List<MapObject> chunkObjects = Project.Map[regionX, regionY][cx, cy].Objects;
                    foreach (MapObject obj in chunkObjects)
                    {
                        if (obj.Location.Z < ZLimit) { objects.Add(obj); }
                    }
                }
            }

            return MapObject.SortObjects(objects);
        }

        public MapChunk FindChunk(int screenX, int screenY)
        {
            if ((screenX < 0) || (screenY < 0) || 
                (screenX >= MapUnits.PixelsPerMap) || (screenY >= MapUnits.PixelsPerMap))
            {
                return null;
            }

            MapPoint worldPoint = ScreenToWorld(screenX, screenY);
            return Project.Map[worldPoint.WorldX, worldPoint.WorldY][worldPoint.RegionX, worldPoint.RegionY];
        }

        public MapObject FindObjectThatYouClickedOn(int screenX, int screenY)
        {
            MapPoint worldPoint = ScreenToWorld(screenX, screenY);
            IList<MapObject> objectsAroundPoint = GetObjectsAroundPoint(worldPoint);
            //
            // Because the objects around the point come back sorted from back-to-front, we must reverse when
            // doing hit-testing.
            //
            foreach (MapObject candidate in objectsAroundPoint.Reverse())
            {
                if (candidate.Location.Z > ZLimit) { continue; }
                if (candidate.ScreenRect.Contains(worldPoint.X, worldPoint.Y))
                {
                    return candidate;
                }
            }

            return null;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                if (child != null)
                {
                    child.Measure(availableSize);
                }
            }
            return new Size();
        }

        static void OnProjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (MapView)d;
            if (e.OldValue != null)
            {
                ((UltimaProject)e.OldValue).PropertyChanged -= view.OnProjectPropertyChanged;
            }

            if (e.NewValue != null)
            {
                ((UltimaProject)e.NewValue).PropertyChanged += view.OnProjectPropertyChanged;
            }
        }

        void OnProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Map":
                case "Shapes":
                case "Palettes":
                    InvalidateVisual();
                    break;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Project != null)
            {
                drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, RenderSize.Width, RenderSize.Height)));

                var worldView = new Rectangle(CameraX, CameraY, (int)RenderSize.Width + 1, (int)RenderSize.Height + 1);

                RenderTerrain(drawingContext, worldView);
                RenderMapObjects(drawingContext, worldView);

                drawingContext.Pop();
            }
        }

        void RenderMapObjects(DrawingContext graphics, Rectangle worldView)
        {
            if (ZLimit < 0) { return; }
            if (!ItemsVisible) { return; }

            IList<MapObject> sortedObjects = GetObjectsInView(worldView); ;
            for (int i = 0; i < sortedObjects.Count; i++)
            {
                MapObject mapObject = sortedObjects[i];

                Point screenPoint = mapObject.Location.ScreenPoint;

                // Correct for upper-left of canvas.
                //
                screenPoint.X -= worldView.Left;
                screenPoint.Y -= worldView.Top;

                // Correct for hotspot.
                //
                screenPoint.X -= mapObject.Frame.HotSpotX;
                screenPoint.Y -= mapObject.Frame.HotSpotY;

                BitmapSource bitmap = Cache.RenderFrame(mapObject.Frame);
                Rect rect = new Rect(screenPoint.X, screenPoint.Y, bitmap.Width, bitmap.Height);

                graphics.DrawImage(bitmap, rect);
            }
        }

        void RenderTerrain(DrawingContext graphics, Rectangle worldView)
        {
            int left = worldView.Left / MapUnits.PixelsPerChunk;
            int top = worldView.Top / MapUnits.PixelsPerChunk;

            int right = (worldView.Right / MapUnits.PixelsPerChunk) + 1;
            int bottom = (worldView.Bottom / MapUnits.PixelsPerChunk) + 1;

            for (int y = Math.Max(top, 0); y < bottom; y += 1)
            {
                for (int x = Math.Max(left, 0); x < right; x += 1)
                {
                    // What chunk are we on?
                    //
                    int regionX = x / MapUnits.ChunksPerRegion;
                    int regionY = y / MapUnits.ChunksPerRegion;

                    if (regionX >= Project.Map.Width) { continue; }
                    if (regionY >= Project.Map.Height) { continue; }

                    int chunkX = x - (regionX * MapUnits.ChunksPerRegion);
                    int chunkY = y - (regionY * MapUnits.ChunksPerRegion);

                    MapChunk chunk = Project.Map[regionX, regionY][chunkX, chunkY];

                    BitmapSource bitmap = Cache.RenderTerrain(chunk.Template);
                    //
                    // The coordinates are *slightly* off here because, when assembling the drawing, the 
                    // relative offset of the land tiles was not taken into account. (The hotspot for a 
                    // tile is actually in the lower-right corner of the tile.)
                    //                    
                    int screenX = (x * MapUnits.PixelsPerChunk) - worldView.Left - MapUnits.PixelsPerTile;
                    int screenY = (y * MapUnits.PixelsPerChunk) - worldView.Top - MapUnits.PixelsPerTile;

                    Rect rect = new Rect(screenX, screenY, bitmap.Width, bitmap.Height);
                    graphics.DrawImage(bitmap, rect);
                }
            }
        }

        public static void SetLeft(UIElement element, double value)
        {
            element.SetValue(LeftProperty, value);
        }

        public static void SetTop(UIElement element, double value)
        {
            element.SetValue(TopProperty, value);
        }
    }
}