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
    using System.Windows.Shapes;
    using System.Collections.ObjectModel;
    using Volcano.Model;
    using System.Globalization;
    using Microsoft.Win32;

    public partial class MapWindow : Window
    {
        static DependencyProperty CurrentChunkProperty =
            DependencyProperty.Register("CurrentChunk", typeof(ChunkTemplate), typeof(MapWindow));

        static DependencyProperty CurrentToolProperty =
            DependencyProperty.Register(
                "CurrentTool",
                typeof(MapTool),
                typeof(MapWindow),
                new FrameworkPropertyMetadata(MapTool.Pan, OnCurrentToolChanged));

        static DependencyProperty ProjectProperty =
            DependencyProperty.Register("Project", typeof(UltimaProject), typeof(MapWindow));

        static DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(
                "SelectedObject", 
                typeof(MapObject), 
                typeof(MapWindow),
                new FrameworkPropertyMetadata(OnSelectedObjectChanged));

        const bool itemsVisibleWhileDragging = false;

        ToolState currentState = PanState.Instance;
        MapTool pushedTool;
        Point dragStart;
        Rectangle selectedObjectReticle = new Rectangle { Stroke = Brushes.Yellow, StrokeThickness = 3.0 };

        public MapWindow()
        {
            InitializeComponent();

            SetBinding(CurrentChunkProperty, new Binding { Path = new PropertyPath("CurrentChunk"), Mode = BindingMode.TwoWay });
            SetBinding(ProjectProperty, new Binding { Path = new PropertyPath("Project"), Mode = BindingMode.TwoWay });

            // (This binding is 2-way because we want it to impact the cursor.)
            SetBinding(CurrentToolProperty, new Binding { Path = new PropertyPath("CurrentTool"), Mode = BindingMode.TwoWay });
        }

        public ChunkTemplate CurrentChunk
        {
            get { return (ChunkTemplate)GetValue(CurrentChunkProperty); }
            set { SetValue(CurrentChunkProperty, value); }
        }

        public MapTool CurrentTool
        {
            get { return (MapTool)GetValue(CurrentToolProperty); }
            set { SetValue(CurrentToolProperty, value); }
        }

        public UltimaProject Project
        {
            get { return (UltimaProject)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public MapObject SelectedObject
        {
            get { return (MapObject)GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }

        void ClearSelection()
        {
            MapView.Children.Remove(this.selectedObjectReticle);
        }

        void SelectRegion(int x, int y, int width, int height)
        {
            MapView.SetLeft(this.selectedObjectReticle, x);
            MapView.SetTop(this.selectedObjectReticle, y);
            this.selectedObjectReticle.Width = width;
            this.selectedObjectReticle.Height = height;

            MapView.Children.Add(this.selectedObjectReticle);            
        }

        void OnCommandOpen(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (Project != null)
            {
                dialog.SelectedPath = Project.GameDirectory;
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (Project == null) { Project = new UltimaProject(); }
                Project.GameDirectory = dialog.SelectedPath;
                Project.Load();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (this.pushedTool == MapTool.None)
                {
                    this.pushedTool = CurrentTool;
                    CurrentTool = MapTool.Pan;
                    e.Handled = true;
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (this.pushedTool != MapTool.None)
                {
                    CurrentTool = this.pushedTool;
                    this.pushedTool = MapTool.None;
                    e.Handled = true;
                }
            }
            base.OnKeyUp(e);
        }

        void OnMapMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.currentState.MouseDown(this, e);
            e.Handled = true;
        }

        void OnMapMouseMove(object sender, MouseEventArgs e)
        {
            this.currentState.MouseMove(this, e);
            e.Handled = true;
        }

        void OnMapMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.currentState.MouseUp(this, e);
            e.Handled = true;
        }

        static void OnCurrentToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (MapWindow)d;

            ToolState newState;
            switch ((MapTool)e.NewValue)
            {
                case MapTool.Pan:
                    newState = PanState.Instance;
                    break;

                case MapTool.SelectObject:
                    newState = SelectObjectState.Instance;
                    break;

                case MapTool.PaintChunk:
                    newState = PaintChunkState.Instance;
                    break;

                case MapTool.LiftChunk:
                    newState = LiftChunkState.Instance;
                    break;

                default:
                    newState = window.currentState;
                    break;
            }

            if (newState != window.currentState)
            {
                window.currentState.LeaveState(window);
                window.currentState = newState;
                window.currentState.EnterState(window);
            }
        }
        
        static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (MapWindow)d;
            window.ClearSelection();

            if (e.NewValue != null)
            {
                var obj = (MapObject)e.NewValue;

                window.SelectRegion(
                    obj.ScreenRect.Left, 
                    obj.ScreenRect.Top, 
                    obj.ScreenRect.Width, 
                    obj.ScreenRect.Height);
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {   
            base.OnContentRendered(e);

            var toolPalette = new ToolWindow();
            toolPalette.Owner = this;
            toolPalette.Show();
        }

        public abstract class ToolState
        {
            public virtual void EnterState(MapWindow window) { }
            public virtual void LeaveState(MapWindow window) { }

            public abstract void MouseDown(MapWindow window, MouseButtonEventArgs e);
            public abstract void MouseMove(MapWindow window, MouseEventArgs e);
            public virtual void MouseUp(MapWindow window, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    window.MapView.ItemsVisible = true;
                }
            }
        }

        public class PaintChunkState : ToolState
        {
            public static readonly ToolState Instance = new PaintChunkState();

            public override void LeaveState(MapWindow window)
            {
                window.ClearSelection();
            }

            public override void MouseDown(MapWindow window, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (window.CurrentChunk != null)
                    {
                        Point clickPoint = e.GetPosition(window.MapView);
                        MapChunk chunk = window.MapView.FindChunk((int)clickPoint.X, (int)clickPoint.Y);
                        if (chunk != null)
                        {
                            chunk.Template = window.CurrentChunk;
                            window.MapView.InvalidateVisual(); // TODO: FIX THIS.
                        }
                    }  
                }
            }

            public override void MouseMove(MapWindow window, MouseEventArgs e)
            {
                window.ClearSelection();

                Point clickPoint = e.GetPosition(window.MapView);
                MapChunk chunk = window.MapView.FindChunk((int)clickPoint.X, (int)clickPoint.Y);
                if (chunk != null)
                {
                    if (e.LeftButton == MouseButtonState.Pressed && window.CurrentChunk != null)
                    {
                        chunk.Template = window.CurrentChunk;
                        window.MapView.InvalidateVisual(); // TODO: FIX THIS.
                    }

                    // Don't know why I need this adjustment.
                    //
                    window.SelectRegion(
                        chunk.Location.X - MapUnits.PixelsPerTile,
                        chunk.Location.Y - MapUnits.PixelsPerTile, 
                        MapUnits.PixelsPerChunk, 
                        MapUnits.PixelsPerChunk);
                }
            }
        }

        public class LiftChunkState : ToolState
        {
            public static readonly ToolState Instance = new LiftChunkState();

            public override void MouseDown(MapWindow window, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Point clickPoint = e.GetPosition(window.MapView);
                    MapChunk chunk = window.MapView.FindChunk((int)clickPoint.X, (int)clickPoint.Y);
                    if (chunk != null)
                    {
                        window.CurrentChunk = chunk.Template;
                    }
                    window.currentState = PaintChunkState.Instance;
                }
            }

            public override void MouseMove(MapWindow window, MouseEventArgs e)
            {
                window.ClearSelection();

                Point clickPoint = e.GetPosition(window.MapView);
                MapChunk chunk = window.MapView.FindChunk((int)clickPoint.X, (int)clickPoint.Y);
                if (chunk != null)
                {
                    // Don't know why I need this adjustment.
                    //
                    window.SelectRegion(
                        chunk.Location.X - MapUnits.PixelsPerTile,
                        chunk.Location.Y - MapUnits.PixelsPerTile,
                        MapUnits.PixelsPerChunk,
                        MapUnits.PixelsPerChunk);
                }
            }
        }

        public class PanState : ToolState
        {
            public static readonly ToolState Instance = new PanState();

            public override void MouseDown(MapWindow window, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    window.dragStart = e.GetPosition(window);
                    window.MapView.CaptureMouse();
                }
            }

            public override void MouseMove(MapWindow window, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentPoint = e.GetPosition(window);
                    int deltaX = (int)(currentPoint.X - window.dragStart.X);
                    int deltaY = (int)(currentPoint.Y - window.dragStart.Y);

                    window.MapView.CameraX -= deltaX;
                    window.MapView.CameraY -= deltaY;

                    window.dragStart = currentPoint;
                    window.MapView.ItemsVisible = itemsVisibleWhileDragging;
                }
            }

            public override void MouseUp(MapWindow window, MouseButtonEventArgs e)
            {
                window.MapView.ItemsVisible = true;
                window.MapView.ReleaseMouseCapture();
            }
        }

        public class SelectObjectState : ToolState
        {
            public static readonly ToolState Instance = new SelectObjectState();

            public override void MouseDown(MapWindow window, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Point point = e.GetPosition(window.MapView);
                    window.SelectedObject = window.MapView.FindObjectThatYouClickedOn((int)point.X, (int)point.Y);
                }
            }

            public override void MouseMove(MapWindow window, MouseEventArgs e)
            {
            }
        }
    }
}
