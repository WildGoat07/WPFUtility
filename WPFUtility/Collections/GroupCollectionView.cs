using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace Wildgoat.WPFUtility.Collections
{
    public class GroupCollectionView : IBaseCollectionSource, IEnumerable<GroupItemView>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private List<GroupItemView> buffer;

        private Func<object?, object> keySelector;

        private IBaseCollectionSource source;

        public GroupCollectionView(Func<object?, object> keySelector, IBaseCollectionSource source)
        {
            buffer = new List<GroupItemView>();
            this.source = source;
            this.keySelector = keySelector;
            LinkSource();
        }

        ~GroupCollectionView()
        {
            foreach (var group in buffer)
                group.Values.CollectionChanged -= OnFilteredGroupChanged;
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public IBaseCollectionSource Source
        {
            get => source;
            set
            {
                UnlinkSource();
                source = value;
                LinkSource();
                Initialize();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<GroupItemView> GetEnumerator() => buffer.GetEnumerator();

        private void Initialize()
        {
            if (buffer.Any())
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, buffer.ToList(), 0));
            buffer.Clear();
            foreach (var item in source)
            {
                var key = keySelector(item);
                if (!buffer.Any(g => g.Key == key))
                    buffer.Add(new GroupItemView(key, new FilteredCollectionView(source, item => key == keySelector(item))));
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, buffer.ToList(), 0));
        }

        private void LinkSource()
        {
            source.CollectionChanged += OnSourceCollectionChanged;
        }

        private void OnFilteredGroupChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var key = keySelector(item);
                        var group = buffer.First(g => g.Key == key);
                        if (!group.Values.GetEnumerator().MoveNext())
                        {
                            var index = buffer.IndexOf(group);
                            buffer.Remove(group);
                            group.Values.CollectionChanged -= OnFilteredGroupChanged;
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, group, index));
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; ++i)
                    {
                        var oldItem = e.OldItems[i];
                        var newItem = e.NewItems[i];

                        var oldKey = keySelector(oldItem);
                        var newKey = keySelector(newItem);
                        if (oldKey != newKey)
                        {
                            var oldGroup = buffer.First(g => g.Key == oldKey);
                            if (!oldGroup.Values.GetEnumerator().MoveNext())
                            {
                                var index = buffer.IndexOf(oldGroup);
                                buffer.Remove(oldGroup);
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldGroup, index));
                            }
                        }
                        if (!buffer.Any(g => g.Key == newKey))
                        {
                            var group = new GroupItemView(newKey, new FilteredCollectionView(source, item => newKey == keySelector(item)));
                            buffer.Add(group);
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, group, buffer.IndexOf(group)));
                        }
                    }
                    break;
            }
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var key = keySelector(item);
                        if (!buffer.Any(g => g.Key == key))
                        {
                            var group = new GroupItemView(key, new FilteredCollectionView(source, item => key == keySelector(item)));
                            group.Values.CollectionChanged += OnFilteredGroupChanged;
                            buffer.Add(group);
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, group, buffer.IndexOf(group)));
                        }
                    }
                    break;
            }
        }

        private void UnlinkSource()
        {
            source.CollectionChanged -= OnSourceCollectionChanged;
        }
    }

    public class GroupItemView : IEquatable<GroupItemView?>
    {
        internal GroupItemView(object key, IBaseCollectionSource values)
        {
            Key = key;
            Values = values;
        }

        public object Key { get; }
        public IBaseCollectionSource Values { get; }

        public static bool operator !=(GroupItemView? left, GroupItemView? right)
        {
            return !(left == right);
        }

        public static bool operator ==(GroupItemView? left, GroupItemView? right)
        {
            return EqualityComparer<GroupItemView>.Default.Equals(left, right);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as GroupItemView);
        }

        public bool Equals(GroupItemView? other)
        {
            return other != null &&
                   EqualityComparer<object>.Default.Equals(Key, other.Key);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key);
        }
    }
}