using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFBase.Converters
{
    /// <summary>
    /// Converts a DocumentCount integer to a formatted string for display
    /// </summary>
    public class DocumentCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count switch
                {
                    0 => "No documents open",
                    1 => "1 document open",
                    _ => $"{count} documents open"
                };
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a theme name string to a user-friendly display name
    /// </summary>
    public class ThemeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string themeName)
            {
                return themeName switch
                {
                    "Light" => "Light Theme",
                    "Dark" => "Dark Theme",
                    "HighContrast" => "High Contrast",
                    _ => themeName
                };
            }
            return "Unknown Theme";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean dirty state to an appropriate icon or symbol
    /// </summary>
    public class DirtyStateToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDirty)
            {
                return isDirty ? "●" : "";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts progress value to a brush color based on completion
    /// </summary>
    public class ProgressToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                return progress switch
                {
                    <= 25 => new SolidColorBrush(Colors.Red),
                    <= 50 => new SolidColorBrush(Colors.Orange),
                    <= 75 => new SolidColorBrush(Colors.Yellow),
                    <= 100 => new SolidColorBrush(Colors.Green),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts file size in bytes to a human-readable format
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long bytes && bytes >= 0)
            {
                int i = 0;
                double dValue = bytes;
                while (Math.Round(dValue, 1) >= 1000 && i < SizeSuffixes.Length - 1)
                {
                    dValue /= 1024;
                    i++;
                }

                return $"{dValue:0.##} {SizeSuffixes[i]}";
            }
            return "0 bytes";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Multi-value converter that combines multiple boolean values with AND logic
    /// </summary>
    public class MultiBooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (value is bool boolValue && !boolValue)
                {
                    return false;
                }
                if (value == DependencyProperty.UnsetValue)
                {
                    return false;
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Multi-value converter that combines multiple boolean values with OR logic
    /// </summary>
    public class MultiBooleanOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (value is bool boolValue && boolValue)
                {
                    return true;
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}