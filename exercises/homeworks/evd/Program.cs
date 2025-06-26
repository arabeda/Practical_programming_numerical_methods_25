using System;
using System.Linq;

class Matrix
{
    public int Size;
    public double[,] Data;

    public Matrix(int size)
    {
        Size = size;
        Data = new double[size, size];
    }

    public double this[int i, int j]
    {
        get => Data[i, j];
        set => Data[i, j] = value;
    }

    public Matrix Copy()
    {
        Matrix copy = new Matrix(Size);
        Array.Copy(Data, copy.Data, Data.Length);
        return copy;
    }

    public static Matrix Identity(int size)
    {
        Matrix I = new Matrix(size);
        for (int i = 0; i < size; i++)
            I[i, i] = 1;
        return I;
    }
}

class Vector
{
    public int Size;
    public double[] Data;

    public Vector(int size)
    {
        Size = size;
        Data = new double[size];
    }

    public double this[int i]
    {
        get => Data[i];
        set => Data[i] = value;
    }
}

class Jacobi
{
    public static void TimesJ(Matrix A, int p, int q, double theta)
    {
        double c = Math.Cos(theta), s = Math.Sin(theta);
        for (int i = 0; i < A.Size; i++)
        {
            double aip = A[i, p], aiq = A[i, q];
            A[i, p] = c * aip - s * aiq;
            A[i, q] = s * aip + c * aiq;
        }
    }

    public static void Jtimes(Matrix A, int p, int q, double theta)
    {
        double c = Math.Cos(theta), s = Math.Sin(theta);
        for (int j = 0; j < A.Size; j++)
        {
            double apj = A[p, j], aqj = A[q, j];
            A[p, j] = c * apj + s * aqj;
            A[q, j] = -s * apj + c * aqj;
        }
    }

    public static (Vector, Matrix) Cyclic(Matrix M)
    {
        int n = M.Size;
        Matrix A = M.Copy();
        Matrix V = Matrix.Identity(n);
        Vector w = new Vector(n);
        
        bool changed;
        do
        {
            changed = false;
            for (int p = 0; p < n - 1; p++)
            {
                for (int q = p + 1; q < n; q++)
                {
                    double apq = A[p, q], app = A[p, p], aqq = A[q, q];
                    double theta = 0.5 * Math.Atan2(2 * apq, aqq - app);
                    double c = Math.Cos(theta), s = Math.Sin(theta);
                    double new_app = c * c * app - 2 * s * c * apq + s * s * aqq;
                    double new_aqq = s * s * app + 2 * s * c * apq + c * c * aqq;
                    
                    if (new_app != app || new_aqq != aqq)
                    {
                        changed = true;
                        TimesJ(A, p, q, theta);
                        Jtimes(A, p, q, -theta);
                        TimesJ(V, p, q, theta);
                    }
                }
            }
        } while (changed);
        
        for (int i = 0; i < n; i++)
            w[i] = A[i, i];

        return (w, V);
    }
}

class HydrogenSolver
{
    public static void Solve(double rmax, double dr)
    {
        int npoints = (int)(rmax / dr) - 1;
        Vector r = new Vector(npoints);
        Matrix H = new Matrix(npoints);

        for (int i = 0; i < npoints; i++)
            r[i] = dr * (i + 1);

        for (int i = 0; i < npoints - 1; i++)
        {
            H[i, i] = -2 * (-0.5 / dr / dr);
            H[i, i + 1] = 1 * (-0.5 / dr / dr);
            H[i + 1, i] = 1 * (-0.5 / dr / dr);
        }
        H[npoints - 1, npoints - 1] = -2 * (-0.5 / dr / dr);

        for (int i = 0; i < npoints; i++)
            H[i, i] += -1 / r[i];

        var (eigenvalues, eigenvectors) = Jacobi.Cyclic(H);

        Console.WriteLine("Eigenvalues:");
        for (int i = 0; i < 5; i++)
            Console.WriteLine($"E_{i} = {eigenvalues[i]}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        double rmax = 10.0;
        double dr = 0.3;
        HydrogenSolver.Solve(rmax, dr);
    }
}
