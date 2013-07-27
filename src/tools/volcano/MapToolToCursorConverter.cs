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
    using System.Windows.Input;

    public class MapToolToCursorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((MapTool)value)
            {
                case MapTool.Pan:
                    return Cursors.ScrollAll;

                case MapTool.SelectObject:
                    return Cursors.Cross;

                case MapTool.PaintChunk:
                    return Cursors.Pen;

                case MapTool.LiftChunk:
                    return Cursors.Arrow;

                default:
                    return Cursors.Arrow;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}