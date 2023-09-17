namespace RingKeyBuffer;

public class ConcurrentRingKeyBuffer<TValue> : ConcurrentRingKeyBuffer<string, TValue>
{
    public ConcurrentRingKeyBuffer(int size, RingKeyBuffer<string, TValue>.GetKeyDelegate getKey, TValue garbageItem)
        : base(size, getKey, garbageItem)
    {}
}

public class ConcurrentRingKeyBuffer<TKey, TValue> : IRingKeyBuffer<TKey, TValue>
    where TKey : notnull
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly RingKeyBuffer<TKey, TValue> _unsafeBuffer;

    public ConcurrentRingKeyBuffer(int size, RingKeyBuffer<TKey, TValue>.GetKeyDelegate getKey, TValue garbageItem)
        => _unsafeBuffer = new RingKeyBuffer<TKey, TValue>(size, getKey, garbageItem);

    public void Add(TValue item)
    {
        if (item == null)
            return;

        _lock.EnterWriteLock();

        try
        {
            _unsafeBuffer.Add(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool TryGet(TKey key, out TValue? item)
    {
        _lock.EnterReadLock();

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
        _lock.EnterWriteLock();

        try
        {
            return _unsafeBuffer.Delete(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}