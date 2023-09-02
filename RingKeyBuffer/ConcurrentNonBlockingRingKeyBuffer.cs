using System.Collections.Generic;
using System.Threading;

namespace RingKeyBuffer;

public class ConcurrentNonBlockingRingKeyBuffer<T>: IRingKeyBuffer<T>
    where T : new()
{
    private const int RwLockWriteTimeout = 50; // 50ms
    private const int RwLockReadTimeout = 10; // 10ms

    public delegate string GetKeyDelegate(T item);

    private readonly T[] _buffer = null;
    private readonly Dictionary<string, int> _keyIndex = null;
    private int _bufHead = 0;
    private readonly GetKeyDelegate _getKey = null;
    private readonly ReaderWriterLockSlim _lock = new();

    public ConcurrentNonBlockingRingKeyBuffer(int size, GetKeyDelegate getKey)
    {
        _keyIndex = new(capacity: size);
        _buffer = new T[size];
        _getKey = getKey;
    }

    public void Add(T item) => TryAdd(item);

    public bool TryAdd(T item)
    {
        if (item == null || string.IsNullOrEmpty(_getKey(item)))
            return false;

        if (!_lock.TryEnterWriteLock(RwLockWriteTimeout))
            return false;

        try
        {
            UnsafeAdd(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return true;
    }

    private void UnsafeAdd(T item)
    {
        var oldItem = _buffer[_bufHead];
        if (oldItem != null && _getKey(oldItem) != null)
            _keyIndex.Remove(_getKey(oldItem));

        _buffer[_bufHead] = item;
        _keyIndex[_getKey(item)] = _bufHead;
        if (_bufHead < _buffer.Length - 1)
            ++_bufHead;
        else
            _bufHead = 0;
    }

    public bool TryGet(string key, out T item)
    {
        if (string.IsNullOrEmpty(key))
        {
            item = new T { };
            return false;
        }

        if (!_lock.TryEnterReadLock(RwLockReadTimeout))
        {
            item = new T { };
            return false;
        }

        bool ans = false;
        try
        {
            ans = UnsafeTryGet(key, out item);
        }
        finally
        {
            _lock.ExitReadLock();
        }

        return ans;
    }

    private bool UnsafeTryGet(string key, out T item)
    {
        var ans = _keyIndex.ContainsKey(key);
        if (!ans)
        {
            item = new T { };
        }
        else
        {
            var i = _keyIndex[key];
            item = _buffer[i];
        }

        return ans;
    }

    public bool Delete(string key)
    {
        if (!_lock.TryEnterReadLock(RwLockReadTimeout))
            return false;

        return _keyIndex.Remove(key);
    }
}