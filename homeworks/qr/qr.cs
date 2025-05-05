using System;

public class Matrix
{
    public int Rows, Cols;
    public double[,] Data;

    public Matrix(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        Data = new double[rows, cols];
    }

    public double this[int i, int j]
    {
        get => Data[i, j];
        set => Data[i, j] = value;
    }

    public static Matrix Identity(int size)
    {
        Matrix I = new Matrix(size, size);
        for (int i = 0; i < size; i++) I[i, i] = 1.0;
        return I;
    }

    public Matrix Copy()
    {
        Matrix copy = new Matrix(Rows, Cols);
        Array.Copy(Data, copy.Data, Data.Length);
        return copy;
    }
}

public class Vector
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

public class QR
{
    public Matrix Q, R;

    public QR(Matrix A)
    {
        int m = A.Rows, n = A.Cols;
        Q = A.Copy();
        R = new Matrix(n, n);

        for (int i = 0; i < n; i++)
        {
            double norm = 0;
            for (int j = 0; j < m; j++)
                norm += Q[j, i] * Q[j, i];
            norm = Math.Sqrt(norm);
            R[i, i] = norm;
            for (int j = 0; j < m; j++)
                Q[j, i] /= norm;

            for (int j = i + 1; j < n; j++)
            {
                double dot = 0;
                for (int k = 0; k < m; k++)
                    dot += Q[k, i] * Q[k, j];
                R[i, j] = dot;
                for (int k = 0; k < m; k++)
                    Q[k, j] -= dot * Q[k, i];
            }
        }
    }

    public Vector Solve(Vector b)
    {
        int n = R.Cols;
        Vector x = new Vector(n);
        Vector Qtb = new Vector(b.Size);

        for (int i = 0; i < b.Size; i++)
        {
            double sum = 0;
            for (int j = 0; j < b.Size; j++)
                sum += Q[j, i] * b[j];
            Qtb[i] = sum;
        }

        for (int i = n - 1; i >= 0; i--)
        {
            double sum = Qtb[i];
            for (int j = i + 1; j < n; j++)
                sum -= R[i, j] * x[j];
            x[i] = sum / R[i, i];
        }
        return x;
    }

    public double Determinant()
    {
        double det = 1.0;
        for (int i = 0; i < R.Cols; i++)
            det *= R[i, i];
        return det;
    }

    public Matrix Inverse()
    {
        int n = R.Cols;
        Matrix inv = new Matrix(n, n);
        Matrix I = Matrix.Identity(n);

        for (int i = 0; i < n; i++)
        {
            Vector e = new Vector(n);
            for (int j = 0; j < n; j++)
                e[j] = I[j, i];
            Vector x = Solve(e);
            for (int j = 0; j < n; j++)
                inv[j, i] = x[j];
        }
        return inv;
    }
}

public class Program
{
    public static void Main()
    {
        Random rnd = new Random();
        int n = 4;
        Matrix A = new Matrix(n, n);
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                A[i, j] = rnd.NextDouble();

        QR qr = new QR(A);

        Vector b = new Vector(n);
        for (int i = 0; i < n; i++)
            b[i] = rnd.NextDouble();

        Vector x = qr.Solve(b);
        double det = qr.Determinant();
        Matrix inv = qr.Inverse();

        Console.WriteLine("Solution:");
        for (int i = 0; i < n; i++)
            Console.WriteLine(x[i]);
        
        Console.WriteLine($"Determinant: {det}");
        Console.WriteLine("Inverse Matrix:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                Console.Write($"{inv[i, j]:F3} ");
            Console.WriteLine();
        }
    }
}
