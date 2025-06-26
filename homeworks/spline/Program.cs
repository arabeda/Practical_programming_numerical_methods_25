using System;

class Program
{
    static void Main()
    {
        // Przykładowe dane: cos(x) dla x = 0..9
        int n = 10;
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = i;
            y[i] = Math.Cos(i);
        }

        Console.WriteLine("=== Linear spline ===");
        for (double z = 0; z <= 9; z += 1.0)
        {
            double interp = LinearSpline.linterp(x, y, z);
            double integ = LinearSpline.linterpInteg(x, y, z);
            Console.WriteLine($"z={z:F2}, interp={interp:F4}, integral={integ:F4}");
        }

        Console.WriteLine("\n=== Quadratic spline ===");
        qspline q = new qspline(x, y);
        for (double z = 0; z <= 9; z += 1.0)
        {
            Console.WriteLine($"z={z:F2}, f={q.evaluate(z):F4}, f'={q.derivative(z):F4}, ∫f={q.integral(z):F4}");
        }

        Console.WriteLine("\n=== Cubic spline ===");
        cspline c = new cspline(x, y);
        for (double z = 0; z <= 9; z += 1.0)
        {
            Console.WriteLine($"z={z:F2}, f={c.evaluate(z):F4}, f'={c.derivative(z):F4}, ∫f={c.integral(z):F4}");
        }
    }
}

// =================== Punkt A: Linear Spline ===================

public class LinearSpline
{
    public static int binsearch(double[] x, double z)
    {
        if (z < x[0] || z > x[x.Length - 1]) throw new Exception("binsearch: bad z");
        int i = 0, j = x.Length - 1;
        while (j - i > 1)
        {
            int mid = (i + j) / 2;
            if (z > x[mid]) i = mid;
            else j = mid;
        }
        return i;
    }

    public static double linterp(double[] x, double[] y, double z)
    {
        int i = binsearch(x, z);
        double dx = x[i + 1] - x[i];
        if (!(dx > 0)) throw new Exception("uups...");
        double dy = y[i + 1] - y[i];
        return y[i] + dy / dx * (z - x[i]);
    }

    public static double linterpInteg(double[] x, double[] y, double z)
    {
        if (z < x[0]) return 0;
        if (z > x[x.Length - 1]) z = x[x.Length - 1];

        double integral = 0;
        int i = 0;
        while (i < x.Length - 1 && x[i + 1] < z)
        {
            double dx = x[i + 1] - x[i];
            double dy = y[i + 1] - y[i];
            double a = y[i];
            double b = dy / dx;
            integral += a * dx + 0.5 * b * dx * dx;
            i++;
        }

        if (i < x.Length - 1)
        {
            double dx = z - x[i];
            double dy = y[i + 1] - y[i];
            double full_dx = x[i + 1] - x[i];
            double a = y[i];
            double b = dy / full_dx;
            integral += a * dx + 0.5 * b * dx * dx;
        }

        return integral;
    }
}

// =================== Punkt B: Quadratic Spline ===================

public class qspline
{
    private int n;
    private double[] x, y, b, c;

    public qspline(double[] xs, double[] ys)
    {
        n = xs.Length;
        x = (double[])xs.Clone();
        y = (double[])ys.Clone();
        b = new double[n - 1];
        c = new double[n - 1];

        double[] dx = new double[n - 1];
        double[] dy = new double[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            dx[i] = x[i + 1] - x[i];
            dy[i] = y[i + 1] - y[i];
        }

        c[0] = 0;
        for (int i = 0; i < n - 2; i++)
        {
            c[i + 1] = (dy[i + 1] - dy[i] - c[i] * dx[i]) / dx[i + 1];
        }

        c[n - 2] /= 2;
        for (int i = n - 3; i >= 0; i--)
        {
            c[i] = (dy[i + 1] - dy[i] - c[i + 1] * dx[i + 1]) / dx[i];
        }

        for (int i = 0; i < n - 1; i++)
        {
            b[i] = dy[i] / dx[i] - c[i] * dx[i];
        }
    }

