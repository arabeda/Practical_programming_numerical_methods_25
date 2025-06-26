// to run this file use 
// dotnet run < data.txt
// data.txt file contains input data 


using System;

public class genlist<T>
{
    private T[] data;
    private int size;
    private int capacity;

    public int Size => size; // Property to get the number of elements
    public T this[int i] => data[i]; // Indexer to access elements

    public genlist()
    {
        capacity = 8; // Initial capacity
        data = new T[capacity];
        size = 0;
    }

    public void add(T item)
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

    public void remove(int i)
    {
        if (i < 0 || i >= size)
            throw new IndexOutOfRangeException("Index out of range");

        for (int j = i; j < size - 1; j++)
        {
            data[j] = data[j + 1];
        }
        size--; // Reduce the size by one
    }
}

// Linked List Implementation
public class node<T>
{
    public T item;
    public node<T> next;
    public node(T item) { this.item = item; }
}

public class list<T>
{
    public node<T> first = null, current = null;

    public void add(T item)
    {
        if (first == null)
        {
            first = new node<T>(item);
            current = first;
        }
        else
        {
            current.next = new node<T>(item);
            current = current.next;
        }
    }

    public void start() { current = first; }
    public void next() { if (current != null) current = current.next; }
}

class Program
{
    public static void Main()
    {
        var list = new genlist<double[]>();
        char[] delimiters = { ' ', '\t' };
        var options = StringSplitOptions.RemoveEmptyEntries;

        string line;
        while ((line = Console.ReadLine()) != null)
        {
            var words = line.Split(delimiters, options);
            int n = words.Length;
            var numbers = new double[n];

            for (int i = 0; i < n; i++)
                numbers[i] = double.Parse(words[i]);

            list.add(numbers);
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
        var linkedList = new list<int>();
        linkedList.add(1);
        linkedList.add(2);
        linkedList.add(3);

        for (linkedList.start(); linkedList.current != null; linkedList.next())
        {
            Console.WriteLine(linkedList.current.item);
        }
    }
}
