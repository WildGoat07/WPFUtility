using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wildgoat.WPFUtility.Collections
{
    public interface IExtendableItem
    {
        public bool Extended { get; }
        public IEnumerable? Items { get; }
    }

    public class ExtendableCollectionView : IBaseCollectionSource, IEnumerable<object?>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private IBaseCollectionSource source;

        public ExtendableCollectionView(object source)
        {
            this.source = CollectionSourceWrapper.GetCollection(source);
            LinkSource();
        }

        ~ExtendableCollectionView()
        {
            UnlinkSource();
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public object Source
        {
            get => source;
            set
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.ToList(), 0));
                UnlinkSource();
                source = CollectionSourceWrapper.GetCollection(value);
                LinkSource();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.ToList(), 0));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            }
        }

        public IEnumerator<object?> GetEnumerator()
        {
            foreach (var item in source)
            {
                yield return item;
                if (item is IExtendableItem extendable && extendable.Extended && extendable.Items != null)
                    foreach (var subItem in extendable.Items)
                        yield return subItem;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int GetItemIndex(int sourceIndex)
        {
            var index = 0;
            int i = 0;
            foreach (var item in source)
            {
                if (i >= sourceIndex)
                    break;
                ++i;
                ++index;
                if (item is IExtendableItem extendable && extendable.Extended && extendable.Items != null)
                    index += extendable.Items.Cast<object?>().Count();
            }
            return index;
        }

        private int GetItemsCollectionIndex(object collection)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (item is IExtendableItem extendable)
                {
                    if (extendable.Items == collection)
                        if (extendable.Extended)
                            return index + 1;
                        else
                            return -1;
                    if (extendable.Extended && extendable.Items != null)
                        index += extendable.Items.Cast<object?>().Count();
                }
                ++index;
            }
            return -1;
        }

        private void LinkItem(object? item)
        {
            if (item is IExtendableItem extendable)
            {
                if (extendable.Items is IBaseCollectionSource collectionNotifier)
                    collectionNotifier.CollectionChanged += OnSubItemsCollectionChanged;
                if (extendable is INotifyPropertyChanged propertyNotifier)
                    propertyNotifier.PropertyChanged += OnItemChanged;
            }
        }

        private void LinkSource()
        {
            source.CollectionChanged += OnSourceCollectionChanged;
            foreach (var item in source)
                LinkItem(item);
        }

        private void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IExtendableItem extendable && e.PropertyName == nameof(IExtendableItem.Extended))
                if (extendable.Extended && extendable.Items != null)
                {
                    var sourceIndex = 0;
                    foreach (var item in source)
                    {
                        if (item == extendable)
                            break;
                        ++sourceIndex;
                    }
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, extendable.Items.Cast<object?>().ToList(), 1 + GetItemIndex(sourceIndex)));
                }
                else if (extendable.Items != null)
                {
                    var sourceIndex = 0;
                    foreach (var item in source)
                    {
                        if (item == extendable)
                            break;
                        ++sourceIndex;
                    }
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, extendable.Items.Cast<object?>().ToList(), 1 + GetItemIndex(sourceIndex)));
                }
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            e.NewItems.Cast<object?>().SelectMany(item =>
                            item is IExtendableItem extendable && extendable.Items != null && extendable.Extended
                                ? new object?[] { item }.Concat(extendable.Items.Cast<object?>())
                                : new object?[] { item }).ToList(),
                            GetItemIndex(e.NewStartingIndex)));
                        foreach (var item in e.NewItems)
                            LinkItem(item);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                            e.OldItems.Cast<object?>().SelectMany(item =>
                            item is IExtendableItem extendable && extendable.Items != null && extendable.Extended
                                ? new object?[] { item }.Concat(extendable.Items.Cast<object?>())
                                : new object?[] { item }).ToList(),
                            GetItemIndex(e.OldStartingIndex)));
                        foreach (var item in e.OldItems)
                            UnlinkItem(item);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                            e.OldItems.Cast<object?>().SelectMany(item =>
                            item is IExtendableItem extendable && extendable.Items != null && extendable.Extended
                                ? new object?[] { item }.Concat(extendable.Items.Cast<object?>())
                                : new object?[] { item }).ToList(),
                            GetItemIndex(e.NewStartingIndex)));
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            e.NewItems.Cast<object?>().SelectMany(item =>
                            item is IExtendableItem extendable && extendable.Items != null && extendable.Extended
                                ? new object?[] { item }.Concat(extendable.Items.Cast<object?>())
                                : new object?[] { item }).ToList(),
                            GetItemIndex(e.NewStartingIndex)));

                        foreach (var item in e.OldItems)
                            UnlinkItem(item);
                        foreach (var item in e.NewItems)
                            LinkItem(item);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
                            e.NewItems.Cast<object?>().SelectMany(item =>
                            item is IExtendableItem extendable && extendable.Items != null && extendable.Extended
                                ? new object?[] { item }.Concat(extendable.Items.Cast<object?>())
                                : new object?[] { item }).ToList(),
                            GetItemIndex(e.NewStartingIndex),
                            GetItemIndex(e.OldStartingIndex)));
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }

        private void OnSubItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var index = GetItemsCollectionIndex(sender);
                        if (index != -1)
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, index + e.NewStartingIndex));
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        var index = GetItemsCollectionIndex(sender);
                        if (index != -1)
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, index + e.OldStartingIndex));
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        var index = GetItemsCollectionIndex(sender);
                        if (index != -1)
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, index + e.NewStartingIndex));
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        var index = GetItemsCollectionIndex(sender);
                        if (index != -1)
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems, index + e.NewStartingIndex, index + e.OldStartingIndex));
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }

        private void UnlinkItem(object? item)
        {
            if (item is IExtendableItem extendable)
            {
                if (extendable.Items is IBaseCollectionSource collectionNotifier)
                    collectionNotifier.CollectionChanged -= OnSubItemsCollectionChanged;
                if (extendable is INotifyPropertyChanged propertyNotifier)
                    propertyNotifier.PropertyChanged -= OnItemChanged;
            }
        }

        private void UnlinkSource()
        {
            source.CollectionChanged -= OnSourceCollectionChanged;
            foreach (var item in source)
                UnlinkItem(item);
        }
    }
}