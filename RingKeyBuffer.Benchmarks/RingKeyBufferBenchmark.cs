using BenchmarkDotNet.Attributes;
using System.Collections.Concurrent;

namespace RingKeyBuffer.Benchmarks;

internal class Item
{
    public string Id { get; set; }
}

[MemoryDiagnoser]
public class BenchmarkDictVsRB
{
    private Dictionary<string, Item> _dict = null;
    private RingKeyBuffer<Item> _buffer = null;

    [Params(1000, 10_000)]
    public int Size { get; set; }

    [Params(100_000, 1_000_000)]
    public int N { get; set; }

    [GlobalSetup]
    public void Setup()
    {
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _dict = new(Size);
        _buffer = new(Size, (item) => item.Id);
    }

    [Benchmark(Baseline = true)]
    public void DictAdd()
    {
        for (int i = 0; i < N; ++i)
        {
            var s = i.ToString();
            _dict.Add(s, new Item() { Id = s });
        }
    }

    [Benchmark]
    public void RKBAdd()
    {
        for (int i = 0; i < N; ++i)
        {
            var s = i.ToString();
            _buffer.Add(new Item() { Id = s });
        }
    }
}

[MemoryDiagnoser]
public class BenchmarkConcurrent
{
    private ConcurrentDictionary<string, Item> _dict = null;
    private ConcurrentRingKeyBuffer<Item> _buffer = null;
    private ConcurrentNonBlockingRingKeyBuffer<Item> _nbBuffer = null;

    [Params(1000, 10_000)]
    public int Size { get; set; }

    [Params(10000, 100_000)]
    public int N { get; set; }

    [GlobalSetup]
    public void Setup()
    {
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _dict = new();
        _buffer = new(Size, (item) => item.Id);
        _nbBuffer = new(Size, (item) => item.Id);
    }

    [Benchmark(Baseline = true)]
    public void DictAdd()
    {
        for (int i = 0; i < N; ++i)
        {
            var s = i.ToString();
            _dict.TryAdd(s, new Item() { Id = s });
        }
    }

    [Benchmark]
    public void RKBAdd()
    {
        for (int i = 0; i < N; ++i)
        {
            var s = i.ToString();
            _buffer.Add(new Item() { Id = s });
        }
    }

    [Benchmark]
    public void RKNBBAdd()
    {
        for (int i = 0; i < N; ++i)
        {
            var s = i.ToString();
            _nbBuffer.TryAdd(new Item() { Id = s });
        }
    }
}
