using System.Globalization;
using System.Windows;
using System.Windows.Data;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Converters;

public class ViewModeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EmployeeViewMode currentMode && parameter is string requestedMode)
        {
            if (Enum.TryParse<EmployeeViewMode>(requestedMode, out var parsedMode))
            {
                return currentMode == parsedMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ViewModeToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EmployeeViewMode currentMode && parameter is string requestedMode)
        {
            if (Enum.TryParse<EmployeeViewMode>(requestedMode, out var parsedMode))
            {
                return currentMode == parsedMode ? "#FF81C784" : "Transparent";
            }
        }
        
        return "Transparent";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 