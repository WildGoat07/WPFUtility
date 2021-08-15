using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Wildgoat.WPFUtility.ValueConverters
{
    public class StringConcatenatorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var builder = new StringBuilder();
            foreach (var value in values)
                if (value is IEnumerable enumerable)
                    foreach (var item in enumerable)
                        builder.Append(item);
                else
                    builder.Append(value);
            return builder.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}