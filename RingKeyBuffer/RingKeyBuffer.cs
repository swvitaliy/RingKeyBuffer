namespace RingKeyBuffer;

public class RingKeyBuffer<TValue> : RingKeyBuffer<string, TValue>
{
    public RingKeyBuffer(int size, GetKeyDelegate getKey, TValue garbageItem)
        : base(size, getKey, garbageItem)
    {}
}

/**
 * Thread-unsafe version.
 */
public class RingKeyBuffer<TKey, TValue> : IRingKeyBuffer<TKey, TValue>
    where TKey : notnull
{
    public delegate TKey GetKeyDelegate(TValue item);

    private readonly TValue[] _buffer;
    private readonly Dictionary<TKey, uint> _keyOrder;
    private readonly GetKeyDelegate _getKey;
    private uint _globalCounter = 0;
    private TValue GarbageItem { get; }

    public RingKeyBuffer(int size, GetKeyDelegate getKey, TValue garbageItem)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        _keyOrder = new Dictionary<TKey, uint>(capacity: size);
        _getKey = getKey;
        _buffer = new TValue[size];
        GarbageItem = garbageItem;
        for (var i = 0; i < _buffer.Length; i++)
            _buffer[i] = GarbageItem;
    }

    private uint BufHead => _globalCounter % (uint)_buffer.Length;

    private void ClearOldItem()
    {
        var oldItem = _buffer[BufHead];
        if (oldItem == null)
            return;

        var oldKey = _getKey(oldItem);
        Delete(oldKey);
    }

    public void Add(TValue item)
    {
        if (!Equals(item, GarbageItem))
            ClearOldItem();

        var key = _getKey(item);

        _buffer[BufHead] = item;

        if (_keyOrder.TryGetValue(key, out var existIndex))
            _buffer[existIndex % _buffer.Length] = GarbageItem;

        _keyOrder[key] = _globalCounter;
        _globalCounter++;
    }

    public bool TryGet(TKey key, out TValue? item)
    {
        if (!_keyOrder.TryGetValue(key, out var globalIndex))
        {
            item = default;
            return false;
        }

        var bufIndex = globalIndex % _buffer.Length;
        item = _buffer[bufIndex];
        return true;
    }

    public bool Delete(TKey key)
    {
        if (!_keyOrder.TryGetValue(key, out _))
            return false;

        _keyOrder.Remove(key);
        return true;
    }
}