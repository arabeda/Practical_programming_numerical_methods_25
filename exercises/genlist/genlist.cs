using System;

public class GenList<T>
{
    private T[] data;
    private int size;
    private int capacity;

    public int Size => size; // Property to get the number of elements

    // Indexer with range check
    public T this[int i]
    {
        get
        {
            if (i < 0 || i >= size)
                throw new IndexOutOfRangeException("Index out of range");
            return data[i];
        }
    }

    public GenList()
    {
        capacity = 8; // Initial capacity
        data = new T[capacity];
        size = 0;
    }

    public void Add(T item)
    {
        if (size == capacity)
        {
            capacity *= 2; // Double the capacity
            T[] newdata = new T[capacity];
            Array.Copy(data, newdata, size);
            data = newdata;
        }
        data[size] = item;
        size++;
    }

    public void Remove(int i)
    {
        if (i < 0 || i >= size)
            throw new IndexOutOfRangeException("Index out of range");

        for (int j = i; j < size - 1; j++)
        {
            data[j] = data[j + 1];
        }
        size--; // Reduce the size by one
        data[size] = default(T); // Optional: clear removed spot (important for reference types)
    }
}

// Linked List Implementation
public class Node<T>
{
    public T item;
    public Node<T> next;
    public Node(T item) { this.item = item; }
}

public class MyList<T>
{
    public Node<T> first = null, current = null;

    public void Add(T item)
    {
        if (first == null)
        {
            first = new Node<T>(item);
            current = first;
        }
        else
        {
            current.next = new Node<T>(item);
            current = current.next;
        }
    }

    public void Start() { current = first; }
    public void Next() { if (current != null) current = current.next; }
}

class Program
{
    public static void Main()
    {
        var list = new GenList<double[]>();
        char[] delimiters = { ' ', '\t' };
        var options = StringSplitOptions.RemoveEmptyEntries;

        string line;
        // Read lines until empty line or end-of-input
        while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
        {
            var words = line.Split(delimiters, options);
            int n = words.Length;
            var numbers = new double[n];
            bool ok = true;

            for (int i = 0; i < n; i++)
            {
                if (!double.TryParse(words[i], out numbers[i]))
                {
                    Console.WriteLine($"Nieprawidłowa liczba: '{words[i]}' – linia zostanie pominięta");
                    ok = false;
                    break;
                }
            }
            if (ok)
                list.Add(numbers);
        }

        // Print numbers in exponential format
        for (int i = 0; i < list.Size; i++)
        {
            var numbers = list[i];
            foreach (var number in numbers)
                Console.Write($"{number:0.00e+00;-0.00e+00} ");
            Console.WriteLine();
        }

        // Demonstrate linked list usage
        var linkedList = new MyList<int>();
        linkedList.Add(1);
        linkedList.Add(2);
        linkedList.Add(3);

        for (linkedList.Start(); linkedList.current != null; linkedList.Next())
        {
            Console.WriteLine(linkedList.current.item);
        }
    }
}
