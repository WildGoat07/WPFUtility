using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Wildgoat.WPFUtility
{
    /// <summary>
    /// A base class for the ViewModel that listen to every INotifyPropertyChanged property that retrigger the event as itself
    /// </summary>
    public abstract class PropertyChangedPropagator : INotifyPropertyChanged
    {
        public PropertyChangedPropagator()
        {
            NotifyPropertyChangedProperties = GetType().GetProperties()
                .Where(property => property.PropertyType.GetInterface(nameof(INotifyPropertyChanged)) != null)
                .Select<PropertyInfo, (PropertyInfo, INotifyPropertyChanged?)>(property => (property, null))
                .ToDictionary(property => property.Item1.Name);
            NotifyCollectionChangedProperties = GetType().GetProperties()
                .Where(property => property.PropertyType.GetInterface(nameof(INotifyCollectionChanged)) != null)
                .Select<PropertyInfo, (PropertyInfo, INotifyCollectionChanged?)>(property => (property, null))
                .ToDictionary(property => property.Item1.Name);
        }

        ~PropertyChangedPropagator()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private Dictionary<string, (PropertyInfo, INotifyCollectionChanged?)> NotifyCollectionChangedProperties { get; }
        private Dictionary<string, (PropertyInfo, INotifyPropertyChanged?)> NotifyPropertyChangedProperties { get; }

        /// <summary>
        /// Use this method to trigger the PropertyChanged changed event in the implemented classes
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (NotifyPropertyChangedProperties.ContainsKey(propertyName))
            {
                var property = NotifyPropertyChangedProperties[propertyName];
                if (property.Item2 != null)
                    property.Item2.PropertyChanged -= OnSubPropertyChanged;
                var value = property.Item1.GetValue(this) as INotifyPropertyChanged;
                NotifyPropertyChangedProperties[propertyName] = (property.Item1, value);
                if (value != null)
                    value.PropertyChanged += OnSubPropertyChanged;
            }
            if (NotifyCollectionChangedProperties.ContainsKey(propertyName))
            {
                var property = NotifyCollectionChangedProperties[propertyName];
                if (property.Item2 != null)
                    property.Item2.CollectionChanged -= OnSubCollectionChanged;
                var value = property.Item1.GetValue(this) as INotifyCollectionChanged;
                NotifyCollectionChangedProperties[propertyName] = (property.Item1, value);
                if (value != null)
                    value.CollectionChanged += OnSubCollectionChanged;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnSubCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var property = NotifyPropertyChangedProperties.First(prop => prop.Value.Item2 == sender);
            OnPropertyChanged($"{property.Key}");
        }

        private void OnSubPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var property = NotifyPropertyChangedProperties.First(prop => prop.Value.Item2 == sender);
            OnPropertyChanged($"{property.Key}.{e.PropertyName}");
        }
    }
}