using System;

public class Vec
{
    private double[] data;

    public Vec(double[] values)
    {
        data = values;
    }

    public Vec(double a, double b)
    {
        data = new double[] { a, b };
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

    // Operator * (skalar razy wektor)
    public static Vec operator *(Vec v, double scalar)
    {
        double[] result = new double[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            result[i] = v[i] * scalar;
        }
        return new Vec(result);
    }

    // Operator - (odejmowanie wektorów)
    public static Vec operator -(Vec a, Vec b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same length");

        double[] result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] - b[i];
        }
        return new Vec(result);
    }

    // Operator + (dodawanie wektorów)
    public static Vec operator +(Vec a, Vec b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same length");

        double[] result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] + b[i];
        }
        return new Vec(result);
    }

    // Kopiowanie wektora
    public Vec Copy()
    {
        double[] newData = new double[data.Length];
        Array.Copy(data, newData, data.Length);
        return new Vec(newData);
    }

    // Norma (długość) wektora
    public double Norm()
    {
        double sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += data[i] * data[i];
        }
        return Math.Sqrt(sum);
    }
}
