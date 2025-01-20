using System;
using System.Collections;
using System.Collections.Generic;

namespace DarkUI.Collections
{
    // A standard event args approach, with info about added or removed items
    public class CollectionChangedEventArgs<T> : EventArgs
    {
        public IReadOnlyList<T> ChangedItems { get; }
        public bool ItemsAdded { get; }

        public CollectionChangedEventArgs(IReadOnlyList<T> changedItems, bool itemsAdded)
        {
            ChangedItems = changedItems;
            ItemsAdded = itemsAdded;
        }
    }

    public class ObservableList<T> : IList<T>, IDisposable
    {
        private readonly List<T> _internalList = new List<T>();
        private bool _disposed;

        public event EventHandler<CollectionChangedEventArgs<T>> CollectionChanged;

        // Dispose
        public void Dispose()
        {
            if (!_disposed)
            {
                // Null out event subscribers
                CollectionChanged = null;
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        // We do not need a finalizer here since no unmanaged resources
        // ~ObservableList() { ... }

        // Private helper to raise event
        private void OnCollectionChanged(IList<T> items, bool added)
        {
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs<T>(new List<T>(items), added));
        }

        #region IList<T> Members

        public int Count => _internalList.Count;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => _internalList[index];
            set
            {
                var oldItem = _internalList[index];
                _internalList[index] = value;
                // Here, you could raise an event for removal + addition, or a single "changed" event
                OnCollectionChanged(new List<T> { oldItem }, false);
                OnCollectionChanged(new List<T> { value }, true);
            }
        }

        public void Add(T item)
        {
            _internalList.Add(item);
            OnCollectionChanged(new List<T> { item }, true);
        }

        public void Clear()
        {
            if (_internalList.Count > 0)
            {
                var removedItems = new List<T>(_internalList);
                _internalList.Clear();
                OnCollectionChanged(removedItems, false);
            }
        }

        public bool Contains(T item) => _internalList.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _internalList.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _internalList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(T item)
        {
            if (_internalList.Remove(item))
            {
                OnCollectionChanged(new List<T> { item }, false);
                return true;
            }
            return false;
        }

        public int IndexOf(T item) => _internalList.IndexOf(item);

        public void Insert(int index, T item)
        {
            _internalList.Insert(index, item);
            OnCollectionChanged(new List<T> { item }, true);
        }

        public void RemoveAt(int index)
        {
            var oldItem = _internalList[index];
            _internalList.RemoveAt(index);
            OnCollectionChanged(new List<T> { oldItem }, false);
        }

        #endregion

        // Additional methods for AddRange, RemoveRange, etc.
        public void AddRange(IEnumerable<T> collection)
        {
            var list = new List<T>(collection);
            _internalList.AddRange(list);
            OnCollectionChanged(list, true);
        }

        public void RemoveRange(int index, int count)
        {
            var removedItems = _internalList.GetRange(index, count);
            _internalList.RemoveRange(index, count);
            OnCollectionChanged(removedItems, false);
        }
    }
}

