using System.Globalization;
using System.Windows.Data;
using FolderSC.Core.Models;

namespace FolderSC.UI.Converters;

/// <summary>
/// Converts between <see cref="OutputFormat"/> and boolean for RadioButton group binding.
/// </summary>
public class FormatToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is OutputFormat format && parameter is string targetParam)
        {
            if (Enum.TryParse<OutputFormat>(targetParam, ignoreCase: true, out var targetFormat))
            {
                return format == targetFormat;
            }
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is string targetParam)
        {
            if (Enum.TryParse<OutputFormat>(targetParam, ignoreCase: true, out var targetFormat))
            {
                return targetFormat;
            }
        }

        return Binding.DoNothing;
    }
}
