// Unified Newton + Hydrogen Shooting + Tests
using System;
using System.IO;
using System.Collections.Generic;
using static System.Math;

// --- Begin Vec ---
public class Vec
{
    public static Vec operator -(Vec v) => new(VecMap(v, x => -x));
    private double[] data;

    public Vec(double[] values) { data = values; }
    public Vec(double a, double b) { data = new[] { a, b }; }
    public double this[int i] { get => data[i]; set => data[i] = value; }
    public int Length => data.Length;
    public override string ToString() => "[" + string.Join(", ", data) + "]";
    public static Vec operator *(Vec v, double s) => new(VecMap(v, x => x * s));
    public static Vec operator -(Vec a, Vec b) => new(VecZip(a, b, (x, y) => x - y));
    public static Vec operator +(Vec a, Vec b) => new(VecZip(a, b, (x, y) => x + y));
    public Vec Copy() => new((double[])data.Clone());
    public double Norm() => Sqrt(VecMap(data, x => x * x).Sum());
    private static double[] VecMap(Vec v, Func<double, double> f) { var r = new double[v.Length]; for (int i = 0; i < v.Length; i++) r[i] = f(v[i]); return r; }
    private static double[] VecZip(Vec a, Vec b, Func<double, double, double> f) { var r = new double[a.Length]; for (int i = 0; i < a.Length; i++) r[i] = f(a[i], b[i]); return r; }
    public static implicit operator double[](Vec v) => v.data;
    public static implicit operator Vec(double[] arr) => new(arr);
}

// --- Begin Matrix ---
public class Matrix
{
    public int Rows, Cols;
    public double[,] Data;
    public Matrix(int r, int c) { Rows = r; Cols = c; Data = new double[r, c]; }
    public double this[int i, int j] { get => Data[i, j]; set => Data[i, j] = value; }
    public Matrix Copy() { var m = new Matrix(Rows, Cols); Array.Copy(Data, m.Data, Data.Length); return m; }
    public static Matrix Identity(int n) { var I = new Matrix(n, n); for (int i = 0; i < n; i++) I[i, i] = 1; return I; }
}

// --- Begin QR ---
public class QR
{
    public Matrix Q, R;
    public QR(Matrix A)
    {
        int m = A.Rows, n = A.Cols;
        Q = A.Copy(); R = new Matrix(n, n);
        for (int i = 0; i < n; i++)
        {
            double norm = 0; for (int j = 0; j < m; j++) norm += Q[j, i] * Q[j, i];
            norm = Sqrt(norm); R[i, i] = norm;
            for (int j = 0; j < m; j++) Q[j, i] /= norm;
            for (int j = i + 1; j < n; j++)
            {
                double dot = 0; for (int k = 0; k < m; k++) dot += Q[k, i] * Q[k, j];
                R[i, j] = dot;
                for (int k = 0; k < m; k++) Q[k, j] -= dot * Q[k, i];
            }
        }
    }
    public Vec Solve(Vec b)
    {
        int n = R.Cols; Vec x = new double[n];
        Vec Qtb = new double[n]; for (int i = 0; i < b.Length; i++) { double sum = 0; for (int j = 0; j < b.Length; j++) sum += Q[j, i] * b[j]; Qtb[i] = sum; }
        for (int i = n - 1; i >= 0; i--) { double sum = Qtb[i]; for (int j = i + 1; j < n; j++) sum -= R[i, j] * x[j]; x[i] = sum / R[i, i]; }
        return x;
    }
}

// --- Begin genlist ---
public class genlist<T>
{
    private List<T> data = new();
    public void add(T item) => data.Add(item);
    public T this[int i] { get => data[i]; set => data[i] = value; }
    public int size() => data.Count;
    public void clear() => data.Clear();
    public bool remove(T item) => data.Remove(item);
}

// --- Begin ODE Solver ---
public class OdeSolver
{
    public static (Vec, Vec) RKStep(Func<double, Vec, Vec> f, double x, Vec y, double h)
    {
        var k0 = f(x, y);
        var k1 = f(x + h / 2, y + k0 * (h / 2));
        var yh = y + k1 * h;
        var dy = (k1 - k0) * h;
        return (yh, dy);
    }
    public static (genlist<double>, genlist<Vec>) RKDriver(Func<double, Vec, Vec> f, double a, double b, Vec y, double h = 0.1, double acc = 1e-3, double eps = 1e-3)
    {
        var xlist = new genlist<double>();
        var ylist = new genlist<Vec>();
        xlist.add(a); ylist.add(y.Copy());
        while (a < b)
        {
            if (a + h > b) h = b - a;
            var (yh, dy) = RKStep(f, a, y, h);
            double tol = (acc + eps * yh.Norm()) * Sqrt(h / (b - a));
            double err = dy.Norm();
            if (err <= tol)
            {
                a += h; y = yh;
                xlist.add(a); ylist.add(y.Copy());
            }
            h *= Min(Pow(tol / err, 0.25) * 0.95, 2);
        }
        return (xlist, ylist);
    }
}

