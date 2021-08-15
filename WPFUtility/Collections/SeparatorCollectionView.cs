using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wildgoat.WPFUtility.Collections
{
    public class SeparatorCollectionView : IBaseCollectionSource, IEnumerable<object?>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private int count;
        private object separator;
        private IBaseCollectionSource source;

        public SeparatorCollectionView(object source, object separator)
        {
            this.source = CollectionSourceWrapper.GetCollection(source);
            count = this.source.Cast<object?>().Count();
            LinkSource();
            this.separator = separator;
        }

        ~SeparatorCollectionView()
        {
            UnlinkSource();
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public object Separator
        {
            get => separator;
            set
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.ToList(), 0));
                separator = value;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.ToList(), 0));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Separator)));
            }
        }

        public object Source
        {
            get => source;
            set
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.ToList(), 0));
                UnlinkSource();
                source = CollectionSourceWrapper.GetCollection(value);
                count = source.Cast<object?>().Count();
                LinkSource();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.ToList(), 0));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            }
        }

        public IEnumerator<object?> GetEnumerator()
        {
            bool first = true;
            foreach (var item in source)
            {
                if (first)
                    first = false;
                else
                    yield return separator;
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int GetActualIndex(int index)
            => index == 0
                ? 0
            : (index << 1) - 1;

        private void LinkSource()
        {
            source.CollectionChanged += OnSourceCollectionChanged;
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var newItems = new List<object?>();
                        bool first = e.NewStartingIndex == 0;
                        foreach (var item in e.NewItems)
                        {
                            if (first)
                                first = false;
                            else
                                newItems.Add(separator);
                            newItems.Add(item);
                        }
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, GetActualIndex(e.NewStartingIndex)));
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        var oldItems = new List<object?>();
                        bool first = e.OldStartingIndex == 0;
                        foreach (var item in e.OldItems)
                        {
                            if (first)
                                first = false;
                            else
                                oldItems.Add(separator);
                            oldItems.Add(item);
                        }
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, GetActualIndex(e.OldStartingIndex)));
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        var newItems = new List<object?>();
                        bool first = e.NewStartingIndex == 0;
                        foreach (var item in e.NewItems)
                        {
                            if (first)
                                first = false;
                            else
                                newItems.Add(separator);
                            newItems.Add(item);
                        }
                        var oldItems = new List<object?>();
                        first = e.NewStartingIndex == 0;
                        foreach (var item in e.OldItems)
                        {
                            if (first)
                                first = false;
                            else
                                oldItems.Add(separator);
                            oldItems.Add(item);
                        }
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, GetActualIndex(e.NewStartingIndex)));
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        var items = new List<object?>();
                        bool first = e.NewStartingIndex == 0;
                        foreach (var item in e.NewItems)
                        {
                            if (first)
                                first = false;
                            else
                                items.Add(separator);
                            items.Add(item);
                        }
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, GetActualIndex(e.NewStartingIndex), GetActualIndex(e.OldStartingIndex)));
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }

        private void UnlinkSource()
        {
            source.CollectionChanged -= OnSourceCollectionChanged;
        }
    }
}