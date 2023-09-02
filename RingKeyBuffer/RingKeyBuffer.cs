using System.Collections.Generic;
using System.Threading;

namespace RingKeyBuffer;

public class RingKeyBuffer<T> : IRingKeyBuffer<T>
    where T : new()
{
    public delegate string GetKeyDelegate(T item);

    private readonly T[] _buffer = null;
    private readonly Dictionary<string, int> _keyIndex = null;
    private int _bufHead = 0;
    private readonly GetKeyDelegate _getKey = null;

    public RingKeyBuffer(int size, GetKeyDelegate getKey)
    {
        _keyIndex = new(capacity: size);
        _buffer = new T[size];
        _getKey = getKey;
    }

    public void Add(T item)
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
        return _keyIndex.Remove(key);
    }
}