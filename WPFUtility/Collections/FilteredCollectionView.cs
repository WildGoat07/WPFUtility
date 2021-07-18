using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Linq;

namespace Wildgoat.WPFUtility.Collections
{
    /// <summary>
    /// A collection view that filters an underlying collection
    /// </summary>
    public class FilteredCollectionView : IBaseCollectionSource, IEnumerable<object?>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private Predicate<object?> filter;
        private List<bool> filtered;

        private IBaseCollectionSource source;

        public FilteredCollectionView(IBaseCollectionSource source, Predicate<object?> filter)
        {
            this.source = source;
            this.filter = filter;
            filtered = new List<bool>();
            LinkSource();
            InitSource();
        }

        ~FilteredCollectionView()
        {
            UnlinkSource();
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Filter used on the underlying collection
        /// </summary>
        public Predicate<object?> Filter
        {
            get => filter;
            set
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.ToList(), 0));
                filter = value;
                InitSource();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.ToList(), 0));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filter)));
            }
        }

        /// <summary>
        /// The underlying list
        /// </summary>
        public IBaseCollectionSource Source
        {
            get => source;
            set
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.ToList(), 0));
                UnlinkSource();
                source = value;
                LinkSource();
                InitSource();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.ToList(), 0));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            }
        }

        public IEnumerator<object?> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        filtered.InsertRange(e.NewStartingIndex, e.NewItems.Cast<object?>().Select(item => filter(item)));

                        var items = e.NewItems.Cast<object?>().Where(item => filter(item));
                        if (items.Any())
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList(), GetFilteredIndex(e.NewStartingIndex)));
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        var items = e.OldItems.Cast<object?>();
                        var filteredItems = items.Where((item, index) => filtered[index + e.OldStartingIndex]);
                        if (filteredItems.Any())
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, filteredItems.ToList(), GetFilteredIndex(e.OldStartingIndex)));
                        filtered.RemoveRange(e.OldStartingIndex, items.Count());
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        for (int i = 0; i < e.OldItems.Count; ++i)
                        {
                            var oldItem = e.OldItems[i];
                            var newItem = e.NewItems[i];
                            var oldFiltered = filtered[e.NewStartingIndex + i];
                            filtered[e.NewStartingIndex + i] = filter(e.NewItems[i]);
                            var newFiltered = filtered[e.NewStartingIndex + i];
                            if (oldFiltered && newFiltered)
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, GetFilteredIndex(e.NewStartingIndex + i)));
                            else if (oldFiltered)
                            {
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, GetFilteredIndex(e.NewStartingIndex + i)));
                            }
                            else if (newFiltered)
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, GetFilteredIndex(e.NewStartingIndex + i)));
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        var range = filtered.GetRange(e.OldStartingIndex, e.OldItems.Count);
                        var oldIndex = GetFilteredIndex(e.OldStartingIndex);
                        var newIndex = GetFilteredIndex(e.NewStartingIndex);
                        if (e.OldStartingIndex < e.NewStartingIndex)
                        {
                            filtered.InsertRange(e.NewStartingIndex, range);
                            filtered.RemoveRange(e.OldStartingIndex, range.Count);
                        }
                        else
                        {
                            filtered.RemoveRange(e.OldStartingIndex, range.Count);
                            filtered.InsertRange(e.NewStartingIndex, range);
                        }
                        var items = e.NewItems.Cast<object?>().Where(item => filter(item));
                        if (items.Any())
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items.ToList(), newIndex, oldIndex));
                    }
                    break;
            }
        }

        private int GetFilteredIndex(int sourceIndex)
        {
            var result = 0;
            for (int i = 0; i < sourceIndex; ++i)
            {
                if (filtered[i])
                    ++result;
            }
            return result;
        }

        private void InitSource()
        {
            filtered.Clear();
            foreach (var item in source)
                filtered.Add(filter(item));
        }

        private void LinkSource()
        {
            source.CollectionChanged += OnSourceCollectionChanged;
        }

        private void UnlinkSource()
        {
            source.CollectionChanged -= OnSourceCollectionChanged;
        }

        private class Enumerator : IEnumerator<object?>, IEnumerator
        {
            private int index;

            public Enumerator(FilteredCollectionView source)
            {
                Source = source;
            }

            public object? Current => UnderlyingEnumerator?.Current;
            private FilteredCollectionView Source { get; }
            private IEnumerator? UnderlyingEnumerator { get; set; }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (UnderlyingEnumerator == null)
                    UnderlyingEnumerator = Source.source.GetEnumerator();
                while (UnderlyingEnumerator.MoveNext())
                    if (Source.filtered[index++])
                        return true;
                return false;
            }

            public void Reset()
            {
                UnderlyingEnumerator = null;
            }
        }
    }
}