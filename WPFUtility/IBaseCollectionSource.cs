using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Wildgoat.WPFUtility.Collections
{
    /// <summary>
    /// Base interface compatible with the library
    /// </summary>
    public interface IBaseCollectionSource : IEnumerable, INotifyCollectionChanged
    {
    }
}