using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Wildgoat.WPFUtility.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(object))]
    public class ConvertedValueExtension : MarkupExtension
    {
        public IValueConverter? Converter { get; set; }
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
        public object? Parameter { get; set; }
        public object? Source { get; set; }
        public Type TargetType { get; set; } = typeof(object);

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            return Converter?.Convert(Source, TargetType, Parameter, Culture) ?? Source;
        }
    }
}