    private int binsearch(double z)
    {
        int i = 0, j = n - 1;
        while (j - i > 1)
        {
            int mid = (i + j) / 2;
            if (z > x[mid]) i = mid;
            else j = mid;
        }
        return i;
    }

    public double evaluate(double z)
    {
        int i = binsearch(z);
        double dx = z - x[i];
        return y[i] + b[i] * dx + c[i] * dx * dx;
    }

    public double derivative(double z)
    {
        int i = binsearch(z);
        double dx = z - x[i];
        return b[i] + 2 * c[i] * dx;
    }

    public double integral(double z)
    {
        int i = binsearch(z);
        double sum = 0;
        for (int j = 0; j < i; j++)
        {
            double dx = x[j + 1] - x[j];
            sum += y[j] * dx + b[j] * dx * dx / 2 + c[j] * dx * dx * dx / 3;
        }

        double dz = z - x[i];
        sum += y[i] * dz + b[i] * dz * dz / 2 + c[i] * dz * dz * dz / 3;
        return sum;
    }
}

// =================== Punkt C: Cubic Spline ===================

public class cspline
{
    private int n;
    private double[] x, y, b, c, d;

    public cspline(double[] xs, double[] ys)
    {
        n = xs.Length;
        x = (double[])xs.Clone();
        y = (double[])ys.Clone();
        b = new double[n - 1];
        c = new double[n];
        d = new double[n - 1];

        double[] dx = new double[n - 1];
        double[] dy = new double[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            dx[i] = x[i + 1] - x[i];
            dy[i] = y[i + 1] - y[i];
        }

        double[] D = new double[n];
        double[] Q = new double[n - 1];
        double[] B = new double[n];

        D[0] = 1; D[n - 1] = 1;
        B[0] = 0; B[n - 1] = 0;

        for (int i = 1; i < n - 1; i++)
        {
            D[i] = 2 * (dx[i - 1] + dx[i]);
            Q[i] = dx[i];
            B[i] = 3 * (dy[i] / dx[i] - dy[i - 1] / dx[i - 1]);
        }

        // Forward elimination
        for (int i = 1; i < n - 1; i++)
        {
            double mult = dx[i - 1] / D[i - 1];
            D[i] -= mult * Q[i - 1];
            B[i] -= mult * B[i - 1];
        }

        c[n - 1] = 0;
        for (int i = n - 2; i >= 1; i--)
        {
            c[i] = (B[i] - Q[i] * c[i + 1]) / D[i];
        }
        c[0] = 0;

        for (int i = 0; i < n - 1; i++)
        {
            b[i] = dy[i] / dx[i] - dx[i] * (2 * c[i] + c[i + 1]) / 3;
            d[i] = (c[i + 1] - c[i]) / (3 * dx[i]);
        }
    }

    private int binsearch(double z)
    {
        int i = 0, j = n - 1;
        while (j - i > 1)
        {
            int mid = (i + j) / 2;
            if (z > x[mid]) i = mid;
            else j = mid;
        }
        return i;
    }

    public double evaluate(double z)
    {
        int i = binsearch(z);
        double dx = z - x[i];
        return y[i] + b[i] * dx + c[i] * dx * dx + d[i] * dx * dx * dx;
    }

    public double derivative(double z)
    {
        int i = binsearch(z);
        double dx = z - x[i];
        return b[i] + 2 * c[i] * dx + 3 * d[i] * dx * dx;
    }

    public double integral(double z)
    {
        int i = binsearch(z);
        double sum = 0;
        for (int j = 0; j < i; j++)
        {
            double dx = x[j + 1] - x[j];
            sum += y[j] * dx + b[j] * dx * dx / 2 + c[j] * dx * dx * dx / 3 + d[j] * dx * dx * dx * dx / 4;
        }

        double dz = z - x[i];
        sum += y[i] * dz + b[i] * dz * dz / 2 + c[i] * dz * dz * dz / 3 + d[i] * dz * dz * dz * dz / 4;
        return sum;
    }
}
