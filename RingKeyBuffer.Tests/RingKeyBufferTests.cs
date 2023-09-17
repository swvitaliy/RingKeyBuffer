namespace RingKeyBuffer.Tests;

[TestFixture]
public class RingKeyBufferTests
{
    [Test]
    public void AllValuesAreContainsInBufferWhenNotOverlapSizeTest()
    {
        const int BUF_SIZE = 10_000;
        IRingKeyBuffer<int, int> buf = new RingKeyBuffer<int, int>(size: BUF_SIZE, getKey: i => i, -1);

        for (var i = 0; i < 10_000; i++)
            buf.Add(i);

        Assert.IsTrue(buf.TryGet(0, out var firstItem) && firstItem == 0);
        Assert.IsTrue(buf.TryGet(5_000, out var middleItem) && middleItem == 5_000);
        Assert.IsTrue(buf.TryGet(10_000-1, out var lastItem) && lastItem == 10_000 - 1);

        Assert.Pass();
    }

    [Test]
    public void OldestValuesDisappearWhenSizeWasOverlappedTest()
    {
        const int BUF_SIZE = 5_000;
        IRingKeyBuffer<int, int> buf = new RingKeyBuffer<int, int>(size: BUF_SIZE, getKey: i => i, -1);

        for (var i = 0; i < 10_000; i++)
                buf.Add(i);

        Assert.IsFalse(buf.TryGet(0, out _));
        Assert.IsFalse(buf.TryGet(4_999, out _));
        Assert.IsTrue(buf.TryGet(5_000, out var firstItem) && firstItem == 5_000);
        Assert.IsTrue(buf.TryGet(10_000-1, out var lastItem) && lastItem == 10_000 - 1);

        Assert.Pass();
    }

    [Test]
    public void GenerationTest()
    {
        const int BUF_SIZE = 5;
        IRingKeyBuffer<int, int> buf = new RingKeyBuffer<int, int>(size: BUF_SIZE, getKey: i => i, -1);

        // fill whole buffer
        buf.Add(7);
        buf.Add(1);
        buf.Add(1);
        buf.Add(7);
        buf.Add(1);

        // next generation
        buf.Add(1); // this can delete key 7 to 4th buffer item

        Assert.IsTrue(buf.TryGet(7, out var firstItem) && firstItem == 7);
    }

    [Test]
    public void BigKeyTest()
    {
        const int BUF_SIZE = 5;
        IRingKeyBuffer<int, int> buf = new RingKeyBuffer<int, int>(size: BUF_SIZE, getKey: i => i, -1);

        buf.Add(99765);

        Assert.IsTrue(buf.TryGet(99765, out var firstItem) && firstItem == 99765);
    }
}