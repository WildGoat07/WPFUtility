using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wildgoat.WPFUtility.Collections
{
    public class SetCollectionView : IBaseCollectionSource, IEnumerable<object?>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const int NullHash = -684684657;
        private IEqualityComparer? comparer;
        private IBaseCollectionSource source;

        public SetCollectionView(object source, IEqualityComparer? comparer)
        {
            this.source = CollectionSourceWrapper.GetCollection(source);
            this.comparer = comparer;
            Hashes = new List<int>();
            InitSource();
            LinkSource();
        }

        public SetCollectionView(object source) : this(source, null)
        {
        }

        ~SetCollectionView()
        {
            UnlinkSource();
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public IEqualityComparer? Comparer
        {
            get => comparer;
            set
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.ToList(), 0));
                comparer = value;
                InitSource();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.ToList(), 0));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comparer)));
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
                InitSource();
                LinkSource();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.ToList(), 0));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            }
        }

        private List<int> Hashes { get; }

        public IEnumerator<object?> GetEnumerator()
        {
            var index = 0;
            var itemYield = new HashSet<int>();
            foreach (var item in source)
            {
                var itemHash = Hashes[index];
                if (itemYield.Add(itemHash))
                    yield return item;
                ++index;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int GetSetIndex(int targetIndex)
        {
            var index = 0;
            var itemHashed = new HashSet<int>();
            for (int i = 0; i < targetIndex; ++i)
                if (itemHashed.Add(Hashes[i]))
                    ++index;
            return index;
        }

        private void InitSource()
        {
            Hashes.Clear();
            foreach (var item in source)
                Hashes.Add(item is null
                    ? NullHash
                    : comparer is null
                        ? item.GetHashCode()
                        : comparer.GetHashCode(item));
        }

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
                        var index = e.NewStartingIndex;
                        foreach (var item in e.NewItems)
                        {
                            var itemHash = item is null
                                ? NullHash
                                : comparer is null
                                    ? item.GetHashCode()
                                    : comparer.GetHashCode(item);
                            var instanceFound = false;
                            for (int i = 0; i < index; ++i)
                                if (itemHash == Hashes[i])
                                {
                                    instanceFound = true;
                                    break;
                                }
                            if (!instanceFound)
                            {
                                instanceFound = false;
                                int copyIndex;
                                for (copyIndex = index; copyIndex < Hashes.Count; ++copyIndex)
                                    if (itemHash == Hashes[copyIndex])
                                    {
                                        instanceFound = true;
                                        break;
                                    }
                                if (instanceFound)
                                {
                                    var setIndex = GetSetIndex(index);
                                    var setCopyIndex = GetSetIndex(copyIndex);
                                    if (setIndex != setCopyIndex)
                                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, setIndex, setCopyIndex));
                                }
                                else
                                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, GetSetIndex(index)));
                            }
                            Hashes.Insert(index, itemHash);
                            ++index;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var item in e.OldItems)
                        {
                            var itemHash = item is null
                                ? NullHash
                                : comparer is null
                                    ? item.GetHashCode()
                                    : comparer.GetHashCode(item);
                            var instanceFound = false;
                            for (int i = 0; i < e.OldStartingIndex; ++i)
                                if (itemHash == Hashes[i])
                                {
                                    instanceFound = true;
                                    break;
                                }
                            if (!instanceFound)
                            {
                                instanceFound = false;
                                int copyIndex;
                                for (copyIndex = e.OldStartingIndex + 1; copyIndex < Hashes.Count; ++copyIndex)
                                    if (itemHash == Hashes[copyIndex])
                                    {
                                        instanceFound = true;
                                        break;
                                    }
                                if (instanceFound)
                                {
                                    var setIndex = GetSetIndex(e.OldStartingIndex);
                                    var setCopyIndex = GetSetIndex(copyIndex) - 1;
                                    if (setIndex != setCopyIndex)
                                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, setCopyIndex, setIndex));
                                }
                                else
                                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, GetSetIndex(e.OldStartingIndex)));
                            }
                            Hashes.RemoveAt(e.OldStartingIndex);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        var index = e.NewStartingIndex;
                        foreach (var item in e.NewItems)
                        {
                            var itemHash = item is null
                                ? NullHash
                                : comparer is null
                                    ? item.GetHashCode()
                                    : comparer.GetHashCode(item);
                            var oldHash = Hashes[index];
                            var oldItem = e.OldItems[index - e.NewStartingIndex];
                            if (itemHash != oldHash)
                            {
                                // remove old
                                var instanceFound = false;
                                for (int i = 0; i < index; ++i)
                                    if (oldHash == Hashes[i])
                                    {
                                        instanceFound = true;
                                        break;
                                    }
                                if (!instanceFound)
                                {
                                    instanceFound = false;
                                    int copyIndex;
                                    for (copyIndex = index + 1; copyIndex < Hashes.Count; ++copyIndex)
                                        if (oldHash == Hashes[copyIndex])
                                        {
                                            instanceFound = true;
                                            break;
                                        }
                                    if (instanceFound)
                                    {
                                        var setIndex = GetSetIndex(index);
                                        var setCopyIndex = GetSetIndex(copyIndex) - 1;
                                        if (setIndex != setCopyIndex)
                                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, oldItem, setCopyIndex, setIndex));
                                    }
                                    else
                                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, GetSetIndex(index)));
                                }
                                Hashes.RemoveAt(index);
                                // add new
                                instanceFound = false;
                                for (int i = 0; i < index; ++i)
                                    if (itemHash == Hashes[i])
                                    {
                                        instanceFound = true;
                                        break;
                                    }
                                if (!instanceFound)
                                {
                                    instanceFound = false;
                                    int copyIndex;
                                    for (copyIndex = index; copyIndex < Hashes.Count; ++copyIndex)
                                        if (itemHash == Hashes[copyIndex])
                                        {
                                            instanceFound = true;
                                            break;
                                        }
                                    if (instanceFound)
                                    {
                                        var setIndex = GetSetIndex(index);
                                        var setCopyIndex = GetSetIndex(copyIndex);
                                        if (setIndex != setCopyIndex)
                                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, setIndex, setCopyIndex));
                                    }
                                    else
                                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, GetSetIndex(index)));
                                }

                                Hashes.Insert(index, itemHash);
                            }
                            ++index;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        if (e.NewStartingIndex > e.OldStartingIndex)
                        {
                            foreach (var item in e.NewItems)
                            {
                                var itemHash = Hashes[e.OldStartingIndex];

                                var instanceFound = false;
                                for (int i = 0; i < e.OldStartingIndex; ++i)
                                    if (itemHash == Hashes[i])
                                    {
                                        instanceFound = true;
                                        break;
                                    }
                                if (!instanceFound)
                                {
                                    var smallestFutureIndex = e.NewStartingIndex;
                                    for (int i = e.OldStartingIndex + 1; i < Hashes.Count && i < e.NewStartingIndex; ++i)
                                        if (itemHash == Hashes[i])
                                        {
                                            smallestFutureIndex = i;
                                            break;
                                        }
                                    var smallestFutureSetIndex = GetSetIndex(smallestFutureIndex);
                                    var smallestOldSetIndex = GetSetIndex(e.OldStartingIndex);
                                    if (smallestFutureSetIndex != smallestOldSetIndex)
                                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, smallestFutureSetIndex, smallestOldSetIndex));
                                }
                                Hashes.Insert(e.NewStartingIndex, itemHash);
                                Hashes.RemoveAt(e.OldStartingIndex);
                            }
                        }
                        else
                        {
                            var offset = 0;
                            foreach (var item in e.NewItems)
                            {
                                var itemHash = Hashes[e.OldStartingIndex + offset];

                                var instanceFound = false;
                                for (int i = 0; i < e.NewStartingIndex + offset; ++i)
                                    if (itemHash == Hashes[i])
                                    {
                                        instanceFound = true;
                                        break;
                                    }
                                if (!instanceFound)
                                {
                                    var smallestOldIndex = e.OldStartingIndex + offset;
                                    for (int i = e.NewStartingIndex + offset; i < Hashes.Count && i < e.OldStartingIndex + offset; ++i)
                                        if (itemHash == Hashes[i])
                                        {
                                            smallestOldIndex = i;
                                            break;
                                        }
                                    var smallestFutureSetIndex = GetSetIndex(e.NewStartingIndex + offset);
                                    var smallestOldSetIndex = GetSetIndex(smallestOldIndex);
                                    if (smallestFutureSetIndex != smallestOldSetIndex)
                                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, smallestFutureSetIndex, smallestOldSetIndex));
                                }
                                Hashes.RemoveAt(e.OldStartingIndex + offset);
                                Hashes.Insert(e.NewStartingIndex + offset, itemHash);
                                ++offset;
                            }
                        }
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