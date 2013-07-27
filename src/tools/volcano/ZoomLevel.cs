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

    public class ZoomLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((double)value);
        }

        double Convert(double value)
        {
            if (value < 0) { return 0.1; }
            if (value > 1) { return 5.0; }

            if (value < 0.5)
            {
                return LinearInterpolate(value / 0.5, 0.25, 1.0);
            }
            else
            {
                return LinearInterpolate((value - 0.5) / 0.5, 1.0, 5.0);
            }
        }

        double LinearInterpolate(double value, double min, double max)
        {
            return (value * (max - min)) + min;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}