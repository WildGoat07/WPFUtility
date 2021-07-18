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
    /// A base collection that can be used as a source of items
    /// </summary>
    /// <typeparam name="T">Type of the items in the list</typeparam>
    public class SourceCollectionList<T> : IBaseCollectionSource, IList<T>, IList, IReadOnlyList<T>, ICollection<T>, ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Create a new source collection using a specified existing collection.
        /// </summary>
        /// <param name="underlyingCollection">Collection to use as storage</param>
        public SourceCollectionList(IList<T> underlyingCollection)
        {
            UnderlyingCollection = underlyingCollection;
            foreach (var item in underlyingCollection)
                if (item is INotifyPropertyChanged notifier)
                    notifier.PropertyChanged += OnItemPropertyChanged;
            SyncRoot = new object();
        }

        /// <summary>
        /// Create a new source collection.
        /// </summary>
        public SourceCollectionList() : this(new List<T>())
        {
        }

        ~SourceCollectionList()
        {
            foreach (var item in UnderlyingCollection)
                if (item is INotifyPropertyChanged notifier)
                    notifier.PropertyChanged -= OnItemPropertyChanged;
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Count => UnderlyingCollection.Count;

        public bool IsFixedSize => UnderlyingCollection is IList list ? list.IsFixedSize : false;

        public bool IsReadOnly => UnderlyingCollection.IsReadOnly;

        public bool IsSynchronized => UnderlyingCollection is ICollection collection ? collection.IsSynchronized : false;

        public object SyncRoot { get; }

        private IList<T> UnderlyingCollection { get; }

        object? IList.this[int index]
        {
            get => UnderlyingCollection[index];
            set
            {
                if (value is T item)
                    this[index] = item;
                else
                    throw new InvalidOperationException($"The item of type {value?.GetType()} can not be added as {typeof(T)}");
            }
        }

        public T this[int index]
        {
            get => UnderlyingCollection[index];
            set
            {
                var oldValue = UnderlyingCollection[index];
                if (oldValue is INotifyPropertyChanged notifier)
                    notifier.PropertyChanged -= OnItemPropertyChanged;
                UnderlyingCollection[index] = value;
                if (value is INotifyPropertyChanged newNotifier)
                    newNotifier.PropertyChanged += OnItemPropertyChanged;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, index));
            }
        }

        public T this[Index index]
        {
            get => index.IsFromEnd
                ? UnderlyingCollection[UnderlyingCollection.Count - index.Value]
                : UnderlyingCollection[index.Value];
            set
            {
                var integerIndex = index.IsFromEnd
                    ? UnderlyingCollection.Count - index.Value
                    : index.Value;
                var oldValue = UnderlyingCollection[integerIndex];
                if (oldValue is INotifyPropertyChanged notifier)
                    notifier.PropertyChanged -= OnItemPropertyChanged;
                UnderlyingCollection[integerIndex] = value;
                if (value is INotifyPropertyChanged newNotifier)
                    newNotifier.PropertyChanged += OnItemPropertyChanged;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, integerIndex));
            }
        }

        public T[] this[Range range]
        {
            get
            {
                var (start, size) = range.GetOffsetAndLength(UnderlyingCollection.Count);
                var array = new T[size];
                for (int i = 0; i < size; ++i)
                    array[i] = UnderlyingCollection[i + start];
                return array;
            }
        }

        public void Add(T item)
        {
            UnderlyingCollection.Add(item);
            if (item is INotifyPropertyChanged notifier)
                notifier.PropertyChanged += OnItemPropertyChanged;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Count - 1));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        int IList.Add(object? value)
        {
            if (value is T item)
            {
                Add(item);
                return Count - 1;
            }
            else
                throw new InvalidOperationException($"The item of type {value?.GetType()} can not be added as {typeof(T)}");
        }

        /// <summary>
        /// Adds many items to the collection.
        /// </summary>
        /// <param name="items">List of items to add</param>
        public void AddRange(IEnumerable<T> items)
        {
            var startIndex = Count;
            foreach (var item in items)
            {
                UnderlyingCollection.Add(item);
                if (item is INotifyPropertyChanged notifier)
                    notifier.PropertyChanged += OnItemPropertyChanged;
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList(), startIndex));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        /// <summary>
        /// Adds many items to the collection.
        /// </summary>
        /// <param name="items">List of items to add</param>
        public void AddRange(params T[] items) => AddRange(items.AsEnumerable());

        public void Clear()
        {
            var removedItems = UnderlyingCollection.ToList();
            foreach (var item in UnderlyingCollection)
                if (item is INotifyPropertyChanged notifier)
                    notifier.PropertyChanged -= OnItemPropertyChanged;
            UnderlyingCollection.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, 0));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        public bool Contains(T item) => UnderlyingCollection.Contains(item);

        bool IList.Contains(object? value)
        {
            if (value is T item)
                return Contains(item);
            else
                return false;
        }

        public void CopyTo(T[] array, int arrayIndex) => UnderlyingCollection.CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index)
        {
            foreach (var item in UnderlyingCollection)
                array.SetValue(item, index++);
        }

        public IEnumerator<T> GetEnumerator() => UnderlyingCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(T item) => UnderlyingCollection.IndexOf(item);

        int IList.IndexOf(object? value)
        {
            if (value is T item)
                return IndexOf(item);
            else
                return -1;
        }

        public void Insert(int index, T item)
        {
            UnderlyingCollection.Insert(index, item);
            if (item is INotifyPropertyChanged notifier)
                notifier.PropertyChanged += OnItemPropertyChanged;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        void IList.Insert(int index, object? value)
        {
            if (value is T item)
                Insert(index, item);
            else
                throw new InvalidOperationException($"The item of type {value?.GetType()} can not be added as {typeof(T)}");
        }

        /// <summary>
        /// Move an item to another place in the list
        /// </summary>
        /// <param name="source">Initial index of the item</param>
        /// <param name="target">Target index of the item</param>
        public void Move(int source, int target)
        {
            if (source != target)
            {
                var item = UnderlyingCollection[source];
                UnderlyingCollection.Insert(target, item);
                if (source < target)
                    UnderlyingCollection.RemoveAt(source);
                else
                    UnderlyingCollection.RemoveAt(source + 1);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, target, source));
            }
        }

        /// <summary>
        /// Move an item to another place in the list
        /// </summary>
        /// <param name="item">Item to mvoe</param>
        /// <param name="target">Target index of the item</param>
        public void Move(T item, int target)
        {
            var source = UnderlyingCollection.IndexOf(item);
            if (source != target)
            {
                UnderlyingCollection.Insert(target, item);
                if (source < target)
                    UnderlyingCollection.RemoveAt(source);
                else
                    UnderlyingCollection.RemoveAt(source + 1);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, target, source));
            }
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            var result = UnderlyingCollection.Remove(item);
            if (result)
            {
                if (item is INotifyPropertyChanged notifier)
                    notifier.PropertyChanged -= OnItemPropertyChanged;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
            return result;
        }

        void IList.Remove(object? value)
        {
            if (value is T item)
                Remove(item);
        }

        public void RemoveAt(int index)
        {
            var removedItem = UnderlyingCollection[index];
            UnderlyingCollection.RemoveAt(index);
            if (removedItem is INotifyPropertyChanged notifier)
                notifier.PropertyChanged -= OnItemPropertyChanged;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (T)sender;
            var index = UnderlyingCollection.IndexOf(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, item, index));
        }
    }
}