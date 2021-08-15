using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Wildgoat.WPFUtility.Collections
{
    public static class Extensions
    {
        public static IBaseCollectionSource AsBaseCollectionSource<T>(this ObservableCollection<T> array)
            => new CollectionSourceWrapper<T>(array);
    }
}