public class CircularBuffer<T>
{
    private readonly T[] backing;
    private int next;

    public T this[int index]
    {
        get => backing[index];
        set => backing[index] = value;
    }

    public CircularBuffer(int size)
    {
        backing = new T[size];
    }

    public void Add(T item)
    {
        backing[next] = item;
        next = (next + 1) % backing.Length;
    }

    public bool Contains(T item)
    {
        T[] array = backing; //load field
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].Equals(item))
                return true;
        }

        return false;
    }
}
