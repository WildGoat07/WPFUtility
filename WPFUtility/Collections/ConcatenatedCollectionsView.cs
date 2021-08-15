using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wildgoat.WPFUtility.Collections
{
    public class ConcatenatedCollectionsView : IBaseCollectionSource, IEnumerable<object?>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private IBaseCollectionSource[] sources;

        public ConcatenatedCollectionsView(params object[] sources)
        {
            this.sources = sources.Select(source => CollectionSourceWrapper.GetCollection(source)).ToArray();
            InitSources();
        }

        ~ConcatenatedCollectionsView()
        {
            UnlinkSources();
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public object[] Sources
        {
            get => sources;
            set
            {
                UnlinkSources();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, sources.Select(source => source.Cast<object?>()).SelectMany(source => source).ToList(), 0));
                sources = value.Select(source => CollectionSourceWrapper.GetCollection(source)).ToArray();
                InitSources();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sources.Select(source => source.Cast<object?>()).SelectMany(source => source).ToList(), 0));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sources)));
            }
        }

        private int[]? Counts { get; set; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<object?> GetEnumerator() => new Enumerator(this);

        protected void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int offset = 0;
            if (Sources != null && Counts != null)
                for (int i = 0; i < Sources.Length; ++i)
                    if (sender == Sources[i])
                    {
                        Counts[i] += e.NewItems?.Count ?? 0 - e.OldItems?.Count ?? 0;
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, e.NewStartingIndex + offset));
                                break;

                            case NotifyCollectionChangedAction.Remove:
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, e.OldStartingIndex + offset));
                                break;

                            case NotifyCollectionChangedAction.Replace:
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, e.NewStartingIndex + offset));
                                break;

                            case NotifyCollectionChangedAction.Move:
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems, e.NewStartingIndex + offset, e.OldStartingIndex + offset));
                                break;
                        }
                    }
                    else
                        offset += Counts[i];
        }

        private int Count(IEnumerable list)
        {
            int count = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
                ++count;
            return count;
        }

        private void InitSources()
        {
            Counts = sources.Select(source => Count(source)).ToArray();
            for (int i = 0; i < sources.Length; ++i)
                sources[i].CollectionChanged += OnSourceCollectionChanged;
        }

        private void UnlinkSources()
        {
            foreach (var source in sources)
                source.CollectionChanged -= OnSourceCollectionChanged;
        }

        private class Enumerator : IEnumerator<object?>, IEnumerator
        {
            public Enumerator(ConcatenatedCollectionsView list)
            {
                Source = list;
            }

            public object? Current => UnderLyingEnumerator?.Current;
            private IEnumerator<IBaseCollectionSource>? EnumerableEnumerator { get; set; }
            private ConcatenatedCollectionsView Source { get; set; }
            private IEnumerator? UnderLyingEnumerator { get; set; }

            public void Dispose()
            {
                EnumerableEnumerator?.Dispose();
            }

            public bool MoveNext()
            {
                if (EnumerableEnumerator == null)
                {
                    EnumerableEnumerator = Source.Sources.Cast<IBaseCollectionSource>().GetEnumerator();
                    UnderLyingEnumerator = null;
                }
                if (EnumerableEnumerator != null)
                {
                    while (UnderLyingEnumerator == null || !UnderLyingEnumerator.MoveNext())
                        if (EnumerableEnumerator.MoveNext())
                            UnderLyingEnumerator = EnumerableEnumerator.Current.GetEnumerator();
                        else
                            return false;
                    return true;
                }
                else
                    return false;
            }

            public void Reset()
            {
                EnumerableEnumerator = null;
            }
        }
    }
}