// --- Begin Newton + Tests + Schrödinger ---
public static class NewtonSolver
{
    public static void Main()
    {
        Test1D(); RosenbrockMin(); HimmelblauMin(); HydrogenGroundState();
        ExportHydrogenWaveFunction();
    }

    static Vec Newton(Func<Vec, Vec> f, Vec x, double acc = 1e-6)
    {
        Vec fx = f(x);
        while (true)
        {
            if (fx.Norm() < acc) return x;
            Matrix J = Jacobian(f, x, fx);
            var qr = new QR(J);
            Vec Dx = qr.Solve(-fx);
            double lambda = 1;
            Vec z, fz;
            while (true)
            {
                z = x + Dx * lambda;
                fz = f(z);
                if (fz.Norm() < (1 - lambda / 2) * fx.Norm() || lambda < 1e-10) break;
                lambda /= 2;
            }
            if (Dx.Norm() < 1e-10) break;
            x = z; fx = fz;
        }
        return x;
    }

    static Matrix Jacobian(Func<Vec, Vec> f, Vec x, Vec fx = null)
    {
        int n = x.Length;
        if (fx == null) fx = f(x);
        Matrix J = new Matrix(n, n);
        for (int j = 0; j < n; j++)
        {
            double dxj = Max(Abs(x[j]), 1.0) * Pow(2, -26);
            x[j] += dxj;
            Vec dF = f(x) - fx;
            for (int i = 0; i < n; i++) J[i, j] = dF[i] / dxj;
            x[j] -= dxj;
        }
        return J;
    }

    static void Test1D()
    {
        Func<Vec, Vec> f = v => new Vec(v[0] * v[0] - 2);
        var root = Newton(f, new Vec(new double[] { 1.0 }));
        Console.WriteLine($"1D root: {root[0]} ≈ {Sqrt(2)}");
    }

    static void RosenbrockMin()
    {
        Func<Vec, Vec> grad = v => new Vec(
            -2 * (1 - v[0]) - 400 * v[0] * (v[1] - v[0] * v[0]),
            200 * (v[1] - v[0] * v[0])
        );
        var root = Newton(grad, new double[] { -1.2, 1.0 });
        Console.WriteLine($"Rosenbrock min at: {root}");
    }

    static void HimmelblauMin()
    {
        Func<Vec, Vec> grad = v => new Vec(
            4 * v[0] * (v[0] * v[0] + v[1] - 11) + 2 * (v[0] + v[1] * v[1] - 7),
            2 * (v[0] * v[0] + v[1] - 11) + 4 * v[1] * (v[0] + v[1] * v[1] - 7)
        );
        var root = Newton(grad, new double[] { 6.0, 6.0 });
        Console.WriteLine($"Himmelblau min at: {root}");
    }

    static void HydrogenGroundState()
    {
        double rmin = 1e-4, rmax = 8.0;
        Func<double, double> M = E =>
        {
            Func<double, Vec, Vec> F = (r, y) => new Vec(y[1], -2 * (-1 / r - E) * y[0]);
            Vec y0 = new Vec(rmin - rmin * rmin, 1 - 2 * rmin);
            var (rs, ys) = OdeSolver.RKDriver(F, rmin, rmax, y0);
            return ys[ys.size() - 1][0];
        };

        double E1 = -1.0, E2 = -0.1;
        double root = Bisection(M, E1, E2);
        Console.WriteLine($"Hydrogen ground state E0 = {root} (expected -0.5)");
    }

    static double Bisection(Func<double, double> f, double a, double b, double acc = 1e-6)
    {
        double fa = f(a), fb = f(b);
        if (fa * fb > 0) throw new Exception("Bisection error");
        while (b - a > acc)
        {
            double c = (a + b) / 2, fc = f(c);
            if (fa * fc < 0) { b = c; fb = fc; } else { a = c; fa = fc; }
        }
        return (a + b) / 2;
    }

    static void ExportHydrogenWaveFunction()
    {
        double rmin = 1e-4, rmax = 8.0;
        double E = -0.5;
        Func<double, Vec, Vec> F = (r, y) => new Vec(y[1], -2 * (-1 / r - E) * y[0]);
        Vec y0 = new Vec(rmin - rmin * rmin, 1 - 2 * rmin);
        var (rs, ys) = OdeSolver.RKDriver(F, rmin, rmax, y0);

        using var writer = new StreamWriter("wavefunction.csv");
        writer.WriteLine("r,f(r)");
        for (int i = 0; i < rs.size(); i++)
        {
            writer.WriteLine($"{rs[i]},{ys[i][0]}");
        }
        Console.WriteLine("Wavefunction data written to wavefunction.csv");
    }
}
