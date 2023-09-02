namespace RingKeyBuffer;

public interface IRingKeyBuffer<T>
{
    void Add(T item);
    bool TryGet(string key, out T item);
    bool Delete(string key);

}