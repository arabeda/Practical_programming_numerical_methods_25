using System;

public class Vec3D
{
    public double x, y, z;

    public Vec3D() { x = y = z = 0; }

    public Vec3D(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vec3D operator +(Vec3D a, Vec3D b)
        => new Vec3D(a.x + b.x, a.y + b.y, a.z + b.z);

    public static Vec3D operator -(Vec3D a, Vec3D b)
        => new Vec3D(a.x - b.x, a.y - b.y, a.z - b.z);

    public static Vec3D operator *(Vec3D a, double scalar)
        => new Vec3D(a.x * scalar, a.y * scalar, a.z * scalar);

    public static Vec3D operator *(double scalar, Vec3D a)
        => a * scalar;

    public static Vec3D operator /(Vec3D a, double scalar)
    {
        if (scalar == 0)
            throw new DivideByZeroException("Error, zero division.");
        return new Vec3D(a.x / scalar, a.y / scalar, a.z / scalar);
    }

    public static double Dot(Vec3D a, Vec3D b)
        => a.x * b.x + a.y * b.y + a.z * b.z;

    public static Vec3D Cross(Vec3D a, Vec3D b)
        => new Vec3D(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );

    public override string ToString()
        => $"({x}, {y}, {z})";
}
