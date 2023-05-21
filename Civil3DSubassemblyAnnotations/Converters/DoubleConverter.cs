﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Civil3DSubassemblyAnnotations.Converters
{
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("{0:F2}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value?.ToString().Replace(" ", ""), out double number))
                return number;
            return null;
        }
    }
}
