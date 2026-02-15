using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantManagement.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool inverse = parameter != null && parameter.ToString().ToLower() == "inverse";
        bool isNull = value == null || string.IsNullOrEmpty(value.ToString());

        if (inverse)
        {
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 