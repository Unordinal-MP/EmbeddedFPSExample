using System.Collections.Generic;
using System.Linq;

public class Buffer<T>
{
    private struct Entry
    {
        public uint Id;
        public T Element;
    }

    private struct IdComparer : IComparer<Entry>
    {
        public int Compare(Entry a, Entry b)
        {
            return a.Id.CompareTo(b.Id);
        }
    }

#pragma warning disable S2743 // Static fields should not be used in generic types
    private static readonly IdComparer _comparer = new IdComparer();
#pragma warning restore S2743 // Static fields should not be used in generic types

    private readonly List<Entry> elements;
    private readonly int bufferSize;
    private readonly int correctionTolerance;

    private int counter;

    public Buffer(int bufferSize, int correctionTolerance)
    {
        this.bufferSize = bufferSize;
        this.correctionTolerance = correctionTolerance;
        elements = new List<Entry>();
    }

    public int Count => elements.Count;

    public void Add(T element, uint sequenceId)
    {
        var entry = new Entry()
        {
            Id = sequenceId,
            Element = element,
        };

        int index = elements.BinarySearch(entry, _comparer);
        if (index < 0)
        {
            index = ~index;
        }

        elements.Insert(index, entry);
    }

    public T[] Get()
    {
        int size = elements.Count - 1;

        if (size == bufferSize)
        {
            counter = 0;
        }

        if (size > bufferSize)
        {
            if (counter < 0)
            {
                counter = 0;
            }
            counter++;
            if (counter > correctionTolerance)
            {
                int amount = elements.Count - bufferSize;
                T[] temp = new T[amount];
                for (int i = 0; i < amount; i++)
                {
                    temp[i] = elements[i].Element;
                }
                elements.RemoveRange(0, amount);

                return temp;
            }
        }

        if (size < bufferSize)
        {
            if (counter > 0)
            {
                counter = 0;
            }
            counter--;
            if (-counter > correctionTolerance)
            {
                return new T[0];
            }
        }

        if (elements.Any())
        {
            var temp = new T[] { elements[0].Element };
            elements.RemoveAt(0);
            return temp;
        }
        return new T[0];
    }
}

