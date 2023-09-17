namespace RingKeyBuffer;

public class ConcurrentNonBlockingRingKeyBuffer<TValue> : ConcurrentNonBlockingRingKeyBuffer<string, TValue>
{
    public ConcurrentNonBlockingRingKeyBuffer(int size, RingKeyBuffer<string, TValue>.GetKeyDelegate getKey, TValue garbageItem)
        : base(size, getKey, garbageItem)
    {}
}


public class ConcurrentNonBlockingRingKeyBuffer<TKey, TValue>: IRingKeyBuffer<TKey, TValue>
    where TKey : notnull
{
    private const int RwLockWriteTimeout = 50; // 50ms
    private const int RwLockReadTimeout = 10; // 10ms

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly RingKeyBuffer<TKey, TValue> _unsafeBuffer;

    public ConcurrentNonBlockingRingKeyBuffer(int size, RingKeyBuffer<TKey, TValue>.GetKeyDelegate getKey, TValue garbageItem)
        => _unsafeBuffer = new RingKeyBuffer<TKey, TValue>(size, getKey, garbageItem);

    public void Add(TValue item) => TryAdd(item);

    public bool TryAdd(TValue item)
    {
        if (!_lock.TryEnterWriteLock(RwLockWriteTimeout))
            return false;

        try
        {
            _unsafeBuffer.Add(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return true;
    }

    public bool TryGet(TKey key, out TValue? item)
    {
        if (!_lock.TryEnterReadLock(RwLockReadTimeout))
        {
            item = default;
            return false;
        }

        bool ans = false;
        try
        {
            ans = _unsafeBuffer.TryGet(key, out item);
        }
        finally
        {
            _lock.ExitReadLock();
        }

        return ans;
    }

    public bool Delete(TKey key)
    {
        if (!_lock.TryEnterReadLock(RwLockReadTimeout))
            return false;

        return _unsafeBuffer.Delete(key);
    }
}