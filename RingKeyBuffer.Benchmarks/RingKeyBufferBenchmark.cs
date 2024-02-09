using BenchmarkDotNet.Attributes;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RingKeyBuffer.Benchmarks;

internal class Item
{
    public string Id { get; set; }
}

[MemoryDiagnoser]
public class BenchmarkThreadUnsafeBuffer
{
    private Dictionary<string, Item> _dict = null;
    private RingKeyBuffer<Item> _buffer = null;

    public int Size { get; set; } = 100_000;

    [Params(10_000, 100_000)] // the last value for memleak test
    public int N { get; set; }

    [GlobalSetup]
    public void Setup()
    {
    }

    [IterationSetup]
    public void IterationSetup()
    {
        var garbageItem = new Item()
        {
            Id="__GARBAGE__"
        };

        _dict = new(Size);
        _buffer = new(Size, (item) => item.Id, garbageItem);
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
    public void RingKeyBufferAdd()
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
    private ThreadSafeRingKeyBuffer<Item> _buffer = null;
    private ThreadSafeNonBlockingRingKeyBuffer<Item> _nbBuffer = null;

    public int Size { get; set; } = 100_000;

    [Params(10000, 100_000)]
    public int N { get; set; }

    [GlobalSetup]
    public void Setup()
    {
    }

    [IterationSetup]
    public void IterationSetup()
    {
        var garbageItem = new Item()
        {
            Id="__GARBAGE__"
        };

        _dict = new();
        _buffer = new(Size, (item) => item.Id, garbageItem);
        _nbBuffer = new(Size, (item) => item.Id, garbageItem);
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
    public void RingKeyBufferAdd()
    {
        for (int i = 0; i < N; ++i)
        {
            var s = i.ToString();
            _buffer.Add(new Item() { Id = s });
        }
    }

    [Benchmark]
    public void NonBlockingBufferAdd()
    {
        for (int i = 0; i < N; ++i)
        {
            var s = i.ToString();
            _nbBuffer.TryAdd(new Item() { Id = s });
        }
    }
}
