using System;
using System.Collections.Generic;

public class genlist<T>
{
    private List<T> internalList;

    public genlist()
    {
        internalList = new List<T>();
    }

    public void add(T item)
    {
        internalList.Add(item);
    }

    public T this[int index]
    {
        get => internalList[index];
        set => internalList[index] = value;
    }

    public int size()
    {
        return internalList.Count;
    }

    public void clear()
    {
        internalList.Clear();
    }

    public bool remove(T item)
    {
        return internalList.Remove(item);
    }
}
