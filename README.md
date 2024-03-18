# RingKeyBuffer

The data structure combines a ring buffer (circular buffer) and a Dictionary (Map).

It can be used as an LRU cache.

There are 3 classes:

* RingKeyBuffer
* ThreadSafeRingKeyBuffer
* ThreadSafeNonBlockingRingKeyBuffer

ThreadSafeNonBlockingRingKeyBuffer uses `TryEnterWriteLock` and `TryEnterReadLock` methods and don't acquire lock when they return false.

They 2 generic parameters: TKey and TValue (key type and value type respectively). 
TKey is a string by default.

They implement a common interface:

```csharp
public interface IRingKeyBuffer<in TKey, TValue>
{
    void Add(TValue item);
    bool TryGet(TKey key, out TValue? item);
    bool Delete(TKey key);

}
```

Below few examples of usage.

Using of thread-unsafe buffer:

```csharp
const int BUF_SIZE = 1_000;
IRingKeyBuffer<int, int> buf = new RingKeyBuffer<int, int>(size: BUF_SIZE, getKey: i => i, garbageItem: -1);

for (var i = 0; i < 10_000; i++)
    buf.Add(i);

// buf.TryGet(0, out _) == false
// buf.TryGet(8_999, out _) == false
// buf.TryGet(9_000, out var a) == true && a == 9_000
// buf.TryGet(9_999, out var b) == true && b == 9_999

```

Creation thread-safe buffers:

```csharp
class Item
{
    public string Id;
}

var garbageItem = new Item()
{
    Id="__GARBAGE__"
};

const int BUF_SIZE = 1_000;
var buffer = new ThreadSafeRingKeyBuffer<Item>(BUF_SIZE, (item) => item.Id, garbageItem);
var nonBlockingBuffer = new ThreadSafeNonBlockingRingKeyBuffer<Item>(BUF_SIZE, (item) => item.Id, garbageItem);
```

Use thread-safe buffers:

```csharp
var buffer = new ThreadSafeRingKeyBuffer<Item>(BUF_SIZE, (item) => item.Id, garbageItem);

var tasks = new Task[10_000];
void RunTask(int i)
{
    tasks[i] = Task.Run(() =>
    {
        buffer.Add(new Item(){Id=i});
    });
}
for (var i = 0; i < 10_000; i++)
   RunTask(i);

await Task.WhenAll(tasks);

// buf.TryGet("0", out _) == most probably false 
// buf.TryGet("8999", out _) == ?
// buf.TryGet("9000", out _) == ?
// buf.TryGet("9999", out _) == most probably true
```
