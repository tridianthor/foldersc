using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FolderSC.UI.Converters;

/// <summary>
/// Converts a boolean value to an inverted <see cref="Visibility"/> (true -> Collapsed, false -> Visible).
/// </summary>
public class InvertedBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility visibility && visibility != Visibility.Visible;
    }
}
