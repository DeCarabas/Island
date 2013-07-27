namespace Volcano
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Volcano.Model;

    public partial class ToolWindow : Window
    {
        public static readonly DependencyProperty SelectedToolProperty =
            DependencyProperty.Register("SelectedTool", typeof(MapTool), typeof(ToolWindow));

        public ToolWindow()
        {
            InitializeComponent();

            SetBinding(SelectedToolProperty, new Binding { Path = new PropertyPath("CurrentTool"), Mode = BindingMode.TwoWay });
            TabControl.SetBinding(
                TabControl.SelectedItemProperty, 
                new Binding 
                { 
                    Path = new PropertyPath("CurrentTool"), 
                    Mode = BindingMode.TwoWay, 
                    Converter = new TabToToolConverter(this) 
                });
        }

        public MapTool SelectedTool
        {
            get { return (MapTool)GetValue(SelectedToolProperty); }
            set { SetValue(SelectedToolProperty, value); }
        }

        void OnClickLiftChunkButton(object sender, RoutedEventArgs e)
        {
            SelectedTool = MapTool.LiftChunk;
        }

        class TabToToolConverter : SimpleValueConverter<MapTool>
        {
            ToolWindow toolWindow;

            public TabToToolConverter(ToolWindow toolWindow)
            {
                this.toolWindow = toolWindow;
            }

            protected override object Convert(MapTool value)
            {
                switch ((MapTool)value)
                {
                    case MapTool.None:
                    case MapTool.Pan:
                    case MapTool.SelectObject:
                        return this.toolWindow.PropertiesTab;

                    case MapTool.PaintChunk:
                    case MapTool.LiftChunk:
                    default:
                        return this.toolWindow.ChunksTab;
                }
            }

            protected override MapTool ConvertBack(object value)
            {
                if (value == toolWindow.ChunksTab)
                {
                    return MapTool.PaintChunk;
                }
                else if (value == toolWindow.PropertiesTab)
                {
                    return MapTool.SelectObject;
                }

                return MapTool.Pan; // ?
            }
        }
    }

    public abstract class SimpleValueConverter<TInput> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((TInput)value);
        }

        protected abstract object Convert(TInput value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value);
        }

        protected virtual TInput ConvertBack(object value)
        {
            throw new NotSupportedException();
        }
    }
}
