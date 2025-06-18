using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace HPLC.Helpers;

public class InputConverterHelper : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString(); // Optional: format how value appears in the UI
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var input = value?.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
            return BindingOperations.DoNothing;

        // Normalize comma to dot
        input = input.Replace(',', '.');

        // If input ends with dot, wait for more input
        if (input.EndsWith("."))
            return BindingOperations.DoNothing;

        // Try to parse when input is valid
        if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return result;

        return BindingOperations.DoNothing;
    }
}