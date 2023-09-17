namespace RingKeyBuffer.Tests;

[TestFixture]
public class ConcurrentRingKeyBufferTests
{
    [Test]
    public async Task AllValuesAreContainsInBufferWhenNotOverlapSizeTest()
    {
        const int BUF_SIZE = 10_000;
        IRingKeyBuffer<int, int> buf = new ConcurrentRingKeyBuffer<int, int>(size: BUF_SIZE, getKey: i => i, -1);

        var tasks = new Task[10_000];

        void RunTask(int i)
        {
            tasks[i] = Task.Run(() =>
            {
                buf.Add(i);
            });
        }

        for (var i = 0; i < 10_000; i++)
            RunTask(i);

        await Task.WhenAll(tasks);

        Assert.IsTrue(buf.TryGet(0, out var firstItem) && firstItem == 0);
        Assert.IsTrue(buf.TryGet(5_000, out var middleItem) && middleItem == 5_000);
        Assert.IsTrue(buf.TryGet(10_000-1, out var lastItem) && lastItem == 10_000 - 1);

        Assert.Pass();
    }

    [Test]
    public async Task OldestValuesDisappearWhenSizeWasOverlappedTest()
    {
        const int BUF_SIZE = 5_000;
        IRingKeyBuffer<int, int> buf = new ConcurrentRingKeyBuffer<int, int>(size: BUF_SIZE, getKey: i => i, -1);

        var tasks = new Task[10_000];
        void RunTask(int i)
        {
            tasks[i] = Task.Run(() =>
            {
                buf.Add(i);
            });
        }
        for (var i = 0; i < 10_000; i++)
           RunTask(i);

        await Task.WhenAll(tasks);

        Assert.IsFalse(buf.TryGet(0, out _));
        Assert.IsFalse(buf.TryGet(1_999, out _));
        Assert.IsTrue(buf.TryGet(7_000, out var firstItem) && firstItem == 7_000);
        Assert.IsTrue(buf.TryGet(10_000-1, out var lastItem) && lastItem == 10_000 - 1);

        Assert.Pass();
    }
}