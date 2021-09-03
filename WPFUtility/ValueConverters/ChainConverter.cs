using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace Wildgoat.WPFUtility.ValueConverters
{
    /// <summary>
    /// Converter used to call multiple other converter, one after another
    /// </summary>
    public class ChainConverter : List<ConverterCall>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var call in this)
            {
                if (call.Converter is IValueConverter converter)
                    value = converter.Convert(value, call.TargetType, call.Parameter, culture);
                else if (call.Converter is IMultiValueConverter multiConverter && value is IEnumerable enumerable)
                    value = multiConverter.Convert(enumerable.Cast<object>().ToArray(), call.TargetType, call.Parameter, culture);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A call instance of the chain converter
    /// </summary>
    public class ConverterCall
    {
        /// <summary>
        /// Converter to use
        /// </summary>
        public object? Converter { get; set; }

        /// <summary>
        /// Optional parameter to pass to the converter
        /// </summary>
        public object? Parameter { get; set; }

        /// <summary>
        /// The target type of the converter, if anybody use that
        /// </summary>
        public Type? TargetType { get; set; }
    }
}