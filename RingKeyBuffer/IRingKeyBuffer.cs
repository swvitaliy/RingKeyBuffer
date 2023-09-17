namespace RingKeyBuffer;

public interface IRingKeyBuffer<in TKey, TValue>
{
    void Add(TValue item);
    bool TryGet(TKey key, out TValue? item);
    bool Delete(TKey key);

}