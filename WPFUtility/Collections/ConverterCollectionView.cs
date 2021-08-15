using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wildgoat.WPFUtility.Collections
{
    /// <summary>
    /// A collection used to convert
    /// </summary>
    public class ConverterCollectionView : IBaseCollectionSource, IEnumerable<object?>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private Func<object?, object?> converter;

        private List<object?> internalBuffer;
        private IBaseCollectionSource source;

        public ConverterCollectionView(IBaseCollectionSource source, Func<object?, object?> converter)
        {
            this.source = source;
            this.converter = converter;
            internalBuffer = new List<object?>();
            LinkSource();
            Initialize();
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Converter used to map the source items into the converted type
        /// </summary>
        public Func<object?, object?> Converter
        {
            get => converter;
            set
            {
                converter = value;
                Refresh();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Converter)));
            }
        }

        /// <summary>
        /// Source collection to map to the target type
        /// </summary>
        public IBaseCollectionSource Source
        {
            get => source;
            set
            {
                UnlinkSource();
                source = value;
                Initialize();
                LinkSource();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            }
        }

        public IEnumerator<object?> GetEnumerator() => internalBuffer.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void Initialize()
        {
            if (internalBuffer.Any())
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, internalBuffer.ToList(), 0));
            internalBuffer.Clear();
            foreach (var item in source)
                internalBuffer.Add(converter(item));
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, internalBuffer.ToList(), 0));
        }

        private void LinkSource()
        {
            source.CollectionChanged += OnSourceChanged;
        }

        private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var newItems = e.NewItems.Cast<object?>().Select(item => converter(item)).ToList();
                        internalBuffer.InsertRange(e.NewStartingIndex, newItems);
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, e.NewStartingIndex));
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        var oldItems = internalBuffer.GetRange(e.OldStartingIndex, e.OldItems.Count);
                        internalBuffer.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, e.OldStartingIndex));
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        var oldItems = internalBuffer.GetRange(e.OldStartingIndex, e.OldItems.Count);
                        for (int i = 0; i < e.NewItems.Count; ++i)
                            internalBuffer[i + e.NewStartingIndex] = converter(e.NewItems[i]);
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, internalBuffer.GetRange(e.OldStartingIndex, e.OldItems.Count), oldItems, e.NewStartingIndex));
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        var range = internalBuffer.GetRange(e.OldStartingIndex, e.OldItems.Count).ToList();
                        if (e.OldStartingIndex < e.NewStartingIndex)
                        {
                            internalBuffer.InsertRange(e.NewStartingIndex, range);
                            internalBuffer.RemoveRange(e.OldStartingIndex, range.Count);
                        }
                        else
                        {
                            internalBuffer.RemoveRange(e.OldStartingIndex, range.Count);
                            internalBuffer.InsertRange(e.NewStartingIndex, range);
                        }
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, range, e.NewStartingIndex, e.OldStartingIndex));
                    }
                    break;
            }
        }

        private void Refresh()
        {
            var oldItems = internalBuffer.ToList();
            var index = 0;
            foreach (var item in source)
            {
                internalBuffer[index] = converter(item);
                ++index;
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, internalBuffer.ToList(), oldItems, 0));
        }

        private void UnlinkSource()
        {
            source.CollectionChanged -= OnSourceChanged;
        }
    }
}