using System;
using System.Globalization;
using System.Windows.Data;

namespace KinematicsDemo.Views;

/// <summary>
/// Inverts a numeric coordinate for viewport transforms without changing robot state.
/// </summary>
public sealed class InvertDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is double coordinate ? -coordinate : 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is double coordinate ? -coordinate : 0d;
    }
}
