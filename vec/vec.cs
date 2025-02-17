using System;

public class Vec
{
    public double x, y, z; // 

    public Vec() { x = y = z = 0; }
    
    
    public Vec(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Additive operation
    public static Vec operator +(Vec a, Vec b)
    {
        return new Vec(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    // Subtraction operation
    public static Vec operator -(Vec a, Vec b)
    {
        return new Vec(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    // Scalar multiplication
    public static Vec operator *(Vec a, double scalar)
    {
        return new Vec(a.x * scalar, a.y * scalar, a.z * scalar);
    }
    
    public static Vec operator *(double scalar, Vec a)
    {
        return a * scalar;
    }

    // Scalar division 
    public static Vec operator /(Vec a, double scalar)
    {
        if (scalar == 0)
            throw new DivideByZeroException("Nie można dzielić przez zero.");
        return new Vec(a.x / scalar, a.y / scalar, a.z / scalar);
    }

    // Dot product
    public static double Dot(Vec a, Vec b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    // Vector product
    public static Vec Cross(Vec a, Vec b)
    {
        return new Vec(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );
    }

    // ToString() debugging 
    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}

// Example of how to use
public class Program
{
    public static void Main()
    {
        Vec v1 = new Vec(1, 2, 3);
        Vec v2 = new Vec(4, 5, 6);

        Console.WriteLine("v1: " + v1);
        Console.WriteLine("v2: " + v2);
        Console.WriteLine("v1 + v2: " + (v1 + v2));
        Console.WriteLine("v1 - v2: " + (v1 - v2));
        Console.WriteLine("v1 * 2: " + (v1 * 2));
        Console.WriteLine("Dot(v1, v2): " + Vec.Dot(v1, v2));
        Console.WriteLine("Cross(v1, v2): " + Vec.Cross(v1, v2));
    }
}
