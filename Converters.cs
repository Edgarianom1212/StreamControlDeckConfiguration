using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration.Converters;

public class EmptyKeyActionToGridSpanConverter : IValueConverter
{
	object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value != null)
			if (value is string ActionName)
				if (ActionName == "")
					return 5;
		return 1;
	}

	object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

public class EmptyKeyActionToGridPosConverter : IValueConverter
{
	object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value != null)
			if (value is string ActionName)
				if (ActionName == "")
					return 0;
		return 1;
	}

	object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

public class CheckedToBrushConverter : IValueConverter
{
	object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value != null)
			if (value is bool isChecked)
				if (isChecked)
					return new SolidColorBrush(Color.Parse("#FF3399FF"));
		return Application.Current.Resources["ControlBorder"];
	}

	object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
