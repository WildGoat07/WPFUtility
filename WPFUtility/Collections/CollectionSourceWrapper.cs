using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wildgoat.WPFUtility.Collections
{
    internal class CollectionSourceWrapper : IBaseCollectionSource, IEnumerable<object?>, IEnumerable, INotifyCollectionChanged
    {
        private List<object?> buffer;
        private object source;

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        internal CollectionSourceWrapper(object source)
        {
            this.source = source;
            buffer = new List<object?>(((IEnumerable)source).Cast<object?>());
            ((INotifyCollectionChanged)source).CollectionChanged += OnSourceCollectionChanged;
        }

        ~CollectionSourceWrapper()
        {
            ((INotifyCollectionChanged)source).CollectionChanged -= OnSourceCollectionChanged;
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        IEnumerator<object?> IEnumerable<object?>.GetEnumerator() => buffer.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => buffer.GetEnumerator();

        internal static IBaseCollectionSource GetCollection(object source)
        {
            if (source is IBaseCollectionSource collection)
                return collection;
            else if (source is INotifyCollectionChanged && source is IEnumerable)
                return new CollectionSourceWrapper(source);
            else
                throw new InvalidOperationException("The source is not valid");
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    buffer.InsertRange(e.NewStartingIndex, e.NewItems.Cast<object?>());
                    CollectionChanged?.Invoke(this, e);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    buffer.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    CollectionChanged?.Invoke(this, e);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; ++i)
                    {
                        var item = e.NewItems[i];
                        var oldItem = buffer[i + e.NewStartingIndex];
                        buffer[i + e.NewStartingIndex] = item;
                    }
                    CollectionChanged?.Invoke(this, e);
                    break;

                case NotifyCollectionChangedAction.Move:
                    var range = buffer.GetRange(e.OldStartingIndex, e.OldItems.Count);
                    buffer.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    var insertion = e.NewStartingIndex;
                    if (e.NewStartingIndex > e.OldStartingIndex)
                        insertion -= e.OldItems.Count;
                    buffer.InsertRange(insertion, range);
                    CollectionChanged?.Invoke(this, e);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    var removed = buffer.ToList();
                    buffer.Clear();
                    if (e.NewItems != null)
                        buffer.AddRange(e.NewItems.Cast<object?>());
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, 0));
                    if (buffer.Any())
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, buffer, 0));
                    break;
            }
        }
    }
}