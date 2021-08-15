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
    /// A collection view that is automatically sorted
    /// </summary>
    public class SortedCollectionView : IBaseCollectionSource, IEnumerable<object?>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private IComparer comparer;
        private LinkedList<object?> sortedBuffer;
        private IBaseCollectionSource source;

        public SortedCollectionView(object source, Comparison<object?> comparer)
        {
            sortedBuffer = new LinkedList<object?>();
            this.source = CollectionSourceWrapper.GetCollection(source);
            this.comparer = Comparer<object?>.Create(comparer);
            Initialize();
            LinkSource();
        }

        public SortedCollectionView(object source, IComparer comparer)
        {
            sortedBuffer = new LinkedList<object?>();
            this.source = CollectionSourceWrapper.GetCollection(source);
            this.comparer = comparer;
            Initialize();
            LinkSource();
        }

        ~SortedCollectionView()
        {
            UnlinkSource();
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Comparator used to sort
        /// </summary>
        public IComparer Comparer
        {
            get => comparer;
            set
            {
                comparer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comparer)));
                Refresh();
            }
        }

        /// <summary>
        /// Source collection to be sorted
        /// </summary>
        public object Source
        {
            get => source;
            set
            {
                UnlinkSource();
                source = CollectionSourceWrapper.GetCollection(value);
                Initialize();
                LinkSource();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<object?> GetEnumerator() => sortedBuffer.GetEnumerator();

        private void Initialize()
        {
            if (sortedBuffer.Any())
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, sortedBuffer.ToList(), 0));
            sortedBuffer.Clear();
            foreach (var item in source)
            {
                var current = sortedBuffer.First;
                while (true)
                    if (current is null)
                        break;
                    else if (comparer.Compare(item, current.Value) > 0)
                        current = current.Next;
                    else
                        break;
                if (current is null)
                    sortedBuffer.AddLast(item);
                else
                    sortedBuffer.AddBefore(current, item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sortedBuffer.ToList(), 0));
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
                    foreach (var item in e.NewItems)
                    {
                        var current = sortedBuffer.First;
                        var index = 0;
                        while (true)
                            if (current is null)
                                break;
                            else if (comparer.Compare(item, current.Value) > 0)
                            {
                                current = current.Next;
                                ++index;
                            }
                            else
                                break;
                        if (current is null)
                            sortedBuffer.AddLast(item);
                        else
                            sortedBuffer.AddBefore(current, item);
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var index = 0;
                        var node = sortedBuffer.First;
                        while (node != null && node.Value != item)
                        {
                            node = node.Next;
                            ++index;
                        }
                        if (node != null)
                        {
                            sortedBuffer.Remove(node);
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node.Value, index));
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; ++i)
                    {
                        var oldItem = e.OldItems[i];
                        var replaceIndex = 0;
                        var node = sortedBuffer.First;
                        while (node != null && node.Value != oldItem)
                        {
                            node = node.Next;
                            ++replaceIndex;
                        }
                        if (node != null)
                        {
                            var newItem = e.NewItems[i];
                            sortedBuffer.Remove(node);
                            var current = sortedBuffer.First;
                            var index = 0;
                            while (true)
                                if (current is null)
                                    break;
                                else if (comparer.Compare(newItem, current.Value) > 0)
                                {
                                    current = current.Next;
                                    ++index;
                                }
                                else
                                    break;
                            if (current is null)
                                sortedBuffer.AddLast(newItem);
                            else
                                sortedBuffer.AddBefore(current, newItem);
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, replaceIndex));
                            if (index != replaceIndex)
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, newItem, index, replaceIndex));
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;
            }
        }

        private void Refresh()
        {
            var insertionPoint = sortedBuffer.First;
            var newIndex = 0;
            while (insertionPoint != null)
            {
                var smallest = insertionPoint;
                var node = insertionPoint;
                var oldIndex = -1;
                var currIndex = newIndex;
                while (node != null)
                {
                    if (comparer.Compare(smallest.Value, node.Value) > 0)
                    {
                        smallest = node;
                        oldIndex = currIndex;
                    }
                    ++currIndex;
                    node = node.Next;
                }
                if (smallest != insertionPoint)
                {
                    sortedBuffer.Remove(smallest);
                    sortedBuffer.AddBefore(insertionPoint, smallest);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, smallest.Value, newIndex, oldIndex));
                }
                insertionPoint = insertionPoint.Next;
                ++newIndex;
            }
        }

        private void UnlinkSource()
        {
            source.CollectionChanged -= OnSourceChanged;
        }
    }
}