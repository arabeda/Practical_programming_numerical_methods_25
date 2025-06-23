using System;

public class Vec
{
    private double[] data;

    public Vec(double[] values)
    {
        data = values;
    }

    public double this[int i]
    {
        get => data[i];
        set => data[i] = value;
    }

    public int Length => data.Length;

    public override string ToString()
    {
        return "[" + string.Join(", ", data) + "]";
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Vec v = new Vec(new double[] { 1, 2, 3 });
        Console.WriteLine("Vector: " + v);
    }
}
