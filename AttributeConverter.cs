using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Xslt2Game
{
    public class AttributeConverter : IValueConverter
    {
        public Brush ColorToBrush(string color)
        {
            // https://stackoverflow.com/a/35215401/433626
            Windows.UI.Color c = (Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), color);
            return new SolidColorBrush(c);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Dictionary<string, object> dict = value as Dictionary<string, object>;
            string key = parameter as string;

            if (dict != null && key != null && dict.TryGetValue(key, out var result))
            {
                switch (targetType)
                {
                    case Type t when t == typeof(double):
                        return double.Parse((string)result);

                    case Type t when t == typeof(Brush):
                        return ColorToBrush((string)result);

                    default:
                        throw new NotSupportedException($"Unsupported type: {targetType}");
                }
            }

            if (targetType == typeof(double))
            {
                return 0.0;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
